using ApiProxies.Generic;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Admin;
using ScoutModels.Common;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System;
using System.Windows.Input;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class LoginViewModel : BaseDialogViewModel
    {

        public LoginViewModel(LoginEventArgs eventArgs, System.Windows.Window parentWindow) : base(eventArgs, parentWindow)
        {
            DisplayedUsername = eventArgs.DisplayedUsername;
            CurrentLoggedInUsername = eventArgs.CurrentLoggedInUsername;
            ReturnOnFirstFailure = eventArgs.ReturnOnFirstFailure;
            ReturnPassword = eventArgs.ReturnPassword;
            LoginResult = ScoutUtilities.CustomEventArgs.LoginResult.None;
            AdCfgVal = eventArgs.ActiveDirValArgs;
            // Set the login state last because it uses other members
            LoginState = eventArgs.LoginState;
            Message = eventArgs.Message == null ?
                LanguageResourceHelper.Get("LID_Label_LockMsg") :
                eventArgs.Message;
        }

        #region Properties

        public string CurrentLoggedInUsername { get; }
        public bool ReturnOnFirstFailure { get; }
        public bool ReturnPassword { get; }
        public string Message { get; set; }

        public AdCfgValidation AdCfgVal { get; set; }

        public LoginResult LoginResult
        {
            get { return GetProperty<LoginResult>(); }
            set { SetProperty(value); }
        }

        public LoginState LoginState
        {
            get { return GetProperty<LoginState>(); }
            private set
            {
                SetProperty(value);
                switch(value)
                {
                    case LoginState.AdminUnlock:
                        ShowAdminLoginOption = false;
                        UsernameIsLocked = false;
                        ShowLockedMessage = true;
                        ShowDialogTitleBar = false;
                        CanCancelLogin = false;
                        break;
                    case LoginState.LockScreen:
                        ShowAdminLoginOption = !CurrentLoggedInUsername.Equals(ScoutUtilities.Common.ApplicationConstants.ServiceUser);
                        UsernameIsLocked = true;
                        ShowLockedMessage = true;
                        ShowDialogTitleBar = false;
                        CanCancelLogin = false;
                        break;
                    case LoginState.PasswordReset:
                        ShowAdminLoginOption = false;
                        UsernameIsLocked = true;
                        ShowLockedMessage = false;
                        ShowDialogTitleBar = true;
                        CanCancelLogin = true;
                        break;
                    case LoginState.ValidateServiceUser:
                        ShowAdminLoginOption = false;
                        UsernameIsLocked = true;
                        ShowLockedMessage = false;
                        ShowDialogTitleBar = true;
                        CanCancelLogin = true;
                        break;
                    case LoginState.ValidateCurrentUserOnly:
                        ShowAdminLoginOption = false;
                        UsernameIsLocked = true;
                        ShowLockedMessage = false;
                        ShowDialogTitleBar = true;
                        CanCancelLogin = true;
                        break;
                    case LoginState.SecuritySettingsUpdate:
                    case LoginState.SecurityChangeToNone:
                        ShowAdminLoginOption = false;
                        UsernameIsLocked = false;
                        ShowLockedMessage = false;
                        ShowDialogTitleBar = true;
                        CanCancelLogin = true;
                        break;
                    case LoginState.SecurityChangeToLocal:
                    case LoginState.SecurityChangeToAD:
                        ShowAdminLoginOption = false;
                        UsernameIsLocked = false;
                        ShowLockedMessage = false;
                        ShowDialogTitleBar = true;
                        CanCancelLogin = true;
                        break;
                    case LoginState.AutomationLock:
                        if (LoggedInUser.CurrentUserRoleId != UserPermissionLevel.eAdministrator &&
                            LoggedInUser.CurrentUserRoleId != UserPermissionLevel.eService)
                            DisplayedUsername = String.Empty; //Don't want to populate username of normal user
                        ShowAdminLoginOption = false;
                        UsernameIsLocked = false;
                        ShowLockedMessage = true;
                        ShowDialogTitleBar = true;
                        CanCancelLogin = true;
                        break;
                }
            }
        }

        public bool CanCancelLogin
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                CancelCommand.RaiseCanExecuteChanged();
            }
        }

        public bool ShowLockedMessage
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsSilentAdminEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string DisplayedUsername
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Password
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool UsernameIsLocked
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool ShowAdminLoginOption
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        #region Admin Unlock Command

        private ICommand _adminUnlockCommand;
        public ICommand AdminUnlockCommand
        {
            get
            {
                if (_adminUnlockCommand == null) _adminUnlockCommand = new RelayCommand(PerformAdminUnlock);
                return _adminUnlockCommand;
            }
        }

        private void PerformAdminUnlock()
        {
            // Set the view to allow admin override authentication
            Password = string.Empty;
            LoginState = LoginState.AdminUnlock;
        }

        #endregion

        #region Accept Action

        protected override void OnAccept() // User is attempting to login/validate -- check credentials
        {
            // validate the properties/arguments
            if (string.IsNullOrEmpty(DisplayedUsername))
            {
                LoginResult = LoginResult.SwapUserLoginFailed;
                InformUserOfFailureOrClose(false, HawkeyeError.eValidationFailed);
                return; // making an API call with null is bad
            }

            var sameUser = DisplayedUsername.Equals(CurrentLoggedInUsername);

            if (string.IsNullOrEmpty(Password))
            {
                LoginResult = sameUser ? LoginResult.CurrentUserLoginFailed : LoginResult.SwapUserLoginFailed;
                InformUserOfFailureOrClose(false, HawkeyeError.eValidationFailed);
                return; // making an API call with null is bad
            }

            // check the credentials:
            switch (LoginState)
            {
                case LoginState.AdminUnlock:
                    UnlockWithAdmin();
                    break;

                case LoginState.PasswordReset:
                    DialogEventBus.DialogBoxOk(this, "Password Reset hasn't been implemented in LoginViewModel yet."); // todo: implement this
                    break;

                case LoginState.ValidateCurrentUserOnly:
                    ValidateUserCredentials(CurrentLoggedInUsername, Password);
                    break;

                case LoginState.ValidateServiceUser:
                    if (sameUser) ValidateUserCredentials(CurrentLoggedInUsername, Password);
                    else LoginSwapUser(SwapUserRole.eServiceOnly);
                    break;
                case LoginState.LockScreen:
                    if (sameUser) ValidateUserCredentials(CurrentLoggedInUsername, Password);
                    else LoginSwapUser(SwapUserRole.eServiceOrAdmin);
                    break;
                case LoginState.SecuritySettingsUpdate:
                case LoginState.SecurityChangeToNone:
                    if (sameUser) ValidateUserCredentials(CurrentLoggedInUsername, Password);
                    else LoginSwapUser(SwapUserRole.eAdminOnly);
                    break;
                case LoginState.SecurityChangeToLocal:
                    ValidateLocalAdminAccount();
                    break;
                case LoginState.SecurityChangeToAD:
                    ValidateADConfig();
                    break;
                case LoginState.AutomationLock:
                    if (UserModel.GetUserRole(DisplayedUsername) == UserPermissionLevel.eAdministrator ||
                        UserModel.GetUserRole(DisplayedUsername) == UserPermissionLevel.eService)
                    {
                        ValidateUserCredentials(DisplayedUsername, Password);
                    }
                    else
                        InformUserOfFailureOrClose(false, HawkeyeError.eNotPermittedByUser);
                    break;
                default:
                    DialogEventBus.DialogBoxOk(this, $"Unknown LoginState ({LoginState}) in LoginViewModel.");
                    break;
            }
        }

        #endregion

        #region Cancel Action

        public override bool CanCancel()
        {
            return CanCancelLogin;
        }

        #endregion

        #endregion

        private void UnlockWithAdmin()
        {
            LoginResult = LoginModel.Instance.UnlockWithAdmin(DisplayedUsername, CurrentLoggedInUsername, Password, out var hawkeyeError);
            if (LoginResult == LoginResult.AdminUnlockSuccess)
            {
                InformUserOfFailureOrClose(true, HawkeyeError.eSuccess);
            }
            else
            {
                // Sanitize the displayed error to limit security risk
                if (hawkeyeError == HawkeyeError.eNotPermittedByUser)
                    InformUserOfFailureOrClose(false, hawkeyeError);
                else
                    InformUserOfFailureOrClose(false, HawkeyeError.eValidationFailed);
            }
        }

        private void ValidateUserCredentials(string uname, string pword)
        {
            LoginResult = LoginModel.Instance.ValidateUserCredentials(uname, pword, out var hawkeyeResult);
            if (LoginResult == LoginResult.CurrentUserLoginSuccess)
            {
                InformUserOfFailureOrClose(true, HawkeyeError.eSuccess);
            }
            else
            {
                if ((hawkeyeResult == HawkeyeError.eTimedout) ||
                    (hawkeyeResult == HawkeyeError.eBusy))
                {
                    InformUserOfFailureOrClose(false, HawkeyeError.eNotPermittedAtThisTime);
                }
                else
                {
                    // Sanitize the displayed error to limit security risk
                    InformUserOfFailureOrClose(false, HawkeyeError.eValidationFailed);
                }
            }
        }
        private void LoginSwapUser(SwapUserRole swapRole)
        {
            try
            {
                LoginResult = LoginModel.Instance.LoginSwapUser(DisplayedUsername, Password, swapRole, out var hawkeyeError);
                if (LoginResult == LoginResult.SwapUserLoginSuccess)
                {
                    InformUserOfFailureOrClose(true, HawkeyeError.eSuccess);
                }
                else
                {
                    // Sanitize the displayed error to limit security risk
                    if (hawkeyeError == HawkeyeError.eNotPermittedByUser)
                        InformUserOfFailureOrClose(false, hawkeyeError);
                    else
                        InformUserOfFailureOrClose(false, HawkeyeError.eValidationFailed);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SWAP_USER_ERROR"));
                LoginResult = LoginResult.SwapUserLoginFailed;
                Close(null);
            }
        }

        /// <summary>
        /// Regardless of the current security mode, validate that the given credentials 
        /// are for a local, enabled, admin user. We validate the credentials before
        /// we allow the security mode to change. These same credentials are then known 
        /// to be valid in the new security mode before we make the change. 
        /// </summary>
        private void ValidateLocalAdminAccount()
        {
            try
            {
                LoginResult = LoginModel.Instance.ValidateLocalAdminAccount(DisplayedUsername, Password, out var hawkeyeError);
                if (LoginResult == LoginResult.AdminValidSuccess)
                {
                    InformUserOfFailureOrClose(true, HawkeyeError.eSuccess);
                }
                else
                {
                    // Sanitize the displayed error to limit security risk
                    if (hawkeyeError == HawkeyeError.eNotPermittedByUser)
                        InformUserOfFailureOrClose(false, hawkeyeError);
                    else if (hawkeyeError == HawkeyeError.eNotPermittedAtThisTime)
                        InformUserOfFailureOrClose(false, hawkeyeError);
                    else
                        InformUserOfFailureOrClose(false, HawkeyeError.eValidationFailed);
                }

            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SWAP_USER_ERROR"));
                LoginResult = LoginResult.AdminValidFailed;
                Close(null);
            }
        }

        /// <summary>
        /// Validate the AD configuration AND an admin account on the AD server. 
        /// Only change the security mode if the given credentials and config
        /// validate to an admin user if the given info was applied to the system. 
        /// These same credentials are then known to be valid in the new security mode before we make the change. 
        /// </summary>
        private void ValidateADConfig()
        {
            try
            {
                LoginResult = LoginResult.None;
                if (AdCfgVal != null)
                {
                    ScoutDomains.ActiveDirectoryConfigDomain adTestcfg = new ScoutDomains.ActiveDirectoryConfigDomain(AdCfgVal.server, (UInt16)AdCfgVal.port, AdCfgVal.domain);
                    if (HawkeyeCoreAPI.ActiveDirectory.ValidateActiveDirConfigAPI(adTestcfg, AdCfgVal.adminGroup, DisplayedUsername, Password))
                        LoginResult = LoginResult.AdminValidSuccess;
                }

                if (LoginResult == LoginResult.AdminValidSuccess)
                {
                    InformUserOfFailureOrClose(true, HawkeyeError.eSuccess);
                }
                else
                {
                    // Regarless of the reason - report validation failed not just user login failure
                    // The AD config could be invalid OR the user credentials could be invalid
                    Password = string.Empty;
                    ApiHawkeyeMsgHelper.ValidationError(HawkeyeError.eValidationFailed);
                    if (ReturnOnFirstFailure)
                    {
                        Close(false);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SWAP_USER_ERROR"));
                LoginResult = LoginResult.SwapUserLoginFailed;
                Close(null);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="loginAttemptWasSuccessful">The result of the login attempt</param>
        /// <param name="hawkeyeError"></param>
        /// <returns>Returns TRUE if we are closing the window.</returns>
        private bool InformUserOfFailureOrClose(bool? loginAttemptWasSuccessful, HawkeyeError hawkeyeError)
        {
            if (loginAttemptWasSuccessful == false && !ReturnOnFirstFailure)
            {
                Password = string.Empty;

                if (hawkeyeError != HawkeyeError.eSuccess) 
					ApiHawkeyeMsgHelper.ErrorValidateMe(hawkeyeError);
					
                return false;
            }

            Close(loginAttemptWasSuccessful);
            return true;
        }
    }
}
