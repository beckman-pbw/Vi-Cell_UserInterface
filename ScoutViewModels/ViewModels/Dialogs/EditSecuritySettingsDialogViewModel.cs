using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using ScoutLanguageResources;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System.Windows;
using System.Windows.Forms;
using ApiProxies.Generic;
using HawkeyeCoreAPI.Facade;
using ScoutDomains;
using ScoutModels;
using ScoutModels.Admin;
using ScoutModels.Common;
using ScoutModels.Dialogs;
using ScoutUtilities.Common;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class EditSecuritySettingsDialogViewModel : BaseDialogViewModel
    {
        public EditSecuritySettingsDialogViewModel(EditSecuritySettingsDialogEventArgs args, Window parentWindow)
            : base(args, parentWindow)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            DialogTitle = LanguageResourceHelper.Get("LID_Label_SecuritySettings");

            _originalSecurityType = args.SelectedSecurityType;
            _originalInactivityTimeout = args.InactivityTimeoutMinutes;
            _originalPasswordExpiry = args.PasswordChangeDays;
            PasswordExpiry = args.PasswordChangeDays;
            InactivityTimeout = args.InactivityTimeoutMinutes;

            SecurityTypes = new List<SecurityTypeViewModel>();
            _securityTypeLookup = new Dictionary<SecurityType, SecurityTypeViewModel>();
            var noSecurity = new SecurityTypeViewModel(SecurityType.NoSecurity);
            _securityTypeLookup[SecurityType.NoSecurity] = noSecurity;
            SecurityTypes.Add(noSecurity);
            var localSecurity = new SecurityTypeViewModel(SecurityType.LocalSecurity);
            _securityTypeLookup[SecurityType.LocalSecurity] = localSecurity;
            SecurityTypes.Add(localSecurity);
            _activeDirectory = new ActiveDirectorySecurityTypeViewModel(SecurityType.ActiveDirectory, this);
            _securityTypeLookup[SecurityType.ActiveDirectory] = _activeDirectory;
            SecurityTypes.Add(_activeDirectory);

            // Get the ActiveDirectoryDomain values first or when you set the SelectedSecurityType
            ActiveDirectoryDomain = ActiveDirectoryModel.GetActiveDirectorySettings();
            _originalActiveDirectoryDomain = new ActiveDirectoryDomain(ActiveDirectoryDomain);

            // ReSharper disable once PossibleNullReferenceException
            SelectedSecurityType = SecurityTypes.FirstOrDefault(t => t.SecurityType.Equals(args.SelectedSecurityType)).SecurityType;
        }

        #region Properties & Fields

        private readonly SecurityType _originalSecurityType;
        private readonly int _originalInactivityTimeout;
        private readonly int _originalPasswordExpiry;
        private readonly ActiveDirectorySecurityTypeViewModel _activeDirectory;
        private readonly Dictionary<SecurityType, SecurityTypeViewModel> _securityTypeLookup;
        private readonly ActiveDirectoryDomain _originalActiveDirectoryDomain;
        private ActiveDirectoryDomain _activeDirectoryDomain;

        public ActiveDirectoryDomain ActiveDirectoryDomain
        {
            get => _activeDirectoryDomain;
            set
            {
                _activeDirectoryDomain = value;
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public int InactivityTimeout
        {
            get => GetProperty<int>();
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
                _activeDirectory?.ActiveDirConfigCommand.RaiseCanExecuteChanged();
            }
        }

        public int PasswordExpiry
        {
            get => GetProperty<int>();
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public List<SecurityTypeViewModel> SecurityTypes
        {
            get => GetProperty<List<SecurityTypeViewModel>>();
            private set => SetProperty(value);
        }

        public SecurityType SelectedSecurityType
        {
            get => GetProperty<SecurityType>();
            set
            {
                SetProperty(value);
                _activeDirectory.ShowButton = false;
                _securityTypeLookup[value].IsChecked = true;
                switch (value)
                {
                    case SecurityType.ActiveDirectory:
                        ShowAutoSignOut = true;
                        ShowPasswordExpiry = false;
                        PasswordExpiry = _originalPasswordExpiry; //this doesn't get saved, it's just for display
                        if (null == ActiveDirectoryDomain || ! ActiveDirectoryDomain.IsPopulated())
                        {
                            _activeDirectory.OpenActiveDirDialog();
                        }
                        break;
                    case SecurityType.LocalSecurity:
                        ShowAutoSignOut = true;
                        ShowPasswordExpiry = true;
                        break;
                    case SecurityType.NoSecurity:
                        ShowAutoSignOut = false;
                        ShowPasswordExpiry = false;
                        InactivityTimeout = _originalInactivityTimeout; //this doesn't get saved, it's just for display
                        PasswordExpiry = _originalPasswordExpiry; //this doesn't get saved, it's just for display
                        break;
                }

                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public bool ShowAutoSignOut
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public bool ShowPasswordExpiry
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public string ActiveDirectoryUser { get; set; }

        public string ActiveDirectoryPassword { get; set; }

        #endregion

        #region Commands

        #region Accept Command

        public override bool CanAccept()
        {
            return (SelectedSecurityType != _originalSecurityType ||
                    InactivityTimeout != _originalInactivityTimeout ||
                    PasswordExpiry != _originalPasswordExpiry ||
                    SecurityType.ActiveDirectory == SelectedSecurityType && ActiveDirectoryDomain != null && ActiveDirectoryDomain.IsPopulated() && !_originalActiveDirectoryDomain.Equals(ActiveDirectoryDomain)) &&
                   IsInactivityTimeoutValid() && IsPasswordExpiryValid();
        }

        protected override void OnAccept()
        {
            if (SecurityType.ActiveDirectory == SelectedSecurityType)
            {
                var result = false;
                AdCfgValidation cfgVal = new AdCfgValidation
                {
                    domain = ActiveDirectoryDomain.ActiveDirConfig.BaseDn,
                    server = ActiveDirectoryDomain.ActiveDirConfig.Server,
                    port = ActiveDirectoryDomain.ActiveDirConfig.Port,
                    adminGroup = ActiveDirectoryDomain.ActiveDirMapAdmin
                };

                if (ActiveDirectoryModel.SetActiveDirectorySettings(ActiveDirectoryDomain) &&
                    ValidateUserAndUpdateSecuritySettings(cfgVal))
                {
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_SecuritySuccessful"));
                    result = true;
                }
                Close(result);
            }
            else if (ValidateUserAndUpdateSecuritySettings(null))
            {
                    // we're done here -- mark as success and return
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_SecuritySuccessful"));
                    base.OnAccept();
            }
        }

        #endregion

        #endregion

        #region Private Methods

        internal bool IsInactivityTimeoutValid()
        {
            return InactivityTimeout >= ApplicationConstants.MinimumInactivityTimeoutMins
                    && InactivityTimeout <= ApplicationConstants.MaximumInactivityTimeoutMins;
        }

        private bool IsPasswordExpiryValid()
        {
            return PasswordExpiry >= ApplicationConstants.MinimumPasswordExpirationDays
                    && PasswordExpiry <= ApplicationConstants.MaximumPasswordExpirationDays;
        }

        /// <summary>
        /// Update the security settings and optionally change security type. 
        /// If the security type changes, the current user is logged out
        /// </summary>
        /// <param name="adCfgVal"></param>
        /// <returns>True on success, False on failure</returns>
        internal bool ValidateUserAndUpdateSecuritySettings(AdCfgValidation adCfgVal)
        {
            try
            {
                string uname = LoggedInUser.CurrentUserId;
                LoginState loginState = LoginState.SecuritySettingsUpdate;
                if (_originalSecurityType != SelectedSecurityType)
                {
                    if (_originalSecurityType == SecurityType.NoSecurity)
                        uname = ""; // Clear the username - asking for an admin in the new security mode

                    if (SelectedSecurityType == SecurityType.ActiveDirectory)
                    {
                        loginState = LoginState.SecurityChangeToAD;
                        uname = ""; // Clear the username - asking for an admin in the new security mode
                    }
                    else if (SelectedSecurityType == SecurityType.NoSecurity)
                    {
                        loginState = LoginState.SecurityChangeToNone;
                    }
                    else if (SelectedSecurityType == SecurityType.LocalSecurity)
                    {
                        loginState = LoginState.SecurityChangeToLocal;
                        uname = ""; // Clear the username - asking for an admin in the new security mode
                    }
                }
                else
                {
                    // Even when not changing security type, force AD config to be validated
                    if (SelectedSecurityType == SecurityType.ActiveDirectory)
                    {
                        loginState = LoginState.SecurityChangeToAD;
                    }
                }

                if (SelectedSecurityType != SecurityType.ActiveDirectory || string.IsNullOrWhiteSpace(ActiveDirectoryUser) || string.IsNullOrWhiteSpace(ActiveDirectoryPassword))
                {
                    //
                    // Setup and then show the login dialog for No Security or Local Security
                    //
                    var args = new LoginEventArgs(uname, LoggedInUser.CurrentUserId, loginState,
                        DialogLocation.CenterApp, false, true, adCfgVal);
                    var result = DialogEventBus.Login(this, args);

                    if ((result == LoginResult.SwapUserLoginSuccess) ||
                        (result == LoginResult.AdminValidSuccess) ||
                        (result == LoginResult.CurrentUserLoginSuccess))
                    {
                        var callResult = SaveSecuritySettings(SelectedSecurityType,
                            (_originalSecurityType != SelectedSecurityType), args.DisplayedUsername, args.Password);
                        if (!callResult)
                        {
                            // reset the value:
                            SelectedSecurityType = SystemStatusFacade.Instance.GetSecurity();
                            return false;
                        }

                        return true;
                    }
                }
                else
                {
                    // For Active Directory - no login dialog
                    var adminGroup = ActiveDirectoryDomain.ActiveDirGroups.First(g => g.UserRole == UserPermissionLevel.eAdministrator)?.ActiveDirGroupMap;

                    if (HawkeyeCoreAPI.ActiveDirectory.ValidateActiveDirConfigAPI(ActiveDirectoryDomain.ActiveDirConfig, adminGroup, ActiveDirectoryUser, ActiveDirectoryPassword))
                    {
                        var callResult = SaveSecuritySettings(SelectedSecurityType, (_originalSecurityType != SelectedSecurityType), ActiveDirectoryUser, ActiveDirectoryPassword);
                        if (!callResult)
                        {
                            // reset the value:
                            SelectedSecurityType = SystemStatusFacade.Instance.GetSecurity();
                            return false;
                        }

                        return true;
                    }
                }

                // reset the value:
                return false;
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SecurityChangeFailed"));
            }
            return false;
        }

        /// <summary>
        /// Save security settings and optionally change the security type. 
        /// </summary>
        /// <param name="securityType">The target security type - may be changing</param>
        /// <param name="changingSecurity">Indcicates the security mode is changing</param>
        /// <param name="userName">username of a valid admin in the target security mode. When changing to no security this is the current user</param>
        /// <param name="password">the password for the given user</param>
        /// <returns></returns>
        private bool SaveSecuritySettings(SecurityType securityType, bool changingSecurity, string userName, string password)
        {
            // 
            // By the time we get here, we know the following:
            // The currently logged in user has been validated as an admin in the current security mode.
            // If we are changing security modes, the given username and password have been validated in the new security mode.
            // If switching to AD security mode, not only have the credentials been validated but the given user
            // would validate to an admin using the Role to Group map.
            //
            var apiResult = HawkeyeError.eSuccess; 

            // Save the settings regardless of changing security type or not
            // Save them before we change security type
            ChangeInactiveTimeout();
            ChangePasswordExpiry();

            if (changingSecurity)
            {
                // The backend logs the user out and loads the user list for the new security mode
                if (SystemStatusFacade.Instance.SetSecurity(securityType, userName, password, out apiResult))
                {
                    PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_SystemSecuritySettingChanged"));
                }
            }

            if (apiResult.Equals(HawkeyeError.eSuccess))
            {
                if (changingSecurity)
                {
                    // Although the backend logs the user out, 
                    // we should call logout just to be sure everything is cleaned up properly. 
                    // The backend will ignore extra logout calls 
                    UserModel.LogOutUser();
                    MainWindowViewModel.Instance.OnLogOut();
                    //
                    // Do NOT attempt to login here - the AD config has not been set yet. 
                    // When switching to no security it's okay
                    //
                    if (securityType == SecurityType.NoSecurity)
                    {
                        MainWindowViewModel.Instance.OnSilentAdminLogin();
                    }
                }
                return true;
            }
            else if (apiResult.Equals(HawkeyeError.eValidationFailed))
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX__SecurityMsg"));
                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX__SecurityMsg"));
            }
            else
            {
                DisplayErrorDialogByApi(apiResult, LanguageResourceHelper.Get("LID_Label_SecuritySettings"));
            }
            return false;
        }

        private bool ChangePasswordExpiry()
        {
            if (_originalPasswordExpiry == PasswordExpiry) return true;

            var passwordExpireStatue = SecuritySettingsModel.SetUserPasswordExpiration((ushort)PasswordExpiry);
            if (passwordExpireStatue.Equals(HawkeyeError.eSuccess))
            {
                return true;
            }
            else
            {
                ApiHawkeyeMsgHelper.ErrorInvalid(passwordExpireStatue, LanguageResourceHelper.Get("LID_Label_PassworsExpire"));
                return false;
            }
        }

        private bool ChangeInactiveTimeout()
        {
            if (_originalInactivityTimeout == InactivityTimeout) return true;

            var result = SecuritySettingsModel.SetUserInactivityTimeout((ushort)InactivityTimeout);
            if (result.Equals(HawkeyeError.eSuccess))
            {
                MessageBus.Default.Publish(new Notification(MessageToken.UpdateInactivityTimeout));
                return true;
            }
            else
            {
                DisplayErrorDialogByApi(result, LanguageResourceHelper.Get("LID_Label_InActivityTimeout"));
                return false;
            }
        }

        #endregion

        public void SetSelectedSecurityType(string content)
        {
            SelectedSecurityType = SecurityTypes.First(t => t.Name.Equals(content)).SecurityType;
        }
    }
}