using System.Diagnostics;
using HawkeyeCoreAPI;
using ScoutModels.Interfaces;
using ScoutUtilities;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ScoutModels.Settings
{
    public class AutomationSettingsService : IAutomationSettingsService
    {
        private readonly AutomationSettingsApi _automationSettingsApi;

        public AutomationSettingsService()
        {
            _automationSettingsApi = new AutomationSettingsApi();
        }

        public AutomationConfig GetAutomationConfig()
        {
            var config = _automationSettingsApi.GetAutomationSettingsApi();
            return config;
        }

        public bool IsAutomationEnabled()
        {
            return Misc.ByteToBool(_automationSettingsApi.GetAutomationSettingsApi().AutomationIsEnabled);
        }
        
        public HawkeyeError SaveAutomationConfig(bool automationIsEnabled, bool acupIsEnabled, uint port)
        {
            var config = new AutomationConfig (automationIsEnabled, acupIsEnabled, port);
            return SaveAutomationConfig(config);
        }

        public HawkeyeError SaveAutomationConfig(AutomationConfig config)
        {
            return _automationSettingsApi.SetAutomationSettingsApi(config);
        }

        
        /// <summary>
        /// TRUE if isEnabled is false
        /// TRUE if isEnabled is true AND port is > 0
        /// FALSE otherwise
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool IsValid(AutomationConfig config)
        {
            return IsValid(Misc.ByteToBool(config.AutomationIsEnabled), Misc.ByteToBool(config.ACupIsEnabled), config.Port);
        }

		/// <summary>
		/// TRUE if isAutomationEnabled is false
		/// TRUE if isAutomationEnabled is true AND port is > 0
		/// FALSE otherwise
		/// </summary>
		/// <param name="isEnabled"></param>
		/// <param name="port"></param>
		/// <returns></returns>
		public bool IsValid(bool isAutomationEnabled, bool isACupEnabled, uint port)
        {
            if (!isAutomationEnabled)
                return true;
            
            if (isAutomationEnabled && port > 0)
                return true;
            
            return false;
        }

        /// <summary>
        /// Service users can turn automation ON if port > 0.
        /// Service users can turn automation OFF.
        /// Admin users can turn automation OFF.
        /// </summary>
        /// <param name="newConfig"></param>
        /// <param name="userIsAdmin"></param>
        /// <param name="userIsService"></param>
        /// <returns>TRUE if the config settings given allow a Save operation</returns>
        public bool CanSaveAutomationConfig(AutomationConfig newConfig, bool userIsAdmin, bool userIsService)
        {
            var newIsAutomationEnabled = Misc.ByteToBool(newConfig.AutomationIsEnabled);
			var newIsACupEnabled = Misc.ByteToBool(newConfig.ACupIsEnabled);
			var newPort = newConfig.Port;
            return CanSaveAutomationConfig(newIsAutomationEnabled, newIsACupEnabled, newPort, userIsAdmin, userIsService);
        }

        /// <summary>
        /// Service users can turn automation ON if port > 0.
        /// Service users can turn automation OFF.
        /// Admin users can turn automation OFF.
        /// Admin users can turn A-Cup OFF.
        /// </summary>
        /// <param name="newIsEnabled"></param>
        /// <param name="newPort"></param>
        /// <param name="userIsAdmin"></param>
        /// <param name="userIsService"></param>
        /// <returns>TRUE if the config settings given allow a Save operation</returns>
        public bool CanSaveAutomationConfig(bool newIsAutomationEnabled, bool newIsACupEnabled, uint newPort, bool userIsAdmin, bool userIsService)
        {
            if (!IsValid(newIsAutomationEnabled, newIsACupEnabled, newPort))
                return false;

            if (userIsService)
                return true;

            if (!newIsAutomationEnabled && userIsAdmin)
                return true;

            if (!newIsACupEnabled && userIsAdmin)
                return true;

            return false;
        }
    }
}