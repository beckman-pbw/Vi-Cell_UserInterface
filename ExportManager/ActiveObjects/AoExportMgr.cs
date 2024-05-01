using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using ScoutDomains;
using ScoutDomains.Reports.ScheduledExports;
using ScoutLanguageResources;

using BAFW;

using HawkeyeCoreAPI;
using ScoutUtilities;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using ScoutDomains.EnhancedSampleWorkflow;

namespace ExportManager
{
    // ***********************************************************************
    public class AoExportMgr : BActive
    {
        #region Global_Access
        public static void PublishCancelOffline()
        {
            BPublicEvent ev = new BPublicEvent((uint)PubEvIds.ExportMgr_CancelOffline);
            BAppFW.Publish(ev);
        }
        public static void PublishCancelDelete()
        {
            BPublicEvent ev = new BPublicEvent((uint)PubEvIds.SampleDataMgr_CancelDelete);
            BAppFW.Publish(ev);
        }


        #endregion

        #region Private_Const
        private const UInt32 kUSER_CSV_EXPORT_DELAY = 3;
        private const UInt32 kSCHED_CSV_EXPORT_DELAY = 311;
        private const UInt32 kOFFLINE_EXPORT_DELAY = 127;
        private const int kSYS_START_EXPORT_DELAY_SECS = 173;

        private const int kTIMEOUT_SECS = 71; // Timeout for any single action 
        private const int kLOG_NAME_PAD = 24;

        private const int kMIN_QUERY_SECS = 7;

        // Time from the end of one export to the start of the next is controlled by this
        private const int kQUERY_SECS = 43; 

        private const int kMAX_QUERY_SECS = 293;

        #endregion

        #region Construct_Destruct

        // ***********************************************************************
        public AoExportMgr()
        {
            SetRootState(ST_Root);
            _tmrQuery = new BTimer(GetEventQueue(), (UInt32)TimerIds.Query);
            _tmrTimeout = new BTimer(GetEventQueue(), (UInt32)TimerIds.Timeout);

            BAppFW.SubscribeAll(this);

            _ldExport = new LdDataRecords(this);
            _ldDelete = new LdDataRecords(this);

            _orExportOffline = new OrExportOffline(this, OrExportOffline.kORTHO_ID_MASK, _ldExport);
            _orUserExportCsv = new OrExportCsv(this, OrExportCsv.kORTHO_ID_MASK);
            _orSchedExportCsv = new OrExportCsv(this, OrExportCsv.kORTHO_ID_MASK + 1);
            _orUserExportCsv.ExportDelayMs = kUSER_CSV_EXPORT_DELAY; // Slow down the export until auto-export details uses this too

            _orSchedExportLogs = new OrExportLogs(this, OrExportLogs.kORTHO_ID_MASK, _ldDelete);
            _orDeleteRecords = new OrDeleteRecords(this, OrDeleteRecords.kORTHO_ID_MASK, _ldDelete);

			_orExportLogDataCsv = new OrExportLogDataCsv(this, OrExportLogDataCsv.kORTHO_ID_MASK, _ldExport);
			_orExportLogDataCsv.SaveOutputDelayMs = kUSER_CSV_EXPORT_DELAY; // Slow down the export until auto-export details uses this too

            _systemStart = DateTime.Now;
            StartThread();
        }

        // ***********************************************************************
        ~AoExportMgr()
        {
            BAppFW.UnsubscribeAll(this);
            Shutdown();
        }

        // ******************************************************************
        public override void Shutdown()
        {
            try
            {
                BAppFW.UnsubscribeAll(this);
                if (_ldExport != null)
                {
                    _ldExport.Shutdown();
                    _ldExport = null;
                }
                if (_ldDelete != null)
                {
                    _ldDelete.Shutdown();
                    _ldDelete = null;
                }
            }
            catch (Exception e)
            {
                EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Error, "Shutdown Exception: " + e.Message, LogSubSysId);
            }
            base.Shutdown();
        }
        #endregion

        #region Trace_Debug
        public static byte LogSubSysId = 0;
        private const string kMODULE_NAME = "AoExportMgr";
        // ***********************************************************************
        private static void TraceThis(string strData)
        {
            EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Trace, strData, LogSubSysId);
        }
        private static void DebugThis(string strData)
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
        #endregion

        #region Private_Members
        private OrDeleteRecords _orDeleteRecords = null;

        private OrExportOffline _orExportOffline = null;

        private OrExportCsv _orUserExportCsv = null;
        private OrExportCsv _orSchedExportCsv = null;

        private OrExportLogs _orSchedExportLogs = null;

		private OrExportLogDataCsv _orExportLogDataCsv = null;

        private LdDataRecords _ldDelete = null;
        private LdDataRecords _ldExport = null;

        #endregion

        #region Timers
        private BTimer _tmrQuery;
        private BTimer _tmrTimeout;

        // ***********************************************************************
        private enum TimerIds
        {
            Query = 0,
            Timeout
        }
        #endregion

        #region Ortho_Callbacks
        // **********************************************************************
        internal void PostCsvDone(UInt32 orthoId, ExportDoneEv.ExpStatus status, string outFile, string details)
        {
            var doneev = new CsvDoneEv(orthoId, status, outFile, details);
            PostInternalEvent(doneev);
        }

        // **********************************************************************
        internal void PostLogDone(UInt32 orthoId, ExportDoneEv.ExpStatus status, string outFile, string details)
        {
            PostInternalEvent(new LogDoneEv(orthoId, status, outFile, details));
        }
        // **********************************************************************
        internal void PostOfflineDone(UInt32 orthoId, ExportDoneEv.ExpStatus status, string outFile, string details)
        {
            PostInternalEvent(new OfflineDoneEv(orthoId, status, outFile, details));
        }
        #endregion

        #region LD_Callbacks
        // **********************************************************************
        internal void ReportExportOfflineStatus(OfflineExportStatusIndEv.ReqStatus status, string outname = "")
        {
            OfflineExportStatusIndEv statusEv = new OfflineExportStatusIndEv(status, outname);
            PostEvent(statusEv);
        }

        // **********************************************************************
        internal void ReportDeleteStatus(HawkeyeError status, uuidDLL uuid)
        {
            //TraceThis("ReportDeleteStatus");
            DeleteRecordStatusIndEv statusEv = new DeleteRecordStatusIndEv(status, uuid);
            PostEvent(statusEv);
        }

        // **********************************************************************
        internal void ReportExportLogsStatus(
	        HawkeyeError status, 
	        List<AuditLogDomain> auditEntries, 
	        List<ErrorLogDomain> errorEntries)
        {
            ExportLogStatusIndEv statusEv = new ExportLogStatusIndEv(status, auditEntries, errorEntries);
            PostEvent(statusEv);
        }

		// **********************************************************************
		internal void ReportExportLogsStatus(HawkeyeError status, 
			List<AuditLogDomain> auditEntries, 
			List<ErrorLogDomain> errorEntries,
			List<SampleActivityDomain> sampleEntries)
		{
			ExportLogDataCsvStatusIndEv statusEv = new ExportLogDataCsvStatusIndEv(status, auditEntries, errorEntries, sampleEntries);
			PostEvent(statusEv);
		}

		#endregion

		#region Private_Events
		// ******************************************************************
		// Event IDs that are private to ExportManager.
		// ******************************************************************
		internal enum PrivateEvIds
        {
            None = 0,
            pExportOffline_Status,
            pExportLog_Status,
			pExportLogDataCsv_Status,
            pDelete_Status,

            // Event IDs Internal to AoExportMgr.
            iOfflineDone,
            iCsvDone,
            iLogDone,
            iDeleteComplete,
			iLogDataCsvDone,
        }
        // **********************************************************************
        internal class ExportDoneEv : BPrivateEvent
        {
            public enum ExpStatus : UInt32
            {
                Unknown = 0,
                Success,
                NoData,
                Error,
                ReScheduled
            }
            protected ExportDoneEv(PrivateEvIds evId, UInt32 orthoId, ExpStatus status, string outFile, string details)
                : base((uint)evId, 0, orthoId)
            {
                Status = status;
                OutFile = outFile;
                Details = details;
            }
            public ExpStatus Status { get; set; } = ExpStatus.Unknown;
            public string OutFile { get; set; } = "";
            public string Details { get; set; } = "";
        }

        // **********************************************************************
        internal class CsvDoneEv : ExportDoneEv
        {
            public CsvDoneEv(UInt32 orthoId, ExpStatus status, string outFile, string details)
                : base(PrivateEvIds.iCsvDone, orthoId, status, outFile, details) { }
        }

        // **********************************************************************
        internal class LogDoneEv : ExportDoneEv
        {
            public LogDoneEv(UInt32 orthoId, ExpStatus status, string outFile, string details)
                : base(PrivateEvIds.iLogDone, orthoId, status, outFile, details) { }
        }

        // **********************************************************************
        internal class OfflineDoneEv : ExportDoneEv
        {
            public OfflineDoneEv(UInt32 orthoId, ExpStatus status, string outFile, string details)
                : base(PrivateEvIds.iOfflineDone, orthoId, status, outFile, details) { }
        }

        // **********************************************************************
        internal class OfflineExportStatusIndEv : BPrivateEvent
        {
            public enum ReqStatus : UInt32
            {
                Unknown = 0,
                StartOk,
                StartError,
                CollectMetaOk,
                CollectMetaError,
                VerifySpaceOk,
                VerifySpaceError,
                SaveOk,
                SaveError,
                CleanupOk,
                CleanupError
            }

            // **********************************************************************
            public OfflineExportStatusIndEv(ReqStatus status, string outname)
                : base((uint)PrivateEvIds.pExportOffline_Status)
            {
                Status = status;
                Outname = outname;
            }
            public ReqStatus Status { get; set; } = ReqStatus.Unknown;
            public string Outname { get; set; } = "";
        }


        // **********************************************************************
        internal class DeleteRecordStatusIndEv : BPrivateEvent
        {
            public DeleteRecordStatusIndEv(HawkeyeError status, uuidDLL uuid)
                : base((uint)PrivateEvIds.pDelete_Status)
            {
                Status = status;
                Uuid = uuid;
            }
            public uuidDLL Uuid { get; set; } = new uuidDLL();
            public HawkeyeError Status { get; set; } = HawkeyeError.eHardwareFault;
        }

        // **********************************************************************
        internal class ExportLogStatusIndEv : BPrivateEvent
        {
            public ExportLogStatusIndEv(HawkeyeError status, List<AuditLogDomain> auditEntries, List<ErrorLogDomain> errorEntries)
                : base((uint)PrivateEvIds.pExportLog_Status)
            {
                Status = status;
                AuditEntries = auditEntries;
                ErrorEntries = errorEntries;
            }
            public HawkeyeError Status { get; set; } = HawkeyeError.eHardwareFault;
            public List<AuditLogDomain> AuditEntries { get; set; } = new List<AuditLogDomain>();
            public List<ErrorLogDomain> ErrorEntries { get; set; } = new List<ErrorLogDomain>();
        }

        // **********************************************************************
        internal class ExportLogDataCsvStatusIndEv : BPrivateEvent
        {
	        public ExportLogDataCsvStatusIndEv(HawkeyeError status, 
		        List<AuditLogDomain> auditEntries,
		        List<ErrorLogDomain> errorEntries,
		        List<SampleActivityDomain> sampleEntries)
		        : base((uint)PrivateEvIds.pExportLogDataCsv_Status)
	        {
		        Status = status;
		        AuditEntries = auditEntries;
		        ErrorEntries = errorEntries;
		        SampleEntries = sampleEntries;
	        }
	        public HawkeyeError Status { get; set; }
	        public List<AuditLogDomain> AuditEntries { get; set; }
	        public List<ErrorLogDomain> ErrorEntries { get; set; } 
	        public List<SampleActivityDomain> SampleEntries { get; set; }
        }

        #endregion

        #region State_Machine
        private LastReading<ScoutUtilities.Enums.SystemStatus> _lastInstrumentStatus =
            new LastReading<ScoutUtilities.Enums.SystemStatus>(ScoutUtilities.Enums.SystemStatus.Faulted);
        private DateTime _systemStart = DateTime.Now;
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
                    // Need to start one of the 'LDs' polling to update system status
                    _ldDelete.StartPolling();                    
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
                    TraceThis("ST_Root Init => ST_Idle");
                    SetState(ST_Idle);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Public:
                {
                    switch (ev.Id)
                    {
                        case (uint)PubEvIds.SystemStatus:
                        {
                            EvSystemStatusInd statusEv = (EvSystemStatusInd)ev;
                            // Keep for debug 
                            //TraceThis("ST_Root Public SystemStatus: " +
                            //    statusEv.Status.status.ToString() + " : " +
                            //    statusEv.Status.syringePosition.ToString() + " : " +
                            //    statusEv.Status.syringeValvePosition.ToString());

                            if (!_lastInstrumentStatus.IsValid() ||
                                (statusEv.Status.status != _lastInstrumentStatus.Data))
                            {
                                // Sets the last changed timestamp
                                _lastInstrumentStatus.SetValid(statusEv.Status.status);
                                TraceThis("ST_Root Public SystemStatus changed to " + _lastInstrumentStatus.Data.ToString());
                                
                                // Status changed
                                if ((statusEv.Status.status == ScoutUtilities.Enums.SystemStatus.Idle) ||
                                    (statusEv.Status.status == ScoutUtilities.Enums.SystemStatus.Paused) ||
                                    (statusEv.Status.status == ScoutUtilities.Enums.SystemStatus.SearchingTube))
                                {
                                    DebugThis("ST_Root Public SystemStatus, speed up exports");
                                    _orExportOffline.ExportDelayMs = 1;
                                    _orSchedExportCsv.ExportDelayMs = 1;
                                    _orSchedExportLogs.ExportDelayMs = 1;
                                    break;
                                }
                                DebugThis("     slow down exports");
                                _orExportOffline.ExportDelayMs = kOFFLINE_EXPORT_DELAY;
                                _orSchedExportCsv.ExportDelayMs = kSCHED_CSV_EXPORT_DELAY;
                                _orSchedExportLogs.ExportDelayMs = kSCHED_CSV_EXPORT_DELAY;
                                break;
                            }
                            return null;
                        }
                    }
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Private:
                case BEvent.EvType.Timer:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return null;
        }

        private SampleResultsScheduledExportDomain _schedOfflineExport = null;
        private SampleResultsScheduledExportDomain _schedCsvExport = null;
        private AuditLogScheduledExportDomain _schedLogExport = null;
        // ******************************************************************
        //                    ST_Idle
        // ******************************************************************
        private State ST_Idle(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    _schedOfflineExport = null;
                    _schedCsvExport = null;
                    _schedLogExport = null;
                    _backlogDeleteSamplesReq = null;
                    _backlogExportOfflineReq = null;

                    // Adjust the application log level for this sub-system
#if DEBUG
                    EvSetAppLogLevel.Publish(LogSubSysId, EvAppLogReq.LogLevel.Trace);
#else
                    EvSetAppLogLevel.Publish(LogSubSysId, EvAppLogReq.LogLevel.Debug);
#endif

                    DebugThis("ST_Idle Entry - Query in: " + kQUERY_SECS.ToString() + " seconds");
                    _tmrQuery.FireInSecs(kQUERY_SECS);
                    GC.Collect();
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    DebugThis("ST_Idle Exit");
                    _tmrQuery.Disarm();
                    _backlogDeleteSamplesReq = null;
                    _backlogExportOfflineReq = null;
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Public:
                {
                    switch (ev.Id)
                    {
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.ScheduledExportChanged:
                        {
                            DebugThis("ST_Idle Public  Schedule  Changed  - restart timer");
                            _tmrQuery.Disarm();
                            _tmrQuery.FireInSecs(kMIN_QUERY_SECS);
                            return null;
                        }
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.ExportMgr_StartLogs:
                        {
	                        DebugThis("ST_Idle: Public, ExportMgr_StartLogs Trans => ST_Exporting");
	                        _orSchedExportLogs.DeliverEvent(ev);
	                        DoTransition(ST_Exporting);
	                        return null;
                        }
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.ExportMgr_StartOffline:
                        {
                            DebugThis("ST_Idle: Public, ExportMgr_StartOffline Trans => ST_Exporting");
                            _orExportOffline.DeliverEvent(ev);
                            DoTransition(ST_Exporting);
                            return null;
                        }
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.SampleDataMgr_Delete:
                        {
                            DebugThis("ST_Idle: Public, SampleDataMgr_Delete Trans => ST_Deleting");
                            _orDeleteRecords.DeliverEvent(ev);
                            DoTransition(ST_Deleting);
                            return null;
                        }
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.ExportMgr_StartCsv:
                        {
                            DebugThis("ST_Idle: Public, ExportMgr_StartCsv Trans => ST_Exporting");
                            _orUserExportCsv.ExportDelayMs = kUSER_CSV_EXPORT_DELAY; // Slow down the export until auto-export details uses this too
                            _orUserExportCsv.DeliverEvent(ev);
                            DoTransition(ST_Exporting);
                            return null;
                        }
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.ExportMgr_StartLogDataExport:
                        {
	                        DebugThis("ST_Idle: Public, ExportMgr_StartLogDataExport Trans => ST_Exporting");
	                        _orExportLogDataCsv.DeliverEvent(ev);
	                        DoTransition(ST_Exporting);
	                        return null;
                        }
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.SystemStatus:
                        {
                            EvSystemStatusInd statusEv = (EvSystemStatusInd)ev;
                            if (!_lastInstrumentStatus.IsValid() || 
                                (statusEv.Status.status != _lastInstrumentStatus.Data))
                            {
                                // Status changed
                                if (statusEv.Status.status == ScoutUtilities.Enums.SystemStatus.Idle)
                                {
                                    TraceThis("ST_Idle Public SystemStatus changed to Idle");
                                    GC.Collect();
                                }
                            }
                            break;
                        }
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
	                DebugThis("ST_Idle Timer");

                    //return null; // leave for debug - easy way to turn off scheduled export
                    double minSecondsTillSched = double.MaxValue;

                    DateTime now = DateTime.Now;
                    _schedLogExport = null;
                    _schedOfflineExport = null;
                    _schedCsvExport = null;

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    Thread.Sleep(1);

                    Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                    long totalBytes = currentProcess.WorkingSet64;
                    double totalMB = (totalBytes / 1024.0) / 1024.0;
                    var totalVB = System.Diagnostics.Process.GetCurrentProcess().VirtualMemorySize64;
                    double virtualMB = (totalVB / 1024.0) / 1024.0;
                    DebugThis("ST_Idle Timer - Total MB: " + totalMB.ToString("N1") + "    Virtual MB: " + virtualMB.ToString("N1"));

                    var tsSystemStart = DateTime.Now - _systemStart;
                    var startSecsRemain = (kSYS_START_EXPORT_DELAY_SECS - tsSystemStart.TotalSeconds);
                    if (startSecsRemain > 0)
                    {
                        DebugThis("ST_Idle Timer - delay after system start - startSecsRemain " + startSecsRemain.ToString("N0"));
                        _tmrQuery.FireInSecs((uint)(kQUERY_SECS));
                        return null;
                    }
                    DebugThis("ST_Idle Timer - Check for scheduled exports ...");
                    DebugThis("  log exports ...           Last Success         Next Scheduled              dd hh mm ss");
                    try
                    {
                        int schedLogCount = 0;
                        string username = "";
                        try
                        {
                            username = LanguageResourceHelper.Get("LID_GridLabel_ScheduledDataExports");
                        }
                        catch (Exception e)
                        {
                            ErrorThis("ST_Idle Timer - Log LanguageResourceHelper exception " + e.Message);
                        }

                        // **************************************************
                        //
                        // Check for scheduled LOG exports
                        //
                        // **************************************************
                        List<AuditLogScheduledExportDomain> auditLogsExports = new List<AuditLogScheduledExportDomain>();
                        var res = ScheduledExportApi.GetAuditLogScheduledExportsApi(out auditLogsExports);
                        if ((res != HawkeyeError.eSuccess) || (auditLogsExports.Count() == 0))
                        {
                            DebugThis("  No scheduled LOG exports found");
                        }
                        else
                        {
                            schedLogCount = auditLogsExports.Count();
                            for (int j = 0; j < auditLogsExports.Count(); j++)
                            {
                                _schedLogExport = null;
                                var logExp = auditLogsExports[j];

                                string expname = ("[" + logExp.Name + "]").PadRight(kLOG_NAME_PAD);
                                if (!logExp.IsEnabled)
                                {
                                    TraceThis("   " + expname + " NOT enabled");
                                    continue;
                                }

                                DateTime expStart = DateTime.Now;
                                double secsTillReady = 0;
                                if (!IsReady("   " + expname + " ", logExp.RecurrenceRule, logExp.LastRunStatus,
                                    logExp.LastRunTime, logExp.LastSuccessRunTime, ref expStart, out secsTillReady))
                                {
                                    minSecondsTillSched = Math.Min(minSecondsTillSched, secsTillReady);
                                }
                                else
                                {
                                    // Update the status to the DB
                                    logExp.LastRunTime = now;
                                    logExp.LastRunStatus = ScheduledExportLastRunStatus.Running;
                                    SaveScheduledLog(ref logExp);

                                    _schedLogExport = auditLogsExports[j];
                                    string fullPathName = _schedLogExport.DestinationFolder + Path.DirectorySeparatorChar + _schedLogExport.FilenameTemplate
                                                          + "_" + Misc.ConvertToFileNameFormat(now) + ".csv";

                                    DateTime endDate = now;
                                    if (_schedLogExport.RecurrenceRule.RecurrenceFrequency == RecurrenceFrequency.Once)
                                    {
                                        endDate = _schedLogExport.DataFilterCriteria.ToDate;
                                        expStart = _schedLogExport.DataFilterCriteria.FromDate;
                                    }

                                    string tmp1 = expStart.ToShortDateString() + " " + expStart.ToShortTimeString();
                                    string tmp2 = endDate.ToShortDateString() + " " + endDate.ToShortTimeString();
                                    TraceThis("  Start scheduled log export: from: " + tmp1 + " to " + tmp2);

                                    EvExportLogsReq logev = new EvExportLogsReq(username, "",
                                        expStart, endDate, fullPathName, _schedLogExport.IncludeAuditLog, _schedLogExport.IncludeErrorLog);

                                    TraceThis("ST_SchedExports Timer - Log export running Trans => ST_Exporting");
                                    _orSchedExportLogs.DeliverEvent(logev);
                                    DoTransition(ST_Exporting);
                                    return null;
                                }
                            }
                        }

                        // **************************************************
                        //
                        // Check for scheduled DATA exports
                        //
                        // **************************************************

                        DebugThis("  data exports ...");
                        List<SampleResultsScheduledExportDomain> scheduledExportDomains = new List<SampleResultsScheduledExportDomain>();

                        res = ScheduledExportApi.GetScheduledExportsApi(out scheduledExportDomains);

                        if ((res != HawkeyeError.eSuccess) || !scheduledExportDomains.Any())
                        {
                            _schedOfflineExport = null;
                            _schedCsvExport = null;

                            if (schedLogCount == 0)
                            {
                                DebugThis("  No scheduled exports found - DATA or LOG - check again in " + kQUERY_SECS.ToString());
                                // No scheduled exports defined 
                                _tmrQuery.FireInSecs((uint)(kQUERY_SECS));
                                return null;
                            }
                            DebugThis("  No scheduled DATA exports found");
                            // we found log exports, start timer based on next scheduled log time
                            // happens below
                        }
                        else
                        {
                            for (int j = 0; j < scheduledExportDomains.Count(); j++)
                            {
                                _schedOfflineExport = null;
                                string expname = ("[" + scheduledExportDomains[j].Name + "]").PadRight(kLOG_NAME_PAD);

                                var schedExport = scheduledExportDomains[j];
                                if (!schedExport.IsEnabled)
                                {
                                    TraceThis("   " + expname + " NOT enabled");
                                    continue;
                                }

                                DateTime exportStart = now;

                                if (!IsReady("   " + expname + " ", schedExport.RecurrenceRule,
                                    schedExport.LastRunStatus,
                                    schedExport.LastRunTime,
                                    schedExport.LastSuccessRunTime,
                                    ref exportStart, out double secsTillReady))
                                {
                                    minSecondsTillSched = Math.Min(minSecondsTillSched, secsTillReady);
                                }
                                else
                                {
                                    // Update the status to the DB
                                    schedExport.LastRunTime = now;
                                    schedExport.LastRunStatus = ScheduledExportLastRunStatus.Running;
                                    SaveScheduledSample(ref schedExport);

                                    List<ScoutDomains.SampleRecordDomain> sampleRecordList = new List<ScoutDomains.SampleRecordDomain>();

                                    DateTime endDate = now;
                                    if (schedExport.RecurrenceRule.RecurrenceFrequency == RecurrenceFrequency.Once)
                                    {
                                        endDate = schedExport.DataFilterCriteria.ToDate;
                                        exportStart = schedExport.DataFilterCriteria.FromDate;
                                    }

                                    if (FindSamples(exportStart, endDate, schedExport.DataFilterCriteria, ref sampleRecordList))
                                    {
                                        if (schedExport.IsEncrypted)
                                        {
                                            _schedOfflineExport = scheduledExportDomains[j];

                                            if (String.IsNullOrEmpty(_schedOfflineExport.FilenameTemplate))
                                            {
                                                _schedOfflineExport.FilenameTemplate = LanguageResourceHelper.Get("LID_Label_Summary");
                                            }

                                            string zipfile = _schedOfflineExport.DestinationFolder + Path.DirectorySeparatorChar + _schedOfflineExport.FilenameTemplate;
                                            zipfile += "_" + Misc.ConvertToFileNameFormat(now) + ".zip";

                                            List<uuidDLL> sampleIds = new List<uuidDLL>();
                                            foreach (var sr in sampleRecordList)
                                            {
                                                if (sr.SelectedResultSummary != null)
                                                {
                                                    sampleIds.Add(sr.SelectedResultSummary.UUID);
                                                }
                                            }

                                            TraceThis("  Start encrypted data export " + expname + " : " + sampleIds.Count + " records to " + zipfile + " Trans => ST_Exporting");
                                            EvExportOfflineReq req = new EvExportOfflineReq(username, "", sampleIds,
                                                _schedOfflineExport.DestinationFolder, zipfile,
                                                ScoutUtilities.Enums.eExportImages.eAll, 1, 
	                                            false,
                                                null, null, null);

                                            TraceThis("ST_Idle Timer - Data export running Trans => ST_Exporting");
                                            _orExportOffline.DeliverEvent(req);
                                            DoTransition(ST_Exporting);
                                            return null;
                                        }
                                        else
                                        {
                                            _schedCsvExport = scheduledExportDomains[j];
                                            TraceThis("  Start CSV export " + expname + " : " + sampleRecordList.Count + " records  Trans => ST_Exporting");

                                            string zipfile = _schedCsvExport.FilenameTemplate;
                                            zipfile += "_" + Misc.ConvertToFileNameFormat(now);

                                            var summaryCsvBase = "Summary";
                                            try
                                            {
                                                summaryCsvBase = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_Summary");
                                            }
                                            catch (Exception e)
                                            {
                                                ErrorThis("ST_Idle translation error LID_Label_Summary " + e.Message);
                                            }

                                            EvExportCsvReq reqcsv = new EvExportCsvReq(username, "", sampleRecordList,
                                                _schedCsvExport.DestinationFolder, _schedCsvExport.DestinationFolder,
                                                summaryCsvBase, zipfile, true, false, true, true);

                                            TraceThis("ST_Idle Timer - CSV export running Trans => ST_Exporting");
                                            _orSchedExportCsv.DeliverEvent(reqcsv);
                                            DoTransition(ST_Exporting);
                                            return null;
                                        }
                                    }

                                    // Nothing to export.
                                    // Mark this sched export as run OK             
                                    TraceThis("  Sched Data " + expname + " nothing to export");
                                    if (!string.IsNullOrEmpty(schedExport?.NotificationEmail))
                                    {
                                        SendEmail(schedExport, new OfflineDoneEv(0, ExportDoneEv.ExpStatus.NoData, "", ""));
                                    }
                                    schedExport.LastRunStatus = ScheduledExportLastRunStatus.Success;
                                    SaveScheduledSample(ref schedExport);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _schedOfflineExport = null;
                        _schedCsvExport = null;
                        _schedLogExport = null;
                        ErrorThis("ST_Idle Timer - exception " + e.Message);
                    }
                    
                    // ******************************************************
                    // We have scheduled exports but none ready to run now
                    //
                    // Reset timer based on next scheduled export 
                    // Limit the min and max timeout period
                    //
                    uint delaySecs = (uint)Math.Ceiling(minSecondsTillSched + 1);
                    delaySecs = Math.Max(kMIN_QUERY_SECS, delaySecs);

                    TimeSpan delta = new TimeSpan(0, 0, (int)delaySecs);
                    var logstr = "  Next scheduled export in: " + delta.ToString(@"dd\ hh\:mm\:ss");
                    TraceThis(logstr);

                    // Limit the delay time
                    delaySecs = (uint)Math.Min(delaySecs, kMAX_QUERY_SECS);

                    delta = new TimeSpan(0, 0, (int)delaySecs);
                    DebugThis("ST_Idle Timer - Check for scheduled exports again in " + delta.ToString(@"dd\ hh\:mm\:ss"));

                    _tmrQuery.FireInSecs((uint)(delaySecs));

                    return null;
                }
                //...................................................................
                case BEvent.EvType.Init:
                case BEvent.EvType.Private:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Root;
        }

        EvDeleteSamplesReq _backlogDeleteSamplesReq;
        EvExportOfflineReq _backlogExportOfflineReq;
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
                    DebugThis("ST_Exporting: Entry");
                    _tmrTimeout.FireInSecs(kTIMEOUT_SECS);
                    _backlogDeleteSamplesReq = null;
                    _backlogExportOfflineReq = null;
                    EvSetAppLogLevel.Publish(0, EvAppLogReq.LogLevel.Trace);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    DebugThis("ST_Exporting: Exit");
                    _tmrTimeout.Disarm();
                    _tmrQuery.Disarm();
                    _backlogDeleteSamplesReq = null;
                    _backlogExportOfflineReq = null;

                    if (!_orExportOffline.InReset)
                        _orExportOffline.ResetNow();

                    if (!_orUserExportCsv.InReset)
                        _orUserExportCsv.ResetNow();

                    if (!_orSchedExportCsv.InReset)
                        _orSchedExportCsv.ResetNow();

                    if (!_orExportLogDataCsv.InReset)
                    {
	                    TraceThis("ST_Exporting: Exit, calling ResetNow");
	                    _orExportLogDataCsv.ResetNow();
                    }

                    return null;
                }
                //...................................................................
                case BEvent.EvType.Public:
                {
                    switch (ev.Id)
                    {
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.ExportMgr_StartOffline:
                        {
                            if (_orExportOffline.InReset)
                            {
                                TraceThis("ST_Exporting: Public, ExportMgr_StartOffline, deliver event to OR export offline");
                                _orExportOffline.DeliverEvent(ev);
                            }
                            if (_backlogExportOfflineReq == null)
                            {
                                TraceThis("ST_Exporting: Public, ExportMgr_StartOffline, add to backlog of operations to perform");
                                _backlogExportOfflineReq = (EvExportOfflineReq)ev;
                            }
                            return null;
                        }
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.ExportMgr_CancelOffline:
                        {
                            if (!_orExportOffline.InReset)
                            {
                                TraceThis("ST_Exporting: Public, ExportMgr_CancelOffline, deliver to OR export offline");
                                _orExportOffline.DeliverEvent(ev);
                            }
                            _tmrQuery.FireIn(BAppFW.kTIMER_PERIOD_MS);
                            return null;
                        }
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.ExportMgr_StartCsv:
                        {
                            if (_orUserExportCsv.InReset)
                            {
                                TraceThis("ST_Exporting: Public, ExportMgr_StartCsv Deliver to OR export CSV");
                                _orUserExportCsv.ExportDelayMs = kUSER_CSV_EXPORT_DELAY; // Slow down the export until auto-export details uses this too
                                _orUserExportCsv.DeliverEvent(ev);
                            }
                            else
                            {
                                ErrorThis("ST_Exporting: Public, ExportMgr_StartCsv export - no storge space");
                            }
                            return null;
                        }
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.SampleDataMgr_Delete:
                        {
                            if (_backlogDeleteSamplesReq != null)
                            {
	                            ErrorThis("ST_Exporting: Public, SampleDataMgr_Delete, already have a delete backlogged, skipping");
                            }
                            else
                            {
	                            DebugThis("ST_Exporting: Public, SampleDataMgr_Delete add to backlog");
	                            _backlogDeleteSamplesReq = (EvDeleteSamplesReq)ev;
                            }
                            return null;
                        }
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.SampleDataMgr_CancelDelete:
                        {
                            if (_backlogDeleteSamplesReq != null)
                            {
                                DebugThis("ST_Exporting Public SampleDataMgr_CancelDelete clear backlog");
                                _backlogDeleteSamplesReq = null;
                            }
                            return null;
                        }
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.ExportMgr_StartLogDataExport:
                        {
	                        TraceThis($"ST_Exporting: Timer, case (uint)PubEvIds.ExportMgr_StartLogDataExport");

							if (_orExportLogDataCsv.InReset)
	                        {
		                        TraceThis("ST_Exporting Public ExportMgr_StartCsv Deliver to OR Export Audit CSV");
		                        _orExportLogDataCsv.SaveOutputDelayMs = kUSER_CSV_EXPORT_DELAY; // Slow down the export until auto-export details uses this too
		                        _orExportLogDataCsv.DeliverEvent(ev);
	                        }
	                        else
	                        {
		                        ErrorThis("ST_Exporting: Public, ExportMgr_StartCsv, too many operations queued, skipping");
	                        }
	                        return null;
                        }
					}
                    break;
                }
                //...................................................................
                case BEvent.EvType.Private:
                {
                    // Route private events to ortho regions
					// These are private events that ONLY the specified OR will know about.
					// No one else needs to know about the other events.  Other ORs do not even know about the other events.
					// These are events that AO doesn't know anything about because these are OR events.
                    try
                    {
                        if (ev.CheckEventId(OrExportOffline.kORTHO_ID_MASK))
                        {
                            TraceThis("ST_Exporting Private Deliver Event to OR export Offline");
                            _orExportOffline.DeliverEvent(ev);
                            _tmrTimeout.Disarm();
                            _tmrTimeout.FireInSecs(kTIMEOUT_SECS);
                            return null;
                        }
                        if (ev.CheckEventId(OrExportLogs.kORTHO_ID_MASK))
                        {
                            TraceThis("ST_Exporting Private Deliver Event to OR export Log");
                            _orSchedExportLogs.DeliverEvent(ev);
                            _tmrTimeout.Disarm();
                            _tmrTimeout.FireInSecs(kTIMEOUT_SECS);
                            return null;
                        } 
                        if (ev.CheckEventId(OrExportLogDataCsv.kORTHO_ID_MASK))
						{
							TraceThis("ST_Exporting Private Deliver ST_Exporting Event to OR OrExportLogDataCsv");
							_orExportLogDataCsv.DeliverEvent(ev);
							_tmrTimeout.Disarm();
							_tmrTimeout.FireInSecs(kTIMEOUT_SECS);
							return null;
						}
                    }
                    catch (Exception e)
                    {
                        ErrorThis("ST_Exporting Private Deliver Event (log) exception " + e.Message);
                    }

					// These are private to this AO.
					// Route based on AO private event Ids.
                    switch (ev.Id)
                    {
                        // +++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PrivateEvIds.pExportOffline_Status:
                        {
                            if (!_orExportOffline.InReset)
                            {
                                TraceThis("ST_Exporting Private pExportOffline_Status Deliver Event to OR export Offline");
                                try
                                {
                                    _orExportOffline.DeliverEvent(ev);
                                    _tmrTimeout.Disarm();
                                    _tmrTimeout.FireInSecs(kTIMEOUT_SECS);
                                }
                                catch (Exception e)
                                {
                                    ErrorThis("ST_Exporting Private Deliver Event (offline) exception " + e.Message);
                                }

                            }
                            return null;
                        }
                        // +++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PrivateEvIds.pExportLog_Status:
                        {
                            if (!_orSchedExportLogs.InReset)
                            {
                                TraceThis("ST_Exporting Private pExportLog_Status Deliver Event to OR export log");
                                _orSchedExportLogs.DeliverEvent(ev);
		                        _tmrTimeout.Disarm();
		                        _tmrTimeout.FireInSecs(kTIMEOUT_SECS);
	                        }
                            return null;
                        }
                        // +++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PrivateEvIds.pExportLogDataCsv_Status:
                        {
	                        TraceThis($"ST_Exporting: Timer, case (uint)PrivateEvIds.pExportLogDataCsv_Status");

							if (!_orExportLogDataCsv.InReset)
	                        {
		                        TraceThis("ST_Exporting Private pExportLogDataCsv_Status Deliver Event to OR export log");
		                        _orExportLogDataCsv.DeliverEvent(ev);
                                _tmrTimeout.Disarm();
                                _tmrTimeout.FireInSecs(kTIMEOUT_SECS);
                            }
                            return null;
                        }
                        // +++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PrivateEvIds.iOfflineDone:
                        {
                            ExportDoneEv doneEv = (ExportDoneEv)ev;
                            TraceThis("ST_Exporting Private iOfflineComplete");
                            try
                            {
                                if (_schedOfflineExport != null)
                                {
                                    if ((doneEv.Status == ExportDoneEv.ExpStatus.Success) || (doneEv.Status == ExportDoneEv.ExpStatus.NoData))
                                    {
                                        _schedOfflineExport.LastRunStatus = ScheduledExportLastRunStatus.Success;

                                    }
                                    else if (doneEv.Status == ExportDoneEv.ExpStatus.ReScheduled)
                                    {
                                        _schedOfflineExport.LastRunStatus = ScheduledExportLastRunStatus.RunPaused;
                                    }
                                    else
                                    {
                                        _schedOfflineExport.LastRunStatus = ScheduledExportLastRunStatus.Error;
                                    }
                                    SaveScheduledSample(ref _schedOfflineExport);

                                    if (!string.IsNullOrEmpty(_schedOfflineExport?.NotificationEmail))
                                    {
                                        SendEmail(_schedOfflineExport, doneEv);
                                    }
                                    _schedOfflineExport = null;
                                }
                            }
                            catch (Exception e)
                            {
                                ErrorThis("ST_Exporting Private iOfflineComplete exception " + e.Message);
                            }
                            // Start a timer to check if all exports are done
                            _tmrQuery.FireIn(BAppFW.kTIMER_PERIOD_MS);
                            return null;
                        }
                        // +++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PrivateEvIds.iCsvDone:
                        {
                            ExportDoneEv doneEv = (ExportDoneEv)ev;
                            TraceThis("ST_Exporting Private iCsvComplete");
                            try
                            {
                                if (doneEv.OrthoId == _orSchedExportCsv.OrthoId)
                                {
                                    if (_schedCsvExport != null)
                                    {
                                        if ((doneEv.Status == ExportDoneEv.ExpStatus.Success) || (doneEv.Status == ExportDoneEv.ExpStatus.NoData))
                                        {
                                            _schedCsvExport.LastRunStatus = ScheduledExportLastRunStatus.Success;
                                        }
                                        else if (doneEv.Status == ExportDoneEv.ExpStatus.ReScheduled)
                                        {
                                            _schedCsvExport.LastRunStatus = ScheduledExportLastRunStatus.RunPaused;
                                        }
                                        else
                                        {
                                            _schedCsvExport.LastRunStatus = ScheduledExportLastRunStatus.Error;
                                        }
                                        SaveScheduledSample(ref _schedCsvExport);
                                        if (!string.IsNullOrEmpty(_schedCsvExport?.NotificationEmail))
                                        {
                                            SendEmail(_schedCsvExport, doneEv);
                                        }
                                        _schedCsvExport = null;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                ErrorThis("ST_Exporting Private iCsvComplete exception " + e.Message);
                            }
                            // Start a timer to check if all exports are done
                            _tmrQuery.FireIn(BAppFW.kTIMER_PERIOD_MS);
                            return null;
                        }
                        // +++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PrivateEvIds.iLogDone:
                        {
                            ExportDoneEv doneEv = (ExportDoneEv)ev;
                            TraceThis("ST_Exporting Private iLogComplete");
                            try
                            {
                                if (_schedLogExport != null)
                                {
                                    if ((doneEv.Status == ExportDoneEv.ExpStatus.Success) || (doneEv.Status == ExportDoneEv.ExpStatus.NoData))
                                    {
                                        _schedLogExport.LastRunStatus = ScheduledExportLastRunStatus.Success;
                                    }
                                    else if (doneEv.Status == ExportDoneEv.ExpStatus.ReScheduled)
                                    {
                                        _schedLogExport.LastRunStatus = ScheduledExportLastRunStatus.RunPaused;
                                    }
                                    else
                                    {
                                        _schedLogExport.LastRunStatus = ScheduledExportLastRunStatus.Error;
                                    }
                                    SaveScheduledLog(ref _schedLogExport);

                                    if (!string.IsNullOrEmpty(_schedLogExport?.NotificationEmail))
                                    {
                                        SendEmail(_schedLogExport, doneEv);
                                    }
                                    _schedLogExport = null;
                                }
                            }
                            catch (Exception e)
                            {
                                ErrorThis("ST_Exporting Private iLogComplete exception " + e.Message);
                            }

                            // Start a timer to check if all exports are done
                            _tmrQuery.FireIn(BAppFW.kTIMER_PERIOD_MS);
                            return null;
                        }
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    try
                    {
                        // Route events to the correct Ortho Region
                        if (ev.CheckEventId(OrExportCsv.kORTHO_ID_MASK))
                        {
                            _tmrTimeout.Disarm();
                            _tmrTimeout.FireInSecs(kTIMEOUT_SECS);

                            // Export CSV usses the App Data in an event for routing
                            if (ev.AppData == _orUserExportCsv.OrthoId)
                            {
                                // keep for debug DebugThis("ST_Exporting Timer - route to _orUserExportCsv");
                                _orUserExportCsv.DeliverEvent(ev);
                                return null;
                            }
                            if (ev.AppData == _orSchedExportCsv.OrthoId)
                            {
                                // keep for debug DebugThis("ST_Exporting Timer - route to _orSchedExportCsv");
                                _orSchedExportCsv.DeliverEvent(ev);
                                return null;
                            }
                        }
                        if (ev.CheckEventId(OrExportLogs.kORTHO_ID_MASK))
                        {
                            // keep for debug DebugThis("ST_Exporting Timer - route to _orSchedExportLogs");
                            _orSchedExportLogs.DeliverEvent(ev);
                            _tmrTimeout.Disarm();
                            _tmrTimeout.FireInSecs(kTIMEOUT_SECS);
                            return null;
                        }
                        if (ev.CheckEventId(OrExportOffline.kORTHO_ID_MASK))
                        {
                            // keep for debug DebugThis("ST_Exporting Timer - route to _orExportOffline");
                            _orExportOffline.DeliverEvent(ev);
                            _tmrTimeout.Disarm();
                            _tmrTimeout.FireInSecs(kTIMEOUT_SECS);
                            return null;
                        }
                        if (ev.CheckEventId(OrExportLogDataCsv.kORTHO_ID_MASK))
						{
							_tmrTimeout.Disarm();
							_tmrTimeout.FireInSecs(kTIMEOUT_SECS);
							// Export CSV uses the App Data in an event for routing.
							if (ev.AppData == _orExportLogDataCsv.OrthoId)
							{
								//TraceThis("ST_Exporting: Timer, ev.AppData == _orExportLogDataCsv.OrthoId");
								_orExportLogDataCsv.DeliverEvent(ev);
								return null;
							}
                        }
                    }
                    catch (Exception e)
                    {
                        ErrorThis("ST_Exporting Timer => OR exception " + e.Message);
                    }

                    try
                    {
                        // Must be one of our timers 
                        if (ev.Id == (uint)TimerIds.Query)
                        {
                            if (_orUserExportCsv.InReset &&
                                _orSchedExportCsv.InReset &&
                                _orExportOffline.InReset)
                            {
                                // All ortho regions are in reset
                                if (_backlogDeleteSamplesReq != null)
                                {
                                    DebugThis("ST_Exporting: TimerQuery, delete backlog Trans => ST_Deleting");
                                    _orDeleteRecords.DeliverEvent(_backlogDeleteSamplesReq);
                                    _backlogDeleteSamplesReq = null;
                                    DoTransition(ST_Deleting);
                                    return null;
                                }
                                if (_backlogExportOfflineReq != null)
                                {
                                    DebugThis("ST_Exporting: TimerQuery, offline export in processing backlog.  Delievering event to OR export offline");
                                    _orExportOffline.DeliverEvent(_backlogExportOfflineReq);
                                    _backlogExportOfflineReq = null;
                                    return null;
                                }
                                DebugThis("ST_Exporting: TimerQuery, all done Trans => ST_Idle");
                                DoTransition(ST_Idle);
                                return null;
                            }
                            _tmrQuery.FireInSecs(1);
                            return null;
                        }
                    }
                    catch (Exception e)
                    {
                        ErrorThis("ST_Exporting Timer Query exception " + e.Message);
                    }

                    if (ev.Id == (uint)TimerIds.Timeout)
                    {
                        if (_backlogDeleteSamplesReq != null)
                        {
                            TraceThis("ST_Exporting: Timer Timeout, delete in backlog Trans => ST_Deleting");
                            try
                            {
                                _orDeleteRecords.DeliverEvent(_backlogDeleteSamplesReq);
                                DoTransition(ST_Deleting);
                                return null;
                            }
                            catch (Exception e)
                            {
                                ErrorThis("ST_Exporting Timer start delete from backlog exception " + e.Message);
                            }
                        }

                        DebugThis("ST_Exporting: Timer Timeout, Trans => ST_Idle");
                        DoTransition(ST_Idle);
                        return null;
                    }


                    return null;
                }
                //...................................................................
                case BEvent.EvType.Init:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Root;
        }


        // ******************************************************************
        //                    ST_Deleting
        // ******************************************************************
        private State ST_Deleting(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    DebugThis("################# ST_Deleting Entry #################");
                    EvSetAppLogLevel.Publish(0, EvAppLogReq.LogLevel.Trace);

                    if (_backlogDeleteSamplesReq != null)
                    {
                        DebugThis("ST_Deleting: Entry Deliver delete samples to OR ");
                        _orDeleteRecords.DeliverEvent(_backlogDeleteSamplesReq);
                        _backlogDeleteSamplesReq = null;
                    }
                    _tmrTimeout.Disarm();
                    _tmrTimeout.FireInSecs(kTIMEOUT_SECS);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    DebugThis("################# ST_Deleting Exit #################");
                    _tmrTimeout.Disarm();
                    if (!_orDeleteRecords.InReset)
                    {
                        _orDeleteRecords.ResetNow();
                    }
                    _backlogDeleteSamplesReq = null;
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Public:
                {
                    switch (ev.Id)
                    {
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.SampleDataMgr_CancelDelete:
                        {
                            DebugThis("ST_Deleting Public SampleDataMgr_CancelDelete Deliver to OR Delete");
                            _orDeleteRecords.DeliverEvent(ev);
                            _tmrQuery.FireInSecs(1);
                            return null;
                        }
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.SampleDataMgr_Delete:
                        {
                            WarnThis("ST_Deleting Public SampleDataMgr_Delete do nothing");
                            return null;
                        }
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.ExportMgr_StartOffline:
                        {
                            WarnThis("ST_Deleting Public ExportMgr_StartOffline do nothing");
                            return null;
                        }
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.ExportMgr_StartCsv:
                        {
                            WarnThis("ST_Deleting Public ExportMgr_StartCsv do nothing");
                            return null;
                        }
                        // ++++++++++++++++++++++++++++++++++++++++++++++++++
                        case (uint)PubEvIds.ExportMgr_StartLogs:
                        {
                            WarnThis("ST_Deleting Public ExportMgr_StartLogs do nothing");
                            return null;
                        }
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Private:
                {
                    // Route private events to ortho regions
                    if (ev.CheckEventId(OrDeleteRecords.kORTHO_ID_MASK))
                    {
                        TraceThis("ST_Deleting Private (ortho specific) Deliver Event to OR Delete");
                        _orDeleteRecords.DeliverEvent(ev);
                        _tmrTimeout.Disarm();
                        _tmrTimeout.FireInSecs(kTIMEOUT_SECS);
                        return null;
                    }
                    if (ev.Id == (uint)PrivateEvIds.pDelete_Status)
                    {
                        TraceThis("ST_Deleting Private pDelete_Status Deliver Event to OR Delete");
                        _orDeleteRecords.DeliverEvent(ev);
                        _tmrTimeout.Disarm();
                        _tmrTimeout.FireInSecs(kTIMEOUT_SECS);
                        return null;
                    }
                    if (ev.Id == (uint)PrivateEvIds.iDeleteComplete)
                    {
                        DebugThis("ST_Deleting Private Delete done Trans => ST_Idle");
                        DoTransition(ST_Idle);
                        return null;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    if (ev.CheckEventId(OrDeleteRecords.kORTHO_ID_MASK))
                    {
                        _orDeleteRecords.DeliverEvent(ev);
                        return null;
                    }

                    if (ev.Id == (uint)TimerIds.Timeout)
                    {
                        DebugThis("ST_Deleting: Timer Timeout, Trans => ST_Idle");
                        DoTransition(ST_Idle);
                        return null;
                    }
                    if (_orDeleteRecords.InReset)
                    {
                        DebugThis("ST_Deleting: Timer - Delete, done Trans => ST_Idle");
                        DoTransition(ST_Idle);
                        return null;
                    }
                    if (ev.Id == (uint)TimerIds.Query)
                    {
                        _tmrQuery.FireInSecs(1);
                    }
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Init:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
            return ST_Root;
        }
	#endregion

	#region Utility_Functions

        private const int kRETRY_HOURS = 24;
        // ******************************************************************
        bool IsReady(string logPrefix, RecurrenceRuleDomain rule, 
            ScheduledExportLastRunStatus lastRunStatus,
            DateTime lastRuntime, DateTime lastSuccessTime, ref DateTime exportFrom, out double secsTillReady)
        {
            try
            {
                DateTime now = DateTime.Now;
                DateTime schedDate;
                string log;
                if ((lastRunStatus == ScheduledExportLastRunStatus.Error) ||
                    (lastRunStatus == ScheduledExportLastRunStatus.Running))
                {
                    schedDate = lastRuntime.AddHours(kRETRY_HOURS);
                }
                else
                {
                    switch (rule.RecurrenceFrequency)
                    {
                        //...........................................................
                        case RecurrenceFrequency.Once:
                        {
                            schedDate = new DateTime(rule.ExportOnDate.Year, rule.ExportOnDate.Month, rule.ExportOnDate.Day, rule.Get24Hour(), rule.Minutes, 0);
                            if (lastRunStatus == ScheduledExportLastRunStatus.Success)
                            {
                                if (lastSuccessTime >= schedDate)
                                {
                                    // Already run after the scheduled date - don't run again
                                    schedDate = now.AddDays(99);
                                }
                            }
                            break;
                        }
                        //...........................................................
                        case RecurrenceFrequency.Daily:
                        {
                            exportFrom = now.AddDays(-1);
                            schedDate = new DateTime(now.Year, now.Month, now.Day, rule.Get24Hour(), rule.Minutes, 0);

                            if (lastRunStatus == ScheduledExportLastRunStatus.Success)
                            {
                                if (lastSuccessTime < exportFrom)
                                    exportFrom = lastSuccessTime;

                                if (lastSuccessTime >= schedDate.AddMinutes(-1))
                                    schedDate = schedDate.AddDays(1);
                            }
                            break;
                        }
                        //...........................................................
                        case RecurrenceFrequency.Weekly:
                        {
                            exportFrom = now.AddDays(-7);
                            schedDate = new DateTime(now.Year, now.Month, now.Day, rule.Get24Hour(), rule.Minutes, 0);

                            int deltaDays = (int)rule.Weekday - (int)now.DayOfWeek;
                            if (deltaDays != 0)
                            {
                                // Not ready - wrong day
                                if (deltaDays < 0)
                                    deltaDays += 7;
                                schedDate = schedDate.AddDays(deltaDays);
                            }
                            if (lastRunStatus == ScheduledExportLastRunStatus.Success)
                            {
                                if (lastSuccessTime < exportFrom)
                                    exportFrom = lastSuccessTime;

                                if (lastSuccessTime >= schedDate.AddMinutes(-1))
                                    schedDate = schedDate.AddDays(7);
                            }
                            break;
                        }
                        //...........................................................
                        case RecurrenceFrequency.Monthly:
                        {
                            exportFrom = now.AddMonths(-1);
                            schedDate = new DateTime(now.Year, now.Month, rule.DayOfTheMonth, rule.Hour, rule.Minutes, 0);

                            if (lastRunStatus == ScheduledExportLastRunStatus.Success)
                            {
                                if (lastSuccessTime < exportFrom)
                                    exportFrom = lastSuccessTime;

                                if (lastSuccessTime >= schedDate.AddMinutes(-1))
                                    schedDate = schedDate.AddMonths(1);
                            }
                            break;
                        }
                        //...........................................................
                        default:
                        {
                            log = " Bad frequency: " + rule.RecurrenceFrequency.ToString();
                            ErrorThis(logPrefix + log);
                            schedDate = DateTime.Now.AddDays(1);
                            break;
                        }
                    }
                }

                if (schedDate > now)
                {
                    TimeSpan delta = (schedDate - now);
                    secsTillReady = Math.Ceiling(delta.TotalSeconds);

                    string tmp = "";
                    if ((lastRunStatus == ScheduledExportLastRunStatus.Error) ||
                        (lastRunStatus == ScheduledExportLastRunStatus.Running))
                        tmp = lastRuntime.ToShortDateString() + " " + lastRuntime.ToShortTimeString() + "r";
                    else
                        tmp = lastSuccessTime.ToShortDateString() + " " + lastSuccessTime.ToShortTimeString();

                    log = tmp.PadRight(18);
                    log += " < ";
                    tmp = schedDate.ToShortDateString() + " " + schedDate.ToShortTimeString();
                    log += tmp.PadRight(18);
                    log += " ready in " + delta.ToString(@"dd\ hh\:mm\:ss");
                    DebugThis(logPrefix + log);
                    return false;
                }
                secsTillReady = 0;
                DebugThis(logPrefix + " *************** Ready to Run Now **************");
                return true;
            }
            catch (Exception e)
            {
                secsTillReady = 60;
                ErrorThis("IsReady exception " + e.Message);
            }
            return false;
        }


        // ******************************************************************
        bool FindSamples(
	        DateTime startDate, 
	        DateTime endDate, 
	        DataFilterCriteriaDomain filter, 
	        ref List<ScoutDomains.SampleRecordDomain> sampleRecordList)
        {
            try
            {
                startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day);
                var fromDate = DateTimeConversionHelper.DateTimeToUnixSecondRounded(startDate);
                var toDate = DateTimeConversionHelper.DateTimeToUnixSecondRounded(endDate);

                List<SampleSetDomain> sampleSets = new List<SampleSetDomain>();

                string userStr = LanguageResourceHelper.Get("LID_Label_All") == filter.SelectedUsername ? "" : filter.SelectedUsername;
                string cellStr = "";
                if (!filter.IsAllCellTypeSelected)
                {
                    if (filter.SelectedCellTypeOrQualityControlGroup != null)
                    {
                        cellStr = filter.SelectedCellTypeOrQualityControlGroup.Name;
                    }
                }

                string log = "FindSamples - Export from: " + startDate.ToString() + " to " + endDate.ToString();
                log += " filter type <" + filter.FilterType.ToString() + ">";
                log += " user <" + userStr + ">";
                log += " search <" + filter.SampleSearchString + ">";
                log += " tag <" + filter.Tag + ">";
                log += " Cell Type <" + cellStr + ">";
                TraceThis(log);

                var res = HawkeyeCoreAPI.SampleSet.GetSampleSetListApiCall(
                    filter.FilterType,
                    fromDate,
                    toDate,
                    userStr,
                    filter.SampleSearchString,
                    filter.Tag,
                    cellStr,
                    0, 0,
                    out uint totalQueryResultCount,
                    out sampleSets);

                if (res != HawkeyeError.eSuccess)
                    return false;

                if (sampleSets.Count == 0)
                {
                    // Nothing to export
                    return false;
                }

                List<ScoutUtilities.Common.SamplePosition> samplePositions = new List<ScoutUtilities.Common.SamplePosition>();
				List<uuidDLL> sampleIds = new List<uuidDLL>();

                foreach (var ss in sampleSets)
                {
                    foreach (var sample in ss.Samples)
                    {
                        if ((sample.SampleStatus == ScoutUtilities.Enums.SampleStatus.Completed) && (!sample.SampleDataUuid.IsNullOrEmpty()))
                        {
                            sampleIds.Add(sample.SampleDataUuid);
							samplePositions.Add(sample.SamplePosition);
                        }
                    }
                }

                if (sampleIds.Count == 0)
                {
                    // Nothing to export
                    return false;
                }

                sampleRecordList.Clear();
                res = Sample.RetrieveSampleRecordListAPI(sampleIds.ToArray(), out sampleRecordList, out var retrieveSize);

                if ((res == ScoutUtilities.Enums.HawkeyeError.eSuccess) && (sampleRecordList.Count > 0))
                {
                    int sampleIndex = 0;
					
					foreach (var sr in sampleRecordList)
                    {
                        if (sr.SelectedResultSummary != null && sampleIds[sampleIndex].Equals(sr.UUID))
                        {
							sr.Position = samplePositions[sampleIndex];
                        }
                        sampleIndex++;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                WarnThis("FindSamples exception: " + e.Message);
            }
            return false;
        }


        // ******************************************************************
        void SaveScheduledSample(ref SampleResultsScheduledExportDomain sched)
        {
            if (sched.LastRunStatus == ScheduledExportLastRunStatus.Success)
            {
                sched.LastSuccessRunTime = sched.LastRunTime;
                if (sched.RecurrenceRule.RecurrenceFrequency == RecurrenceFrequency.Once)
                {
                    // Disable run-once on success 
                    sched.IsEnabled = false;
                    TraceThis(sched.Name + " Run-Once OK - disable");
                }
            }
            else if (sched.LastRunStatus == ScheduledExportLastRunStatus.Error)
            {
                try
                {
                    string msgStr = LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                    msgStr += LanguageResourceHelper.Get("LID_GridLabel_ScheduledDataExports");
                    PostToMessageHub(msgStr, MessageType.System);
                }
                catch (Exception ex)
                {
                    ErrorThis("SaveScheduledSample translation / PostToMessageHub exception " + ex.Message);
                }
            }

            var res = ScheduledExportApi.EditScheduledExportApi(sched);
            if (res != HawkeyeError.eSuccess)
            {
                WarnThis("SaveScheduledSample: ScheduledExportApi.EditScheduledExportApi FAILED " + res.ToString());
            }
            else
            {
                TraceThis("SaveScheduledSample: ScheduledExportApi.EditScheduledExportApi OK");
            }
        }

        // ******************************************************************
        void SaveScheduledLog(ref AuditLogScheduledExportDomain sched)
        {
            if (sched.LastRunStatus == ScheduledExportLastRunStatus.Success)
            {
                sched.LastSuccessRunTime = sched.LastRunTime;
                if (sched.RecurrenceRule.RecurrenceFrequency == RecurrenceFrequency.Once)
                {
                    // Disable run-once on success 
                    sched.IsEnabled = false;
                    TraceThis(sched.Name + " Run-Once OK - disable");
                }
            }
            else if (sched.LastRunStatus == ScheduledExportLastRunStatus.Error)
            {
                try
                {
                    string msgStr = LanguageResourceHelper.Get("LID_EXCEPTIONMSG_STORAGE_EXPORT_RECORDS") + Environment.NewLine;
                    msgStr += LanguageResourceHelper.Get("LID_LogsList_ScheduledLogExports");
                    PostToMessageHub(msgStr, MessageType.System);
                }
                catch (Exception ex)
                {
                    ErrorThis("SaveScheduledLog translation / PostToMessageHub exception " + ex.Message);
                }
            }

            var res = ScheduledExportApi.EditScheduledExportApi(sched);
            if (res != HawkeyeError.eSuccess)
            {
                WarnThis("SaveScheduledLog: ScheduledExportApi.EditScheduledExportApi FAILED " + res.ToString());
            }
            else
            {
                TraceThis("SaveScheduledLog: ScheduledExportApi.EditScheduledExportApi OK");
            }
        }

        // ******************************************************************
        private void SendEmail(SampleResultsScheduledExportDomain dataExport, ExportDoneEv doneEv)
        {
            // Send an email
            string subject = "";
            string body = "";
            try
            {
                subject += LanguageResourceHelper.Get("LID_GridLabel_ScheduledDataExports") + " [" + dataExport.Name + "] ";
                body += LanguageResourceHelper.Get("LID_GridLabel_ScheduledDataExports") + " [" + dataExport.Name + "] " + Environment.NewLine;
            }
            catch (Exception e)
            {
                ErrorThis("SendEmail exception: " + e.Message);
            }
            SendEmail(dataExport.NotificationEmail, subject, body, doneEv);
        }

        // ******************************************************************
        void SendEmail(AuditLogScheduledExportDomain logExport, ExportDoneEv doneEv)
        {
            // Send an email
            string subject = "";
            string body = "";
            try
            {
                subject = LanguageResourceHelper.Get("LID_LogsList_ScheduledLogExports") + " [" + logExport.Name + "] ";
                body += LanguageResourceHelper.Get("LID_LogsList_ScheduledLogExports") + " [" + logExport.Name + "] " + Environment.NewLine;
                body += LanguageResourceHelper.Get("LID_Label_ExportLogs") + " ";
                if (logExport.IncludeAuditLog)
                    body += "[" + LanguageResourceHelper.Get("LID_ScheduledExport_IncludeAuditLog") + "] ";
                if (logExport.IncludeErrorLog)
                    body += "[" + LanguageResourceHelper.Get("LID_ScheduledExport_IncludeErrorLog") + "]";

                body += Environment.NewLine + Environment.NewLine;
            }
            catch (Exception e)
            {
                ErrorThis("SendEmail exception: " + e.Message);
            }
            SendEmail(logExport.NotificationEmail, subject, body, doneEv);
        }

        // ******************************************************************
        void SendEmail(string toAddr, string subject, string body, ExportDoneEv doneEv)
        {
            try
            {
                if (doneEv.Status == ExportDoneEv.ExpStatus.Success)
                {
                    subject += LanguageResourceHelper.Get("LID_MSGBOX_ExportSucces");
                    body += LanguageResourceHelper.Get("LID_MSGBOX_ExportSucces") + Environment.NewLine;
                    body += Environment.NewLine;
                    body += LanguageResourceHelper.Get("LID_Label_ExportFilename") + " " + doneEv.OutFile + Environment.NewLine;
                    if (doneEv.Details.Length > 0)
                    {
                        body += doneEv.Details + Environment.NewLine;
                    }
                }
                else if (doneEv.Status == ExportDoneEv.ExpStatus.NoData)
                {
                    subject += LanguageResourceHelper.Get("LID_MSGBOX_ExportSucces"); // Export to file successful
                    body += LanguageResourceHelper.Get("LID_Report_NoData") + Environment.NewLine;
                }
                else if (doneEv.Status == ExportDoneEv.ExpStatus.ReScheduled)
                {
                    subject += LanguageResourceHelper.Get("LID_Enum_Paused");
                    body += LanguageResourceHelper.Get("LID_Enum_Paused") + Environment.NewLine;
                }
                else
                {
                    subject += LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONEXPORT");  // Failed to export
                    body += LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONEXPORT") + Environment.NewLine;
                    body += doneEv.Details;
                }
            }
            catch (Exception e)
            {
                ErrorThis("SendEmail exception: " + e.Message);
            }
            SendEmail(toAddr, subject, body);
        }

        // ******************************************************************
        public bool SendEmail(string toAddr, string subject, string textBody)
        {
            try
            {
                ScoutModels.Settings.SmtpSettingsModel smtp = new ScoutModels.Settings.SmtpSettingsModel();
                var smtpConfig = smtp.GetSmtpConfig();
                if (string.IsNullOrEmpty(smtpConfig.Server) || smtpConfig.Port == 0)
                {
                    DebugThis("Send Email - not configured");
                    return false;
                }

                TraceThis("Send email to " + toAddr);

                var messageToSend = new MimeMessage();
                messageToSend.From.Add(new MailboxAddress(LanguageResourceHelper.Get("LID_Title_ViCellBlu"), smtpConfig.Username));
                messageToSend.To.Add(new MailboxAddress(toAddr, toAddr));
                messageToSend.Subject = subject;
                messageToSend.Body = new TextPart(TextFormat.Plain) { Text = textBody };

                using (var client = new SmtpClient())
                {
                    client.Connect(smtpConfig.Server, (int)smtpConfig.Port);
                    if (ScoutUtilities.Misc.ByteToBool(smtpConfig.AuthEnabled))
                    {
                        client.Authenticate(smtpConfig.Username, smtpConfig.Password);
                    }
                    client.Send(messageToSend);
                    client.Disconnect(true);
                }
                return true;
            }
            catch (ServiceNotAuthenticatedException svcAuthException)
            {
                try
                {
                    string msgStr = LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SEND_EMAIL_ERROR_AUTH") + Environment.NewLine;
                    msgStr += LanguageResourceHelper.Get("LID_GridLabel_ScheduledDataExports");
                    PostToMessageHub(msgStr, MessageType.System);
                }
                catch (Exception ex)
                {
                    ErrorThis("SendEmail translation / PostToMessageHub exception " + ex.Message);
                }
                EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Error, "SendEmail() - svc not auth exception: " + svcAuthException.Message, LogSubSysId);
                return false;
            }
            catch (AuthenticationException authException)
            {
                try
                {
                    string msgStr = LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SEND_EMAIL_ERROR_AUTH") + Environment.NewLine;
                    msgStr += LanguageResourceHelper.Get("LID_GridLabel_ScheduledDataExports");
                    PostToMessageHub(msgStr, MessageType.System);
                }
                catch (Exception ex)
                {
                    ErrorThis("SendEmail translation / PostToMessageHub exception " + ex.Message);
                }

                EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Error, "SendEmail() - auth exception: " + authException.Message, LogSubSysId);
                return false;
            }
            catch (Exception e)
            {
                try
                {
                    string msgStr = LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SEND_EMAIL_ERROR") + Environment.NewLine;
                    msgStr += LanguageResourceHelper.Get("LID_GridLabel_ScheduledDataExports");
                    PostToMessageHub(msgStr, MessageType.System);
                }
                catch (Exception ex)
                {
                    ErrorThis("SendEmail translation / PostToMessageHub exception " + ex.Message);
                }

                EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Error, "SendEmail() - exception: " + e.Message, LogSubSysId);
                return false;
            }
        }

        // ******************************************************************
        private void PostToMessageHub(string statusBarMessage, MessageType messageType)
        {
            ScoutUtilities.Events.MessageBus.Default.Publish(new SystemMessageDomain
            {
                IsMessageActive = true,
                Message = statusBarMessage,
                MessageType = messageType
            });
        }

#endregion

    }
}