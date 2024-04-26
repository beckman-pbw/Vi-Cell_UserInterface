using System;
using log4net;
using ScoutModels.Admin;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using System.Collections.Generic;
using HawkeyeCoreAPI.Facade;
using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Events;

namespace ScoutModels
{
    public class LoginModel
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructor & Singleton Stuff

        private static LoginModel _instance;
        public static LoginModel Instance
        {
            get
            {
                if (_instance == null) _instance = new LoginModel();
                return _instance;
            }
        }

        private LoginModel() { }

        #endregion

        public LoginResult Login(string username, string password, out HawkeyeError hawkeyeResult)
        {
            hawkeyeResult = UserModel.LoginUser(username, password);
            if (hawkeyeResult == HawkeyeError.eSuccess)
            {
                CheckAndHandlePasswordExpiration();
                return LoginResult.NormalLoginSuccess;
            }
            else
            {
                if (hawkeyeResult == HawkeyeError.eTimedout) return LoginResult.UserHasBeenDisabled;
                return LoginResult.NormalLoginFailed;
            }
        }

        public LoginResult LoginSwapUser(string username, string password, SwapUserRole swapRole, out HawkeyeError hawkeyeResult)
        {            
            hawkeyeResult = UserModel.SwapUser(username, password, swapRole);
            if (hawkeyeResult == HawkeyeError.eSuccess)
                return LoginResult.SwapUserLoginSuccess;
            return LoginResult.SwapUserLoginFailed;
        }

        public LoginResult ValidateUserCredentials(string uname, string password, out HawkeyeError hawkeyeError)
        {
            hawkeyeError = UserModel.ValidateUserCredentials(uname, password);
            if (hawkeyeError.Equals(HawkeyeError.eSuccess))
                return LoginResult.CurrentUserLoginSuccess;
            return LoginResult.CurrentUserLoginFailed;
        }

        public LoginResult ValidateLocalAdminAccount(string uname, string password, out HawkeyeError hawkeyeError)
        {
            hawkeyeError = UserModel.ValidateLocalAdminAccount(uname, password);
            if (hawkeyeError.Equals(HawkeyeError.eSuccess))
                return LoginResult.AdminValidSuccess;
            return LoginResult.AdminValidFailed;
        }

        public LoginResult UnlockWithAdmin(string displayedUsername, string currentUsername, string password, out HawkeyeError hawkeyeError)
        {
            hawkeyeError = SettingsModel.AdministrativeUserEnable(displayedUsername, password, currentUsername);
            if (hawkeyeError.Equals(HawkeyeError.eSuccess))
                return LoginResult.AdminUnlockSuccess;
            return LoginResult.AdminUnlockFailed;
        }

        private void CheckAndHandlePasswordExpiration()
        {
            if (SystemStatusFacade.Instance.GetSecurity() != SecurityType.LocalSecurity)
                return;

            if (LoggedInUser.NoLoggedInUser())
            {
                Log.Warn("CheckAndHandlePasswordExpiration : LoggedInUser.CurrentUser is null.");
                return;
            }

            var isSilentAdmin = LoggedInUser.CurrentUserId?.Equals(ApplicationConstants.SilentAdmin) == true;
            if (LoggedInUser.CurrentUser.RoleID.Equals(UserPermissionLevel.eService) || isSilentAdmin)
            {
                return; // Either service or silent admin user is logged in
            }

            if (UserModel.IsPasswordExpired(LoggedInUser.CurrentUserId))
            {
                DialogEventBus.ChangePasswordDialog(this, new ChangePasswordEventArgs(LoggedInUser.CurrentUserId, ImageHelper.GetUserIconPath(LoggedInUser.CurrentUser), true));
                return;
            }

            //
            // @TODO - do we need to support warnings for password expiring soon?
            //
            //var changePwdDate = LoggedInUser.CurrentUser.PasswordChangeDate.Value;
            //if (changePwdDate.AddDays(-1).Equals(DateTime.Today))
            //{
            //    var tomorrow = Misc.ConvertToCustomLongDateTimeFormat(DateTime.Today.AddDays(1).Date.AddHours(23).AddMinutes(59).AddSeconds(59));
            //    var msg = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_PWDExpQuestion"), tomorrow);

            //    if (DialogEventBus.DialogBoxYesNo(this, msg) != true)
            //    {
            //        return;
            //    }

            //    DialogEventBus.ChangePasswordDialog(this, new ChangePasswordEventArgs(LoggedInUser.CurrentUserId, ImageHelper.GetUserIconPath(LoggedInUser.CurrentUser)));
            //}
        }
    }
}
