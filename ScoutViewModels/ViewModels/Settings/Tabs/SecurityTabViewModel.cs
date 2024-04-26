using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Admin;
using ScoutUtilities;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ApiProxies.Generic;
using ScoutModels.Settings;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;

namespace ScoutViewModels.ViewModels.Tabs
{
    public class SecurityTabViewModel : BaseViewModel
    {
        public SecurityTabViewModel()
        {
            _settingsModel = new SettingsModel();
            _securitySettingsModel = new SecuritySettingsModel();
        }

        protected override void DisposeUnmanaged()
        {
            _securitySettingsModel?.Dispose();
            base.DisposeUnmanaged();
        }

        #region Properties & Fields

        private readonly SettingsModel _settingsModel;
        private readonly SecuritySettingsModel _securitySettingsModel;

        public ObservableCollection<UserDomain> Users
        {
            get { return GetProperty<ObservableCollection<UserDomain>>(); }
            set
            {
                // todo: make/use a different API call for getting a fully populated user list
                foreach (var user in value)
                {
                    var userRec = UserModel.GetUserRecord(user.UserID);
                    user.UpdateFromRec(userRec);
                }
                SetProperty(value);
            }
        }

        public UserDomain SelectedUser
        {
            get { return GetProperty<UserDomain>(); }
            set
            {
                SetProperty(value);
                UpdateButtons();
            }
        }

        public SecurityType SecurityType
        {
            get { return GetProperty<SecurityType>(); }
            set { SetProperty(value); }
        }

        public string InactivityTimeout
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(InactivityTimeoutString));
            }
        }

        public string PasswordExpiry
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(PasswordChangeString));
            }
        }

        public string InactivityTimeoutString => $"{LanguageResourceHelper.Get("LID_Label_InActivityTimeout")} : {InactivityTimeout} {LanguageResourceHelper.Get("LID_Label_MinutesLowerCase")}";
        public string PasswordChangeString => string.Format(LanguageResourceHelper.Get("LID_Label_PasswordChangeColonEvery"), PasswordExpiry);

        #endregion

        #region Commands

        #region On Loaded Event Command

        private RelayCommand _onLoadedCommand;
        public RelayCommand OnLoadedCommand => _onLoadedCommand ?? (_onLoadedCommand = new RelayCommand(OnLoaded));

        private void OnLoaded()
        {
            Users = new ObservableCollection<UserDomain>(UserModel.GetUserList());

            var securitySettingsDomain = _securitySettingsModel.SecuritySettingLoad();
            InactivityTimeout = securitySettingsDomain.InActivityTimeOutMins;
            PasswordExpiry = securitySettingsDomain.PasswordExpiryDays;
            SecurityType = securitySettingsDomain.SecurityType;

            UpdateButtons();
        }

        #endregion

        #region Edit Security Settings Command

        private RelayCommand _editSecuritySettingsCommand;
        public RelayCommand EditSecuritySettingsCommand => _editSecuritySettingsCommand ?? (_editSecuritySettingsCommand = new RelayCommand(EditSecuritySettings, CanEditSecuritySettings));

        private bool CanEditSecuritySettings()
        {
            return IsAdminUser;
        }

        private void EditSecuritySettings()
        {
            var args = new EditSecuritySettingsDialogEventArgs(SecurityType, 
                int.Parse(InactivityTimeout), int.Parse(PasswordExpiry));

            if (DialogEventBus.EditSecuritySettingsDialog(this, args) == true)
            {
                OnLoaded();
            }
        }

        #endregion

        #region Add User Command

        private RelayCommand _addUserCommand;
        public RelayCommand AddUserCommand => _addUserCommand ?? (_addUserCommand = new RelayCommand(AddNewUser, CanAddNewUser));

        private bool CanAddNewUser()
        {
            return IsAdminUser;
        }

        private void AddNewUser()
        {
            if (DialogEventBus<UserDomain>.AddEditUserDialog(this, 
                    new AddEditUserDialogEventArgs<UserDomain>()) == true)
            {
                OnLoaded();
            }
        }

        #endregion

        #region Edit User Command

        private RelayCommand _editUserCommand;
        public RelayCommand EditUserCommand => _editUserCommand ?? (_editUserCommand = new RelayCommand(EditUser, CanEditUser));

        private bool CanEditUser()
        {
            return null != SelectedUser && IsAdminOrServiceUser;
        }

        private void EditUser()
        {
            var curSelUser = SelectedUser.UserID;
            DialogEventBus<UserDomain>.AddEditUserDialog(this, new AddEditUserDialogEventArgs<UserDomain>(SelectedUser));
            OnLoaded();
            SelectedUser = Users.FirstOrDefault(u => u.UserID.Equals(curSelUser));
        }

        #endregion

        #region Delete User Command

        private RelayCommand _deleteUserCommand;
        public RelayCommand DeleteUserCommand => _deleteUserCommand ?? (_deleteUserCommand = new RelayCommand(DeleteUser, CanDeleteUser));

        private bool CanDeleteUser()
        {
            return SelectedUser != null && IsAdminUser &&
                   !LoggedInUser.CurrentUser.UserID.Equals(SelectedUser.UserID);
        }

        private void DeleteUser()
        {
            if (string.IsNullOrEmpty(SelectedUser?.UserID))
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_SelectUserToDelete"));
                Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_SelectUserToDelete"));
                return;
            }

            var msg = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_DeleteConfirmation"), SelectedUser?.UserID);
            if (DialogEventBus.DialogBoxYesNo(this, msg) != true) return;

            var args = new LoginEventArgs(LoggedInUser.CurrentUserId, LoggedInUser.CurrentUserId, LoginState.ValidateCurrentUserOnly, DialogLocation.CenterApp);
            var result = DialogEventBus.Login(this, args);

            try
            {
                if (result == LoginResult.CurrentUserLoginSuccess)
                {
                    Log.Debug("OnDeleteSuccessful::");
                    var deleteResult = SettingsModel.RemoveUser(SelectedUser.UserID);
                    if (deleteResult.Equals(HawkeyeError.eSuccess))
                    {
                        Log.Debug($"User '{SelectedUser.UserID}' deleted");
                        PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_UserhasBeenDeleted"));
                        OnLoaded();
                    }
                    else
                    {
                        DisplayErrorDialogByApi(deleteResult);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to delete user '{SelectedUser?.UserID}'", e);
                ExceptionHelper.HandleExceptions(e, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_DELETE_USER"));
            }
        }

        #endregion

        #endregion

        #region Private Methods

        private void UpdateButtons()
        {
            EditSecuritySettingsCommand.RaiseCanExecuteChanged();
            AddUserCommand.RaiseCanExecuteChanged();
            EditUserCommand.RaiseCanExecuteChanged();
            DeleteUserCommand.RaiseCanExecuteChanged();
        }

        #endregion
    }
}