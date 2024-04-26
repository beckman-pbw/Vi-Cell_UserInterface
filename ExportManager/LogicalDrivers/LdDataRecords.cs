using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using HawkeyeCoreAPI;
using ScoutDomains;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Delegate;
using ScoutUtilities.Structs;


using BAFW;

namespace ExportManager
{
	// **********************************************************************
	public class LdDataRecords : BLogicalDriver
	{

		#region API_Declarations

		[DllImport("HawkeyeCore.dll")]
		static extern HawkeyeError Export_Start(
			string username, string password,
			IntPtr rs_uuid_list, UInt32 num_uuid,
			string outPath,
			eExportImages
			exportImages,
			UInt16 export_nth_image);

		[DllImport("HawkeyeCore.dll")]
		static extern HawkeyeError Export_NextMetaData(UInt32 index, UInt32 delayms);

		[DllImport("HawkeyeCore.dll")]
		static extern HawkeyeError Export_IsStorageAvailable();

		[DllImport("HawkeyeCore.dll")]
		static extern HawkeyeError Export_ArchiveData(string filename, out IntPtr outname);

		[DllImport("HawkeyeCore.dll")]
		static extern HawkeyeError Export_Cleanup(bool removeFile);

		[DllImport("HawkeyeCore.dll")]
		static extern HawkeyeError DeleteSampleRecord(
			string username, string password,
			IntPtr wqi_uuidlist,
			UInt32 num_uuid,
			bool retain_results_and_first_image,
			[MarshalAs(UnmanagedType.FunctionPtr)] delete_sample_record_callback onDeleteCompletion);

		[DllImport("HawkeyeCore.dll")]
		static extern void GetSystemStatus(out IntPtr status);

		[DllImport("HawkeyeCore.dll")]
		static extern void FreeSystemStatus(IntPtr status);

		[DllImport("HawkeyeCore.dll")]
		static extern HawkeyeError RetrieveAuditLog(out UInt32 num_entries, out IntPtr log_entries);

		[DllImport("HawkeyeCore.dll")]
		static extern HawkeyeError RetrieveAuditTrailLogRange(UInt64 startTime, UInt64 endTime, out UInt32 num_entries, out IntPtr log_entries);

		[DllImport("HawkeyeCore.dll")]
		static extern HawkeyeError RetrieveErrorLog(out UInt32 num_entries, out IntPtr log_entries);

		[DllImport("HawkeyeCore.dll")]
		static extern HawkeyeError RetrieveInstrumentErrorLogRange(UInt64 startTime, UInt64 endTime, out UInt32 num_entries, out IntPtr log_entries);

		[DllImport("HawkeyeCore.dll")]
		static extern HawkeyeError RetrieveSampleActivityLog(out UInt32 num_entries, out IntPtr log_entries);

		[DllImport("HawkeyeCore.dll")]
		static extern HawkeyeError RetrieveSampleActivityLogRange(UInt64 startTime, UInt64 endTime, out UInt32 num_entries, out IntPtr log_entries);

		[DllImport("HawkeyeCore.dll")]
		static extern void FreeAuditLogEntry(IntPtr entries, UInt32 num_entries);

		#endregion


		static AoExportMgr _parent = null;

		#region Construct_Destruct
		// **********************************************************************
		public LdDataRecords(AoExportMgr parent)
		{
			if (_parent == null)
			{
				_parent = parent;
			}
			_tmrQuery = new BTimer(GetEventQueue(), (uint)TimerIds.Query);
			StartThread("LdDataRecords");
			TraceThis("Started");
		}

		// **********************************************************************
		~LdDataRecords()
		{
			Shutdown();
		}

		// **********************************************************************
		public override void Shutdown()
		{
			base.Shutdown();
		}
		#endregion

		#region Timers
		private BTimer _tmrQuery;
		// ***********************************************************************
		private enum TimerIds
		{
			Query = 0,
		}
		#endregion

		#region Trace_Debug
		public static byte LogSubSysId = 0;
		private const string kMODULE_NAME = "LdDataRecords";
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

		#region Parent_API
		// **********************************************************************
		internal void StartPolling()
		{
			BPrivateEvent ev = new BPrivateEvent((uint)PrivateEvIds.Start_Polling);
			PostEvent(ev);
		}

		// **********************************************************************
		internal void StopPolling()
		{
			BPrivateEvent ev = new BPrivateEvent((uint)PrivateEvIds.Stop_Polling);
			PostEvent(ev);
		}

		// **********************************************************************
		internal void RequestStartExport(
			string username,
				string password,
				List<uuidDLL> resultSummaryIds,
				string outPath,
				eExportImages exportImages,
				UInt16 exportNthImage)
		{
			TraceThis("Request Start");
			Export_StartReqEv startEv = new Export_StartReqEv(
				username,
				 password,
				 resultSummaryIds,
				 outPath,
				 exportImages,
				 exportNthImage);
			PostEvent(startEv);
		}


		// **********************************************************************
		internal void RequestExportNextMetaData(UInt32 sampleIndex, UInt32 delayms)
		{
			ExportNextReqEv ev = new ExportNextReqEv(sampleIndex, delayms);
			PostEvent(ev);
		}

		// **********************************************************************
		internal void RequestExportIsStorageAvailable()
		{
			BPrivateEvent ev = new BPrivateEvent((uint)PrivateEvIds.Export_IsSpaceAvailable);
			PostEvent(ev);
		}

		// **********************************************************************
		internal void RequestExportSaveOutput(string filename)
		{
			SaveOutputReqEv ev = new SaveOutputReqEv(filename);
			PostEvent(ev);
		}

		// **********************************************************************
		internal void RequestExportCleanup(bool removeFiles)
		{
			BPrivateEvent ev = new BPrivateEvent((uint)PrivateEvIds.Export_Cleanup, (uint)(removeFiles ? 1 : 0));
			PostEvent(ev);
		}

		// **********************************************************************
		internal void RequestDeleteRecord(
			string username,
				string password,
				uuidDLL resultSummaryId,
				bool keepFirst)
		{
			TraceThis("Request Delete");
			DeleteRecordReqEv delEv = new DeleteRecordReqEv(
				username,
				 password,
				 resultSummaryId,
				 keepFirst);
			PostEvent(delEv);
		}
		#endregion

		#region Private_Events
		// **********************************************************************
		private enum PrivateEvIds
		{
			Export_Start = 0,
			Export_NextMeta,
			Export_IsSpaceAvailable,
			Export_SaveOutput,
			Export_Cleanup,
			Delete_Record,
			Start_Polling,
			Stop_Polling
		}

		// **********************************************************************
		private class Export_StartReqEv : BPrivateEvent
		{
			public Export_StartReqEv(
				string username,
				string password,
				List<uuidDLL> resultSummaryIds,
				string outPath,
				eExportImages exportImages,
				UInt16 exportNthImage
				)
				: base((uint)PrivateEvIds.Export_Start)
			{
				Username = username;
				Password = password;
				ResultSummaryIds = resultSummaryIds.ToList();
				OutPath = outPath;
				ExportImages = exportImages;
				ExportNthImage = exportNthImage;
			}
			public string Username { get; set; } = "";
			public string Password { get; set; } = "";
			public List<uuidDLL> ResultSummaryIds { get; set; } = new List<uuidDLL>();
			public string OutPath { get; set; } = "";
			public eExportImages ExportImages { get; set; } = eExportImages.eAll;
			public UInt16 ExportNthImage { get; set; } = 1;
		}

		// **********************************************************************
		public class ExportNextReqEv : BPrivateEvent
		{
			public ExportNextReqEv(UInt32 sampleIndex, UInt32 delayms)
				: base((uint)PrivateEvIds.Export_NextMeta)
			{
				SampleIndex = sampleIndex;
				DelayMs = delayms;
			}

			public UInt32 SampleIndex { get; set; } = 0;
			public UInt32 DelayMs { get; set; } = 1;
		}

		// **********************************************************************
		public class SaveOutputReqEv : BPrivateEvent
		{
			public SaveOutputReqEv(string filename)
				: base((uint)PrivateEvIds.Export_SaveOutput)
			{
				Filename = filename;
			}

			public string Filename { get; set; } = "";
		}


		// **********************************************************************
		public class DeleteRecordReqEv : BPrivateEvent
		{
			public DeleteRecordReqEv(
				string username,
				string password,
				uuidDLL sampleId,
				bool keepFirstImage)
				: base((uint)PrivateEvIds.Delete_Record)
			{
				Username = username;
				Password = password;
				SampleId = sampleId;
				KeepFirstImage = keepFirstImage;
			}

			~DeleteRecordReqEv()
			{
			}

			public string Username { get; set; } = "";
			public string Password { get; set; } = "";
			public uuidDLL SampleId { get; set; } = new uuidDLL();
			public bool KeepFirstImage { get; set; } = false;
		}
		#endregion

		#region Helper_Functions
		// ******************************************************************
		private HawkeyeError Start_Export(Export_StartReqEv req)
		{
			var structSize = Marshal.SizeOf(typeof(uuidDLL));
			IntPtr arrayPtr = Marshal.AllocHGlobal(structSize * req.ResultSummaryIds.Count);
			IntPtr iterator = arrayPtr;
			for (int i = 0; i < req.ResultSummaryIds.Count; i++)
			{
				Marshal.StructureToPtr(req.ResultSummaryIds[i], iterator, false);
				iterator += structSize;
			}
			var res = Export_Start(req.Username, req.Password, arrayPtr, (uint)req.ResultSummaryIds.Count, req.OutPath, req.ExportImages, req.ExportNthImage);
			Marshal.FreeCoTaskMem(arrayPtr);
			if (res != HawkeyeError.eSuccess)
			{
				ErrorThis("Start_Export failed " + res.ToString());
			}
			return res;
		}

		// ******************************************************************
		private HawkeyeError SendRequestDeleteRecord(DeleteRecordReqEv req)
		{
			delete_sample_record_callback OnDeleteCB = (status, uuid) =>
			{
				EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Trace, "delete_sample_record_callback status: " + status.ToString(), LogSubSysId);
				try
				{
					_parent.ReportDeleteStatus(status, uuid);
				}
				catch (Exception e)
				{
					EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Error, "delete_sample_record_callback exception " + e.Message, LogSubSysId);
				}
			};

			var structSize = Marshal.SizeOf(typeof(uuidDLL));
			IntPtr arrayPtr = Marshal.AllocHGlobal(structSize);
			IntPtr iterator = arrayPtr;
			Marshal.StructureToPtr(req.SampleId, iterator, false);
			var res = DeleteSampleRecord(req.Username, req.Password, arrayPtr, (uint)1, req.KeepFirstImage, OnDeleteCB);
			Marshal.FreeCoTaskMem(arrayPtr);
			return res;
		}

		// **********************************************************************
		void UpdateSystemStatus()
		{
			try
			{
				IntPtr intPtr;
				GetSystemStatus(out intPtr);
				if (IntPtr.Zero == intPtr)
				{
					ErrorThis("UpdateSystemStatus GetSystemStatus() returned null");
					return;
				}
				try
				{
					var systemStatusData = (SystemStatusData)Marshal.PtrToStructure(intPtr, typeof(SystemStatusData));
					EvSystemStatusInd.Publish(systemStatusData);
				}
				catch (Exception e)
				{
					ErrorThis("UpdateSystemStatus publish exception " + e.Message);
				}
				FreeSystemStatus(intPtr);
				intPtr = IntPtr.Zero;
			}
			catch (Exception e)
			{
				ErrorThis("UpdateSystemStatus exception " + e.Message);
			}
		}
		#endregion

		private const int kPOLL_PERIOD_MS = 5100;
		private const int kFIRST_POLL_PERIOD_MS = 27000;
		// **********************************************************************
		protected override void ProcessEvent(BEvent ev)
		{
			switch (ev.MyType)
			{
				//...................................................................
				case BEvent.EvType.Public:
				{
					switch (ev.Id)
					{
						case (int)PubEvIds.ExportMgr_StartLogs:
						{
							TraceThis("ProcessEvent: Public, ExportMgr_StartLogs");

							List<AuditLogDomain> auditEntries = new List<AuditLogDomain>();
							var auditRes = HawkeyeCoreAPI.AuditLog.RetrieveAuditTrailLogAPI(ref auditEntries);

							List<ErrorLogDomain> errorEntries = new List<ErrorLogDomain>();
							var errRes = HawkeyeCoreAPI.ErrorLog.RetrieveErrorLogAPI(ref errorEntries);

							if ((auditRes == HawkeyeError.eSuccess) && (errRes == HawkeyeError.eSuccess))
							{
								_parent.ReportExportLogsStatus(HawkeyeError.eSuccess, auditEntries, errorEntries);
							}
							else
							{
								_parent.ReportExportLogsStatus(HawkeyeError.eStorageFault, auditEntries, errorEntries);
							}

							return;
						}
						case (int)PubEvIds.ExportMgr_StartLogDataExport:
						{
							TraceThis("ProcessEvent: Public, ExportMgr_StartLogDataExport");
							ExportManager.EvExportLogDataCsvReq stev = (ExportManager.EvExportLogDataCsvReq)ev;
							List<AuditLogDomain> auditEntries = new List<AuditLogDomain>();

							var startTimeLong = DateTimeConversionHelper.DateTimeToUnixSecondRounded(stev.StartTime);
							var endTimeLong = DateTimeConversionHelper.DateTimeToEndOfDayUnixSecondRounded(stev.EndTime);

							var auditRes = HawkeyeCoreAPI.AuditLog.RetrieveAuditTrailLogRangeAPI(startTimeLong, endTimeLong, ref auditEntries);

							List<ErrorLogDomain> errorEntries = new List<ErrorLogDomain>();
							var errRes = HawkeyeCoreAPI.ErrorLog.RetrieveInstrumentErrorLogRangeAPI(startTimeLong, endTimeLong, ref errorEntries);

							List<SampleActivityDomain> sampleEntries = new List<SampleActivityDomain>();
							var sampleRes = HawkeyeCoreAPI.SampleLog.RetrieveSampleActivityLogRangeAPI(startTimeLong, endTimeLong, ref sampleEntries);

							HawkeyeError he = HawkeyeError.eSuccess;

							if (auditRes != HawkeyeError.eSuccess ||
								errRes != HawkeyeError.eSuccess ||
								sampleRes != HawkeyeError.eSuccess)
							{
								he = HawkeyeError.eStorageFault;
							}



							_parent.ReportExportLogsStatus(he, auditEntries, errorEntries, sampleEntries);

							return;
						}
					}
					break;
				}
				//...................................................................
				case BEvent.EvType.Timer:
				{
					UpdateSystemStatus();
					_tmrQuery.FireIn(kPOLL_PERIOD_MS);
					break;
				}
				//...................................................................
				case BEvent.EvType.Private:
				{
					try
					{
						switch (ev.Id)
						{
							// ++++++++++++++++++++++++++++++++++++++++++
							case (uint)PrivateEvIds.Start_Polling:
							{
								TraceThis("ProcessEvent: Private, Start_Polling - fire in " + kFIRST_POLL_PERIOD_MS.ToString());
								_tmrQuery.FireIn(kFIRST_POLL_PERIOD_MS);
								return;
							}
							// ++++++++++++++++++++++++++++++++++++++++++
							case (uint)PrivateEvIds.Stop_Polling:
							{
								TraceThis("ProcessEvent: Private, Stop_Polling");
								_tmrQuery.Disarm();
								return;
							}
							// ++++++++++++++++++++++++++++++++++++++++++
							case (uint)PrivateEvIds.Export_Start:
							{
								Export_StartReqEv startEv = (Export_StartReqEv)ev;
								TraceThis("ProcessEvent: Private, Export_Start");
								var res = Start_Export(startEv);
								if (res == HawkeyeError.eSuccess)
									_parent.ReportExportOfflineStatus(AoExportMgr.OfflineExportStatusIndEv.ReqStatus.StartOk);
								else
									_parent.ReportExportOfflineStatus(AoExportMgr.OfflineExportStatusIndEv.ReqStatus.StartError);
								return;
							}
							// ++++++++++++++++++++++++++++++++++++++++++
							case (uint)PrivateEvIds.Export_NextMeta:
							{
								ExportNextReqEv nextev = (ExportNextReqEv)ev;
								TraceThis("ProcessEvent: Private, Export_NextMeta");
								var res = Export_NextMetaData(nextev.SampleIndex, nextev.DelayMs);
								if (res == HawkeyeError.eSuccess)
									_parent.ReportExportOfflineStatus(AoExportMgr.OfflineExportStatusIndEv.ReqStatus.CollectMetaOk);
								else
									_parent.ReportExportOfflineStatus(AoExportMgr.OfflineExportStatusIndEv.ReqStatus.CollectMetaError);
								return;
							}
							// ++++++++++++++++++++++++++++++++++++++++++
							case (uint)PrivateEvIds.Export_IsSpaceAvailable:
							{
								TraceThis("ProcessEvent: Private, Export_IsSpaceAvailable");
								var res = Export_IsStorageAvailable();
								if (res == HawkeyeError.eSuccess)
									_parent.ReportExportOfflineStatus(AoExportMgr.OfflineExportStatusIndEv.ReqStatus.VerifySpaceOk);
								else
									_parent.ReportExportOfflineStatus(AoExportMgr.OfflineExportStatusIndEv.ReqStatus.VerifySpaceError);
								return;
							}
							// ++++++++++++++++++++++++++++++++++++++++++
							case (uint)PrivateEvIds.Export_SaveOutput:
							{
								TraceThis("ProcessEvent: Private, Export_SaveOutput");
								SaveOutputReqEv saveev = (SaveOutputReqEv)ev;

								IntPtr buffer_name;
								var res = Export_ArchiveData(saveev.Filename, out buffer_name);
								if (res == HawkeyeError.eSuccess)
								{
									string outname = buffer_name.ToSystemString();
									GenericFree.FreeCharBufferAPI(buffer_name);
									_parent.ReportExportOfflineStatus(AoExportMgr.OfflineExportStatusIndEv.ReqStatus.SaveOk, outname);
								}
								else
								{
									_parent.ReportExportOfflineStatus(AoExportMgr.OfflineExportStatusIndEv.ReqStatus.SaveError);
								}
								return;
							}
							// ++++++++++++++++++++++++++++++++++++++++++
							case (uint)PrivateEvIds.Export_Cleanup:
							{
								TraceThis("ProcessEvent: Private, Export_Cleanup");
								BPrivateEvent privev = (BPrivateEvent)ev;
								var res = Export_Cleanup(privev.AppBool);
								if (res == HawkeyeError.eSuccess)
									_parent.ReportExportOfflineStatus(AoExportMgr.OfflineExportStatusIndEv.ReqStatus.CleanupOk);
								else
									_parent.ReportExportOfflineStatus(AoExportMgr.OfflineExportStatusIndEv.ReqStatus.CleanupError);
								return;
							}
							// ++++++++++++++++++++++++++++++++++++++++++
							case (uint)PrivateEvIds.Delete_Record:
							{
								DeleteRecordReqEv req = (DeleteRecordReqEv)ev;
								TraceThis("ProcessEvent: Private, Delete_Record ID: " + req.SampleId.ToString());
								var res = SendRequestDeleteRecord(req);
								if (res != HawkeyeError.eSuccess)
								{
									_parent.ReportDeleteStatus(res, req.SampleId);
								}
								return;
							}
						}
					}
					catch (Exception e)
					{
						EvAppLogReq.Publish(kMODULE_NAME, EvAppLogReq.LogLevel.Error, "ProcessEvent Exception: " + e.Message);
					}
					break;
				}
				//...................................................................
				case BEvent.EvType.Entry:
				case BEvent.EvType.Exit:
				case BEvent.EvType.Init:
				case BEvent.EvType.None:
				{
					break;
				}
			}
		}
	}


}
