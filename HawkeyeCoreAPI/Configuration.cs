using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using ScoutUtilities;
using ScoutUtilities.Enums;
using ScoutUtilities.Common;

namespace HawkeyeCoreAPI
{
    public static partial class Configuration
    {
        #region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError ExportInstrumentConfiguration(string username, string password, IntPtr filename);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError ImportInstrumentConfiguration(string username, string password, IntPtr filename);

        [DllImport("HawkeyeCore.dll")]
        static extern byte SystemHasData();

        [DllImport("HawkeyeCore.dll")]
        static extern void UseCarouselSimulation(bool state);

        #endregion


        #region API_Calls

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ExportInstrumentConfigurationAPI(string username, string password, string filename)
        {
            var stringPtr = filename.ToIntPtr();
            var status = ExportInstrumentConfiguration(username, password, stringPtr);
            stringPtr.ReleaseIntPtr();
            return status;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ImportInstrumentConfigurationAPI(string username, string password, string filename)
        {
            var stringPtr = filename.ToIntPtr();
            var status = ImportInstrumentConfiguration(username, password, stringPtr);
            stringPtr.ReleaseIntPtr();
            return status;
        }

        public static bool SystemHasDataAPI()
        {
            return Misc.ByteToBool(SystemHasData());
        }

        public static void UseCarouselSimulationAPI(bool state)
        {
	        UseCarouselSimulation (state);
        }

        #endregion


        #region Private Methods

        #endregion

    }
}
