using AutoMapper;
using GrpcService;
using Ninject.Extensions.Logging;
using ScoutDomains;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutModels.Interfaces;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HawkeyeCoreAPI.Facade;
using ScoutUtilities.Common;
using ScoutUtilities.Helper;
using SamplePostWash = ScoutUtilities.Enums.SamplePostWash;
using SampleSet = HawkeyeCoreAPI.SampleSet;
using ScoutModels.Admin;
using SampleRecordDomain = ScoutDomains.SampleRecordDomain;

namespace ScoutServices
{
    public class SampleResultsManager : Disposable, ISampleResultsManager
    {
        #region Fields

        private static readonly Subject<ExportStatusEvent> _exportStatusSubject = new Subject<ExportStatusEvent>();
        private readonly Subject<SampleProgressEventArgs> _deleteSampleResultsProgress;
        private readonly Subject<SamplesDeletedEventArgs> _samplesDeleted;

        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        public SampleResultsManager(IMapper mapper, ILogger logger)
        {
            _deleteSampleResultsProgress = new Subject<SampleProgressEventArgs>();
            _samplesDeleted = new Subject<SamplesDeletedEventArgs>();

            _mapper = mapper;
            _logger = logger;
        }

        protected override void DisposeManaged()
        {
            _deleteSampleResultsProgress?.OnCompleted();
             _exportStatusSubject?.OnCompleted();
            _deleteSampleResultsProgress?.Dispose();
            _exportStatusSubject?.Dispose();
            _samplesDeleted?.Dispose();
            base.DisposeManaged();
        }

        #endregion

        #region Public Methods

        #region Delete Sample Results

        public bool DeleteSampleResults(string username, string password, List<uuidDLL> sampleResultsToDelete, bool retainResultsAndFirstImage)
        {
            try
            {
                _samplesDeleted.OnNext(new SamplesDeletedEventArgs(0, new uuidDLL())); //let listeners know samples are starting to be deleted
                _deleteSampleResultsProgress.OnNext(new SampleProgressEventArgs(0, false, HawkeyeError.eSuccess));
                ExportManager.EvDeleteSamplesReq.Publish(username, password, sampleResultsToDelete, retainResultsAndFirstImage, OnDeleteProgress);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error processing DeleteSampleResults request");
                return false;
            }
        }

        private void OnDeleteProgress(HawkeyeError status, uuidDLL sampleUuid, int percent)
        {
            try
            {
                if (_deleteSampleResultsProgress != null)
                    _deleteSampleResultsProgress.OnNext(new SampleProgressEventArgs(percent, percent >= 100, status));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error processing _deleteSampleResultsProgress.OnNext");
            }
            try
            {
                if (_samplesDeleted != null)
                    _samplesDeleted.OnNext(new SamplesDeletedEventArgs(percent, sampleUuid));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error processing _samplesDeleted.OnNext");
            }
        }

        public IObservable<ExportStatusEvent> SubscribeExportStatus()
        {
            return _exportStatusSubject;
        }

        public IObservable<SampleProgressEventArgs> SubscribeDeleteSampleResultsProgress()
        {
                return _deleteSampleResultsProgress;
        }

        public IObservable<SamplesDeletedEventArgs> SubscribeSamplesDeleted()
        {
            return _samplesDeleted;
        }

        //delete_sample_record_callback_pcnt
        private void DeleteSamplesModel_DeletionProgress(object sender, SampleProgressEventArgs args)
        {
            try
            {
                _deleteSampleResultsProgress.OnNext(args);
            }
            catch (Exception e)
            {
                _deleteSampleResultsProgress.OnError(e);
            }
        }
        #endregion

        #region Retrieve Sample Results


        public bool CheckUserPrivilegedData(string loggedInUser, string requestedUsername, out string errMsg)
        {
            errMsg = string.Empty;
            // Deny a normal/bci_service user from getting sample results owned by others
            if (UserModel.GetUserRole(loggedInUser) != UserPermissionLevel.eAdministrator &&
                UserModel.GetUserRole(loggedInUser) != UserPermissionLevel.eElevated &&
                !String.IsNullOrEmpty(requestedUsername) &&
                loggedInUser != requestedUsername)
            {
                errMsg = "Getting other user's results is not permitted";
                return false;
            }
            return true;
        }

        public bool GetResultsValidation(RequestGetSampleResults request)
        {
            if (!string.IsNullOrWhiteSpace(request.SearchNameString))
            {
                return !Misc.ContainsInvalidCharacter(request.SearchNameString);
            }

            return true;
        }

        public List<SampleResult> GetSampleResults(string username, string password, RequestGetSampleResults request)
        {
            var list = new List<SampleResult>();
            try
            {
                // If a Normal user leaves the username filter empty, fill it in with their own so that they don't get data other user's data
                var searchUser = request.Username;
                if (UserModel.GetUserRole(username) != UserPermissionLevel.eAdministrator &&
                    UserModel.GetUserRole(username) != UserPermissionLevel.eElevated &&
                    String.IsNullOrEmpty(request.Username))
                {
                    searchUser = username;
                }

                var fromDate = DateTimeConversionHelper.DateTimeToUnixSecondRounded(
                    request.FromDate.ToDateTime().ToLocalTime());
                var toDate = DateTimeConversionHelper.DateTimeToUnixSecondRounded(
                    request.ToDate.ToDateTime().ToLocalTime());
                if (!string.IsNullOrWhiteSpace(request.SearchNameString))
                {
                    request.SearchNameString = request.SearchNameString.Trim();
                }
                var filter = _mapper.Map<eFilterItem>(request.FilterType);
                SampleSet.GetSampleSetListApiCall(filter,
                    fromDate, toDate, searchUser,
                    request.SearchNameString, request.SearchTagString, request.CellTypeQualityControlName,
                    0, 0, out var totalQueryResultCount, out var sampleSets);

                foreach (var sampleSet in sampleSets)
                {
                    GetSampleSet(sampleSet.Uuid, true, out var sampleSetDomain);

                    foreach (var sample in sampleSetDomain.Samples)
                    {
                        if (sample.SampleRecord != null)
                        {
                            foreach (var result in sample.SampleRecord.ResultSummaryList)
                            {
                                sample.SampleRecord.SelectedResultSummary = result;
                                var sampleResult = _mapper.Map<SampleResult>(sample);
                                list.Add(sampleResult);
                            }
                        }
                        else
                        {
                            var sampleResult = _mapper.Map<SampleResult>(new SampleEswDomain());
                            list.Add(sampleResult);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error processing GetSampleResults request");
                return null;
            }
           
            return list;
        }

        public HawkeyeError GetSampleSet(uuidDLL sampleSetUuidDll, bool getSampleResults,
            out SampleSetDomain sampleSetDomain)
        {
            var result = SampleSet.GetSampleSetAndSampleListApiCall(sampleSetUuidDll, out sampleSetDomain, 
                getSampleResults);

            if (sampleSetDomain == null)
            {
                return result;
            }

            if (sampleSetDomain.Samples == null)
            {
                sampleSetDomain.Samples = new List<SampleEswDomain>();
            }

            return result;
        }

        private static void PublishExportStatus(ExportStatusEvent stev)
        {
            try
            {
                _exportStatusSubject.OnNext(stev);
            }
            catch (Exception e)
            {
                _exportStatusSubject.OnError(e);
            }
        }

        #endregion

        #region Export Sample Results

        private static string _curBulkDataId = "";
        public string StartExport(string username, string password, RequestStartExport request)
        {
            var sampleIds = new List<uuidDLL>();
            foreach (var uuid in request.SampleListUuid)
            {
                var sampleId = new uuidDLL(Guid.Parse(uuid));
                //If a sample has been reanalyzed the original and reanalyzed samples have the same UUIDs
                //We export both the original and reanalyzed results regardless of which one the user selected, 
                //so we should prevent exporting the same results multiple times if the same UUID was selected more than once 
                if (!sampleIds.Contains(sampleId))
                {
                    sampleIds.Add(sampleId);
                }
            }

            int removeCount = 0;
            for (int j = 0; j < sampleIds.Count(); j++)
            {
                if (sampleIds[j].IsNullOrEmpty())
                {
                    removeCount++;
                    sampleIds.RemoveAt(j);
                    j--;
                }
            }

            _curBulkDataId = Guid.NewGuid().ToString();

            var tempDir = ScoutUtilities.UIConfiguration.UISettings.DriveName + Path.DirectorySeparatorChar;
            tempDir += ApplicationConstants.ExportTempDir + Path.DirectorySeparatorChar;

            ClearDirectory(tempDir);

            if (request.ExportType == GrpcService.ExportTypeEnum.Csv)
            {
	            List<SampleRecordDomain> samples;
	            var res = HawkeyeCoreAPI.Sample.RetrieveSampleRecordListAPI(sampleIds.ToArray(), out samples, out var retrieveSize);
	            if (res != HawkeyeError.eSuccess)
	            {
		            return "";
	            }

	            List<SampleRecordDomain> reanalyzedSamplesList = new List<SampleRecordDomain>();

	            foreach (var sample in samples)
	            {
		            var result = HawkeyeCoreAPI.SampleSet.GetSampleDefinitionBySampleIdApiCall(sample.UUID, out var samplePosition);
		            if (result == HawkeyeError.eSuccess)
		            {
			            sample.Position = samplePosition;
		            }
		            if (sample.ResultSummaryList?.Count > 1)
		            {
			            //The sample has been reanalyzed - original result is sample.ResultSummaryList[0]. Add reanalyzed results
			            for (int i = 1; i < sample.ResultSummaryList.Count; i++)
			            {
				            var reanalyzedSample = (ScoutDomains.SampleRecordDomain)sample.Clone();
				            reanalyzedSample.SelectedResultSummary = sample.ResultSummaryList[i];
				            reanalyzedSamplesList.Add(reanalyzedSample);
			            }
		            }
	            }

	            samples.AddRange(reanalyzedSamplesList);
				var summaryCsvBase = ApplicationConstants.SummaryExportFileNameAppendant;
	            try
	            {
	                summaryCsvBase = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_Summary");
	            }
	            catch (Exception e)
	            {
	                _logger.Error(e, $"StartExport translation error LID_Label_Summary");
	            }
	            
	            _localExportFilename = tempDir + summaryCsvBase + ".csv";

	            ExportManager.EvExportCsvReq.Publish
	                (username, _curBulkDataId, samples,
	                tempDir, tempDir,
	                summaryCsvBase, ApplicationConstants.ExportTempFileName,
	                true, false, 
	                true, true, 1, 
	                _callbackExportDataProgress);
            }
            else
            { // Offline Export for Automation.
	            List<SampleRecordDomain> samples;
	            var res = HawkeyeCoreAPI.Sample.RetrieveSampleRecordListAPI(sampleIds.ToArray(), out samples, out var retrieveSize);
	            if (res != HawkeyeError.eSuccess)
	            {
		            return "";
	            }

	            List<uuidDLL> summaryResultUuidList = new List<uuidDLL>();

	            foreach (var sample in samples)
	            {
		            for (int i = 0; i < sample.ResultSummaryList.Count; i++)
		            {
			            summaryResultUuidList.Add(sample.ResultSummaryList[i].UUID);
		            }
	            }

	            ExportManager.EvExportOfflineReq.Publish(
		            username, _curBulkDataId,
		            summaryResultUuidList, tempDir, "",
		            eExportImages.eExportNthImage, (ushort)request.NthImageToExport, request.IsAutomationExport,
		            null, /*OnExportDataProgress,*/
		            null, //OnExportDataCompletion,
		            _callbackExportDataProgress);
			}

            return _curBulkDataId;
        }

		public string StartLogDataExport (RequestStartLogDataExport request)
		{
			_curBulkDataId = Guid.NewGuid().ToString();
			_logger.Debug($"StartLogDataExport: _curBulkDataId: {_curBulkDataId}");

			var tempDir = ScoutUtilities.UIConfiguration.UISettings.DriveName + Path.DirectorySeparatorChar;
			tempDir += ApplicationConstants.ExportTempDir + Path.DirectorySeparatorChar;
			ClearDirectory(tempDir);

			_localExportFilename = request.Filename;
			_logger.Debug($"StartLogDataExport: _localExportFilename: {_localExportFilename}");

			string localExportFilename = tempDir + "LogData.csv";
			_logger.Debug($"StartLogDataExport: localExportFilename: {localExportFilename}");

			ExportManager.EvExportLogDataCsvReq.Publish
			(
				request.FromDate.ToDateTime().ToLocalTime(), request.ToDate.ToDateTime().ToLocalTime(),
				_localExportFilename,
				localExportFilename,
				_curBulkDataId, 
				_callbackExportDataProgress);

			return _curBulkDataId;
        }

        private static void ClearDirectory(string path)
        {
            try
            {

                if (!Directory.Exists(path))
                {
                    if (Path.HasExtension(path))
                    {
                        path = Path.GetDirectoryName(path);
                        if (!Directory.Exists(path))
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                var allFiles = Directory.GetFiles(path);
                foreach (var file in allFiles)
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("ClearDirectory delete file: " + file + " exception ", e);
                    }
                }

                var subDirs = Directory.GetDirectories(path);
                foreach (var subDir in subDirs)
                {
                    ClearDirectory(subDir);
                    try
                    {
                        if (Directory.Exists(subDir))
                            Directory.Delete(subDir);
                    }
                    catch (Exception e)
                    {
                        Log.Error("ClearDirectory Directory.Delete subDir " + subDir + " exception ", e);
                    }
                }

                try
                {
                    if (Directory.Exists(path))
                        Directory.Delete(path);
                }
                catch (Exception e)
                {
                    Log.Error("ClearDirectory Directory.Delete path " + path + " exception ", e);
                }
            }
            catch (Exception e)
            {
                Log.Error("ClearDirectory exception ", e);
            }
        }


        private static string _localExportFilename = "";
        private BinaryReader _exportReader;
        public RetrieveBulkBlockStatusEnum GetBulkDataBlock(uint blockIdx, string dataId, out byte[] data)
        {
            if (!_curBulkDataId.Equals(dataId))
            {
                Log.Error("GetBulkDataBlock bad dataId " + dataId);
                data = new byte[0];
                return RetrieveBulkBlockStatusEnum.RbsError;
            }

            if (blockIdx == 0)
            {
                try
                {
                    if (_exportReader != null)
                    {
                        _exportReader.Close();
                        _exportReader = null;
                    }
                    _exportReader = new BinaryReader(File.Open(_localExportFilename, FileMode.Open));
                }
                catch (Exception e)
                {
                    Log.Error("GetBulkDataBlock failed to open file " + _localExportFilename, e);
                    data = new byte[0];
                    return RetrieveBulkBlockStatusEnum.RbsError;
                }
            }

            // Might be able to improve transfer speed by adjusting these values
            int maxCount = 1024 * 512;

            data = new byte[maxCount];
            int actualCount = _exportReader.Read(data, 0, maxCount);
            if (actualCount >= maxCount)
            {
                return RetrieveBulkBlockStatusEnum.RbsSuccess;
            }
            _exportReader.Close();
            _exportReader = null;

            // Only return the actual bytes read
            byte[] temp = new byte[actualCount];
            Array.Copy(data, temp, actualCount);
            data = new byte[actualCount];
            Array.Copy(temp, data, actualCount);

            try
            {
                File.Delete(_localExportFilename);
            }
            catch (Exception e)
            {
                Log.Error("GetBulkDataBlock File.Delete " + _localExportFilename + " exception", e);
            }
            ClearDirectory(Path.GetDirectoryName(_localExportFilename));

            _localExportFilename = "";
            return RetrieveBulkBlockStatusEnum.RbsDone;
        }


        private readonly ScoutUtilities.Delegate.export_data_status_callback _callbackExportDataProgress 
	        = (filename, bulkDataId, ExportStatus, percent) =>
        {
            if(ExportStatus == (uint)ExportStatusEnum.EsReady)
            {
                _localExportFilename = filename;
            }
            var stev = new ExportStatusEvent();
            stev.StatusInfo = new ExportStatusData();
            stev.StatusInfo.Status = (ExportStatusEnum)ExportStatus;
            stev.StatusInfo.BulkDataId = bulkDataId;
            stev.StatusInfo.Percent = percent;
            PublishExportStatus(stev);
        };

        #endregion

        #endregion
    }
}
