using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using ScoutDomains;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutLanguageResources;
using ScoutModels.Common;

using BAFW;

namespace ExportManager
{
    // ***********************************************************************
    public class OrExportLogDataCsv : BOrthoRegion
    {

        internal const UInt32 kORTHO_ID_MASK = 0x00001000;

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
		// ***********************************************************************
		public OrExportLogDataCsv(
			AoExportMgr parent, 
			UInt32 orthoId,
			LdDataRecords ldDataRecords)
            : base(parent, orthoId)
        {
            _parent = parent;
            _tmrTimeout = new BTimer(parent.GetEventQueue(), (UInt32)TimerIds.Timeout);
            _tmrQuery = new BTimer(parent.GetEventQueue(), (UInt32)TimerIds.Query);
            _ldDataRecords = ldDataRecords;

			SetRootState(ST_Root);
            InitStateMachine();
        }
	#endregion

	#region Trace_Debug
        public static byte LogSubSysId = 0;
        private const string kMODULE_NAME = "OrExportLogDataCsv";
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
		private const int kTIMEOUT_SECS = 57;
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
                    TraceThis("ST_Root: Init => ST_Reset");
                    SetState(ST_Reset);
                    return null;
                }
                //...................................................................
                case BEvent.EvType.Private:
                {
	                TraceThis("ST_Reset: Private");
                    if (ev.Id == (uint)PrivateEvIds.iReset)
                    {
                        TraceThis("ST_Root: Private, iReset Trans => ST_Reset");
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


        private EvExportLogDataCsvReq _currentRequest = null;
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
					TraceThis("ST_Reset: Public");
                    switch (ev.Id)
                    {
                        case (uint)PubEvIds.ExportMgr_StartLogDataExport:
                        {
	                        TraceThis("ST_Reset: Public, ExportMgr_StartLogDataExport Trans => ST_Exporting");
	                        _currentRequest = (EvExportLogDataCsvReq)ev;
                            DoTransition(ST_Exporting);
                            return null;
                        }
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Private:
                {
	                TraceThis("ST_Reset: Private");
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

					string str = "Export Audit data:";
					str += " from " + _currentRequest.StartTime.ToLongDateString();
					str += " to " + _currentRequest.EndTime.ToLongDateString();
					TraceThis(str);
					return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_Exporting: Exit");
	                _tmrTimeout.Disarm();
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
	                TraceThis("ST_ValidateConfig: Timer");
	                bool isValidFile = FileSystem.IsFileNameValid(_currentRequest.LocalExportFilepath);
	                bool isValidDir = FileSystem.IsFolderValidForExport(_currentRequest.LocalExportFilepath) &&
	                                  FileSystem.EnsureDirectoryExists(_currentRequest.LocalExportFilepath);

	                if (!isValidDir || !isValidFile)
	                {
						ErrorThis($"Invalid directory or filename {_currentRequest.LocalExportFilepath}");
						DoTransition(ST_Reset);
		                return null;
	                }

	                TraceThis("ST_ValidateConfig: Timer, Config IS valid, Trans => ST_CollectLogs");
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
        List<SampleActivityDomain> _sampleOut = new List<SampleActivityDomain>();
		// ******************************************************************
		//                    ST_CollectLogs
		// ******************************************************************
		private State ST_CollectLogs(BEvent ev)   // ST_CollectLogs
		{
			switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Entry:
                {
                    TraceThis("ST_CollectLogs: Entry");
                    _tmrTimeout.FireInSecs(kTIMEOUT_SECS, OrthoId);

					try
					{ // Collect the log data.
						TraceThis("ST_CollectLogs: Entry, collecting the log data");
						_auditOut.Clear();
						_errorOut.Clear();
						_sampleOut.Clear();
						_ldDataRecords.PostEvent(_currentRequest);
					}
					catch (Exception e)
					{
						ErrorThis("ST_CollectLogs: Entry exception: " + e.Message);
					}

					return null;
                }
                //...................................................................
                case BEvent.EvType.Exit:
                {
                    TraceThis("ST_CollectLogs: Exit");
                    _tmrQuery.Disarm();
                    return null;
                }
                //...................................................................
				case BEvent.EvType.Private:
                {
	                if (ev.Id == (uint)AoExportMgr.PrivateEvIds.pExportLogDataCsv_Status)
                    {
						TraceThis("ST_CollectLogs: Private, pExportLogDataCsv_Status from:" + _currentRequest.StartTime.ToShortDateString() + " " + _currentRequest.StartTime.ToShortTimeString());

						AoExportMgr.ExportLogDataCsvStatusIndEv statusEvent = (AoExportMgr.ExportLogDataCsvStatusIndEv)ev;
						if (statusEvent.Status == HawkeyeError.eSuccess)
						{
							TraceThis("ST_CollectLogs: Timer - Sequence (if (stev.Status == HawkeyeError.eSuccess))");
							_auditOut = statusEvent.AuditEntries;
							_errorOut = statusEvent.ErrorEntries;
							_sampleOut = statusEvent.SampleEntries;
						}

						TraceThis("ST_CollectLogs: Private, pExportLog_Status, Trans => ST_SaveOutput");
						DoTransition(ST_SaveOutput);
	                    return null;
                    }
					break;
                }
                //...................................................................
                case BEvent.EvType.Init:
                case BEvent.EvType.Public:
				case BEvent.EvType.Timer:
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
		internal UInt32 SaveOutputDelayMs { get; set; } = BAppFW.kTIMER_PERIOD_MS;
		private State ST_SaveOutput(BEvent ev)
		{
			switch (ev.MyType)
			{
				//...................................................................
				case BEvent.EvType.Entry:
				{
					TraceThis("ST_SaveOutput Entry - " + SaveOutputDelayMs.ToString());
					_tmrTimeout.FireInSecs(kTIMEOUT_SECS, OrthoId);
					_tmrQuery.FireIn(SaveOutputDelayMs, OrthoId);
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
				case BEvent.EvType.Timer:
				{
					DebugThis("ST_SaveOutput: Timer, " + ev.Id);

					if (ev.Id == (uint)TimerIds.Timeout)
					{
						ErrorThis("ST_SaveOutput Timer - Timeout ^iReset");
						if (_currentRequest.StatusCB != null)
						{
							_currentRequest.StatusCB("","",(uint)HawkeyeError.eTimedout, 100);
						}
						Owner.PostInternalEvent((uint)PrivateEvIds.iReset);
						return null;
					}

					if (ev.Id == (uint)TimerIds.Query)
					{
							TraceThis("ST_SaveOutput: Timer");
							try
							{
								System.Data.DataTable dataTable = ExportModel.CreateLogDataKeysDataTable(_auditOut, _errorOut, _sampleOut);
								FileSystem.ExportDataTableWithKeysToFile_NoXlate(dataTable, _currentRequest.LocalExportFilepath, false, true);
							}
							catch (Exception e)
							{
								ErrorThis($"Failed to create CSV file: {_currentRequest.LocalExportFilepath}, Exception: {e.Message}");
								Owner.PostInternalEvent((uint)PrivateEvIds.iReset);
								return null;
							}

							if (_currentRequest.StatusCB != null)
							{
								TraceThis("ST_SaveOutput: Timer, calling StatusCB");
								_currentRequest.StatusCB(_currentRequest.LocalExportFilepath, _currentRequest.BulkDataId, (uint)GrpcService.ExportStatusEnum.EsReady, 100);
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

	#endregion

	#region Private_Methods
	#endregion

    }
}
