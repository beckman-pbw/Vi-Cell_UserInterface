using log4net;
using Microsoft.Win32;
using System;

namespace ScoutModels.Common
{
    public class SystemEventsHandler
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
	        Log.Info("SystemEvents_SessionSwitch:: event: " + e.Reason);
        }

        public void OnPowerChange(object s, PowerModeChangedEventArgs e)
        {
	        Log.Info("OnPowerChange:: event: " + e.Mode);
        }

        public void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionEndReasons.Logoff:
                    break;
                case SessionEndReasons.SystemShutdown:
                    Log.Info("SystemEvents_SessionEnding:: event: " + e.Reason);
                    HawkeyeCoreAPI.InitializeShutdown.ShutdownAPI();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}