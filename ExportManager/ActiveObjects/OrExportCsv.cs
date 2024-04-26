using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.IO.Compression;
using System.Collections.ObjectModel;

using ScoutDomains.DataTransferObjects;
using System.Runtime.InteropServices;
using ScoutDomains;
using ScoutDomains.Common;
using ScoutDomains.Reports.Common;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Delegate;
using ScoutUtilities.Structs;
using ScoutLanguageResources;

using ScoutModels.Settings;
using ScoutModels.Reports;

using ScoutModels.Common;

using BAFW;

namespace ExportManager
{
    // ***********************************************************************
    public class OrExportCsv : BOrthoRegion
    {

        internal const UInt32 kORTHO_ID_MASK = 0x00000200;

        #region Private_Events
        private enum PrivateEvIds : UInt32
        {
            iReset = kORTHO_ID_MASK
        }

        internal void ResetNow()
        {
            DeliverEvent(new BPrivateEvent((uint)PrivateEvIds.iReset));
        }
        #endregion

        internal bool InReset { get; set; } = false;

        #region Private_Members
        private AoExportMgr _parent = null;
        #endregion

        #region Construct_Destruct
        // ***********************************************************************
        public OrExportCsv(AoExportMgr parent, UInt32 orthoId)
            : base(parent, orthoId)
        {
            _parent = parent;
            _tmrSequence = new BTimer(parent.GetEventQueue(), (UInt32)TimerIds.Sequence);

            SetRootState(ST_Root);
            InitStateMachine();
        }
        #endregion

        #region Trace_Debug
        public static byte LogSubSysId = 0;
        private const string kMODULE_NAME = "OrExportCsv";
        // ***********************************************************************
        private void TraceThis(string strData)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Trace, strData, LogSubSysId);
        }
        private void DebugThis(string strData)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Debug, strData, LogSubSysId);
        }
        private static void WarnThis(string strData)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Warning, strData, LogSubSysId);
        }
        private static void ErrorThis(string strData)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Error, strData, LogSubSysId);
        }
        private static void ErrorThis(string strData, Exception ex)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Error, strData + "Exception: " + ex.Message, LogSubSysId);
        }
        #endregion

        #region Timers
        private BTimer _tmrSequence;
        // ***********************************************************************
        private enum TimerIds : UInt32
        {
            Sequence = kORTHO_ID_MASK
        }
        #endregion

        #region State_Machine
        // ******************************************************************
        //                    ST_Root
        // ******************************************************************
        private State ST_Root(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_Root Entry");
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_Root Exit");
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Init:
                {
                    TraceThis("ST_Root Init => ST_Reset");
                    SetState(ST_Reset);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Private:
                {
                    if (ev.Id == (uint)PrivateEvIds.iReset)
                    {
                        TraceThis("ST_Root Private iReset Trans => ST_Reset");
                        DoTransition(ST_Reset);
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Public:
                case BEvent.EvType.Timer:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return null;
        }



        private EvExportCsvReq _currentRequest = null;
        // ******************************************************************
        //                    ST_Reset
        // ******************************************************************
        private State ST_Reset(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_Reset Entry");
                    InReset = true;
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_Reset Exit");
                    InReset = false;
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Public:
                {
                    switch (ev.Id)
                    {
                        case (uint)PubEvIds.ExportMgr_StartCsv:
                        {
                            _currentRequest = (EvExportCsvReq)ev;
                            TraceThis("ST_Reset Public ExportMgr_StartCsv Trans => ST_Exporting");
                            DoTransition(ST_Exporting);
                            return null;
                        }
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Private:
                {
                    if (ev.Id == (uint)PrivateEvIds.iReset)
                    {
                        TraceThis("ST_Reset Private iReset Do nothing");
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Init:
                case BEvent.EvType.Timer:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Root;
        }

        private string _detailsOutDir = "";
        private string _summaryFile = "";
        private List<string> _sampleFiles = new List<string>();
        private List<string> _imageFiles = new List<string>();
        private ZipArchive _zipArchive = null;
        private string _zipFile = "";
        List<SampleResultRecordExportDomain> _exportRecords = new List<SampleResultRecordExportDomain>();
        // ******************************************************************
        //                    ST_Exporting
        // ******************************************************************
        private State ST_Exporting(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_Exporting Entry");
                    _exportRecords.Clear();
                    _sampleFiles.Clear();
                    _imageFiles.Clear();
                    _detailsOutDir = "";
                    _zipArchive = null;
                    _zipFile = "";
                    _summaryFile = "";
                    _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.Unknown;
                    _resultHelper = new ResultRecordHelper(_currentRequest.Username);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_Exporting Exit");
                    if (_resultHelper != null)
                    {
                        try
                        {
                            _resultHelper.Dispose();
                            _resultHelper = null;
                        }
                        catch (Exception ex)
                        {
                            ErrorThis("ST_Exporting _resultHelper dispose ", ex);
                        }
                    }

                    if (_zipArchive != null)
                    {
                        try
                        {
                            _zipArchive.Dispose();
                            _zipArchive = null;
                            if (File.Exists(_summaryFile))
                            {
                                try { File.Delete(_summaryFile); }
                                catch { ErrorThis("ST_Exporting Exit - Delete summary file exception"); }
                            }
                            if (Directory.Exists(_detailsOutDir))
                            {
                                try { Directory.Delete(_detailsOutDir); }
                                catch { ErrorThis("ST_Exporting Exit - Delete dir exception"); }
                            }
                            if (_exportResult != AoExportMgr.ExportDoneEv.ExpStatus.Success)
                            {
                                if (File.Exists(_zipFile))
                                {
                                    try { File.Delete(_zipFile); }
                                    catch { ErrorThis("ST_Exporting Exit - Delete ZIP file exception"); }
                                }
                            }
                            else
                            {
                                if (_currentRequest.StatusCB != null)
                                {
	                                // "_currentRequest.Password" is really the bulk data ID.
	                                // Send event to tell client that it can start uploading the zip file.
	                                TraceThis("Sending Ready event to client");
                                    _currentRequest.StatusCB(_zipFile, _currentRequest.Password, (uint)GrpcService.ExportStatusEnum.EsReady, 100);
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            ErrorThis("ST_Exporting Exit - zip file file", ex);
                        }
                    }
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Init:
                {
                    TraceThis("ST_Exporting Init => ST_ValidateConfig");
                    SetState(ST_ValidateConfig);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Public:
                {
                    if (ev.Id == (uint)PubEvIds.ExportMgr_CancelCsv)
                    {
                        TraceThis("ST_Exporting Public ExportMgr_CancelCsv Trans => ST_Reset");
                        ScoutModels.Common.ExportModel.ExportFailedMessage();
                        string details = "";
                        try
                        {
                            details += LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                            details += LanguageResourceHelper.Get("LID_Status_Cancelled") + Environment.NewLine;
                        }
                        catch (Exception ex)
                        {
                            ErrorThis("ST_Exporting Exception LanguageResourceHelper", ex);
                        }
                        _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.ReScheduled;
                        _parent.PostCsvDone(OrthoId, _exportResult, "", details);
                        DoTransition(ST_Reset);
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                case BEvent.EvType.Private:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Root;
        }

        // ******************************************************************
        //                    ST_ValidateConfig
        // ******************************************************************
        private State ST_ValidateConfig(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_ValidateConfig Entry");
                    _tmrSequence.FireIn(25, OrthoId);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_ValidateConfig Exit");
                    _tmrSequence.Disarm();
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    string invalidDir = "";
                    if (!string.IsNullOrEmpty(_currentRequest.SummaryOutPath))
                    {
                        if (!FileSystem.IsFolderValidForExport(_currentRequest.SummaryOutPath) ||
                            !FileSystem.EnsureDirectoryExists(_currentRequest.SummaryOutPath))
                        {
                            invalidDir += _currentRequest.SummaryOutPath;
                        }
                    }
                    if (!string.IsNullOrEmpty(_currentRequest.DetailsOutPath))
                    {
                        if (!FileSystem.IsFolderValidForExport(_currentRequest.DetailsOutPath) ||
                            !FileSystem.EnsureDirectoryExists(_currentRequest.DetailsOutPath))
                        {
                            if (invalidDir.Length == 0)                             
                                invalidDir = _currentRequest.DetailsOutPath;
                            else
                                invalidDir += Environment.NewLine + _currentRequest.DetailsOutPath;
                        }
                    }

                    if (invalidDir.Length > 0)
                    {
                        TraceThis("ST_ValidateConfig Timer directory " + invalidDir + " not valid for export Trans => ST_Reset");
                        ExportModel.ExportFailedMessage();

                        string details = "";
                        try
                        {
                            details += LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                            details += invalidDir + Environment.NewLine;
                            details += LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_PathError");
                        }
                        catch (Exception ex)
                        {
                            ErrorThis("ST_ValidateConfig LanguageResourceHelper", ex);
                        }
                        _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.Error;
                        _parent.PostCsvDone(OrthoId, _exportResult, "", details);
                        DoTransition(ST_Reset);
                        return null;
                    }

                    string invalidName = "";
                    foreach(var s in _currentRequest.Samples)
                    {
                        if (!FileSystem.IsFileNameValid(s.SampleIdentifier))                        
                            invalidName += s.SampleIdentifier + " ";                        
                    }
                    if (!string.IsNullOrEmpty(_currentRequest?.ZipFileBase))
                    {
                        if (!FileSystem.IsFileNameValid(_currentRequest?.ZipFileBase))
                            invalidName += _currentRequest.ZipFileBase + " ";
                    }
                    if (!string.IsNullOrEmpty(_currentRequest?.SummaryCsvFileBase))
                    {
                        if (!FileSystem.IsFileNameValid(_currentRequest?.SummaryCsvFileBase))
                            invalidName += _currentRequest.SummaryCsvFileBase + " ";
                    }
                    if (invalidName.Length > 0)
                    {
                        TraceThis("ST_ValidateConfig Timer bad filename Trans => ST_Reset");
                        ExportModel.ExportFailedMessage();

                        string details = "";
                        try
                        {
                            details += LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                            details += invalidDir + Environment.NewLine;
                            details += LanguageResourceHelper.Get("LID_Label_ExportFilename");
                        }
                        catch (Exception ex)
                        {
                            ErrorThis("ST_ValidateConfig LanguageResourceHelper", ex);
                        }
                        _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.Error;
                        _parent.PostCsvDone(OrthoId, _exportResult, "", details);
                        DoTransition(ST_Reset);
                        return null;
                    }


                    TraceThis("ST_ValidateConfig Timer - Config IS valid => Trans => ST_ExportSummary");
                    DoTransition(ST_ExportSummary);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Init:
                case BEvent.EvType.Public:
                case BEvent.EvType.Private:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Exporting;
        }


        private AoExportMgr.ExportDoneEv.ExpStatus _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.Unknown;
        // ******************************************************************
        //                    ST_ExportSummary
        // ******************************************************************
        private State ST_ExportSummary(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_ExportSummary Entry");
                    _tmrSequence.FireIn(ExportDelayMs, OrthoId);
                    _exportRecords.Clear();
                    _detailFiles.Clear();
                    _summaryFile = "";
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_ExportSummary Exit");
                    _tmrSequence.Disarm();
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    if (ev.Id == (uint)TimerIds.Sequence)
                    {
                        TraceThis("ST_ExportSummary Timer - Sequence");
                        try
                        {
                            if (string.IsNullOrEmpty(_currentRequest?.SummaryOutPath) || (_currentRequest.Samples == null))
                            {
                                ErrorThis("ST_ExportSummary Timer bad request or no samples Trans => ST_Reset");
                                ScoutModels.Common.ExportModel.ExportSuccessMessage();

                                string details = "";
                                try
                                {
                                    details += LanguageResourceHelper.Get("LID_MSGBOX_NoDataExport") + Environment.NewLine;
                                }
                                catch (Exception ex)
                                {
                                    ErrorThis("ST_ExportSummary Timer Exception LanguageResourceHelper exception: " + ex.Message);
                                }
                                _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.NoData;
                                _parent.PostCsvDone(OrthoId, _exportResult, "", details);
                                DoTransition(ST_Reset);
                                return null;
                            }
                            try
                            {
                                _exportRecords = ScoutModels.Common.ResultRecordHelper.ExportCompleteRunResult(_currentRequest.Samples);
                            }
                            catch (Exception e)
                            {
                                ErrorThis("ST_ExportSummary Timer ExportCompleteRunResult Exception: " + e.Message);
                                ExportModel.ExportFailedMessage();
                                string details = "";
                                try
                                {
                                    details = LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                                    details += LanguageResourceHelper.Get("LID_MSGBOX_StorageFault");
                                }
                                catch (Exception ex)
                                {
                                    ErrorThis("ST_ExportSummary Timer Exception LanguageResourceHelper exception: " + ex.Message);
                                }
                                _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.Error;
                                _parent.PostCsvDone(OrthoId, _exportResult, "", details);
                                DoTransition(ST_Reset);
                                return null;
                            }

                            if ((_exportRecords == null) || (_exportRecords.Count == 0))
                            {
                                ErrorThis("ST_ExportSummary Timer no export records found Trans => ST_Reset");
                                ScoutModels.Common.ExportModel.ExportSuccessMessage();
                                string details = "";
                                try
                                {
                                    details += LanguageResourceHelper.Get("LID_MSGBOX_NoDataExport") + Environment.NewLine;
                                }
                                catch (Exception ex)
                                {
                                    ErrorThis("ST_ExportSummary Timer Exception LanguageResourceHelper exception: " + ex.Message);
                                }
                                _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.NoData;
                                _parent.PostCsvDone(OrthoId, _exportResult, "", details);
                                DoTransition(ST_Reset);
                                return null;
                            }

                            DateTime now = DateTime.Now;
                            _summaryFile = "";
                            if (_currentRequest.ExportSummary)
                            {
                                if (!FileSystem.EnsureDirectoryExists(_currentRequest?.SummaryOutPath))
                                {
                                    ErrorThis("ST_ExportSummary EnsureDirectoryExists _currentRequest.SummaryOutPath failed " + _currentRequest.SummaryOutPath);
                                }

                                if (!string.IsNullOrEmpty(_currentRequest?.ZipFileBase))
                                {
                                    _detailsOutDir = _currentRequest.SummaryOutPath + Path.DirectorySeparatorChar + _currentRequest.ZipFileBase;
                                    _zipFile = _currentRequest.SummaryOutPath + Path.DirectorySeparatorChar + _currentRequest.ZipFileBase + ".zip";
                                    _summaryFile = _detailsOutDir + Path.DirectorySeparatorChar + _currentRequest.SummaryCsvFileBase + ".csv";
                                }
                                else
                                {
                                    _summaryFile = _currentRequest.SummaryOutPath + Path.DirectorySeparatorChar + _currentRequest.SummaryCsvFileBase + ".csv";
                                }

                                TraceThis("ST_ExportSummary Timer - Export summary to file: " + _summaryFile);
                                try
                                {
                                    ExportModel.ExportSamplesToCsv(_exportRecords, _currentRequest.Samples, _summaryFile, _currentRequest.AppendSummary);
                                }
                                catch (Exception e)
                                {
                                    ErrorThis("ST_ExportSummary Timer ExportSamplesToCsv Trans => ST_Reset  Exception: " + e.Message);
                                    ExportModel.ExportFailedMessage();
                                    string details = "";
                                    try
                                    {
                                        details = LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                                        details += LanguageResourceHelper.Get("LID_MSGBOX_StorageFault");
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorThis("ST_ExportSummary Timer Exception LanguageResourceHelper exception: " + ex.Message);
                                    }
                                    _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.Error;
                                    _parent.PostCsvDone(OrthoId, _exportResult, "", details);
                                    DoTransition(ST_Reset);
                                    return null;
                                }
                            }

                            if (!_currentRequest.ExportDetails)
                            {
                                ScoutModels.Common.ExportModel.ExportSuccessMessage();
                                _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.Success;
                                _parent.PostCsvDone(OrthoId, _exportResult, _summaryFile, "");
                                DoTransition(ST_Reset);
                                return null;
                            }

                            if (!string.IsNullOrEmpty(_currentRequest?.ZipFileBase))
                            {
                                _detailsOutDir = _currentRequest.SummaryOutPath + Path.DirectorySeparatorChar + _currentRequest.ZipFileBase;
                                _zipFile = _currentRequest.SummaryOutPath + Path.DirectorySeparatorChar + _currentRequest.ZipFileBase + ".zip";
                            }
                            else
                            {
                                _zipFile = "";
                                _detailsOutDir = _currentRequest.DetailsOutPath + Path.DirectorySeparatorChar;
                            }

                            if (!string.IsNullOrEmpty(_zipFile))
                            {
                                try
                                {
                                    _zipArchive = ZipFile.Open(_zipFile, ZipArchiveMode.Create);
                                }
                                catch (Exception e)
                                {
                                    ErrorThis("ST_ExportSummary Timer ZipFile.Open Exception " + e.Message);
                                }
                                if (_zipArchive != null)
                                {
                                    try
                                    {
                                        _zipArchive.CreateEntry("/");
                                        _zipArchive.CreateEntryFromFile(_summaryFile, Path.GetFileName(_summaryFile), CompressionLevel.Fastest);
                                    }
                                    catch (Exception e)
                                    {
                                        ErrorThis("ST_ExportSummary Timer ZipFile.CreateEntry Exception " + e.Message);
                                    }
                                }
                            }

                            TraceThis("ST_ExportSummary Timer - Export summary done Trans => ST_ExportDetails");
                            _sampleIndex = 0;
                            DoTransition(ST_ExportDetails);
                            return null;
                        }
                        catch (Exception e)
                        {
                            ErrorThis("ST_ExportSummary Timer - Error Trans => ST_Reset - exception: " + e.Message);
                            DoTransition(ST_Reset);
                            return null;
                        }
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Init:
                case BEvent.EvType.Public:
                case BEvent.EvType.Private:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Exporting;
        }

        private List<string> _detailFiles = new List<string>();
        private int _sampleIndex = 0;
        // ******************************************************************
        //                    ST_ExportDetails
        // ******************************************************************
        private State ST_ExportDetails(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_ExportDetails Entry, sample " + (_sampleIndex + 1).ToString() + " of " + _currentRequest.Samples.Count.ToString());

                    if (_currentRequest.StatusCB != null)
                    {
                        try
                        {
                            double pcnt = 100;
                            if (_currentRequest.Samples.Count > 0)
                            {
                                pcnt = (double)_sampleIndex / (double)_currentRequest.Samples.Count;
                                pcnt *= 100;
                                pcnt = Math.Min(95, pcnt);
                            }
                            
                            TraceThis("ST_ExportDetails: StatusCB, " + (_sampleIndex + 1).ToString() + " of " + _currentRequest.Samples.Count.ToString());

                            // "_currentRequest.Password" is really the bulk data ID.
                            _currentRequest.StatusCB("", _currentRequest.Password, (uint)GrpcService.ExportStatusEnum.EsCollecting, (uint)pcnt);
                        }
                        catch (Exception e)
                        {
                            ErrorThis("ST_ExportDetails Entry Callback exception: " + e.Message);
                        }
                    }
                    _tmrSequence.FireIn(ExportDelayMs, OrthoId);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_ExportDetails Exit");
                    _tmrSequence.Disarm();
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    if (ev.Id == (uint)TimerIds.Sequence)
                    {
                        try
                        {
                            TraceThis("ST_ExportDetails Timer Sequence - Export details");
                            if (_sampleIndex >= _currentRequest.Samples.Count)
                            {
                                ErrorThis("ST_ExportDetails Timer bad index: " + _sampleIndex);
                                ScoutModels.Common.ExportModel.ExportFailedMessage();
                                string details = "";
                                try
                                {
                                    details = LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                                    details += LanguageResourceHelper.Get("LID_MSGBOX_StorageFault") + Environment.NewLine;
                                    details += LanguageResourceHelper.Get("LID_API_SystemErrorCode_Failure_Timeout") + Environment.NewLine;
                                }
                                catch (Exception ex)
                                {
                                    ErrorThis("ST_ExportDetails Timer Exception LanguageResourceHelper exception: " + ex.Message);
                                }
                                _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.Error;
                                _parent.PostCsvDone(OrthoId, _exportResult, "", details);
                                DoTransition(ST_Reset);
                            }

                            var sample = _currentRequest.Samples[_sampleIndex];
                            var cleanName = sample.SampleIdentifier;
                            var dateStr = Misc.ConvertToFileNameFormat(sample.SelectedResultSummary.RetrieveDate);
                            var filename = $"{cleanName}_{dateStr}";
                            string sampleFile = _detailsOutDir + Path.DirectorySeparatorChar + filename + ".csv";
                            var exportSample = _exportRecords[_sampleIndex];
                            _detailFiles.Add(sampleFile);

                            try
                            {
                                ScoutModels.Common.ExportModel.ExportSelectedSampleDetailsToCsv(exportSample, sample, sampleFile);
                            } catch (Exception e)
                            {
                                ErrorThis("ST_ExportDetails Timer ExportSelectedSampleDetailsToCsv Exception: " + e.Message);
                                ScoutModels.Common.ExportModel.ExportFailedMessage();
                                string details = "";
                                try
                                {
                                    details += LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                                    details += LanguageResourceHelper.Get("LID_MSGBOX_StorageFault") + Environment.NewLine;
                                    details += e.Message;
                                }
                                catch (Exception ex)
                                {
                                    ErrorThis("ST_ExportDetails Exception LanguageResourceHelper exception: " + ex.Message);
                                }
                                _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.Error;
                                _parent.PostCsvDone(OrthoId, _exportResult, "", details);
                                DoTransition(ST_Reset);

                            }

                            _sampleFiles.Add(sampleFile);
                            if (_zipArchive != null)
                            {
                                try
                                {
                                    _zipArchive.CreateEntryFromFile(sampleFile, Path.GetFileName(sampleFile), CompressionLevel.Fastest);
                                }
                                catch (Exception e)
                                {
                                    ErrorThis("ST_ExportDetails Timer - _zipArchive.CreateEntryFromFile " + e.Message);
                                }
                                if (File.Exists(sampleFile))
                                {
                                    try { File.Delete(sampleFile); }
                                    catch (Exception e) { ErrorThis("ST_ExportDetails Timer - Delete file exception " + e.Message); }
                                }
                            }

                            if (!_currentRequest.ExportImages)
                            {
                                _sampleIndex++;
                                if (_sampleIndex >= _currentRequest.Samples.Count)
                                {
                                    // Done
                                    TraceThis("ST_ExportDetails Timer Sequence - Not exporting images - Done");
                                    ScoutModels.Common.ExportModel.ExportSuccessMessage();
                                    _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.Success;
                                    _parent.PostCsvDone(OrthoId, _exportResult, "", "");
                                    DoTransition(ST_Reset);
                                }
                                TraceThis("ST_ExportDetails Timer Sequence - No images - Export next sample");
                                _tmrSequence.FireIn(ExportDelayMs, OrthoId);
                                return null;
                            }

                            TraceThis("ST_ExportDetails Timer Sequence - Export images for current sample");
                            DoTransition(ST_ExportImages);
                            return null;
                        }
                        catch (Exception e)
                        {
                            ErrorThis("ST_ExportDetails Timer Sequence - Trans => ST_Reset  Exception: " + e.Message);
                            string details = "";
                            try
                            {
                                details += LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                                details += LanguageResourceHelper.Get("LID_MSGBOX_StorageFault") + Environment.NewLine;
                                details += e.Message;
                            }
                            catch (Exception ex)
                            {
                                ErrorThis("ST_ExportDetails Exception LanguageResourceHelper exception: " + ex.Message);
                            }
                            _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.Error;
                            _parent.PostCsvDone(OrthoId, _exportResult, "", details);

                        }
                        DoTransition(ST_Reset);
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Init:
                case BEvent.EvType.Public:
                case BEvent.EvType.Private:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Exporting;
        }

        private int _imageIndex = 0;
        private ResultRecordHelper _resultHelper = null;
        private string _sampleName = "";

        internal UInt32 ExportDelayMs { get; set; } = BAppFW.kTIMER_PERIOD_MS;
        private DateTime _imgStart = DateTime.Now;
        // ******************************************************************
        //                    ST_ExportImages
        // ******************************************************************
        private State ST_ExportImages(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_ExportImages Entry, ExportDelayMs: " + ExportDelayMs.ToString());
                    try
                    {
                        // Get the list of images, records and IDs
                        _resultHelper.SetImageList(_currentRequest.Samples[_sampleIndex]);
                        _resultHelper.SetSampleImageSetRecord(_currentRequest.Samples[_sampleIndex].SampleImageList.ToList(), _currentRequest.Samples[_sampleIndex]);
                        _imageIndex = 0;

                        _sampleName = Path.GetFileNameWithoutExtension(_detailFiles.Last());
                        if (_zipArchive != null)
                        {
                            try
                            {
                                _zipArchive.CreateEntry(_sampleName + "/");
                            }
                            catch (Exception e)
                            {
                                ErrorThis("ST_ExportImages Entry _zipArchive.CreateEntry Exception: " + e.Message);
                            }
                        }
                        _imgStart = DateTime.Now;
                    }
                    catch (Exception e)
                    {
                        ErrorThis("ST_ExportImages Entry Exception: " + e.Message);
                    }
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    var tse = (DateTime.Now - _imgStart);
                    TraceThis("ST_ExportImages Exit : count " + _imageIndex.ToString() + " secs " + tse.TotalSeconds.ToString("N0"));               
                    try 
                    { 
                        Directory.Delete(_detailsOutDir + Path.DirectorySeparatorChar + _sampleName); 
                    } 
                    catch (Exception e)
                    {
                        ErrorThis("ST_ExportImages Exit - Delete dir exception " + e.Message);
                    }
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Init:
                {
                    TraceThis("ST_ExportImages Init => ST_SaveImage");
                    SetState(ST_SaveImage);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                case BEvent.EvType.Public:
                case BEvent.EvType.Private:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Exporting;
        }


        private string _imgFilename = "";
        // ******************************************************************
        //                    ST_SaveImage
        // ******************************************************************
        private State ST_SaveImage(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    _imgFilename = "";
                    _tmrSequence.FireIn(ExportDelayMs, OrthoId);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    _tmrSequence.Disarm();
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    if (ev.Id == (uint)TimerIds.Sequence)
                    {
                        try
                        {
                            string outDir = Path.GetDirectoryName(_detailFiles.Last());
                            outDir += Path.DirectorySeparatorChar + _sampleName + Path.DirectorySeparatorChar;
                            var sample = _currentRequest.Samples[_sampleIndex];
                            var imgRec = sample.SampleImageList[_imageIndex];
                            _imgFilename = outDir + "bfimage_" + (_imageIndex + 1).ToString("D3") + ApplicationConstants.ImageFileExtension;
                            SaveImage(imgRec.BrightFieldId, _imgFilename);
                            _imageFiles.Add(_imgFilename);
                            
                            DoTransition(ST_ZipImage);
                            return null;
                        }
                        catch (Exception e)
                        {
                            ErrorThis("ST_SaveImage Timer Sequence - Exception: " + e.Message);
                            string details = "";
                            try
                            {
                                details += LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                                details += LanguageResourceHelper.Get("LID_MSGBOX_StorageFault") + Environment.NewLine;
                                details += e.Message;
                            }
                            catch (Exception ex)
                            {
                                ErrorThis("ST_ExportDetails Exception LanguageResourceHelper exception: " + ex.Message);
                            }
                            _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.Error;
                            _parent.PostCsvDone(OrthoId, _exportResult, "", details);
                        }
                        DoTransition(ST_Reset);
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Init:
                case BEvent.EvType.Public:
                case BEvent.EvType.Private:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_ExportImages;
        }

        // ******************************************************************
        //                    ST_ZipImage
        // ******************************************************************
        private State ST_ZipImage(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    _tmrSequence.FireIn(ExportDelayMs, OrthoId);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    _tmrSequence.Disarm();
                    _imgFilename = "";
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    if (ev.Id == (uint)TimerIds.Sequence)
                    {
                        try
                        {
                            var maxImageIndex = _currentRequest.Samples[_sampleIndex].SampleImageList.Count() - 1;

                            if (_zipArchive != null)
                            {
                                if (File.Exists(_imgFilename))
                                {
                                    string entryname = _sampleName + "/" + Path.GetFileName(_imgFilename);
                                    try
                                    {
                                        _zipArchive.CreateEntryFromFile(_imgFilename, entryname, CompressionLevel.Fastest);
                                    }
                                    catch (Exception e)
                                    {
                                        DebugThis("ST_ZipImage Timer - Zip file exception " + e.Message);
                                    }
                                    try { File.Delete(_imgFilename); }
                                    catch (Exception e) { DebugThis("ST_ZipImage Timer - Delete file exception " + e.Message); }
                                }
                            }
                            _imageIndex++;
                            if (_currentRequest.SaveEveryNthImage > 1)
                            {
                                if (_imageIndex < maxImageIndex)
                                {
                                    _imageIndex += (int)(_currentRequest.SaveEveryNthImage - 1);
                                    if (_imageIndex > maxImageIndex)
                                    {
                                        _imageIndex = maxImageIndex;
                                    }
                                }
                            }
                            
                            if (_imageIndex > maxImageIndex)
                            {
                                TraceThis("ST_ZipImage Timer Sequence - Done with images for the current sample");
                                _sampleIndex++;
                                if (_sampleIndex >= _currentRequest.Samples.Count)
                                {
                                    TraceThis("ST_ZipImage Timer Sequence - Done with all samples Trans => ST_Reset");
                                    ExportModel.ExportSuccessMessage();
                                    string details = "";
                                    try
                                    {
                                        details += LanguageResourceHelper.Get("LID_CheckBox_TotalCount") + " : " + _sampleFiles.Count() + Environment.NewLine;
                                        details += LanguageResourceHelper.Get("LID_Label_Images") + " : " + _imageFiles.Count() + Environment.NewLine;
                                    } catch(Exception e)
                                    {
                                        ErrorThis("ST_ZipImage LanguageResourceHelper exception: " + e.Message);
                                    }
                                    _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.Success;
                                    _parent.PostCsvDone(OrthoId, _exportResult, _zipFile, details);
                                    DoTransition(ST_Reset);
                                    return null;
                                }
                                TraceThis("ST_ZipImage Timer Sequence - Export next sample Trans => ST_ExportDetails");
                                DoTransition(ST_ExportDetails);
                                return null;
                            }
                            // Save the next image
                            DoTransition(ST_SaveImage);
                            return null;
                        }
                        catch (Exception e)
                        {
                            ErrorThis("ST_ZipImage Timer Sequence - Exception: " + e.Message);
                            string details = "";
                            try
                            {
                                details += LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                                details += LanguageResourceHelper.Get("LID_MSGBOX_StorageFault") + Environment.NewLine;
                                details += e.Message;
                            }
                            catch (Exception ex)
                            {
                                ErrorThis("ST_ZipImage Exception LanguageResourceHelper exception: " + ex.Message);
                            }
                            _exportResult = AoExportMgr.ExportDoneEv.ExpStatus.Error;
                            _parent.PostCsvDone(OrthoId, _exportResult, "", details);
                        }
                        DoTransition(ST_Reset);
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Init:
                case BEvent.EvType.Public:
                case BEvent.EvType.Private:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_ExportImages;
        }
        #endregion

        #region Private_Methods
        // ******************************************************************
        void SaveImage(uuidDLL imgId, string filename)
        {
            try
            {
                IntPtr imgPtr;
                var res = HawkeyeCoreAPI.Sample.RetrieveImageAPI(imgId, out imgPtr);
                if (res == HawkeyeError.eSuccess)
                {
                    ImageDto imageDataDto = imgPtr.MarshalToImageDto();
                    ApiProxies.Extensions.ImageDtoExts.SaveImage(imageDataDto, filename);
                    HawkeyeCoreAPI.Sample.FreeImageWrapperAPI(imgPtr);
                }
                else
                {
                    ErrorThis("   SaveImage: " + filename + " failed");
                }
            }
            catch (Exception e)
            {
                ErrorThis("SaveImage exception " + e.Message);
            }
        }
        #endregion

    }
}
