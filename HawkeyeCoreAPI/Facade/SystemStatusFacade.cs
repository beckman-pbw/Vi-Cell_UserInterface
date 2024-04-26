using System;
using System.Reflection;
using System.Runtime.InteropServices;
using log4net;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;

namespace HawkeyeCoreAPI.Facade
{
    public class SystemStatusFacade
    {
        #region Singleton Stuff

        private static readonly object _instanceLock = new object();

        private static SystemStatusFacade _instance;
        public static SystemStatusFacade Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null) _instance = new SystemStatusFacade();
                    }
                }

                return _instance;
            }
        }

        private SystemStatusFacade()
        {
            _securitySettingRetrieved = false;
            _securitySetting = SecurityType.LocalSecurity; // the default
        }

        #endregion

        #region Properties & Fields

        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private object _securitySettingLock = new object();
        private bool _securitySettingRetrieved;
        private SecurityType _securitySetting;

        #endregion

        #region Public Methods

        public bool SetSecurity(SecurityType newSecurityType, string userName, string password, out HawkeyeError hawkeyeError)
        {
            hawkeyeError = SetSystemSecurityType(newSecurityType, userName, password);
            
            if (hawkeyeError == HawkeyeError.eSuccess)
            {
                lock (_securitySettingLock)
                {
                    _securitySetting = newSecurityType;
                }
                
                MessageBus.Default.Publish(new Notification<SecurityType>(newSecurityType, MessageToken.SecuritySettingsChanged));
                return true;
            }

            return false;
        }

        public SecurityType GetSecurity()
        {
            if (!_securitySettingRetrieved)
            {
                if (GetSystemSecurityType(out var securityType))
                {
                    lock (_securitySettingLock)
                    {
                        _securitySetting = securityType;
                    }

                    _securitySettingRetrieved = true;
                    MessageBus.Default.Publish(new Notification<SecurityType>(securityType, MessageToken.SecuritySettingsChanged));
                }
            }

            lock (_securitySettingLock)
            {
                return _securitySetting;
            }
        }

        #endregion

        #region Private Methods

        private static bool GetSystemSecurityType(out SecurityType securityType)
        {
            try
            {
                securityType = SystemStatus.GetSystemSecurityTypeAPI();
                Log.Debug($"securityType : {securityType}");
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to get security type.", e);
                securityType = SecurityType.LocalSecurity;
                return false;
            }
        }

        private static HawkeyeError SetSystemSecurityType(SecurityType securityType, string userName, string password)
        {
            try
            {
                var hawkeyeError = SystemStatus.SetSystemSecurityTypeAPI(securityType, userName, password);
                Log.Debug($"SetSystemSecurityType({securityType}):: hawkeyeError : " + hawkeyeError);
                return hawkeyeError;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to set security type. securityType: {securityType}, username: {userName}", e);
                return HawkeyeError.eNoneFound;
            }
        }

        #endregion
    }
}