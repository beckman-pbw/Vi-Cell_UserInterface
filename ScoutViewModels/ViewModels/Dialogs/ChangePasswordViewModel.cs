using ApiProxies.Generic;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Admin;
using ScoutModels.Common;
using ScoutModels.Interfaces;
using ScoutModels.Settings;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class ChangePasswordViewModel : BaseDialogViewModel
    {
        #region Properties

        private readonly ISmtpSettingsService _smtpSettingsModel;

        public string Username
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public string OldPassword
        {
            get { return GetProperty<string>(); }
            set 
            { 
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public string NewPassword
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public string NewPasswordConfirm
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public bool UserMustChangePassword
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion

        public ChangePasswordViewModel(ChangePasswordEventArgs args, System.Windows.Window parentWindow,
            ISmtpSettingsService smtpSettingsService = null)
            : base(args, parentWindow)
        {
            ShowDialogTitleBar = true;
            DialogTitle = LanguageResourceHelper.Get("LID_POPUPHeader_ChangePassword");
            Username = args.Username;
            UserMustChangePassword = args.UserMustChangePassword;
            _smtpSettingsModel = smtpSettingsService ?? new SmtpSettingsModel();
        }

        protected override void OnAccept()
        {
            Log.Debug($"Attempting to change password for '{Username}'...");

            try
            {
                if (string.IsNullOrEmpty(Username)) return;

                if (Validation.ValidateChangePassword(OldPassword, NewPassword, NewPasswordConfirm))
                {
                    var changePasswordResult = UserModel.ChangeMyPassword(OldPassword, NewPassword);
                    if (changePasswordResult.Equals(HawkeyeError.eSuccess))
                    {
                        base.OnAccept(); // close window
                        DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_StatusBar_PasswordhasBeenChanged"));
                        if (_smtpSettingsModel.IsValidEmail(LoggedInUser.CurrentUser.Email) &&
                            _smtpSettingsModel.SendEmail(LoggedInUser.CurrentUser.UserID, LoggedInUser.CurrentUser.Email))
                        {
                            PostToMessageHub(LanguageResourceHelper.Get("LID_MSGBOX_Email_Sent"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_CHANGE_PASSWORD_ERROR"));
            }
            finally
            {
                OldPassword = string.Empty;
                NewPassword = string.Empty;
                NewPasswordConfirm = string.Empty;
            }
        }

        public override bool CanAccept()
        {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(OldPassword) && !string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(NewPasswordConfirm);
        }

        protected override void OnCancel()
        {
            if (UserMustChangePassword)
            {
                UserModel.LogOutUser();
                MainWindowViewModel.Instance.OnLogOut();
            }
            base.OnCancel();
        }

    }
}
