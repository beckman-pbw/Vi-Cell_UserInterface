using log4net;
using ScoutUtilities.Enums;
using System;
using System.Runtime.InteropServices;

namespace HawkeyeCoreAPI
{
    public static partial class InitializeShutdown
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport("HawkeyeCore.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void Initialize(out ushort instrumentType, bool withHardware = true);

        [DllImport("HawkeyeCore.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern InitializationState IsInitializationComplete();

        [DllImport("HawkeyeCore.dll")]
        static extern void Shutdown();

        /// <returns><c>true</c> if [is shutdown complete]; otherwise, <c>false</c>.</returns>
        /// byte is used as the return type since a C++ bool is a byte.
        [DllImport("HawkeyeCore.dll")]
        static extern byte IsShutdownComplete();

        [DllImport("HawkeyeCore.dll")]
        static extern void ShutdownOrReboot(ShutdownOrRebootEnum operation);

        #endregion


        #region API_Calls

        public static void InitializeAPI(out ushort instrumentType, bool isFromHardware)
        {
            Log.Info($"InitializeAPI:: isFromHardware: {isFromHardware}");
            Initialize(out instrumentType, isFromHardware);
        }

        public static InitializationState IsInitializationCompleteAPI()
        {
            return IsInitializationComplete();
        }

        public static void ShutdownAPI()
        {
            Log.Info($"ShutdownAPI:");
            Shutdown();
        }

        public static bool IsShutdownCompleteAPI()
        {
            return Convert.ToBoolean(IsShutdownComplete());
        }

        public static void ShutdownOrRebootAPI(ShutdownOrRebootEnum operation)
        {
	        ShutdownOrReboot(operation);
        }

        #endregion
    }
}
