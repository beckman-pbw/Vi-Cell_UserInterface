using JetBrains.Annotations;
using log4net;
using ScoutDomains;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HawkeyeCoreAPI.Interfaces;

namespace HawkeyeCoreAPI
{
    public class AuditLog : IAuditLog
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RetrieveAuditTrailLog(out UInt32 num_entries, out IntPtr log_entries);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RetrieveAuditTrailLogRange(UInt64 startTime, UInt64 endTime, out UInt32 num_entries, out IntPtr log_entries);

        [DllImport("HawkeyeCore.dll")]
        static extern void FreeAuditLogEntry(IntPtr entries, UInt32 num_entries);

        [DllImport("HawkeyeCore.dll")]
        static extern void WriteToAuditLog(string username, audit_event_type type, string resource);

        #endregion

        #region API_Calls

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveAuditTrailLogAPI(ref List<AuditLogDomain> logEntryDomainList)
        {
            var hawkeyeStatus = RetrieveAuditTrailLog(out uint numCount, out IntPtr logEntryPtr);
            var ptrAuditLog = logEntryPtr;

			var auditLogList = new List<audit_log_entry>();
            for (int i = 0; i < numCount; i++)
            {
                var auditLogEntry = (audit_log_entry) Marshal.PtrToStructure(ptrAuditLog, typeof(audit_log_entry));
                auditLogList.Add(auditLogEntry);
                ptrAuditLog = new IntPtr(ptrAuditLog.ToInt64() + Marshal.SizeOf(typeof(audit_log_entry)));
            }

            if (auditLogList.Count > 0)
            {
                logEntryDomainList = CreateAuditLogList(auditLogList);
            }

            if (numCount > 0)
                FreeAuditLogEntryAPI(logEntryPtr, numCount);

            return hawkeyeStatus;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveAuditTrailLogRangeAPI(ulong startTime, ulong endTime, ref List<AuditLogDomain> logEntryDomainList)
        {
            
            var hawkeyeStatus = RetrieveAuditTrailLogRange(startTime, endTime, out uint numCount, out IntPtr logEntryPtr);
            var ptrAuditLog = logEntryPtr;
            
            var auditLogList = new List<audit_log_entry>();
            for (int i = 0; i < numCount; i++)
            {
                auditLogList.Add((audit_log_entry)Marshal.PtrToStructure(ptrAuditLog, typeof(audit_log_entry)));
                ptrAuditLog = new IntPtr(ptrAuditLog.ToInt64() + Marshal.SizeOf(typeof(audit_log_entry)));
            }

            if (auditLogList.Count > 0)
            {
                logEntryDomainList = CreateAuditLogList(auditLogList);
            }

            if (numCount > 0)
                FreeAuditLogEntryAPI(logEntryPtr, numCount);
            return hawkeyeStatus;
        }

        #endregion

        #region Private Methods

		private static void FreeAuditLogEntryAPI(IntPtr entryPtr, uint numEntries)
		{
			FreeAuditLogEntry(entryPtr, numEntries);
		}

		private static List<AuditLogDomain> CreateAuditLogList(List<audit_log_entry> auditLogEntry)
        {
            var auditLogList = new List<AuditLogDomain>();
            auditLogEntry.ForEach(auditLog =>
            {
                auditLogList.Add(AuditLogDomain(auditLog));
            });

            return auditLogList;
        }

        private static AuditLogDomain AuditLogDomain(audit_log_entry auditLogStr)
        {
            var auditLogDomain = new AuditLogDomain()
            {
	            Timestamp = DateTimeConversionHelper.FromSecondUnixToDateTime(auditLogStr.timestamp),
                Message = auditLogStr.event_message.ToSystemString(),
                UserId = auditLogStr.active_user_id.ToSystemString(),
                AuditEventType = auditLogStr.event_type
            };

            return auditLogDomain;
        }

        public void WriteToAuditLogAPI(string username, audit_event_type type, string resource)
        {
	        WriteToAuditLog(username, type, resource);
        }

		#endregion

	}
}
