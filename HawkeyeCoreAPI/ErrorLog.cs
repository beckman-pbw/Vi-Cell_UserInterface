using HawkeyeCoreAPI.Interfaces;
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

namespace HawkeyeCoreAPI
{
    public class ErrorLog : IErrorLog
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RetrieveInstrumentErrorLog(out UInt32 num_entries, out IntPtr log_entries);

        [DllImport("HawkeyeCore.dll")]
        static extern void FreeErrorLogEntry(IntPtr entries, UInt32 num_entries);

        [DllImport("HawkeyeCore.dll")]
        static extern void SystemErrorCodeToExpandedResourceStrings(
            UInt32 system_error_code,
            out IntPtr severityResourceKey,
            out IntPtr systemResourceKey, 
            out IntPtr subsystemResourceKey,
            out IntPtr instanceResourceKey, 
            out IntPtr failureModeResourceKey,
            out IntPtr cellHealthErrorCode);

        [DllImport("HawkeyeCore.dll")]
        static extern void SystemErrorCodeToExpandedStrings(
	        UInt32 system_error_code,
	        out IntPtr severityResourceKey,
	        out IntPtr systemResourceKey,
	        out IntPtr subsystemResourceKey,
	        out IntPtr instanceResourceKey,
	        out IntPtr failureModeResourceKey,
	        out IntPtr cellHealthErrorCode);

		[DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError ClearSystemErrorCode(UInt32 active_error);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RetrieveInstrumentErrorLogRange(UInt64 startTime, UInt64 endTime, out UInt32 num_entries, out IntPtr log_entries);

        #endregion

        #region API_Calls

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ClearSystemErrorCodeAPI(UInt32 active_error)
        {
            return ClearSystemErrorCode(active_error);
        }

        public void SystemErrorCodeToExpandedResourceStringsAPI(
            UInt32 system_error_code, 
            ref string severityResourceKey,
            ref string systemResourceKey, 
            ref string subsystemResourceKey, 
            ref string instanceResourceKey, 
            ref string failureModeResourceKey,
            ref string cellHealthErrorCodeKey)
        {
	        SystemErrorCodeToExpandedResourceStrings(system_error_code, 
		        out IntPtr severityPtr, 
		        out IntPtr systemPtr, 
		        out IntPtr subsystemPtr, 
		        out IntPtr instancePtr, 
		        out IntPtr failure_modePtr, 
		        out IntPtr cellHealthErrorCodePtr);

            severityResourceKey = severityPtr.ToSystemString();
            systemResourceKey = systemPtr.ToSystemString();
            subsystemResourceKey = subsystemPtr.ToSystemString();
            instanceResourceKey = instancePtr.ToSystemString();
            failureModeResourceKey = failure_modePtr.ToSystemString();
            // Not currently used for Resourced error messages: cellHealthErrorCodeKey = cellHealthErrorCodePtr.ToSystemString();

            GenericFree.FreeCharBufferAPI(severityPtr);
            GenericFree.FreeCharBufferAPI(systemPtr);
            GenericFree.FreeCharBufferAPI(subsystemPtr);
            GenericFree.FreeCharBufferAPI(instancePtr);
            GenericFree.FreeCharBufferAPI(failure_modePtr);
            GenericFree.FreeCharBufferAPI(cellHealthErrorCodePtr);
        }

        public void SystemErrorCodeToExpandedStringsAPI(
	        UInt32 system_error_code,
	        ref string severity,
	        ref string system,
	        ref string subsystem,
	        ref string instance,
	        ref string failureMode,
	        ref string cellHealthErrorCode)
        {
	        SystemErrorCodeToExpandedStrings(system_error_code, 
		        out IntPtr severityPtr, 
		        out IntPtr systemPtr, 
		        out IntPtr subsystemPtr, 
		        out IntPtr instancePtr, 
		        out IntPtr failure_modePtr, 
		        out IntPtr cellHealthErrorCodePtr);

	        severity = severityPtr.ToSystemString();
	        system = systemPtr.ToSystemString();
	        subsystem = subsystemPtr.ToSystemString();
	        instance = instancePtr.ToSystemString();
	        failureMode = failure_modePtr.ToSystemString();
	        cellHealthErrorCode = cellHealthErrorCodePtr.ToSystemString();

	        GenericFree.FreeCharBufferAPI(severityPtr);
	        GenericFree.FreeCharBufferAPI(systemPtr);
	        GenericFree.FreeCharBufferAPI(subsystemPtr);
	        GenericFree.FreeCharBufferAPI(instancePtr);
	        GenericFree.FreeCharBufferAPI(failure_modePtr);
	        GenericFree.FreeCharBufferAPI(cellHealthErrorCodePtr);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveInstrumentErrorLogRangeAPI(ulong startTime, ulong endTime, ref List<ErrorLogDomain> errorLogDomainList)
        {
            var errorLogList = new List<error_log_entry>();
            var hawkeyeStatus = RetrieveInstrumentErrorLogRange(startTime, endTime, out uint numCount, out IntPtr errorLogPtr);

            var ptrErrorLog = errorLogPtr;
            for (int i = 0; i < numCount; i++)
            {
                errorLogList.Add((error_log_entry)Marshal.PtrToStructure(ptrErrorLog, typeof(error_log_entry)));
                ptrErrorLog = new IntPtr(ptrErrorLog.ToInt64() + Marshal.SizeOf(typeof(error_log_entry)));
            }

            if (errorLogList.Count > 0)
            {
                errorLogDomainList = CreateErrorLogList(errorLogList);
            }


            if (numCount > 0)
                FreeErrorLogEntryAPI(errorLogPtr, numCount);

            return hawkeyeStatus;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RetrieveErrorLogAPI(ref List<ErrorLogDomain> errorLogDomainList)
        {
            var hawkeyeStatus = RetrieveInstrumentErrorLog(out uint numCount, out IntPtr errorLogPtr);
            var ptrErrorLog = errorLogPtr;

            var errorLogList = new List<error_log_entry>();
            for (int i = 0; i < numCount; i++)
            {
	            var errorLogEntry = (error_log_entry) Marshal.PtrToStructure(ptrErrorLog, typeof(error_log_entry));
	            errorLogList.Add(errorLogEntry);
                ptrErrorLog = new IntPtr(ptrErrorLog.ToInt64() + Marshal.SizeOf(typeof(error_log_entry)));
            }

            if (errorLogList.Count > 0)
            {
                errorLogDomainList = CreateErrorLogList(errorLogList);
            }

            if (numCount > 0)
                FreeErrorLogEntryAPI(errorLogPtr, numCount);

            return hawkeyeStatus;
        }

        #endregion

        #region Private Methods

        private static void FreeErrorLogEntryAPI(IntPtr entryPtr, uint numEntries)
        {
            FreeErrorLogEntry(entryPtr, numEntries);
        }

        private static List<ErrorLogDomain> CreateErrorLogList(List<error_log_entry> errorLogStr)
        {
            var ErrorLogList = new List<ErrorLogDomain>();
            errorLogStr.ForEach(errorLog =>
            {
                ErrorLogList.Add(ErrorLogDomain(errorLog));
            });

            return ErrorLogList;
        }

        private static ErrorLogDomain ErrorLogDomain(error_log_entry errLogStr)
        {
            var errorLogDomain = new ErrorLogDomain()
            {
                Timestamp = DateTimeConversionHelper.FromSecondUnixToDateTime(errLogStr.timestamps),
                Message = errLogStr.message.ToSystemString(),
                UserId = errLogStr.user_id.ToSystemString(),
                ErrorCode = (uint)errLogStr.error_code
            };

            return errorLogDomain;
        }

        #endregion

    }
}
