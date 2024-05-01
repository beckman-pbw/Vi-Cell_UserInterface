using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

using System.Runtime.InteropServices;
using ScoutUtilities.Common;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using ScoutLanguageResources;

using BAFW;

namespace ExportManager
{
    // ***********************************************************************
    public class OrExportOffline : BOrthoRegion
    {

        internal const UInt32 kORTHO_ID_MASK = 0x00000100;

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
        private AoExportMgr _parent;
        private LdDataRecords _ldDataRecords = null;
        #endregion

        #region Construct_Destruct
        // ***********************************************************************
        public OrExportOffline(AoExportMgr parent, UInt32 orthoId, LdDataRecords ldDataRecords)          
            :base(parent, orthoId)
        {
            _parent = parent;
            SetRootState(ST_Root);
            _tmrTimeout = new BTimer(parent.GetEventQueue(), (UInt32)TimerIds.Timeout);
            _tmrQuery = new BTimer(parent.GetEventQueue(), (UInt32)TimerIds.Query);
            _ldDataRecords = ldDataRecords;
            InitStateMachine();
        }
        #endregion

        #region Trace_Debug
        public static byte LogSubSysId = 0;
        private const string kMODULE_NAME = "OrExportOffline:";
        // ***********************************************************************
        private void TraceThis(string strData)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Trace, strData, LogSubSysId);
        }
        private void DebugThis(string strData)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Debug, strData, LogSubSysId);
        }
        private void WarnThis(string strData)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Warning, strData, LogSubSysId);
        }
        private void ErrorThis(string strData)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Error, strData, LogSubSysId);
        }
        #endregion

        #region Timers
        private const int kTIMEOUT_SECS = 57;

        private const int kSAVE_REFRESH_MULT = 25;
        private const int kSAVE_REFRESH_MS = 1500; // kSAVE_REFRESH_MS * kSAVE_REFRESH_MULT => max time to save one sample

        private const int kMAX_PCNT_COLLECT = 80;
        private const int kMIN_PCNT_SAVE = kMAX_PCNT_COLLECT + 1;
        private const int kMAX_PCNT_SAVE = 98;
        private const int kPCNT_CLEAN = 99;
        private BTimer _tmrTimeout;
        private BTimer _tmrQuery;
        // ***********************************************************************
        private enum TimerIds : UInt32
        {
            Timeout = kORTHO_ID_MASK,
            Query
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
                    TraceThis("ST_Root: Entry");
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_Root: Exit");
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Init:
                {
                    TraceThis("ST_Root: Init, Trans => ST_Reset");
                    SetState(ST_Reset);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Private:
                {
                    if(ev.Id == (uint)PrivateEvIds.iReset)
                    {
                        DebugThis("ST_Root: Private, iReset Trans => ST_Reset");
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

        internal EvExportOfflineReq _currentRequest = null;
        private UInt32 _currentIndex = 0;
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
                    TraceThis("ST_Reset: Entry");
                    InReset = true;
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_Reset: Exit");
                    InReset = false;
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Public:
                {
                    switch (ev.Id)
                    {
                        case (uint)PubEvIds.ExportMgr_StartOffline:
                        {
                            _currentRequest = (EvExportOfflineReq)ev;
                            TraceThis("ST_Reset: Public, ExportMgr_StartOffline Trans => ST_Exporting");
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
                        TraceThis("ST_Reset: Private, iReset do nothing");
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


        private AoExportMgr.ExportDoneEv.ExpStatus _exportStatus = AoExportMgr.ExportDoneEv.ExpStatus.Unknown;
        private bool _canceled = false;
        private string _failDetails = "";
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
                    TraceThis("ST_Exporting: Entry");
                    _currentIndex = 0;
                    _exportStatus = AoExportMgr.ExportDoneEv.ExpStatus.Unknown;
                    _canceled = false;
                    _failDetails = "";
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_Exporting: Exit");

                    try
                    {
                        // Tell our parent we are done and the result (pass / fail)
                        if (_exportStatus == AoExportMgr.ExportDoneEv.ExpStatus.Success)
                        {
                            string details = _outname;
                            try
                            {
                                details += LanguageResourceHelper.Get("LID_CheckBox_TotalCount") + " : " + _currentRequest.SampleIds.Count.ToString() + Environment.NewLine;
                            }
                            catch { }
                            _parent.PostOfflineDone(OrthoId, _exportStatus, _currentRequest.Zipfile, "");
                        }
                        else
                        {
                            string outfile = "";
                            if (!string.IsNullOrEmpty(_currentRequest?.Zipfile))
                            {
                                outfile = _currentRequest.Zipfile;
                            }
                            _parent.PostOfflineDone(OrthoId, _exportStatus, outfile, _failDetails);
                        }

                        if (_exportStatus != AoExportMgr.ExportDoneEv.ExpStatus.Success)
                        {
                            // Export failed, remove all files
                            TraceThis("ST_Exporting: Exit, calling RequestExportCleanup");
                            _ldDataRecords.RequestExportCleanup(true);
                        }

                        if (_currentRequest?.CompleteCB != null)
                        {
                            // Done with the export, send success to remove the dialog box
                            _currentRequest.CompleteCB(HawkeyeError.eSuccess, "");
                            _currentRequest.CompleteCB = null;
                        }
                    }
                    catch(Exception e)
                    {
                        ErrorThis("ST_Exporting: Exit Exception " + e.Message);
                    }
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Init:
                {
                    TraceThis("ST_Exporting: Init => ST_ValidateConfig");
                    SetState(ST_ValidateConfig);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Public:
                {
                    switch (ev.Id)
                    {
                        case (uint)PubEvIds.ExportMgr_CancelOffline:
                        {
                            DebugThis("ST_Exporting: Public, ExportMgr_CancelOffline set cancel flag to true");
                            _canceled = true;
                            return null;
                        }
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Private:
                case BEvent.EvType.Timer:
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
                    TraceThis("ST_ValidateConfig: Entry");
                    _tmrQuery.FireIn(25, OrthoId);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_ValidateConfig: Exit");
                    _tmrQuery.Disarm();
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    bool isValidFile = true;
                    if (!string.IsNullOrEmpty(_currentRequest?.Zipfile) && !FileSystem.IsFileNameValid(_currentRequest?.Zipfile))
                        isValidFile = false;

	                bool isValidDir = true;
                    if (!FileSystem.IsFolderValidForExport(_currentRequest?.OutPath))
                        isValidDir = false;
                    else if (!FileSystem.EnsureDirectoryExists(_currentRequest?.OutPath))
                        isValidDir = false;

                    if (!isValidDir || !isValidFile)
                    {
                        TraceThis("ST_ValidateConfig Timer: directory " + _currentRequest?.OutPath + " not valid for export Trans => ST_Reset");
                        ScoutModels.Common.ExportModel.ExportFailedMessage();
                        _failDetails = "";
                        try
                        {
                            _failDetails += LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                            if (!isValidDir)
                            {
                                _failDetails += Environment.NewLine + LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_PathError") + Environment.NewLine;
                                _failDetails += System.IO.Path.GetDirectoryName(_currentRequest.OutPath) + Environment.NewLine;
                            }
                            if (!isValidFile)
                            {
                                _failDetails += Environment.NewLine + LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_PathError") + Environment.NewLine;
                                _failDetails += _currentRequest.Zipfile + Environment.NewLine;
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorThis("ST_ValidateConfig: LanguageResourceHelper exception: " + ex.Message);
                        }
                        _exportStatus = AoExportMgr.ExportDoneEv.ExpStatus.Error;
                        DoTransition(ST_Reset);
                        return null;
                    }

                    TraceThis("ST_ValidateConfig: Timer, config IS valid path = " + _currentRequest?.OutPath + " => Trans => ST_WaitForStartOk");
                    DoTransition(ST_WaitForStartOk);
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

        // ******************************************************************
        //                    ST_WaitForStartOk
        // ******************************************************************
        private State ST_WaitForStartOk(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_WaitForStartOk: Entry");
                    _tmrTimeout.FireInSecs(kTIMEOUT_SECS, OrthoId);

                    _ldDataRecords.RequestStartExport(
                        _currentRequest.Username,
                        _currentRequest.Password,
                        _currentRequest.SampleIds,
                        _currentRequest.OutPath,
                        _currentRequest.ExportImages,
                        _currentRequest.ExportNthImage);

                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_WaitForStartOk: Exit");
                    _tmrTimeout.Disarm();
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Private:
                {
                    try
                    {
                        switch (ev.Id)
                        {
                            case (uint)AoExportMgr.PrivateEvIds.pExportOffline_Status:
                            {
                                if (_canceled)
                                {
                                    DebugThis("ST_WaitForStartOk: Private, pExportOffline_Status canceled ^iReset");
                                    Owner.PostInternalEvent((uint)PrivateEvIds.iReset);
                                    return null;
                                }

                                AoExportMgr.OfflineExportStatusIndEv stev = (AoExportMgr.OfflineExportStatusIndEv)ev;
                                if (stev.Status == AoExportMgr.OfflineExportStatusIndEv.ReqStatus.StartOk)
                                {
                                    TraceThis("ST_WaitForStartOk: Private, pExportOffline_Status StartOk Trans => ST_CollectMetadata");
                                    DoTransition(ST_CollectMetadata);
                                    return null;
                                }

                                try
                                {
                                    _failDetails = LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                                    _failDetails += LanguageResourceHelper.Get("LID_API_SystemErrorCode_Failure_Readerror");
                                }
                                catch { }

                                if (_currentRequest.ProgressCB != null)
                                {
                                    _currentRequest.ProgressCB(HawkeyeError.eNotPermittedAtThisTime, new uuidDLL(), 100);
                                }
                                ErrorThis("ST_WaitForStartOk Private pExportOffline_Status: " + stev.Status.ToString() + " ^iReset");
                                Owner.PostInternalEvent((uint)PrivateEvIds.iReset);
                                return null;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ErrorThis("ST_CollectMetadata: Private Exception " + e.Message);
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    if (ev.Id == (uint)TimerIds.Timeout)
                    {
                        ErrorThis("ST_WaitForStartOk: Timer, Timeout ^iReset");
                        Owner.PostInternalEvent((uint)PrivateEvIds.iReset);
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Public:
                case BEvent.EvType.Init:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Exporting;
        }


        internal UInt32 ExportDelayMs { get; set; } = 1;
        private int _currentPercent = 0;
        // ******************************************************************
        //                    ST_CollectMetadata
        // ******************************************************************
        private State ST_CollectMetadata(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_CollectMetadata: Entry, ExportDelayMs: " + ExportDelayMs.ToString());
                    _ldDataRecords.RequestExportNextMetaData(_currentIndex, ExportDelayMs);
                    _tmrTimeout.FireInSecs(kTIMEOUT_SECS, OrthoId);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_CollectMetadata: Exit");
                    _tmrTimeout.Disarm();
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Private:
                {
                    try
                    {
                        switch (ev.Id)
                        {
                            case (uint)AoExportMgr.PrivateEvIds.pExportOffline_Status:
                            {
                                if (_canceled)
                                {
                                    DebugThis("ST_CollectMetadata: Private, pExportOffline_Status canceled ^iReset");
                                    Owner.PostInternalEvent((uint)PrivateEvIds.iReset);
                                    return null;
                                }

                                AoExportMgr.OfflineExportStatusIndEv stev = (AoExportMgr.OfflineExportStatusIndEv)ev;
                                _tmrTimeout.Disarm();
                                
                                if (stev.Status == AoExportMgr.OfflineExportStatusIndEv.ReqStatus.CollectMetaOk)
                                {
                                    double pcntCollect = ((100.0 * (double)(_currentIndex + 1)) / (double)_currentRequest.SampleIds.Count);
                                    _currentPercent = (int)Math.Min(pcntCollect, kMAX_PCNT_COLLECT);

                                    TraceThis($"ST_CollectMetadata: Private, pExportOffline_Status ExportMetaOk index: {_currentIndex}, ExportDelayMs: {ExportDelayMs}, pcnt {_currentPercent}");

									if (_currentRequest.ProgressCB != null)
                                    {
                                        _currentRequest.ProgressCB(HawkeyeError.eSuccess, new uuidDLL(), _currentPercent);
                                    }

                                    _currentIndex++;
                                    if (_currentIndex < _currentRequest.SampleIds.Count)
                                    {
                                        _ldDataRecords.RequestExportNextMetaData(_currentIndex, ExportDelayMs);
                                        _tmrTimeout.Disarm();
                                        _tmrTimeout.FireInSecs(kTIMEOUT_SECS, OrthoId);
                                        return null;
                                    }
                                    TraceThis("ST_CollectMetadata: Private, pExportOffline_Status Done Trans => ST_VerifyResources");
                                    DoTransition(ST_VerifyResources);
                                    return null;
                                }

                                ErrorThis("ST_CollectMetadata: Private, pExportOffline_Status: " + stev.Status.ToString() + " ^iReset");
                                try
                                {
                                    _failDetails = LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                                    _failDetails += LanguageResourceHelper.Get("LID_API_SystemErrorCode_Failure_Readerror");
                                }
                                catch { }

                                if (_currentRequest.ProgressCB != null)
                                {
                                    _currentRequest.ProgressCB(HawkeyeError.eDatabaseError, _currentRequest.SampleIds[(int)_currentIndex], 100);

                                }
                                Owner.PostInternalEvent((uint)PrivateEvIds.iReset);
                                return null;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ErrorThis("ST_CollectMetadata: Private Exception " + e.Message);
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    if (ev.Id == (uint)TimerIds.Timeout)
                    {
                        ErrorThis("ST_CollectMetadata: Timer, Timeout ^iReset");
                        Owner.PostInternalEvent((uint)PrivateEvIds.iReset);
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Public:
                case BEvent.EvType.Init:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Exporting;
        }

        // ******************************************************************
        //                    ST_VerifyResources
        // ******************************************************************
        private State ST_VerifyResources(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_VerifyResources: Entry");
                    _ldDataRecords.RequestExportIsStorageAvailable();
                    _tmrTimeout.FireInSecs(kTIMEOUT_SECS, OrthoId);
                    _currentPercent = kMIN_PCNT_SAVE;
                    if (_currentRequest.ProgressCB != null)
                    {
                        try
                        {
                            _currentRequest.ProgressCB(HawkeyeError.eSuccess, new uuidDLL(), _currentPercent);
                        }
                        catch { }
                    }
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_VerifyResources: Exit");
                    _tmrTimeout.Disarm();
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Private:
                {
                    try
                    {
                        switch (ev.Id)
                        {
                            case (uint)AoExportMgr.PrivateEvIds.pExportOffline_Status:
                            {
                                if (_canceled)
                                {
                                    TraceThis("ST_VerifyResources: Private, pExportOffline_Status canceled ^iReset");
                                    Owner.PostInternalEvent((uint)PrivateEvIds.iReset);
                                    return null;
                                }
                                AoExportMgr.OfflineExportStatusIndEv stev = (AoExportMgr.OfflineExportStatusIndEv)ev;
                                if (stev.Status == AoExportMgr.OfflineExportStatusIndEv.ReqStatus.VerifySpaceOk)
                                {
                                    TraceThis("ST_VerifyResources: Private, pExportOffline_Status VerifySpaceOk Trans => ST_SaveOutput");
                                    DoTransition(ST_SaveOutput);
                                    return null;
                                }
                                TraceThis("ST_VerifyResources: Private, pExportOffline_Status: " + stev.Status.ToString() + " ^iReset");
                                try
                                {
                                    _failDetails = LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                                    _failDetails += LanguageResourceHelper.Get("LID_API_SystemErrorCode_Failure_Storagenearcapacity");
                                }
                                catch { }
                                if (_currentRequest.ProgressCB != null)
                                {
                                    try
                                    {
                                        _currentRequest.ProgressCB(HawkeyeError.eLowDiskSpace, new uuidDLL(), 100);
                                    }
                                    catch (Exception e)
                                    {
                                        ErrorThis("ST_VerifyResources: Private, ProgressCB Exception " + e.Message);
                                    }
                                }
                                Owner.PostInternalEvent((uint)PrivateEvIds.iReset);
                                return null;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ErrorThis("ST_VerifyResources: Private, Exception " + e.Message);
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    if (ev.Id == (uint)TimerIds.Timeout)
                    {
                        ErrorThis("ST_VerifyResources: Timer, Timeout ^iReset");
                        Owner.PostInternalEvent((uint)PrivateEvIds.iReset);
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Public:
                case BEvent.EvType.Init:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Exporting;
        }

        private string _outname = "";
        private int _maxSaveRefreshCount = 0;
        private UInt32 _saveRefreshIndex = 0;
        // ******************************************************************
        //                    ST_SaveOutput
        // ******************************************************************
        private State ST_SaveOutput(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_SaveOutput: Entry");
                    if (_currentRequest != null)
                    {
                        _ldDataRecords.RequestExportSaveOutput(_currentRequest.Zipfile);
                    }
                    _saveRefreshIndex = 0;
                    _maxSaveRefreshCount = _currentRequest.SampleIds.Count * kSAVE_REFRESH_MULT;
                    _tmrTimeout.FireInSecs(kTIMEOUT_SECS, OrthoId);
                    _tmrQuery.FireIn(kSAVE_REFRESH_MS, OrthoId);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_SaveOutput Exit");
                    _tmrTimeout.Disarm();
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Private:
                {
                    try
                    {
                        switch (ev.Id)
                        {
                            case (uint)AoExportMgr.PrivateEvIds.pExportOffline_Status:
                            {
                                if (_canceled)
                                {
                                    TraceThis("ST_SaveOutput: Private, pExportOffline_Status canceled ^iReset");
                                    Owner.PostInternalEvent((uint)PrivateEvIds.iReset);
                                    return null;
                                }
                                AoExportMgr.OfflineExportStatusIndEv stev = (AoExportMgr.OfflineExportStatusIndEv)ev;
                                if (stev.Status == AoExportMgr.OfflineExportStatusIndEv.ReqStatus.SaveOk)
                                {
                                    _outname = stev.Outname;
                                    TraceThis("ST_SaveOutput: Private, pExportOffline_Status SaveOk Trans => ST_Cleanup");

                                    if (_currentRequest.IsAutomationExport)
                                    {
                                        // Notify the Automation client that the data is ready for upload.
	                                    _currentRequest.StatusCB(_outname, _currentRequest.Password, (uint)GrpcService.ExportStatusEnum.EsReady, 100);
                                    }

									DoTransition(ST_Cleanup);
                                    return null;
                                }
                                TraceThis("ST_SaveOutput: Private, pExportOffline_Status: " + stev.Status.ToString() + " ^iReset");
                                try
                                {
                                    _failDetails = LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                                    _failDetails += LanguageResourceHelper.Get("LID_MSGBOX_StorageFault");
                                }
                                catch { }
                                if (_currentRequest.ProgressCB != null)
                                {
                                    _currentRequest.ProgressCB(HawkeyeError.eStorageFault, new uuidDLL(), 100);
                                    try
                                    {
                                        _currentRequest.ProgressCB(HawkeyeError.eStorageFault, new uuidDLL(), 100);
                                    }
                                    catch (Exception e)
                                    {
                                        ErrorThis("ST_SaveOutput: Private, ProgressCB Exception " + e.Message);
                                    }
                                }
                                Owner.PostInternalEvent((uint)PrivateEvIds.iReset);
                                return null;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ErrorThis("ST_SaveOutput Private Exception " + e.Message);
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    try
                    {
                        if (ev.Id == (uint)TimerIds.Timeout)
                        {
                            ErrorThis("ST_SaveOutput: Timer, Timeout ^iReset");
                            if (_currentRequest.ProgressCB != null)
                            {
                                _currentRequest.ProgressCB(HawkeyeError.eTimedout, new uuidDLL(), 100);
                            }
                            Owner.PostInternalEvent((uint)PrivateEvIds.iReset);
                            return null;
                        }
                        if (ev.Id == (uint)TimerIds.Query)
                        {
                            if (_saveRefreshIndex < _maxSaveRefreshCount)
                            {
                                _saveRefreshIndex++;
                                if ((_saveRefreshIndex % kSAVE_REFRESH_MULT) == 0)
                                {
                                    TraceThis("ST_SaveOutput: Timer, reset timeout, _saveRefreshIndex: " + _saveRefreshIndex.ToString() + " of " + _maxSaveRefreshCount);
                                }
                                _tmrTimeout.Disarm();
                                _tmrTimeout.FireInSecs(kTIMEOUT_SECS, OrthoId);
                                _tmrQuery.FireIn(kSAVE_REFRESH_MS, OrthoId);

                                double savePcnt = ((double)_saveRefreshIndex / (double)_maxSaveRefreshCount);
                                savePcnt *= ((double)kMAX_PCNT_SAVE - (double)kMIN_PCNT_SAVE);
                                _currentPercent = (kMIN_PCNT_SAVE + (int)Math.Ceiling(savePcnt));
                                _currentPercent = Math.Min(_currentPercent, kMAX_PCNT_SAVE);

                                try
                                {
                                    if (_currentRequest.IsAutomationExport)
                                    {
	                                    if (_currentRequest.StatusCB != null)
	                                    {
		                                    // Notify the Automation client that the data is ready for upload.
		                                    _currentRequest.StatusCB(_outname, _currentRequest.Password, (uint)GrpcService.ExportStatusEnum.EsCollecting, (uint)_currentPercent);
	                                    }
                                    }
                                    else
                                    {
	                                    if (_currentRequest.ProgressCB != null)
	                                    {
		                                    _currentRequest.ProgressCB(HawkeyeError.eSuccess, new uuidDLL(), _currentPercent);
	                                    }
                                    }
                                }
                                catch { }
                            }
                            else
                            {
                                TraceThis("ST_SaveOutput: Timer, done with timeout resets");
                            }
                            return null;
                        }
                    }
                    catch(Exception e)
                    {
                        ErrorThis("ST_SaveOutput: Timer, Trans => ST_Reset exception: " + e.Message);
                        if (_currentRequest.ProgressCB != null)
                        {
                            _currentRequest.ProgressCB(HawkeyeError.eSuccess, new uuidDLL(), 100);
                            _currentRequest.ProgressCB = null;
                        }
                        DoTransition(ST_Reset);
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Public:
                case BEvent.EvType.Init:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Exporting;
        }

        // ******************************************************************
        //                    ST_Cleanup
        // ******************************************************************
        private State ST_Cleanup(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_Cleanup: Entry");
                    _currentPercent = kPCNT_CLEAN;

                    if (_currentRequest.ProgressCB != null)
                    {
                        _currentRequest.ProgressCB(HawkeyeError.eSuccess, new uuidDLL(), _currentPercent);
                    }

                    _ldDataRecords.RequestExportCleanup(false);
                    _tmrTimeout.FireInSecs(kTIMEOUT_SECS, OrthoId);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_Cleanup: Exit");
                    _tmrTimeout.Disarm();
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Private:
                {
                    try
                    {
                        switch (ev.Id)
                        {
                            case (uint)AoExportMgr.PrivateEvIds.pExportOffline_Status:
                            {
                                AoExportMgr.OfflineExportStatusIndEv stev = (AoExportMgr.OfflineExportStatusIndEv)ev;
                                if (stev.Status == AoExportMgr.OfflineExportStatusIndEv.ReqStatus.CleanupOk)
                                {
                                    TraceThis("ST_Cleanup: Private, pExportOffline_Status CleanupOk Trans => ST_Reset");
                                    _exportStatus = AoExportMgr.ExportDoneEv.ExpStatus.Success;
                                    try
                                    {
                                        if (_currentRequest.ProgressCB != null)
                                        {
                                            _currentRequest.ProgressCB(HawkeyeError.eSuccess, new uuidDLL(), 100);
                                            _currentRequest.ProgressCB = null;
                                        }
                                        if (_currentRequest.CompleteCB != null)
                                        {
                                            _currentRequest.CompleteCB(HawkeyeError.eSuccess, _outname);
                                            _currentRequest.CompleteCB = null;
                                        }
                                    }
                                    catch { }
                                    DoTransition(ST_Reset);
                                    return null;
                                }
                                TraceThis("ST_Cleanup: Private, pExportOffline_Status: " + stev.Status.ToString() + " ^iReset");
                                try
                                {
                                    _failDetails = LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                                    _failDetails += LanguageResourceHelper.Get("LID_FrameLabel_Cleanup");
                                    if (_currentRequest.ProgressCB != null)
                                    {
                                        _currentRequest.ProgressCB(HawkeyeError.eStorageFault, new uuidDLL(), 100);
                                        _currentRequest.ProgressCB = null;
                                    }
                                }
                                catch (Exception e)
                                {
                                    ErrorThis("ST_Cleanup: Private, ProgressCB Exception " + e.Message);
                                }
                                Owner.PostInternalEvent((uint)PrivateEvIds.iReset);
                                return null;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ErrorThis("ST_Cleanup: Private, Exception " + e.Message);
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    if (ev.Id == (uint)TimerIds.Timeout)
                    {
                        WarnThis("ST_Cleanup: Timer, Timeout ^iReset");
                        try
                        {
                            if (_currentRequest.ProgressCB != null)
                            {
                                _currentRequest.ProgressCB(HawkeyeError.eTimedout, new uuidDLL(), 100);
                            }
                        }
                        catch { }
                        Owner.PostInternalEvent((uint)PrivateEvIds.iReset);
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Public:
                case BEvent.EvType.Init:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Exporting;
        }
        #endregion
    }
}
