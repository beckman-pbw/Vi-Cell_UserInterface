using System;

namespace HawkeyeCoreAPI.Interfaces
{
    public interface IErrorLog
    {
        void SystemErrorCodeToExpandedResourceStringsAPI(
            UInt32 system_error_code,
            ref string severityResourceKey,
            ref string systemResourceKey,
            ref string subsystemResourceKey,
            ref string instanceResourceKey,
            ref string failureModeResourceKey,
            ref string cellHealthErrorCodeKey);

        void SystemErrorCodeToExpandedStringsAPI(
	        UInt32 system_error_code,
	        ref string severityResourceKey,
	        ref string systemResourceKey,
	        ref string subsystemResourceKey,
	        ref string instanceResourceKey,
	        ref string failureModeResourceKey,
	        ref string cellHealthErrorCodeKey);
    }
}