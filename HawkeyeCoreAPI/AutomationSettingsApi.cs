using JetBrains.Annotations;
using log4net;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Runtime.InteropServices;

namespace HawkeyeCoreAPI
{
    public class AutomationSettingsApi
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError GetAutomationSettings(out IntPtr config); // config is AutomationConfig

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError SetAutomationSettings(IntPtr config); // config is AutomationConfig

        [DllImport(ApplicationConstants.DllName)]
        static extern void FreeAutomationSettings(IntPtr config); // config is AutomationConfig

        #endregion

        #region API_Calls

        public AutomationConfig GetAutomationSettingsApi()
        {
            try
            {
                var result = GetAutomationSettings(out var automationConfigIntPtr);
                if (result != HawkeyeError.eSuccess)
                {
                    Log.Warn($"GetAutomationSettingsApi:: GetAutomationSettings: {result}, returning default object");
                    return new AutomationConfig();
                }

                var autoConfig = (AutomationConfig) Marshal.PtrToStructure(automationConfigIntPtr, typeof(AutomationConfig));
                FreeAutomationSettings(automationConfigIntPtr);
                return autoConfig;
            }
            catch (Exception e)
            {
                Log.Error($"GetAutomationSettingsApi:: GetAutomationSettings: failed to get Automation Settings", e);
                return new AutomationConfig();
            }
        }

        public HawkeyeError SetAutomationSettingsApi(AutomationConfig config)
        {
            try
            {
                var size = Marshal.SizeOf(typeof(AutomationConfig));
                var autoConfigIntPtr = Marshal.AllocCoTaskMem(size);
                Marshal.StructureToPtr(config, autoConfigIntPtr, false);

                var result = SetAutomationSettings(autoConfigIntPtr);
                Marshal.FreeCoTaskMem(autoConfigIntPtr);

                if (result != HawkeyeError.eSuccess)
                {
                    Log.Warn($"SetAutomationSettingsApi: {result}");
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error($"SetAutomationSettingsApi: failed to set Automation settings, etting hawkeye failure to 'eNoneFound'", e);
                return HawkeyeError.eNoneFound;
            }
        }

        #endregion
    }
}