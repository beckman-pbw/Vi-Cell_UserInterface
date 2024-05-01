using ApiProxies.Generic;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.Common;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Interfaces;
using ScoutServices.Enums;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutViewModels.Interfaces;
using ScoutViewModels.ViewModels.Tabs.SettingsPanel;
using HawkeyeCoreAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace ScoutViewModels.ViewModels
{
    public class TitleBarViewModel : BaseViewModel
    {
        public TitleBarViewModel(ILockManager lockManager, ISampleProcessingService sampleProcessingService, IRunningWorkListModel runningWorkListModel, IScoutViewModelFactory viewModelFactory, IInstrumentStatusService instrumentStatusService, IAuditLog auditLog)
        {
            _viewModelFactory = viewModelFactory;

            _lockManager = lockManager;
            _lockStateSubscriber = _lockManager.SubscribeStateChanges().Subscribe(SetLockStatus);
            _viewModelFactory = viewModelFactory;
            _sampleProcessingService = sampleProcessingService;
            _runningWorkListModel = runningWorkListModel;
            _instrumentStatusService = instrumentStatusService;
            _errorCodeList = new List<uint>();
            _auditLog = auditLog;

            IsSingleton = true;
            IsHamburgerEnable = true;
            ShowGenericLoginUserButton = true;
            MinimizeButtonVisible = false;

            if(IsSilentAdminActive)
            {
                MinimizeButtonVisible = true;
            }

            CurrentDate = DateTime.Now;
            SetTitleBarColor(SystemStatus.Idle);
            MessageHub = new MessageHub();
            SystemStatusVm = _viewModelFactory.CreateSystemStatusViewModel();

            // Create the timer that will update the current time in the title bar
            var _ = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 250),
                DispatcherPriority.Background, (s, e) => { CurrentDate = DateTime.Now; },
                Dispatcher.CurrentDispatcher) {IsEnabled = true};

            _reagentStateSubscriber = MessageBus.Default.Subscribe<Notification<ReagentContainerStateDomain>>(OnReagentStateChanged);
            _systemStatusSubscriber = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe((OnSystemStatusChanged));
        }

        protected override void DisposeUnmanaged()
        {
            _lockStateSubscriber?.Dispose();
            _systemStatusSubscriber?.Dispose();
            MessageBus.Default.UnSubscribe(ref _reagentStateSubscriber);
            base.DisposeUnmanaged();
        }

        public override void OnUserChanged(UserDomain newUser)
        {
            base.OnUserChanged(newUser);
            MinimizeButtonVisible = IsAdminOrServiceUser || IsSilentAdminActive;
            // ToDo: Why are these in the TitleBar? Shouldn't they be in the LoggedInUser class?
            if (! LoggedInUser.NoLoggedInUser())
                MessageHub.OnUserLogin(IsServiceUser);
            else
                MessageHub.OnUserLogout();

            NotifyPropertyChanged(nameof(IsSilentAdminActive));
            NotifyPropertyChanged(nameof(UserDisplayName));
            NotifyPropertyChanged(nameof(UserIcon));
        }

        #region Properties & Fields

        private readonly ISampleProcessingService _sampleProcessingService;
        private readonly IRunningWorkListModel _runningWorkListModel;
        private readonly ILockManager _lockManager;
        private readonly IInstrumentStatusService _instrumentStatusService;
        private readonly IScoutViewModelFactory _viewModelFactory;
        private readonly IAuditLog _auditLog;

        private IDisposable _lockStateSubscriber;
        private IDisposable _systemStatusSubscriber;
        private Subscription<Notification<ReagentContainerStateDomain>> _reagentStateSubscriber;

        private readonly List<uint> _errorCodeList;

        public event EventHandler<EventArgs> OnOpenNavigationMenu;
        public MessageHub MessageHub { get; set; }
        public bool IsSilentAdminActive => LoggedInUser.CurrentUserId?.Equals(ApplicationConstants.SilentAdmin) == true;
        public string UserDisplayName => LoggedInUser.CurrentUserId;
        public string UserIcon => ImageHelper.GetUserIconPath(LoggedInUser.CurrentUser);

        public DateTime CurrentDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public SystemStatusDomain SystemStatusDomain
        {
            get { return GetProperty<SystemStatusDomain>(); }
            set { SetProperty(value); }
        }

        public SolidColorBrush TitleBarBackgroundColor
        {
            get { return GetProperty<SolidColorBrush>(); }
            set { SetProperty(value); }
        }

        public SystemStatusViewModel SystemStatusVm
        {
            get { return GetProperty<SystemStatusViewModel>(); }
            set { SetProperty(value); }
        }

        public bool IsHamburgerEnable
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                HamburgerCommand.RaiseCanExecuteChanged();
            }
        }

        public bool MinimizeButtonVisible
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsUserButtonVisible
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
        
        public SystemStatus SystemStatus
        {
            get { return GetProperty<SystemStatus>(); }
            set
            {
                SetProperty(value);
                SetTitleBarColor(value);
            }
        }

        public bool ShowGenericLoginUserButton
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string InstrumentIconStatus
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string ReagentStatusPath
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string ReagentIconStatus
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsReagentStatusFault
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public int WasteTrayCapacity
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public string UsesRemaining
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsSystemLocked
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        #endregion

        #region Public Methods

        public void SetSystemStatus()
        {
            switch (SystemStatusDomain.SystemStatus)
            {
                case SystemStatus.Idle:
                case SystemStatus.Stopped:
                    InstrumentIconStatus = LanguageResourceHelper.Get("LID_StatusLabel_Idle");
                    break;

                case SystemStatus.ProcessingSample:
                case SystemStatus.SearchingTube:
                    InstrumentIconStatus = LanguageResourceHelper.Get("LID_Enum_Running");
                    break;
                
                case SystemStatus.Paused:
                    InstrumentIconStatus = LanguageResourceHelper.Get("LID_Enum_Paused");
                    break;
                case SystemStatus.Pausing:
                    InstrumentIconStatus = LanguageResourceHelper.Get("LID_MSGBOX_PausingPleaseWait");
                    break;
                case SystemStatus.Stopping:
                    InstrumentIconStatus = LanguageResourceHelper.Get("LID_Enum_Stopping");
                    break;

                case SystemStatus.Faulted:
                    InstrumentIconStatus = LanguageResourceHelper.Get("LID_Label_Error");
                    PopulateMessageHub();
                    break;
            }
        }

        public void ClearSystemErrorCodes()
        {
            try
            {
                _errorCodeList.ForEach(errorCode =>
                {
                    var status = _instrumentStatusService.ClearSystemErrorCode(errorCode);
                    if (status.Equals(HawkeyeError.eSuccess))
                    {
                        if (errorCode.Equals(_errorCodeList.Last()))
                        {
                            PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_SEhasBeenDeleted"));
                        }
                    }
                    else
                        ApiHawkeyeMsgHelper.ErrorCommon(status);
                });

                _errorCodeList.Clear();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CLEAN"));
            }
        }

        public void SetCurrentUserDisplayLanguage()
        {
            var generalSetting = new LanguageSettingsViewModel();
            if (generalSetting?.GeneralSettings != null)
            {
                ScoutModels.Common.ApplicationLanguage.SetLanguage(generalSetting.GeneralSettings.LanguageID);
                // Update the hamburger menu to current language.
                MessageBus.Default.Publish(generalSetting);
            }
        }

        public void RefreshReagentStatus(ReagentContainerStateDomain reagentContainerStatus = null)
        {
            try
            {
                if (reagentContainerStatus == null)
                {
                    reagentContainerStatus = ReagentModel.GetReagentContainerStatusAll()?.FirstOrDefault();
                    if (reagentContainerStatus == null) return;
                }

                switch (reagentContainerStatus.Status)
                {
                    case ReagentContainerStatus.eOK:
                        SetReagentHealth("/Images/Reagent-pack---header.png", false);
                        UsesRemaining = reagentContainerStatus.EventsRemaining.ToString();
                        ReagentIconStatus = ReagentContainerStatus.eOK.ToString();
                        break;
                    case ReagentContainerStatus.eEmpty:
                        SetReagentHealth("/Images/ReagentIcon.png", false);
                        ReagentIconStatus = ReagentContainerStatus.eEmpty.ToString();
                        UsesRemaining = string.Empty;
                        break;
                    case ReagentContainerStatus.eFaulted:
                    case ReagentContainerStatus.eExpired:
                    case ReagentContainerStatus.eNotDetected:
                        SetReagentHealth("/Images/Reagent_Faulted.png", true);
                        ReagentIconStatus = ReagentContainerStatus.eFaulted.ToString();
                        UsesRemaining = string.Empty;
                        break;
                    case ReagentContainerStatus.eInvalid:
                        SetReagentHealth("/Images/ReagentIcon.png", true);
                        ReagentIconStatus = ReagentContainerStatus.eFaulted.ToString();
                        UsesRemaining = string.Empty;
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_DEFAULT_LOGIN"));
            }
        }

        #endregion

        #region Private Methods

        private void SetLockStatus(LockResult res)
        {
            switch (res)
            {
                case LockResult.Locked:
                    IsSystemLocked = true;
                    break;
                case LockResult.Unlocked:
                    IsSystemLocked = false;
                    break;
            }
        }

        private void SetTitleBarColor(SystemStatus status)
        {
            switch (status)
            {
                case SystemStatus.Pausing:
                case SystemStatus.Paused:
                    TitleBarBackgroundColor = (SolidColorBrush) Application.Current.Resources["TitleBar_Warning_Background"];
                    break;
                case SystemStatus.Faulted:
                    TitleBarBackgroundColor = (SolidColorBrush) Application.Current.Resources["Error_Background"];
                    break;
                default:
                    if (_instrumentStatusService.IsRunning())
                    {
                        TitleBarBackgroundColor = (SolidColorBrush) Application.Current.Resources["TitleBar_RunningSamples_Background"];
                    }
                    else
                    {
                        TitleBarBackgroundColor = (SolidColorBrush) Application.Current.Resources["TitleBar_Background"];
                    }
                    break;
            }
        }

        private void SetReagentHealth(string path, bool isFaultHealth)
        {
            ReagentStatusPath = path;
            IsReagentStatusFault = isFaultHealth;
        }

        private void PopulateMessageHub()
        {
            SystemStatusDomain.SystemErrorDomainList?.ForEach(errorDomain =>
            {
                // Prohibit duplicate messages in the message hub.
                if (_errorCodeList.Contains(errorDomain.ErrorCode))
                    return;

                var messageType = MessageType.Normal;
                switch (errorDomain.SeverityKey)
                {
                    case "LID_API_SystemErrorCode_Severity_Warning":
                        messageType = MessageType.Warning;
                        break;
                    case "LID_API_SystemErrorCode_Severity_Error":
                        messageType = MessageType.System;
                        break;
                }

                DispatcherHelper.ApplicationExecute(() =>
                {
                    var description = errorDomain.GetDescription();
                    if (messageType == MessageType.System)
                        description = $"{description} - {LanguageResourceHelper.Get("LID_Label_FaultSuggestRestart")}";
                    Log.Warn(description);
                    PostToMessageHub(description, messageType);
                });

                _errorCodeList.Add(errorDomain.ErrorCode);
            });
        }

        #endregion

        #region Event Handlers

        private void OnReagentStateChanged(Notification<ReagentContainerStateDomain> msg)
        {
            if (msg.Token != MessageToken.RefreshReagentStatus)
                return;

            var reagentContainerStatus = msg.Target;
            RefreshReagentStatus(reagentContainerStatus);
        }

        private void OnSystemStatusChanged(SystemStatusDomain sysStatus)
        {
            if (sysStatus == null) return;

            SystemStatusDomain = sysStatus;
            SystemStatus = sysStatus.SystemStatus;
            WasteTrayCapacity = (int) sysStatus.SampleTubeDisposalRemainingCapacity;
            if (null != ReagentIconStatus && ReagentIconStatus.Equals(ReagentContainerStatus.eOK.ToString()))
            {
                // eEmpty and Faulted should show something else (see RefreshReagentStatus())
                UsesRemaining = sysStatus.RemainingReagentPackUses.ToString();
            }
            SetTitleBarColor(sysStatus.SystemStatus);
            PopulateMessageHub();
            SystemStatusVm?.Update();
            NotifyPropertyChanged(nameof(SystemStatusVm));

        }

        #endregion

        #region Commands

        #region Hamburger Command

        private RelayCommand _hamburgerCommand;
        public RelayCommand HamburgerCommand => _hamburgerCommand ?? (_hamburgerCommand = new RelayCommand(OpenNavigationMenu, CanOpenNavigationMenu));

        private bool CanOpenNavigationMenu()
        {
            return IsHamburgerEnable;
        }

        private void OpenNavigationMenu()
        {
            OnOpenNavigationMenu?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Message Hub Command

        private RelayCommand _messageHubOpenCommand;
        public RelayCommand MessageHubOpenCommand
        {
            get
            {
                if (_messageHubOpenCommand == null) _messageHubOpenCommand = new RelayCommand(OnMessageHubOpenCommand, CanMessageHubOpenCommandExecute);
                return _messageHubOpenCommand;
            }
        }

        public bool CanMessageHubOpenCommandExecute()
        {
            return ! LoggedInUser.NoLoggedInUser();
        }

        private void OnMessageHubOpenCommand()
        {
            DialogEventBus.ShowMessageHub(this, new MessageHubEventArgs(MessageHub.Messages.ToList(),
                OnCloseMessageHubDialog));
        }

        public void OnCloseMessageHubDialog(bool? obj)
        {
            if (MessageHub.HasUninspectedElevatedMessages())
                ClearSystemErrorCodes();

            //TODO: is this needed???
            MainWindowViewModel.Instance.UpdateSystemStatus();

            MessageHub.OnCloseMessageHubDialog();
        }

        #endregion

        #region System Lock Command

        private RelayCommand _systemLockCommand;
        public RelayCommand SystemLockCommand => _systemLockCommand ?? (_systemLockCommand = new RelayCommand(OnSystemLockCommand));
        
        private void OnSystemLockCommand(object param)
        {
            var currentUserRole = LoggedInUser.CurrentUserRoleId;
            var args = new BaseDialogEventArgs();
            var userWantsToUnlock = DialogEventBus.SystemLockDialog(this, args);
            if (userWantsToUnlock != null && (bool)userWantsToUnlock)
            {
                var loginArgs = new LoginEventArgs(LoggedInUser.CurrentUserId, LoggedInUser.CurrentUserId, LoginState.AutomationLock, DialogLocation.TopCenterApp, false, false,null, LanguageResourceHelper.Get("LID_ButtonContent_Administrative_Unlock"));
                var successfulAuth = DialogEventBus.Login(this, loginArgs);
                if (successfulAuth != null && successfulAuth == LoginResult.CurrentUserLoginSuccess)
                {
                    _lockManager.PublishAutomationLock(LockResult.Unlocked, string.Empty);

                    string msg = " Administrative unlock by " + loginArgs.DisplayedUsername;
                    _auditLog.WriteToAuditLogAPI(LoggedInUser.CurrentUser.UserID, audit_event_type.evt_automationunlocked, msg);
                    ApiHawkeyeMsgHelper.PublishHubMessage(ScoutUtilities.Misc.AuditEventString(audit_event_type.evt_automationunlocked) + msg, MessageType.Normal);
                }
            }
        }

        #endregion

        #region Minimize Command

        private RelayCommand _minimizeCommand { get; set; }
        public RelayCommand MinimizeCommand => _minimizeCommand ?? (_minimizeCommand = new RelayCommand(Minimize, CanMinimize));

        private bool CanMinimize()
        {
            return true; // enabled if it is visible
        }

        private void Minimize(object param)
        {
            MainWindowViewModel.Instance.OnSwitchOffLiveImage();
            MessageBus.Default.Publish(new Notification(MessageToken.MinimizeApplicationWindow, MessageToken.MinimizeApplicationWindow));
        }

        #endregion

        #region Instrument Status Command

        private RelayCommand _instrumentStatusCommand { get; set; }
        public RelayCommand InstrumentStatusCommand => _instrumentStatusCommand ?? (_instrumentStatusCommand = new RelayCommand(InstrumentStatus, CanShowInstrumentStatus));

        private bool CanShowInstrumentStatus()
        {
            return ! LoggedInUser.NoLoggedInUser();
        }

        private void InstrumentStatus(object param)
        {
            MainWindowViewModel.Instance.OnSwitchOffLiveImage();
            DialogEventBus.InstrumentStatusDialog(this, new InstrumentStatusEventArgs());
        }

        #endregion

        #region Reagent Status

        private RelayCommand _reagentStatusCommand { get; set; }
        public RelayCommand ReagentStatusCommand => _reagentStatusCommand ?? (_reagentStatusCommand = new RelayCommand(ReagentStatusView, CanShowReagentStatusView));

        private bool CanShowReagentStatusView()
        {
            return !LoggedInUser.NoLoggedInUser();
        }

        private void ReagentStatusView(object param)
        {
            MainWindowViewModel.Instance.OnSwitchOffLiveImage();
            DialogEventBus.ReagentStatusDialog(this, new ReagentStatusEventArgs());
        }

        #endregion

        #region User Profile Command

        private RelayCommand _userProfileCommand;
        public RelayCommand UserProfileCommand => _userProfileCommand ?? (_userProfileCommand = new RelayCommand(OpenUserProfile, CanOpenUserProfile));

        private bool CanOpenUserProfile()
        {
            return ! LoggedInUser.NoLoggedInUser();
        }

        private void OpenUserProfile()
        {
            if (LoggedInUser.CurrentUser.IsServiceUser())
                return;

            var user = LoggedInUser.CurrentUserId;
            var args = new UserProfileDialogEventArgs(user);
            DialogEventBus.UserProfileDialog(this, args);
        }

        private RelayCommand _serviceUserLoginCommand;
        public RelayCommand ServiceUserLoginCommand
        {
            get
            {
                if (_serviceUserLoginCommand == null) _serviceUserLoginCommand = new RelayCommand(ServiceUserlogin, CanServiceUserlogin);
                return _serviceUserLoginCommand;
            }
        }

        private bool CanServiceUserlogin()
        {
            return !LoggedInUser.NoLoggedInUser();
        }

        private void ServiceUserlogin()
        {
            if (LoggedInUser.CurrentUser.IsServiceUser())
                return;

            var args = new LoginEventArgs(ApplicationConstants.ServiceUser, LoggedInUser.CurrentUserId, LoginState.ValidateServiceUser, DialogLocation.CenterApp, false, true);
            var result = DialogEventBus.Login(this, args);

            if (result == LoginResult.CurrentUserLoginSuccess || result == LoginResult.SwapUserLoginSuccess)
            {
                try
                {
                    MainWindowViewModel.Instance.SetHamburgerItemEnable();
                    IsUserButtonVisible = true;
                    MainWindowViewModel.Instance.SetContent(_viewModelFactory.CreateHomeViewModel());
                    SetCurrentUserDisplayLanguage();
                }
                catch (Exception ex)
                {
                    ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_MASTER_NAVIGATE_VALIDATEUSER"));
                }
            }
        }

        #endregion

        #endregion
    }
}