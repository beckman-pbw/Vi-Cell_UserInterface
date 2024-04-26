using HawkeyeCoreAPI.Interfaces;
using JetBrains.Annotations;
using log4net;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Runtime.InteropServices;

namespace HawkeyeCoreAPI
{
    public class SystemStatus : ISystemStatus
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        static extern void GetSystemStatus(out IntPtr status);

        [DllImport("HawkeyeCore.dll")]
        static extern void GetSystemStatusStr(out ScoutUtilities.Enums.SystemStatus status);

        [DllImport("HawkeyeCore.dll")]
        static extern void FreeSystemStatus(IntPtr status);

        [DllImport("HawkeyeCore.dll")]
        static extern void GetVersionInformation(out SystemVersion systemVersion);

        [DllImport("HawkeyeCore.dll")]
        static extern HawkeyeError GetSystemSecurityType(out SecurityType securityType);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SetSystemSecurityType(SecurityType securityType, string userName, string password);

        #endregion

        #region API_Calls

        public IntPtr GetSystemStatusAPI(ref SystemStatusData systemStatusData)
        {
            IntPtr intPtr; 
            GetSystemStatus(out intPtr);

            systemStatusData = (SystemStatusData)Marshal.PtrToStructure(intPtr, typeof(SystemStatusData));

            return intPtr;
        }

        public static void GetSystemStatusAPI(ref ScoutUtilities.Enums.SystemStatus systemStatus)
        {
            GetSystemStatusStr(out systemStatus);
        }

        public void FreeSystemStatusAPI(IntPtr ptrSystemStatus)
        {
            FreeSystemStatus(ptrSystemStatus);
        }

        public static void GetVersionInformationAPI(ref SystemVersion systemVersion)
        {
            GetVersionInformation(out systemVersion);
        }

        #region Internal API Calls

        internal static SecurityType GetSystemSecurityTypeAPI()
        {
            var result = GetSystemSecurityType(out var securityType);
            if (result != HawkeyeError.eSuccess)
            {
                Log.Warn($"GetSystemSecurityType failed: {result}");
            }
            return securityType;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        internal static HawkeyeError SetSystemSecurityTypeAPI(SecurityType securityType, string userName, string password)
        {
            return SetSystemSecurityType(securityType, userName, password);
        }

        #endregion

        #endregion
    }
}

