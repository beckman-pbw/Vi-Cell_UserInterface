using ApiProxies.Generic;
using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Admin;
using ScoutModels.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using HawkeyeCoreAPI.Facade;
using HawkeyeCoreAPI.Interfaces;
using ScoutModels.Interfaces;
using ScoutModels.Service;
using ScoutModels.Settings;
using ScoutUtilities.Common;


namespace ScoutViewModels.ViewModels.Dialogs
{
    public class AddEditUserDialogViewModel : BaseDialogViewModel
    {
        enum UserModificationType
        {
            AddUser,
            EditUser
        }

        public AddEditUserDialogViewModel(AddEditUserDialogEventArgs<UserDomain> args, Window parentWindow,
            ISmtpSettingsService smtpSettingsService = null) 
            : base(args, parentWindow)
        {
            _settingsService = new SettingsService();
            _smtpSettingsModel = smtpSettingsService ?? new SmtpSettingsModel();

            DialogTitle = args.AddNewUser
                ? LanguageResourceHelper.Get("LID_Label_CreateUser")
                : LanguageResourceHelper.Get("LID_Label_EditUser");

            InCreateUserMode = args.AddNewUser;

            AllCellTypes = CellTypeFacade.Instance.GetAllCellTypes_BECall().ToObservableCollection();
            Roles = SettingsModel.GetRoles().ToObservableCollection();
            SelectedRole = Roles.FirstOrDefault();
            ShowChangePassword = SecurityIsLocal && CanModifyPassword && InEditUserMode;

            if (args.AddNewUser)
            {
                User = new UserDomain();
                User.IsEnabled = true;
                ShowPasswordTextBoxes = true;
                User.IsFastModeEnabled = true;
                User.AssignedCellTypes = new List<CellTypeDomain>();
                var defaultCellType =
                    AllCellTypes.FirstOrDefault(c => c.CellTypeIndex == (uint)CellTypeIndex.BciDefault);
                if (null != defaultCellType)
                {
                    User.AssignedCellTypes.Add(defaultCellType);
                }
            }
            else
            {
                User = args.User;
                RetrieveUserProperties(User);
                ShowPasswordTextBoxes = false;
            }
            CheckBoxCellTypesForUser(User.AssignedCellTypes);
        }

        #region Properties & Fields

        private SettingsService _settingsService;
        private ISmtpSettingsService _smtpSettingsModel;

        public ObservableCollection<CellTypeDomain> AllCellTypes
        {
            get { return GetProperty<ObservableCollection<CellTypeDomain>>(); }
            set { SetProperty(value); }
        }

        public UserDomain User
        {
            get { return GetProperty<UserDomain>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<UserPermissionLevel> Roles
        {
            get { return GetProperty<ObservableCollection<UserPermissionLevel>>(); }
            set { SetProperty(value); }
        }

        public UserPermissionLevel SelectedRole
        {
            get { return GetProperty<UserPermissionLevel>(); }
            set
            {
                var isCellSelected = value != UserPermissionLevel.eNormal;
                foreach (var cell in AllCellTypes)
                {
                    cell.IsCellEnable = isCellSelected;
                }
                CanModifyUserCellTypes = !isCellSelected;
                SetProperty(value);
            }
        }

        public bool InCreateUserMode
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(InEditUserMode));
            }
        }

        public bool InEditUserMode
        {
            get { return !InCreateUserMode; }
        }

        public bool ShowChangePassword
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool ShowPasswordTextBoxes
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool TargetUserIsBuiltinAdmin => User?.UserID == ApplicationConstants.FactoryAdminUserId;

        public bool NotEditingSelf => null != User && !LoggedInUser.CurrentUser.UserID.Equals(User.UserID);

        public bool CanModifyUser => !IsServiceUser;

        public bool CanModifyFastMode => IsAdminUser || User?.RoleID == UserPermissionLevel.eNormal;
      
        //Admin and advanced users have access to all cell types
        //Admin and service can add/remove cell types for normal users
        public bool CanModifyUserCellTypes
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool CanModifyPassword => IsAdminOrServiceUser;

        #endregion

        #region Commands

        protected override void OnAccept()
        {
            if (InCreateUserMode)
            {
                if (AddUser())
                {
                    base.OnAccept();
                }
            }
            else
            {
                if (EditUser())
                {
                    base.OnAccept();
                }
            }
        }

        #endregion

        #region Private Methods

        private bool EditUser()
        {
            if (!ValidateInputs(UserModificationType.EditUser))
                return false;

            try
            {
                if (LoggedInUser.CurrentUserRoleId.Equals(UserPermissionLevel.eService))
                {
                    return EditUserWithServiceAccount();
                }

                return EditUserWithAdminAccount();
            }
            catch (UnauthorizedAccessException ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_NO_PERMISSION_EDIT_USER"));
                return false;
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_MODIFY_USER"));
                return false;
            }
        }

        private bool AddUser()
        {
            if (!ValidateInputs(UserModificationType.AddUser))
                return false;

            var args = new LoginEventArgs(LoggedInUser.CurrentUserId, LoggedInUser.CurrentUserId, LoginState.ValidateCurrentUserOnly, DialogLocation.CenterApp);
            var result = DialogEventBus.Login(this, args);

            Log.Debug($"Adding new user '{User.UserID}'...");
            try
            {
                if (result == LoginResult.CurrentUserLoginSuccess)
                {
                    var addUserResult =
                        SettingsModel.AddUser(User.UserID, User.DisplayName, User.NewPassword, SelectedRole);
                    if (addUserResult.Equals(HawkeyeError.eSuccess))
                    {
                        var userEnable = SettingsModel.EnableUser(User.UserID, User.IsEnabled);
                        if (userEnable.Equals(HawkeyeError.eSuccess))
                        {
                            var changeResult = SettingsModel.ChangeUserPermission(User.UserID, SelectedRole);
                            var fastModeResult = UserModel.SetUserFastMode(User.UserID, User.IsFastModeEnabled);
                            if (changeResult.Equals(HawkeyeError.eSuccess) &&
                                fastModeResult.Equals(HawkeyeError.eSuccess))
                            {
                                SetUserValue(User);
                                SetUsersDefaultCellType(User);

                                PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_UserhasBeenAdded"));
                                Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_UserAddition"));
                            }
                            else
                            {
                                DisplayErrorDialogByApi(changeResult);
                                return false;
                            }
                        }
                        else
                        {
                            DisplayErrorDialogByApi(userEnable);
                            return false;
                        }
                    }
                    else
                    {
                        DisplayErrorDialogByUser(addUserResult);
                        return false;
                    }

                    return true;
                }

                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_NO_PERMISSION_NEW_USER"));
                return false;
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_FAILED_NEW_USER"));
                return false;
            }
        }

        private void SetUserValue(UserDomain selectedUser)
        {
            SetAssignedCellType(selectedUser.UserID);
            SetUserAnalyses(selectedUser.UserID);
            SetUserComment(selectedUser);
            SetUserEmail(selectedUser);
            SetUserFastMode(selectedUser);
            SetUserFolder(selectedUser.UserID);
        }

        private bool EditUserWithServiceAccount()
        {
            Log.Debug("EditUserWithServiceAccount");
            var args = new LoginEventArgs(LoggedInUser.CurrentUserId, LoggedInUser.CurrentUserId, LoginState.ValidateCurrentUserOnly, DialogLocation.CenterApp);
            var result = DialogEventBus.Login(this, args);
            if (result == LoginResult.CurrentUserLoginSuccess)
            {
                var userEnable = SettingsModel.EnableUser(User.UserID, User.IsEnabled);
                if (userEnable.Equals(HawkeyeError.eSuccess))
                {
                    if (ShowPasswordTextBoxes)
                    {
                        var passwordResult = SettingsModel.ChangeUserPassword(User.UserID, User.NewPassword);
                        if (passwordResult.Equals(HawkeyeError.eSuccess))
                        {
                            if (!string.IsNullOrEmpty(User.Email) &&
                                _smtpSettingsModel.SendEmail(User.UserID, User.Email))
                            {
                                Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_Email_Sent"));
                                PostToMessageHub(LanguageResourceHelper.Get("LID_MSGBOX_Email_Sent"));
                            }
                            Log.Debug(LanguageResourceHelper.Get("LID_StatusBar_PasswordhasBeenChanged"));
                            PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_PasswordhasBeenChanged"));
                        }
                        else
                        {
                            DisplayErrorDialogByApi(passwordResult);
                            return false;
                        }
                    }
                    SetUserValue(User);
                    Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_ChangeUserDetails"));
                    PostToMessageHub(LanguageResourceHelper.Get("LID_MSGBOX_ChangeUserDetails"));
                    return true;
                }

                DisplayErrorDialogByApi(userEnable);
                return false;
            }

            return false;
        }

        private bool EditUserWithAdminAccount()
        {
            Log.Debug("EditUserWithAdminAccount::");
            var userRec = HawkeyeCoreAPI.User.GetUserRecordApi(User.UserID);
            if (!User.DisplayName.Equals(userRec.DisplayName))
            {
                if (!AdminValidation(User) || !Validation.TextBoxLength(User.UserID, User.DisplayName))
                    return false;

                var displayResult = SettingsModel.SetUserDisplayName(User.UserID, User.DisplayName);
                if (!displayResult.Equals(HawkeyeError.eSuccess))
                {
                    ApiHawkeyeMsgHelper.ErrorCreateUser(displayResult);
                    return false;
                }
            }

            var args = new LoginEventArgs(LoggedInUser.CurrentUserId, LoggedInUser.CurrentUserId,
                LoginState.ValidateCurrentUserOnly, DialogLocation.CenterApp, returnPassword: true);
            var result = DialogEventBus.Login(this, args);

            if (result == LoginResult.CurrentUserLoginSuccess)
            {
                var userEnable = SettingsModel.EnableUser(User.UserID, User.IsEnabled);
                if (userEnable.Equals(HawkeyeError.eSuccess))
                {
                    if (ShowPasswordTextBoxes)
                    {
                        HawkeyeError passwordResult;
                        if (LoggedInUser.CurrentUserId.Equals(User.UserID))
                        {
                            // ChangeUserPassword does not work if the current user is trying to change
                            // their own password. Use ChangeMyPassword() instead:
                            passwordResult = UserModel.ChangeMyPassword(args.Password, User.NewPassword);
                        }
                        else
                        {
                            passwordResult = SettingsModel.ChangeUserPassword(User.UserID, User.NewPassword);
                        }
                        
                        if (passwordResult.Equals(HawkeyeError.eSuccess))
                        {
                            if (!string.IsNullOrEmpty(User.Email) &&
                                _smtpSettingsModel.SendEmail(User.UserID, User.Email))
                            {
                                Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_Email_Sent"));
                                PostToMessageHub(LanguageResourceHelper.Get("LID_MSGBOX_Email_Sent"));
                            }
                            Log.Debug(LanguageResourceHelper.Get("LID_StatusBar_PasswordhasBeenChanged"));
                            PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_PasswordhasBeenChanged"));
                        }
                        else
                        {
                            DisplayErrorDialogByApi(passwordResult);
                            return false;
                        }
                    }

                    var fastModeResult = UserModel.SetUserFastMode(User.UserID, User.IsFastModeEnabled);
                    if (fastModeResult != HawkeyeError.eSuccess)
                    {
	                    DisplayErrorDialogByApi(fastModeResult);
	                    return false;
                    }

					if (SystemStatusFacade.Instance.GetSecurity() == SecurityType.LocalSecurity)
                    {
	                    var changeResult = SettingsModel.ChangeUserPermission(User.UserID, SelectedRole);
	                    //var fastModeResult = UserModel.SetUserFastMode(User.UserID, User.IsFastModeEnabled);
	                    if (changeResult.Equals(HawkeyeError.eSuccess) /*&& fastModeResult == HawkeyeError.eSuccess*/)
	                    {
		                    SetUserValue(User);
		                    UpdateAllowedCellTypes(User);
		                    var runOptions = _settingsService.GetRunOptions();
		                    if (null == User.AssignedCellTypes.FirstOrDefault(
			                    c => c.CellTypeIndex == runOptions.DefaultBPQC))
		                    {
			                    // If the user's default cell type is no longer in their list, reset it to BCI default.
			                    SetUsersDefaultCellType(User);
		                    }

		                    Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_ChangeUserDetails"));
		                    PostToMessageHub(LanguageResourceHelper.Get("LID_MSGBOX_ChangeUserDetails"));
		                    return true;
	                    }

	                    DisplayErrorDialogByApi(changeResult);
	                    return false;
                    }
                    else
                    {
	                    SetAssignedCellType(User.UserID);
	                    SetUserComment(User);
	                    SetUserFastMode(User);
	                    UpdateAllowedCellTypes(User);

						Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_ChangeUserDetails"));
						PostToMessageHub(LanguageResourceHelper.Get("LID_MSGBOX_ChangeUserDetails"));
						return true;
					}
                }

                DisplayErrorDialogByApi(userEnable);
                return false;
            }

            return false;
        }

        /// <summary>
        /// If the user has at least one available cell type, this method will set the user's
        /// default cell type to the first in their list (default cell type).
        /// </summary>
        /// <param name="selectedUser">UserDomain object of the user whose default cell type is being set.</param>
        private void SetUsersDefaultCellType(UserDomain selectedUser)
        {
            if (selectedUser.AssignedCellTypes != null && selectedUser.AssignedCellTypes.Count > 0)
            {
                var cell = selectedUser.AssignedCellTypes.FirstOrDefault();
                SettingsModel.SaveRunOptionSetting(selectedUser.UserID, cell?.CellTypeIndex ?? (uint)CellTypeIndex.BciDefault);
            }
        }

        private void SetAssignedCellType(string userId)
        {
            var assignedCellType = new List<uint>();
            foreach (var cell in AllCellTypes)
            {
                if (!cell.IsCellEnable) continue;
                assignedCellType.Add(cell.CellTypeIndex);
            }
            CellTypeModel.SetUserCellTypeIndices(userId, assignedCellType);
        }

        private void SetUserAnalyses(string userName)
        {
            var assignedAnalysis = new List<ushort> { 0 };
            _ = AnalysisModel.SetUserAnalyses(userName, assignedAnalysis);
        }

        private void SetUserFolder(string userid)
        {
            try
            {
                var folder = Path.Combine(ScoutUtilities.UIConfiguration.UISettings.ExportPath, userid);
                _ = SettingsModel.SetUserFolder(userid, folder);
            }
            catch (FieldAccessException ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_DENIEDACCESS_FOLDER"));
            }
            catch (FileNotFoundException ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_NOTFOUND_FOLDER"));
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_FAILEDTOSETFOLDER"));
            }
        }

        private void SetUserEmail(UserDomain selectedUser)
        {
            if (selectedUser == null) return;
            _ = UserModel.SetUserEmailAddress(selectedUser.UserID, selectedUser.Email);
        }

        private void SetUserComment(UserDomain selectedUser)
        {
            if (selectedUser == null) return;
            _ = UserModel.SetUserComment(selectedUser.UserID, selectedUser.Comments);
        }

        private void SetUserFastMode(UserDomain selectedUser)
        {
            if (selectedUser == null) return;
            _ = UserModel.SetUserFastMode(selectedUser.UserID, selectedUser.IsFastModeEnabled);
        }

        private bool ValidateInputs(UserModificationType modType)
        {
            if (AdminValidation(User))
            {
                if (modType == UserModificationType.AddUser)
                {
                    return
                        Validation.TextBoxLength(User.UserID, User.DisplayName)
                        && Validation.UserPasswordValidation(User.NewPassword, User.ConfirmPassword)
                        && Validation.StrongPasswordInvalidate(User.NewPassword)
                        && (IsServiceUser || string.IsNullOrWhiteSpace(User.Email) || Validation.EmailValidation(User.Email));
                }
                else
                {
                    var result = IsServiceUser || string.IsNullOrWhiteSpace(User.Email) || Validation.EmailValidation(User.Email);

                    if (ShowPasswordTextBoxes)
                    {
                        result = result && Validation.UserPasswordValidation(User.NewPassword, User.ConfirmPassword)
                               && Validation.StrongPasswordInvalidate(User.NewPassword);
                    }

                    return result;
                }
            }
            return false;
        }

        private bool AdminValidation(UserDomain selectedUser)
        {
            //for UserId and DisplayName      
            if (selectedUser == null) return false;

            if (string.IsNullOrWhiteSpace(selectedUser.UserID))
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_UserNameBlank"));
                Log.Info(LanguageResourceHelper.Get("LID_ERRMSGBOX_UserNameBlank"));
                return false;
            }

            if (!string.IsNullOrWhiteSpace(selectedUser.DisplayName))
            {
                return true;
            }

            DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_DisplayNameBlank"));
            Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_DisplayNameBlank"));
            return false;
        }

        private void RetrieveUserProperties(UserDomain user)
        {
            if (user == null) return;

            var userRec = UserModel.GetUserRecord(user.UserID);
            user.UpdateFromRec(userRec);
            SelectedRole = Roles.FirstOrDefault(r => r == user.RoleID);
            UpdateAllowedCellTypes(user);
        }

        public void UpdateAllowedCellTypes(UserDomain user)
        {
            user.AssignedCellTypes = CellTypeFacade.Instance.GetAllowedCellTypes_BECall(user.UserID);
        }

        public void CheckBoxCellTypesForUser(List<CellTypeDomain> userCellTypes)
        {
            if (userCellTypes != null && userCellTypes.Any() && AllCellTypes.Any())
            {
                foreach (var cellType in AllCellTypes)
                {
                    cellType.IsCellEnable = userCellTypes.Any(c => c.CellTypeName.Equals(cellType.CellTypeName));
                }
            }
            else
            {
                foreach (var cellType in AllCellTypes)
                {
                    cellType.IsCellEnable = false;
                }
            }
        }

        #endregion
    }
}