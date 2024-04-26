using ScoutDomains;
using ScoutLanguageResources;
using ScoutModels.Dialogs;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using System;
using System.Linq;
using System.Windows;
using ScoutUtilities.Events;
using System.DirectoryServices.AccountManagement;
using System.Web.Security;
using ApiProxies.Generic;
using HawkeyeCoreAPI.Facade;
using ScoutModels;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class ActiveDirectoryConfigDialogViewModel : BaseDialogViewModel
    {
        public ActiveDirectoryConfigDialogViewModel(ActiveDirectoryConfigDialogEventArgs args, Window parentWindow)
            : base(args, parentWindow)
        {
            _args = args;

            try
            {
                DialogTitle = LanguageResourceHelper.Get("LID_Header_ActiveDirectoryConfig");

                if (args.ActiveDirectoryDomain is ActiveDirectoryDomain ogSettings && ogSettings.ActiveDirConfig != null)
                {
                    ActiveDirServer = ogSettings.ActiveDirConfig.Server;
                    ActiveDirBaseDn = ogSettings.ActiveDirConfig.BaseDn;
                    ActiveDirPort = ogSettings.ActiveDirConfig.Port;

                    ActiveDirMapNormal = ogSettings.ActiveDirGroups.FirstOrDefault(g =>
                        g.UserRole == UserPermissionLevel.eNormal)?.ActiveDirGroupMap;
                    ActiveDirMapAdvanced = ogSettings.ActiveDirGroups.FirstOrDefault(g =>
                        g.UserRole == UserPermissionLevel.eElevated)?.ActiveDirGroupMap;
                    ActiveDirMapAdmin = ogSettings.ActiveDirGroups.FirstOrDefault(g =>
                        g.UserRole == UserPermissionLevel.eAdministrator)?.ActiveDirGroupMap;
                }
                else
                {
                    ActiveDirBaseDn = GetBaseDn();
                }

                if (ActiveDirPort == default(ushort))
                {
                    ActiveDirPort = 636; // LDAP SSL
                }

            }
            catch (Exception e)
            {
                Log.Error($"Failed to initialize the Active Directory config dialog", e);
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_Error_FailedToRetrieveActiveDirSettings"));
            }
        }

        #region Properties & Fields
        /// <summary>
        /// Need args to be able to return the ActiveDirectoryDomain instance.
        /// </summary>
        private ActiveDirectoryConfigDialogEventArgs _args;
        #endregion

        public LoginProc LoginProc { get; }

        public string ActiveDirServer
        {
            get => GetProperty<string>();
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public ushort ActiveDirPort
        {
            get => GetProperty<ushort>();
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public string ActiveDirBaseDn
        {
            get => GetProperty<string>();
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public string ActiveDirMapNormal
        {
            get => GetProperty<string>();
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public string ActiveDirMapAdvanced
        {
            get => GetProperty<string>();
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public string ActiveDirMapAdmin
        {
            get => GetProperty<string>();
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        #region Commands

        public string GetBaseDn()
        {
            try
            {
                using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
                {
                    var baseDn = context.ConnectedServer;
                    if (baseDn != null)
                        return baseDn;
                    return string.Empty;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to get Base DN", e);
                return string.Empty;
            }
        }

        public override bool CanAccept()
        {
            return ActiveDirPort > 0 &&
                   !string.IsNullOrEmpty(ActiveDirServer) &&
                   !string.IsNullOrEmpty(ActiveDirBaseDn) &&
                   !string.IsNullOrEmpty(ActiveDirMapNormal) &&
                   !string.IsNullOrEmpty(ActiveDirMapAdvanced) &&
                   !string.IsNullOrEmpty(ActiveDirMapAdmin);
        }

        protected override void OnAccept()
        {
            try
            {
                var result = false;
                var activeDirDomain = new ActiveDirectoryDomain(ActiveDirServer, ActiveDirPort, ActiveDirBaseDn,
                    ActiveDirMapNormal, ActiveDirMapAdvanced, ActiveDirMapAdmin);

                // We need to validate the given configuration
                AdCfgValidation cfgVal = new AdCfgValidation
                {
                    domain = activeDirDomain.ActiveDirConfig.BaseDn,
                    server = activeDirDomain.ActiveDirConfig.Server,
                    port = activeDirDomain.ActiveDirConfig.Port,
                    adminGroup = ActiveDirMapAdmin
                };

                // Launch the login dialog passing in the AD config to validate
                if (ValidateUser(cfgVal))
                {
                    _args.ActiveDirectoryDomain = activeDirDomain;
                    _args.Username = ActiveDirectoryUser;
                    _args.Password = ActiveDirectoryPassword;

                    // Do not call base OnAccept() as it will do another security check.
                    Close(true);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to Accept Active Directory changes", e);
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_Error_FailedToSaveActiveDirSettings"));
            }
        }

        /// <summary>
        /// Update the security settings and optionally change security type. 
        /// If the security type changes, the current user is logged out
        /// </summary>
        /// <param name="adCfgVal"></param>
        /// <returns>True on success, False on failure</returns>
        private bool ValidateUser(AdCfgValidation adCfgVal)
        {
            try
            {
                var username = LoggedInUser.CurrentUserId;
                LoginState loginState = LoginState.SecurityChangeToAD;

                //
                // Setup and then show the login dialog
                //
                var clearedUsername = string.Empty;
                var args = new LoginEventArgs(clearedUsername, LoggedInUser.CurrentUserId, loginState, DialogLocation.CenterApp, false, true, adCfgVal);
                var result = DialogEventBus.Login(this, args);

                if ((result == LoginResult.SwapUserLoginSuccess) ||
                    (result == LoginResult.AdminValidSuccess) ||
                    (result == LoginResult.CurrentUserLoginSuccess))
                {
                    ActiveDirectoryUser = args.DisplayedUsername;
                    ActiveDirectoryPassword = args.Password;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SecurityChangeFailed"));
                return false;
            }
        }

        public string ActiveDirectoryUser { get; set; }

        public string ActiveDirectoryPassword { get; set; }

        #endregion
    }
}