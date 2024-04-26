using HawkeyeCoreAPI.Facade;
using ScoutDataAccessLayer.DAL;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Admin;
using ScoutModels.Common;
using ScoutModels.Service;
using ScoutModels.Settings;
using ScoutServices.Enums;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using SystemStatus = ScoutUtilities.Enums.SystemStatus;
using ScoutModels.Interfaces;
using System.Collections.Generic;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class UserProfileDialogViewModel : BaseDialogViewModel
    {
        private readonly SettingsService _settingsService;

        public UserProfileDialogViewModel(ILockManager lockManager, IInstrumentStatusService instrumentStatusService, UserProfileDialogEventArgs args, System.Windows.Window parentWindow) : base(args, parentWindow)
        {
            _lockManager = lockManager;
            _lockSubscriber =_lockManager.SubscribeStateChanges().Subscribe(LockStatusChanged);
            _instrumentStatusService = instrumentStatusService;
            _settingsService = new SettingsService();
            DialogTitle = LanguageResourceHelper.Get("LID_POPUPHeader_MyProfile");

            Username = args.Username;
            
            if (args.Username.Equals(LoggedInUser.CurrentUserId))
            {
                FullUserName = LoggedInUser.CurrentUser?.DisplayName ?? string.Empty; // this is currently the only way to get the full name of a user
            }
            else FullUserName = string.Empty;

            RoleId = UserModel.GetUserRole(Username);
            UserIcon = ImageHelper.GetUserIconPath(RoleId);
            DialogIconPath = UserIcon;
            DialogIconIsSquare = false;
            UserCellTypes = new List<CellTypeDomain>(LoggedInUser.CurrentUser.AssignedCellTypes).ToObservableCollection(); //  Get the current user's allowed cell types
            RunOptionModel = new RunOptionSettingsModel(XMLDataAccess.Instance, Username);
            if (UserCellTypes.Any())
            {
                var defaultCtIndex = RunOptionsSettings.DefaultBPQC;
                _ogSelectedCellType = SelectedCellType =
                    UserCellTypes.FirstOrDefault(x => x.CellTypeIndex == defaultCtIndex) ??
                    UserCellTypes.FirstOrDefault(x => x.CellTypeIndex == (ulong)CellTypeIndex.BciDefault);
            }

            _statusSubscriber = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe((OnSystemStatusChanged));

        }
        
        protected override void DisposeUnmanaged()
        {
            _lockSubscriber?.Dispose();
            _statusSubscriber?.Dispose();
            base.DisposeUnmanaged();
        }

        #region Properties

        private CellTypeDomain _ogSelectedCellType;
        private readonly ILockManager _lockManager;
        private readonly IInstrumentStatusService _instrumentStatusService;

        private IDisposable _lockSubscriber;
        private IDisposable _statusSubscriber;

        public RunOptionSettingsModel RunOptionModel { get; set; }
        public RunOptionSettingsDomain RunOptionsSettings
        {
            get { return RunOptionModel.RunOptionsSettings; }
            set
            {
                RunOptionModel.RunOptionsSettings = value;
                NotifyPropertyChanged(nameof(RunOptionsSettings));
            }
        }

        public string UserIcon
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string FullUserName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public UserPermissionLevel RoleId
        {
            get { return GetProperty<UserPermissionLevel>(); }
            set 
            { 
                SetProperty(value);
                NotifyPropertyChanged(nameof(RoleName));
            }
        }

        public string RoleName => EnumToLocalization.GetLocalizedDescription(RoleId);

        public string Username
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<CellTypeDomain> UserCellTypes
        {
            get { return GetProperty<ObservableCollection<CellTypeDomain>>(); }
            set { SetProperty(value); }
        }

        public CellTypeDomain SelectedCellType
        {
            get { return GetProperty<CellTypeDomain>(); }
            set
            {
                SetProperty(value);
                if (value != null)
                {
                    RunOptionsSettings.DefaultBPQC = value.CellTypeIndex;
                }
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsChangeCellTypeEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        #region Accept Command

        public override bool CanAccept()
        {
            if (_ogSelectedCellType == null)
                return true;
            return SelectedCellType.CellTypeIndex != _ogSelectedCellType.CellTypeIndex;
        }

        protected override void OnAccept()
        {
            if (UserCellTypes != null && UserCellTypes.Count > 0)
            {
                var cell = UserCellTypes.FirstOrDefault();
                SettingsModel.SaveRunOptionSetting(Username, SelectedCellType?.CellTypeIndex ?? (uint)CellTypeIndex.BciDefault);
                MessageBus.Default.Publish(new Notification(MessageToken.UserDefaultCellTypeChanged));
            }

            base.OnAccept();
        }

        #endregion

        #region Change Password Command

        private RelayCommand _changePasswordCommand;
        public RelayCommand ChangePasswordCommand
        {
            get
            {
                if (_changePasswordCommand == null) _changePasswordCommand = new RelayCommand(OpenChangePasswordDialog, CanOpenChangePasswordDialog);
                return _changePasswordCommand;
            }
        }

        private bool CanOpenChangePasswordDialog()
        {
            return !_lockManager.IsLocked();
        }

        private void OpenChangePasswordDialog()
        {
            Close(null);
            DialogEventBus.ChangePasswordDialog(this, new ChangePasswordEventArgs(LoggedInUser.CurrentUserId, ImageHelper.GetUserIconPath(LoggedInUser.CurrentUser)));
        }

        #endregion

        #endregion

        #region Private Methods

        private void UpdateButtons()
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                ChangePasswordCommand.RaiseCanExecuteChanged();
            });
        }

        private void LockStatusChanged(LockResult res)
        {
            UpdateButtons();
        }

        private void OnSystemStatusChanged(SystemStatusDomain sysStatus)
        {
            IsChangeCellTypeEnabled = (sysStatus.SystemStatus == SystemStatus.Idle ||
                                       sysStatus.SystemStatus == SystemStatus.Faulted ||
                                       sysStatus.SystemStatus == SystemStatus.Stopped);
        }

        #endregion
    }
}
