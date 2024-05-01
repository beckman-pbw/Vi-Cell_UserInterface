using JetBrains.Annotations;
using log4net;
using ScoutDataAccessLayer.DAL;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutModels.Admin;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;


namespace ScoutModels.Settings
{
    public class SettingsModel
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IList<UserPermissionLevel> _roles;
        private IList<AnalysisDomain> _availableAnalyses;

        public UserModel UserModel { get; set; }

        public IList<AnalysisDomain> AvailableAnalyses
        {
            get
            {
                return _availableAnalyses ?? (_availableAnalyses = new List<AnalysisDomain>(AnalysisModel.GetAllAnalyses()));
            }
            set { _availableAnalyses = value; }
        }

        public IList<UserPermissionLevel> Roles
        {
            get { return _roles ?? (_roles = GetRoles()); }
            set { _roles = value; }
        }

        public SettingsModel()
        {
            UserModel = new UserModel();
        }

        public static List<UserPermissionLevel> GetRoles()
        {
            return Enum.GetValues(typeof(UserPermissionLevel)).Cast<UserPermissionLevel>()
                .Where(r => r.ToString().StartsWith("eAdministrator") ||
                            r.ToString().StartsWith("eNormal") ||
                            r.ToString().StartsWith("eElevated"))
                .Select(r => r)
                .ToList();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ChangeUserPermission(string username, UserPermissionLevel permission)
        {
            Log.Debug("ChangeUserPermission:: name: " + username + " & " + " permission: " + permission);
            var hawkeyeError = HawkeyeCoreAPI.User.ChangeUserPermissionAPI(username, permission);
            Log.Debug("ChangeUserPermission:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }


        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetUserDisplayName(string username, string displayName)
        {
            Log.Debug("SetUserDisplayName:: username: " + username + " displayName: " + displayName);
            var hawkeyeError = HawkeyeCoreAPI.User.SetUserDisplayNameAPI(username, displayName);
            Log.Debug("SetUserDisplayName:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }


        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RemoveUser(string username)
        {
            Log.Debug("RemoveUser:: username: " + username);
            var hawkeyeError = HawkeyeCoreAPI.User.RemoveUserAPI(username);
            Log.Debug("RemoveUser:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }


        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError AddUser(string userId, string displayName, string password, UserPermissionLevel permission)
        {
            Log.Debug("AddUser:: userId: " + userId + ", displayName " + displayName + ", permission: " + permission);
            var hawkeyeError = HawkeyeCoreAPI.User.AddUserAPI(userId, displayName, password, permission);
            Log.Debug("AddUser:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }


        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ChangeUserPassword(string username, string password)
        {
            Log.Debug("ChangeUserPassword:: username: " + username);
            var hawkeyeError = HawkeyeCoreAPI.User.ChangeUserPasswordAPI(username, password);
            Log.Debug("ChangeUserPassword:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }
   
        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetUserFolder(string name, string folder)
        {
            Log.Debug("SetUserFolder:: name: " + name + " folder: " + folder);
            var hawkeyeError = HawkeyeCoreAPI.User.SetUserFolderAPI(name, folder);
            Log.Debug("SetUserFolder:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError IsPasswordExpired(string name, out bool expired)
        {
            var hawkeyeError = HawkeyeCoreAPI.User.IsPasswordExpiredAPI(name, out expired);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError EnableUser(string name, bool enabled)
        {
            Log.Debug("EnableUser:: name: " + name + ", enabled: " + enabled);
            var hawkeyeError = HawkeyeCoreAPI.User.EnableUserAPI(name, enabled);
            Log.Debug("EnableUser:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError AdministrativeUserUnlock(string administrator_account, string administrator_password, string user_account)
        {
            var hawkeyeError = HawkeyeCoreAPI.User.AdministrativeUserUnlockAPI(administrator_account, administrator_password, user_account);
            Log.Debug("AdministrativeUserUnlock:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;

        }

        public static void SaveRunOptionSetting(string name, uint cellTypeIndex)
        {
            bool userFound = false;
            var result = XMLDataAccess.Instance.ReadConfigurationData<RunOptionSettingsDomain>(name,
                typeof(RunOptionSettingsDomain).Name.Replace("Domain", string.Empty), out userFound);
            result.DefaultBPQC = cellTypeIndex;
            XMLDataAccess.Instance.WriteToConfigurationXML(result, name, DateTime.Now);
        }

        public static void UseCarouselSimulation(bool state)
        {
	        HawkeyeCoreAPI.Configuration.UseCarouselSimulationAPI(state);
        }
    }
}
