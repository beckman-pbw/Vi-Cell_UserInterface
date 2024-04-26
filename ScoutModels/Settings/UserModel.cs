using JetBrains.Annotations;
using log4net;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System;
using System.Collections.Generic;

namespace ScoutModels.Admin
{
    public class UserModel
    {
        #region Properties

        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public List<UserDomain> UserList { get; set; }

        #endregion

        #region Constructor

        public UserModel()
        {
            UserList = new List<UserDomain>(GetUserList());
        }

        #endregion

        #region Methods

        public static HawkeyeError LoginUser(string username, string password)
        {
            try
            {
                var result = LocalLoginUser(password, username);
                if (result.Equals(HawkeyeError.eSuccess))
                {
                    LoggedInUser.UserLogin(username);
                    MessageBus.Default.Publish(new Notification<UserDomain>(NotificationClasses.NewCurrentUser, username));
                    Log.Debug($"{username} has been logged in");
                }
                else 
                {
                    DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_MSGBOX_AcoountDisable")); // "The login was not successful..."
                }

                return result;
            }
            catch(Exception e)
            {
                Log.Error($"Error while logging in as user '{username}'", e);
                return HawkeyeError.eInvalidArgs;
            }
        }

        public static bool LoginSilentAdmin()
        {
            var hostPassword = GenerateHostPassword();
            if (string.IsNullOrEmpty(hostPassword)) return false;

            var result = LoginUser(ApplicationConstants.SilentAdmin, hostPassword);
            if (result.Equals(HawkeyeError.eSuccess))
            {
                LoggedInUser.UserLogin("");
                MessageBus.Default.Publish(new Notification<UserDomain>(NotificationClasses.NewCurrentUser, ApplicationConstants.SilentAdmin));
                return true;
            }
            return false;
        }

        public static HawkeyeError SwapUser(string username, string password, SwapUserRole swapRole)
        {
            var hawkeyeError = LocalSwapUser(username, password, swapRole);
            var result = hawkeyeError.Equals(HawkeyeError.eSuccess);
            if (result)
            {
                LoggedInUser.UserLogin(username);
                MessageBus.Default.Publish(new Notification<UserDomain>(NotificationClasses.NewCurrentUser, username));
            }

            return hawkeyeError;
        }

        private static UserPermissionLevel HandleGetUserRole(string username, UserPermissionLevel permission, HawkeyeError hawkeyeError)
        {
            if (!hawkeyeError.Equals(HawkeyeError.eSuccess))
            {
                Log.Error("GetUserRole:: hawkeyeError:" + hawkeyeError.ToString());
            }

            Log.Debug("GetUserRole:: hawkeyeError: " + hawkeyeError + ", username: " + username + ", permission: " + permission);
            return permission;
        }

        public static List<UserDomain> GetUsersForSelectionBoxes()
        {
            var userList = GetUserList();
            userList.Add(new UserDomain { UserID = LanguageResourceHelper.Get("LID_Label_All") });
            switch (LoggedInUser.CurrentUserId)
            {
                case ApplicationConstants.ServiceUser:
                    userList.Add(new UserDomain { UserID = ApplicationConstants.ServiceUser });
                    break;
                case ApplicationConstants.SilentAdmin:
                    userList.Add(new UserDomain { UserID = ApplicationConstants.SilentAdmin });
                    break;
            }

            return userList;
        }

        #endregion

        #region API Calls

        public static bool SetUserEmailAddress(string username, string email)
        {
            return HawkeyeCoreAPI.User.SetUserEmailApi(username, email);
        }

        public static bool SetUserComment(string username, string comment)
        {
            return HawkeyeCoreAPI.User.SetUserCommentApi(username, comment);
        }

        public static ScoutUtilities.Structs.UserRecord GetUserRecord(string username)
        {
            return HawkeyeCoreAPI.User.GetUserRecordApi(username);
        }

        public static HawkeyeError SetUserFastMode(string username, bool enabled)
        {
            return HawkeyeCoreAPI.User.SetUserFastModeApi(username, enabled);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ChangeMyPassword(string oldPassword, string newPassword)
        {
            var hawkeyeError = HawkeyeCoreAPI.User.ChangeMyPasswordAPI(oldPassword, newPassword);
            Log.Debug("ChangeMyPassword:: hawkeyeError: " + hawkeyeError);

            if (hawkeyeError.Equals(HawkeyeError.eSuccess))
            {
                Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_PasswordChange"));

                MessageBus.Default.Publish(new SystemMessageDomain
                {
                    IsMessageActive = true,
                    Message = LanguageResourceHelper.Get("LID_StatusBar_PasswordhasBeenChanged"),
                    MessageType = MessageType.Normal
                });
            }
            else
            {
                ApiHawkeyeMsgHelper.ErrorMyPassword(hawkeyeError);
            }

            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        private static HawkeyeError LocalLoginUser(string password, string userId)
        {
            var hawkeyeError = HawkeyeCoreAPI.User.LoginConsoleUserAPI(userId, password);

            Misc.LogOnHawkeyeError("LoginUser", hawkeyeError);
            return hawkeyeError;
        }

        public static UserPermissionLevel GetUserRole(string username = null)
        {
            var permission = UserPermissionLevel.eNormal;
            if (string.IsNullOrEmpty(username))
            {
                username = string.Empty;
                var hawkeyeError = HawkeyeCoreAPI.User.GetCurrentUserAPI(ref username, ref permission);
                return HandleGetUserRole(username, permission, hawkeyeError);
            }
            else
            {
                var hawkeyeError = HawkeyeCoreAPI.User.GetUserPermissionLevelAPI(username, ref permission);
                return HandleGetUserRole(username, permission, hawkeyeError);
            }			
        }

        public static string GetCurrentUserId()
        {
            var username = string.Empty;
            try
            {
                var permission = UserPermissionLevel.eNormal;
                var hawkeyeError = HawkeyeCoreAPI.User.GetCurrentUserAPI(ref username, ref permission);
                if (hawkeyeError != HawkeyeError.eSuccess)
                {
                    username = string.Empty;
                    Log.Error($"GetCurrentUser() result: {hawkeyeError}");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to {nameof(GetCurrentUserId)}.", e);
            }

            return username;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        private static HawkeyeError LocalSwapUser(string username, string password, SwapUserRole swapRole)
        {
            Log.Debug($"SwapUser:: username: {username}");
            var hawkeyeError = HawkeyeCoreAPI.User.SwapUserAPI(username, password, swapRole);
            Log.Debug($"SwapUserAPI:: hawkeyeError: {hawkeyeError}");
            return hawkeyeError;
        }

        public static void LogOutUser()
        {
            HawkeyeCoreAPI.User.LogoutConsoleUserAPI();
            LoggedInUser.UserLogout();
            MessageBus.Default.Publish(new Notification<UserDomain>(NotificationClasses.NewCurrentUser, ""));
        }

        public static bool IsPasswordExpired(string username)
        {
            return HawkeyeCoreAPI.User.IsPasswordExpiredAPI(username);
        }

        public static List<UserDomain> GetUserList()
        {
            var userLists = new List<UserDomain>();
            UInt32 numUsers = 0;
            var enableUserList = new List<string>();
            var enableDisableUserList = new List<string>();

            HawkeyeCoreAPI.User.GetUserListAPI(true, ref enableUserList, ref numUsers);
            HawkeyeCoreAPI.User.GetUserListAPI(false, ref enableDisableUserList, ref numUsers);

            enableDisableUserList.ForEach(user =>
            {
                if (enableUserList.Exists(x => x.Equals(user)))
                {
                    var userDomain = new UserDomain
                    {
                        UserID = user,
                        IsEnabled = true
                    };
                    userLists.Add(userDomain);
                }
                else
                {
                    var userDomain = new UserDomain
                    {
                        UserID = user,
                        IsEnabled = false
                    };
                    userLists.Add(userDomain);
                }
            });
            return userLists;
        }


        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ValidateMe(string password)
        {
            var hawkeyeError = HawkeyeCoreAPI.User.ValidateMeAPI(password);
            Log.Debug("ValidateMe:: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ValidateUserCredentials(string uname, string password)
        {
            var hawkeyeError = HawkeyeCoreAPI.User.ValidateUserCredentialsAPI(uname, password);
            Log.Debug("ValidateUserCredentials:: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ValidateLocalAdminAccount(string uname, string password)
        {
            var hawkeyeError = HawkeyeCoreAPI.User.ValidateLocalAdminAccountAPI(uname, password);
            Log.Debug("ValidateUserCredentials:: " + hawkeyeError);
            return hawkeyeError;
        }

        public static string GenerateHostPassword()
        {
            const string securityKey = @"""Phil, Dennis and Perry are pretty neat guys""";
            var hostPassword = HawkeyeCoreAPI.User.GenerateHostPasswordAPI(securityKey);
            return hostPassword;
        }

        public static void ShutDown()
        {
            HawkeyeCoreAPI.InitializeShutdown.ShutdownAPI();
        }

        public static bool IsShutdownComplete()
        {
            return HawkeyeCoreAPI.InitializeShutdown.IsShutdownCompleteAPI();
        }

        #endregion
    }
}
