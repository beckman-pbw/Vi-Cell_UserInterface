using GrpcService;
using ScoutServices.Interfaces;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using HawkeyeCoreAPI.Facade;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutModels.Common;
using ScoutServices.DTOs;
using ScoutServices.Enums;
using ScoutUtilities.Common;
using ScoutUtilities.UIConfiguration;

namespace ScoutServices
{
    public class ConfigurationManager : IConfigurationManager
    {
        #region Fields

        private readonly Subject<ConfigSubjectDto> _configStateChangeSubject;
        private readonly Subject<ulong> _availableDiskSpaceSubject;
        private DiskSpaceModel _diskSpaceModel;

        private static readonly object _subscribeLock = new object();
        private static readonly object _publishLock = new object();

        #endregion

        #region Constructors

        public ConfigurationManager()
        {
            _configStateChangeSubject = new Subject<ConfigSubjectDto>();
            _availableDiskSpaceSubject = new Subject<ulong>();
            _diskSpaceModel = new DiskSpaceModel();
        }

        #endregion

        #region Public Methods

        public IObservable<ConfigSubjectDto> SubscribeStateChanges()
        {
            lock (_subscribeLock)
            {
                return _configStateChangeSubject;
            }
        }

        //TODO... doesn't look like we're using this variable, thing about deleting it and its publish/subscribe code
        public IObservable<ulong> SubscribeAvailableDiskSpace()
        {
            return _availableDiskSpaceSubject;
        }

        public byte[] ExportConfig(string username, string password, RequestExportConfig request)
        {
            try
            {
                var runLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string drive = Path.GetPathRoot(runLocation);

                var basePath = drive + ApplicationConstants.BasePathToExportedZip;
                var randomGuid = Guid.NewGuid() + ".cfg";
                var filePath = Path.GetFullPath(Path.Combine(basePath, randomGuid));
                var result = HawkeyeCoreAPI.Configuration.ExportInstrumentConfigurationAPI(username, password, filePath);

                byte[] encryptedContents = null;
                if (result == HawkeyeError.eSuccess)
                {
                    if (File.Exists(filePath))
                    {
                        encryptedContents = File.ReadAllBytes(filePath);
                        File.Delete(filePath);
                    }
                }
                else
                {
                    ExportManager.EvAppLogReq.Publish("ExportConfig", ExportManager.EvAppLogReq.LogLevel.Warning, "ExportInstrumentConfigurationAPI failed", 0);
                }
                var configDto = new ConfigSubjectDto
                {
                    Result = result,
                    State = ConfigState.Export,
                    FileData = encryptedContents
                };

                PublishConfigStatus(configDto);

                return encryptedContents;
            }
            catch (Exception)
            {
                PublishConfigStatus(null);
            }

            return null;
        }

        public HawkeyeError ImportConfig(string username, string password, RequestImportConfig request)
        {
            var result = HawkeyeError.eNotPermittedAtThisTime;

            try
            {
                if (HawkeyeCoreAPI.Configuration.SystemHasDataAPI())
                {
                    ExportManager.EvAppLogReq.Publish("ImportConfig", ExportManager.EvAppLogReq.LogLevel.Warning, "Data exists, cannot import config", 0);
                    return HawkeyeError.eNotPermittedAtThisTime;
                }

                var runLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string drive = Path.GetPathRoot(runLocation);
                var basePath = drive + ApplicationConstants.BasePathToExportedZip;
                var randomGuid = Guid.NewGuid() + ".cfg";                
                var filePath = Path.GetFullPath(Path.Combine(basePath, randomGuid));

                string outDir = Path.GetDirectoryName(filePath);
                DirectoryInfo diOut = new DirectoryInfo(outDir);
                if (!diOut.Exists)
                {
                    diOut.Create();
                    diOut = new DirectoryInfo(outDir);
                }
                File.WriteAllBytes(filePath, request.FileData.ToByteArray());

                result = HawkeyeCoreAPI.Configuration.ImportInstrumentConfigurationAPI(username, password, filePath);

                if (result == HawkeyeError.eSuccess)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                else
                {
                    ExportManager.EvAppLogReq.Publish("ImportConfig", ExportManager.EvAppLogReq.LogLevel.Warning, "ImportInstrumentConfigurationAPI failed", 0);
                }

                var configDto = new ConfigSubjectDto
                {
                    Result = result,
                    State = ConfigState.Import,
                    FileData  = null
                };

                PublishConfigStatus(configDto);

                return result;
            }
            catch (Exception)
            {
                PublishConfigStatus(null);
            }

            return HawkeyeError.eSoftwareFault;
        }

        public bool GetDiskSpaceVariables(out double total_size, out double total_free, out double other_size, out double data_size, out double export_size)
        {
            try
            {   
                _diskSpaceModel.CalculateDiskSpace();
                total_size = _diskSpaceModel.TotalDiskSpace;
                total_free = _diskSpaceModel.TotalFreeSpace;
                other_size = _diskSpaceModel.SizeOfOther;
                data_size = _diskSpaceModel.SizeOfData;
                export_size = _diskSpaceModel.SizeOfExport;
                PublishAvailableDiskSpace((ulong)total_free);
            }
            catch (Exception)
            {
                total_size = 0;
                total_free = 0;
                other_size = 0;
                data_size = 0;
                export_size = 0;
                PublishAvailableDiskSpace((ulong)total_free);
                return false;
            }
            return true;
        }

        public void PublishAvailableDiskSpace(ulong totalFreeSpace)
        {
            if (totalFreeSpace == 0)
                return;

            try
            {
                _availableDiskSpaceSubject.OnNext(totalFreeSpace);
            }
            catch (Exception e)
            {
                _availableDiskSpaceSubject.OnError(e);
            }
        }

        public void PublishConfigStatus(ConfigSubjectDto val)
        {
            if (val == null)
                return;

            try
            {
                lock (_publishLock)
                {
                    _configStateChangeSubject.OnNext(val);
                }
            }
            catch (Exception e)
            {
                lock (_publishLock)
                {
                    _configStateChangeSubject.OnError(e);
                }
            }
        }

        #endregion
    }
}
