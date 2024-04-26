using JetBrains.Annotations;
using log4net;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ScoutDomains;
using ScoutUtilities;

namespace HawkeyeCoreAPI
{
	public static partial class SampleLog
	{
		private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#region API_Declarations

		[DllImport("HawkeyeCore.dll")]
		[MustUseReturnValue("Use HawkeyeError")]
		static extern HawkeyeError RetrieveSampleActivityLog(out UInt32 num_entries, out IntPtr log_entries);

		[DllImport("HawkeyeCore.dll")]
		[MustUseReturnValue("Use HawkeyeError")]
		static extern HawkeyeError RetrieveSampleActivityLogRange(UInt64 startTime, UInt64 endTime, out UInt32 num_entries, out IntPtr log_entries);

		[DllImport("HawkeyeCore.dll")]
		static extern void FreeSampleActivityEntry(IntPtr entries, UInt32 num_entries);

		#endregion

		#region API_Calls

		public static HawkeyeError RetrieveSampleActivityLogAPI(ref List<SampleActivityDomain> logEntryDomainList)
		{
			var hawkeyeStatus = RetrieveSampleActivityLog(out uint numCount, out IntPtr logEntryPtr);
			var ptrSampleLog = logEntryPtr;

			var sampleLogList = new List<sample_activity_entry>();
			for (int i = 0; i < numCount; i++)
			{
				var sampleLogEntry = (sample_activity_entry)Marshal.PtrToStructure(ptrSampleLog, typeof(sample_activity_entry));
				sampleLogList.Add(sampleLogEntry);
				ptrSampleLog = new IntPtr(ptrSampleLog.ToInt64() + Marshal.SizeOf(typeof(sample_activity_entry)));
			}

			if (sampleLogList.Count > 0)
			{
				logEntryDomainList = CreateSampleActivityList(sampleLogList);
			}

			if (numCount > 0)
				FreeSampleActivityEntryAPI(logEntryPtr, numCount);

			return hawkeyeStatus;
		}

		public static HawkeyeError RetrieveSampleActivityLogRangeAPI(ulong startTime, ulong endTime, ref List<SampleActivityDomain> logEntryDomainList)
		{
			var hawkeyeStatus = RetrieveSampleActivityLogRange(startTime, endTime, out uint numCount, out IntPtr logEntryPtr);
			var ptrSampleLog = logEntryPtr;
			var sampleLogList = new List<sample_activity_entry>();
			for (int i = 0; i < numCount; i++)
			{
				var sampleLogEntry = (sample_activity_entry)Marshal.PtrToStructure(ptrSampleLog, typeof(sample_activity_entry));
				sampleLogList.Add(sampleLogEntry);
				ptrSampleLog = new IntPtr(ptrSampleLog.ToInt64() + Marshal.SizeOf(typeof(sample_activity_entry)));
			}

			if (sampleLogList.Count > 0)
			{
				logEntryDomainList = CreateSampleActivityList(sampleLogList);
			}

			if (numCount > 0)
				FreeSampleActivityEntryAPI(logEntryPtr, numCount);

			return hawkeyeStatus;
		}

		public static void FreeSampleActivityEntryAPI(IntPtr entryPtr, uint numEntries)
		{
			FreeSampleActivityEntry(entryPtr, numEntries);
		}

		#endregion

		#region Private Methods

		private static List<SampleActivityDomain> CreateSampleActivityList(List<sample_activity_entry> sampleLogList)
		{
			var sampleActivityList = new List<SampleActivityDomain>();
			sampleLogList.ForEach(sampleLog =>
			{
				var sample = SampleActivityLogDomain(sampleLog);
				sampleActivityList.Add(sample);

				if (sample.WorkQueue.Count > 1)
				{
					foreach (var sam in sample.WorkQueue)
					{
						if (sam.Equals(sample.WorkQueue.First()))
							continue;
						var sampleDomain = new SampleActivityDomain()
						{
							Timestamp = sample.Timestamp,
							UserId = sample.UserId,
							AnalysisName = sam.AnalysisName,
							CellTypeName = sam.CellTypeName,
							SampleStatus = sam.SampleStatus,
							SampleLabel = sam.SampleLabel
						};
						sampleActivityList.Add(sampleDomain);
					}
				}
			});

			return sampleActivityList;
		}

		private static SampleActivityDomain SampleActivityLogDomain(sample_activity_entry sampleActivityEntry)
		{
			var sampleActivityLogDomain = new SampleActivityDomain()
			{
				Timestamp = DateTimeConversionHelper.FromSecondUnixToDateTime(sampleActivityEntry.timestamp),
				UserId = sampleActivityEntry.user_id.ToSystemString()
			};
			sampleActivityLogDomain.WorkQueue = new List<WorkQueueSampleLogDomain>(CreateWorkQueueDomain(sampleActivityEntry));
			if (sampleActivityLogDomain.WorkQueue.Count > 0)
			{
				sampleActivityLogDomain.SampleLabel = sampleActivityLogDomain.WorkQueue[0].SampleLabel;
				sampleActivityLogDomain.SampleStatus = sampleActivityLogDomain.WorkQueue[0].SampleStatus;
				sampleActivityLogDomain.CellTypeName = sampleActivityLogDomain.WorkQueue[0].CellTypeName;
				sampleActivityLogDomain.AnalysisName = sampleActivityLogDomain.WorkQueue[0].AnalysisName;
			}

			return sampleActivityLogDomain;
		}

		private static List<WorkQueueSampleLogDomain> CreateWorkQueueDomain(sample_activity_entry logEntry)
		{
			var listOfWorkQueue = new List<WorkQueueSampleLogDomain>();
			var workQueueEntry = new List<workqueue_sample_entry>();
			for (int i = 0; i < logEntry.number_of_samples; i++)
			{
				workQueueEntry.Add((workqueue_sample_entry)Marshal.PtrToStructure(logEntry.samples, typeof(workqueue_sample_entry)));
				logEntry.samples += Marshal.SizeOf(typeof(workqueue_sample_entry));
			}

			if (workQueueEntry.Count > 0)
			{
				workQueueEntry.ForEach(wq =>
				{
					listOfWorkQueue.Add(new WorkQueueSampleLogDomain()
					{
						SampleLabel = wq.sample_label.ToSystemString(),
						CellTypeName = wq.celltype_name.ToSystemString(),
						AnalysisName = wq.analysis_name.ToSystemString(),
						SampleStatus = wq.completion
					});
				});
			}

			return listOfWorkQueue;
		}
	}

	#endregion
}
