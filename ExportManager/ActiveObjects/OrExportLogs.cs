using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

using System.Runtime.InteropServices;
using ScoutDomains;
using ScoutDomains.Common;
using ScoutModels;
using ScoutModels.Common;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Delegate;
using ScoutUtilities.Structs;
using ScoutLanguageResources;

using BAFW;
using ScoutUtilities.Events;

namespace ExportManager
{

    // ***********************************************************************
    public class OrExportLogs : BOrthoRegion
    {

        internal const UInt32 kORTHO_ID_MASK = 0x00000800;

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
        LdDataRecords _ldDataRecords;
        #endregion

        #region Construct_Destruct
        internal UInt32 ExportDelayMs { get; set; } = BAppFW.kTIMER_PERIOD_MS;
        // ***********************************************************************
        public OrExportLogs(AoExportMgr parent, UInt32 orthoId, LdDataRecords ldDataRecords)
            : base(parent, orthoId)
        {
            _parent = parent;
            _tmrTimeout = new BTimer(parent.GetEventQueue(), (UInt32)TimerIds.Timeout);
            _tmrSequence = new BTimer(parent.GetEventQueue(), (UInt32)TimerIds.Sequence);
            _ldDataRecords = ldDataRecords;

            SetRootState(ST_Root);
            InitStateMachine();
        }
        #endregion

        #region Trace_Debug
        public static byte LogSubSysId = 0;
        private const string kMODULE_NAME = "OrExportLogs";
        // ***********************************************************************
        private void TraceThis(string strData)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Trace, strData, LogSubSysId);
        }
        private void DebugThis(string strData)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Debug, strData, LogSubSysId);
        }
        private void ErrorThis(string strData)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Error, strData, LogSubSysId);
        }
        #endregion

        #region Timers
        private BTimer _tmrTimeout;
        private BTimer _tmrSequence;
        // ***********************************************************************
        private enum TimerIds : UInt32
        {
            Timeout = kORTHO_ID_MASK,
            Sequence
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



        private EvExportLogsReq _currentRequest = null;
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
                        case (uint)PubEvIds.ExportMgr_StartLogs:
                        {
                            _currentRequest = (EvExportLogsReq)ev;
                            TraceThis("ST_Reset Public ExportMgr_StartLogs Trans => ST_Exporting");
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

        private const int kTIMEOUT_SECS = 25;
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
                    string log = "Export logs: ";
                    if (_currentRequest.IncludeAudit)
                        log += " audit ";
                    if (_currentRequest.IncludeError)
                        log += " error ";
                    log += " from : " + _currentRequest.StartTime.ToLongDateString();
                    log += " to : " + _currentRequest.EndTime.ToLongDateString();
                    TraceThis(log);

                    _tmrTimeout.FireInSecs(kTIMEOUT_SECS, OrthoId);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_Exporting Exit");
                    _tmrTimeout.Disarm();
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
                case BEvent.EvType.Timer:
                {
                    if (ev.Id == (uint)TimerIds.Timeout)
                    {
                        TraceThis("ST_Exporting Timer Timeout Trans => ST_Reset");
                        _parent.PostLogDone(OrthoId, AoExportMgr.ExportDoneEv.ExpStatus.Error, "", "");
                        DoTransition(ST_Reset);
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Private:
                case BEvent.EvType.Public:
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
                    _tmrTimeout.FireInSecs(kTIMEOUT_SECS, OrthoId);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_ValidateConfig Exit");
                    _tmrSequence.Disarm();
                    _tmrTimeout.Disarm();
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    bool isValidFile = true;
                    if (!FileSystem.IsFileNameValid(_currentRequest.OutFile))
                        isValidFile = false;

                    bool isValidDir = true;
                    if (!FileSystem.IsFolderValidForExport(_currentRequest.OutFile))
                        isValidDir = false;
                    else if (!FileSystem.EnsureDirectoryExists(_currentRequest.OutFile))
                        isValidDir = false;

                    if (!isValidDir || !isValidFile)
                    {
                        string outDir = "";
                        try
                        {
                            outDir = Path.GetDirectoryName(_currentRequest.OutFile);
                        }
                        catch { }
                        TraceThis("ST_ValidateConfig Timer directory " + outDir + " not valid for export Trans => ST_Reset");
                        ExportModel.ExportFailedMessage();
                        string details = "";
                        try
                        {
                            details += LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                            if (!isValidDir)
                            {
                                details += Environment.NewLine + LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_PathError") + Environment.NewLine;
                                details += outDir + Environment.NewLine;
                            }
                            if (!isValidFile)
                            {
                                details += Environment.NewLine + LanguageResourceHelper.Get("LID_Label_ExportFilename") + Environment.NewLine;
                                details += _currentRequest.OutFile + Environment.NewLine;
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorThis("ST_ValidateConfig LanguageResourceHelper exception: " + ex.Message);
                        }
                        _parent.PostLogDone(OrthoId, AoExportMgr.ExportDoneEv.ExpStatus.Error, _currentRequest.OutFile, details);
                        DoTransition(ST_Reset);
                        return null;
                    }
                    TraceThis("ST_ValidateConfig Timer - Config IS valid => Trans => ST_CollectLogs");
                    DoTransition(ST_CollectLogs);
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


        List<AuditLogDomain> _auditOut = new List<AuditLogDomain>();
        List<ErrorLogDomain> _errorOut = new List<ErrorLogDomain>();
        // ******************************************************************
        //                    ST_CollectLogs
        // ******************************************************************
        private State ST_CollectLogs(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_CollectLogs Entry");
                    try
                    {
                        _tmrTimeout.FireInSecs(kTIMEOUT_SECS, OrthoId);
                        _auditOut.Clear();
                        _errorOut.Clear();
                        // Request the logs here
                        _ldDataRecords.PostEvent(_currentRequest);
                    } catch(Exception e)
                    {
                        ErrorThis("ST_CollectLogs Entry exception: " + e.Message);
                    }
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_CollectLogs Exit");
                    _tmrTimeout.Disarm();
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Private:
                {
                    switch (ev.Id)
                    {
                        case (uint)AoExportMgr.PrivateEvIds.pExportLog_Status:
                        {
                            TraceThis("ST_CollectLogs Private pExportLog_Status From:" + _currentRequest.StartTime.ToShortDateString() + " " + _currentRequest.StartTime.ToShortTimeString());
                            AoExportMgr.ExportLogStatusIndEv stev = (AoExportMgr.ExportLogStatusIndEv)ev;
                            if (stev.Status == HawkeyeError.eSuccess)
                            {
                                DateTime now = DateTime.Now;
                                if (_currentRequest.IncludeAudit)
                                {
                                    foreach (var entry in stev.AuditEntries)
                                    {
                                        if ((entry.Timestamp >= _currentRequest.StartTime) &&
                                            (entry.Timestamp <= _currentRequest.EndTime))
                                        {
                                            AuditLogDomain val = new AuditLogDomain();
                                            val.AuditEventType = entry.AuditEventType;
                                            val.Timestamp = entry.Timestamp;
                                            string user = string.IsNullOrEmpty(entry.UserId) ? "" : entry.UserId;
                                            user = user.Replace('\n', ' ');
                                            user = user.Replace('\r', ' ');
                                            val.UserId = user;
                                            string msg = entry.Message;
                                            msg = msg.Replace('\n', ' ');
                                            msg = msg.Replace('\r', ' ');
                                            val.Message = msg;
                                            _auditOut.Add(val);
                                        }
                                    }
                                }

                                if (_currentRequest.IncludeError)
                                {
                                    foreach (var entry in stev.ErrorEntries)
                                    {
                                        if ((entry.Timestamp >= _currentRequest.StartTime) &&
                                            (entry.Timestamp <= _currentRequest.EndTime))
                                        {
                                            ErrorLogDomain val = new ErrorLogDomain();
                                            val.Timestamp = entry.Timestamp;
                                            val.ErrorCode = entry.ErrorCode;
                                            string user = string.IsNullOrEmpty(entry.UserId) ? "" : entry.UserId;
                                            user = user.Replace('\n', ' ');
                                            user = user.Replace('\r', ' ');
                                            val.UserId = user;
                                            string msg = entry.Message;
                                            msg = msg.Replace('\n', ' ');
                                            msg = msg.Replace('\r', ' ');
                                            val.Message = msg;
                                            _errorOut.Add(val);
                                        }
                                    }
                                }
                            }
                            if ((_auditOut.Count > 0) || (_errorOut.Count > 0))
                            {
                                TraceThis("ST_CollectLogs Private pExportLog_Status Trans => ST_SaveOutput");
                                DoTransition(ST_SaveOutput);
                                return null;
                            }

                            TraceThis("ST_CollectLogs Private pExportLog_Status Nothing to export, Done  Trans => ST_Reset");
                            _parent.PostLogDone(OrthoId, AoExportMgr.ExportDoneEv.ExpStatus.NoData, _currentRequest.OutFile, "");
                            DoTransition(ST_Reset);
                            return null;
                        }
                    }
                    break;
                }

                //...................................................................
                case BEvent.EvType.Timer:
                case BEvent.EvType.Init:
                case BEvent.EvType.Public:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Exporting;
        }

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
                    TraceThis("ST_SaveOutput Entry - " + ExportDelayMs.ToString());
                    _tmrTimeout.FireInSecs(kTIMEOUT_SECS, OrthoId);
                    _tmrSequence.FireIn(ExportDelayMs, OrthoId);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_SaveOutput Exit");
                    _tmrSequence.Disarm();
                    _tmrTimeout.Disarm();
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    if (ev.Id == (uint)TimerIds.Sequence)
                    {
                        TraceThis("ST_SaveOutput Timer - audit entries: " + _auditOut.Count.ToString() + " error entries: " + _errorOut.Count.ToString());
                        try
                        {
                            System.Data.DataTable dataTable;

                            if (_currentRequest.IncludeAudit && _currentRequest.IncludeError)
                            {
                                dataTable = ExportModel.CreateCombinedLogKeysDataTable(_auditOut, _errorOut);
                            }
                            else if (_currentRequest.IncludeAudit)
                            {
                                dataTable = ExportModel.CreateAuditTrailLogKeysDataTable(_auditOut);
                            }
                            else
                            {
                                dataTable = ExportModel.CreateIErrorLogKeysDataTable(_errorOut);
                            }
                            FileSystem.ExportDataTableWithKeysToFile(dataTable, _currentRequest.OutFile, false, true);
                        }
                        catch (Exception e)
                        {
                            // Check if this is a file not found exception
                            string pathStr = "Could not find a part of the path";
                            if (e.Message.Contains(pathStr))
                            {
                                // Extract the file name/path from the exception message
                                string[] pathStrings = e.Message.Split(new string[] {pathStr}, StringSplitOptions.None);
                                if (pathStrings.Length > 1)
                                {
                                    var pathNotFound = LanguageResourceHelper.Get("LID_API_SystemErrorCode_Failure_Filenotfound") +
                                                       pathStrings[1];

                                    // Post 'File not found' warning to message hub.
                                    MessageBus.Default.Publish(new SystemMessageDomain
                                    {
                                        IsMessageActive = true,
                                        Message = pathNotFound,
                                        MessageType = MessageType.Warning
                                    });
                                }
                            }
                        }

                        int total = _auditOut.Count + _errorOut.Count;
                        string details = "";
                        details += LanguageResourceHelper.Get("LID_CheckBox_TotalCount") + " : " + total.ToString() + Environment.NewLine;

                        _parent.PostLogDone(OrthoId, AoExportMgr.ExportDoneEv.ExpStatus.Success, _currentRequest.OutFile, details);
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
        #endregion

        #region Private_Methods

        #endregion
    }
}
