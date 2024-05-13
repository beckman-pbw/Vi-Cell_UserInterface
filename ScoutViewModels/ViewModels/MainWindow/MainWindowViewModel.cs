using ApiProxies.Generic;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.Common;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Admin;
using ScoutModels.Common;
using ScoutModels.Interfaces;
using ScoutModels.Settings;
using ScoutServices.Enums;
using ScoutServices.Interfaces;
using ScoutServices.Watchdog;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.EventDomain;
using ScoutUtilities.Events;
using ScoutUtilities.UIConfiguration;
using ScoutViewModels.Interfaces;
using ScoutViewModels.ViewModels.Service;
using ScoutViewModels.ViewModels.Tabs.SettingsPanel;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HawkeyeCoreAPI.Facade;
using Microsoft.Diagnostics.Runtime;
using Ninject.Extensions.Logging;
using InactiveTimer = System.Windows.Forms.Timer;
using ApiProxies;

namespace ScoutViewModels.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        #region Singleton Stuff

        private static object _instanceLock = new object();
        public static MainWindowViewModel Instance => Application.Current?.MainWindow != null ?
                    Application.Current.MainWindow.DataContext as MainWindowViewModel : null;

        #endregion

        #region Constructor

        public MainWindowViewModel(IWatchdog watchdog, 
            ILockManager lockManager, 
            IRunningWorkListModel runningWorkListModel, 
            ISampleProcessingService sampleProcessingService, 
            IScoutViewModelFactory viewModelFactory, 
            IInstrumentStatusService instrumentStatusService, 
            ILogger logger, 
            IApplicationStateService applicationStateService)
        {
            _watchDog = watchdog;
            _lockManager = lockManager;
            _lockStateSubscriber = _lockManager.SubscribeStateChanges().Subscribe(SetLockStatus);
            _runningWorkListModel = runningWorkListModel;
            _sampleProcessingService = sampleProcessingService;
            _viewModelFactory = viewModelFactory;
            _instrumentStatusService = instrumentStatusService;
            _logger = logger;
            _applicationStateService = applicationStateService;
            _applicationStateServiceSubscription = _applicationStateService.SubscribeStateChanges(ApplicationStateHandler);

            Copyright = Misc.GetCopyright();

            _redStatusBarHasBeenClicked = false;

            _instrumentServiceStatusSubscriber = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe(InstrumentServiceStatusUpdated);

            if (_aoLogger == null)
            {
                var logName = UISettings.InstrumentPath + "\\Logs\\ExportMgr.log";
                var errorName = UISettings.InstrumentPath + "\\Logs\\ExportMgrError.log";
                _aoLogger = new ExportManager.AoLogger(logName, errorName);
            }
            if (_aoExportMgr == null)
            {
                _aoExportMgr = new ExportManager.AoExportMgr();
            }
        }

        private void InstrumentServiceStatusUpdated(SystemStatusDomain systemStatusDomain)
        {
            UpdateSystemStatus(true);
        }

        private void ApplicationStateHandler(ApplicationStateChange stateChange)
        {
            switch (stateChange.State)
            {
                case ApplicationStateEnum.Startup:
                    // Triggered from MainWindow.xaml.cs ScoutUIMasterView_Loaded() event handler
                    OnLoaded();
                    // Service layer operations such as subscriptions
                    break;
                case ApplicationStateEnum.Shutdown:
                    DispatcherHelper.ApplicationExecute(() => OnShutDown(stateChange.Reason, stateChange.Restart));
                    break;
            }
        }

        private void Initialize()
        {
            TitleBarVm = _viewModelFactory.CreateTitleBarViewModel();
            SelectedHamburgerItem = new HamburgerDomain();
            TitleBarVm.OnOpenNavigationMenu += TitleBarVmOnOpenNavigationMenu;

            LastUserActivity = DateTime.Now;
            _userInactivityTimer = new InactiveTimer();
            _userInactivityTimer.Interval = 1000;
            _userInactivityTimer.Tick += UserInactivityTimerTick;
            
            MessageBus.Default.Subscribe<LanguageSettingsViewModel>(UpdateGeneralSettings);
            MessageBus.Default.Subscribe<Notification<UserDomain>>(OnCurrentUserChanged);
            MessageBus.Default.Subscribe<Notification<bool>>(HandleBoolPropertyNotification);
            MessageBus.Default.Subscribe<Notification>(HandleGenericNotifications);
        }

        private void SetLockStatus(LockResult res)
        {
            switch (res)
            {
                case LockResult.Locked:
                    EnableDisableHamburgerItemsWhileLocked(true);
                    var maintenance = HamburgerItems.FirstOrDefault(i => i.Item == HamburgerItem.Maintenance);
                    if (maintenance.IsItemSelected)
                        SelectedHamburgerItem = _isUserLocked ? null : HamburgerItems.FirstOrDefault();
                    break;
                case LockResult.Unlocked:
                    EnableDisableHamburgerItemsWhileLocked(false);
                    break;
            }
        }

        public void OnLoaded()
        {
            // Has to be done as close to the hardware initialization as possible
            SystemStatusFacade.Instance.GetSecurity();
            ValidateSilentAdmin();

            LoadHamburger();
            Initialize();


            // Setup Hamburger menu first time as there is no logged in user.
            SetHamburgerItemEnable();
            TitleBarVm.RefreshReagentStatus();

            using (var securitySettingsModel = new SecuritySettingsModel())
            {
                int inactivityTimeout = securitySettingsModel.GetUserInactivityTimeout();
                MessageBus.Default.Publish(new Notification<int>(inactivityTimeout, MessageToken.UpdateInactivityTimeout));
            }
            _viewModelFactory.CreateHomeViewModel();
        }

        #endregion

        #region Event Handlers

        private void TitleBarVmOnOpenNavigationMenu(object sender, EventArgs e)
        {
            IsNavigationMenuOpen = !IsNavigationMenuOpen;
        }

        private void HandleBoolPropertyNotification(Notification<bool> notification)
        {
            if (string.IsNullOrEmpty(notification.Token))
                return;

            switch (notification.Token)
            {
                case nameof(MainWindowViewModel):
                    if (!string.IsNullOrEmpty(notification.Message))
                    {
                        var property = GetType().GetProperty(notification.Message);
                        if (notification.CallbackAction != null && bool.TryParse(property?.GetValue(this)?.ToString(), out var bResult))
                        {
                            // the caller wants the current value of the property
                            notification.CallbackAction.Invoke(bResult);
                        }
                        else
                        {
                            // the caller wants the change the value of the property
                            var newValue = notification.Target;
                            property?.SetValue(this, newValue);
                        }
                    }
                    break;
                case MessageToken.AdornerVisible:
                    IsAdornerVisible = notification.Target;
                    break;
            }
        }

        private void UpdateGeneralSettings(LanguageSettingsViewModel languageSettingsViewModel)
        {
            SetCultureForHamburger();
            NotifyPropertyChanged(nameof(HamburgerItems));
            NotifyPropertyChanged(nameof(SelectedHamburgerItem));
            uint concDisplayDigits = 2;
            if (LoggedInUser.IsConsoleUserLoggedIn())
            {
                concDisplayDigits = LoggedInUser.CurrentUser.DisplayDigits;
            }
            RunOptionSettingsModel.SetConcTrailingPoint(concDisplayDigits);
        }

        private void OnCurrentUserChanged(Notification<UserDomain> notification)
        {
            if (string.IsNullOrEmpty(notification.Token)
                || !notification.Token.Equals(NotificationClasses.NewCurrentUser))
            {
                return;
            }

            SetHamburgerItemEnable();

            if (_lockManager.IsLocked())
                SetLockStatus(LockResult.Locked);
            else
                SetLockStatus(LockResult.Unlocked);

            // Don't change the language after a user logs out
            // If you do, it will always revert to English
            if (!LoggedInUser.NoLoggedInUser()) 
            {
                TitleBarVm.SetCurrentUserDisplayLanguage();
            }

            if (!_isUserLocked && !LoggedInUser.NoLoggedInUser())
            {
                SetContent(_viewModelFactory.CreateHomeViewModel());
            }
            else
            {
                _isUserLocked = false;
            }

            IsInActivityTimeOutActive = !LoggedInUser.CurrentUserId.Equals(ApplicationConstants.SilentAdmin);
            TitleBarVm.NotifyAllPropertiesChanged();
            NotifyPropertyChanged(nameof(IsSilentAdminActive));
        }

        #endregion

        #region Commands

        #region Hamburger Menu Item Clicked

        private RelayCommand<HamburgerItem> _menuItemSelectedCommanded;
        public RelayCommand<HamburgerItem> MenuItemSelectedCommanded => _menuItemSelectedCommanded ?? (_menuItemSelectedCommanded = new RelayCommand<HamburgerItem>(MenuItemSelected));

        private void MenuItemSelected(HamburgerItem selectedItem)
        {
            var selected = HamburgerItems.FirstOrDefault(i => i.Item == selectedItem);
            if (selected != null)
                SetHamburger(selected);
        }

        #endregion

        #region Close Command

        private RelayCommand _closeCommand { get; set; }
        public RelayCommand CloseCommand => _closeCommand ?? (_closeCommand = new RelayCommand(CloseWindowCommand));

        private void CloseWindowCommand(object param)
        {
            var window = param as Window;
            window?.Close();
        }

        #endregion

        #region Flashing Fault Screen Clicked Command

        private RelayCommand _flashingFaultScreenClickCommand;
        public RelayCommand FlashingFaultScreenClickCommand => _flashingFaultScreenClickCommand ?? (_flashingFaultScreenClickCommand = new RelayCommand(FlashingFaultScreenClicked));

        private void FlashingFaultScreenClicked()
        {
            try
            {
                FlashingFaultScreen = false;
                _redStatusBarHasBeenClicked = true;
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_MASTER_STATUSBAR"));
            }
        }

        #endregion

        #endregion

        #region Properties & Fields

        private ExportManager.AoExportMgr _aoExportMgr = null;
        private ExportManager.AoLogger _aoLogger = null;
        
        private readonly IWatchdog _watchDog;
        private readonly IScoutViewModelFactory _viewModelFactory;

        private readonly object _contentLock = new object();
        private readonly CancellationTokenSource _systemStatusToken = new CancellationTokenSource();
        private bool _isInstrumentSelection;
        private bool _isUserLocked;
        private bool _redStatusBarHasBeenClicked;

        private readonly ILockManager _lockManager;
        private readonly IInstrumentStatusService _instrumentStatusService;
        private readonly ILogger _logger;
        private readonly IRunningWorkListModel _runningWorkListModel;
        private ISampleProcessingService _sampleProcessingService;
        private readonly IApplicationStateService _applicationStateService;
        private readonly IDisposable _applicationStateServiceSubscription;
        private bool _shutdownInProgress = false;

        private IDisposable _lockStateSubscriber;
        private IDisposable _instrumentServiceStatusSubscriber;

        public Action SwitchOffLiveImage { get; set; }
        public bool IsSilentAdminActive => LoggedInUser.CurrentUserId?.Equals(ApplicationConstants.SilentAdmin) == true;

        #region Timer Stuff

        private InactiveTimer _userInactivityTimer;

        public DateTime LastUserActivity { get; set; }
        public TimeSpan InactivityTimeSpan { get; set; }
        public bool IsInActivityTimeOutActive { get; set; }
        public bool IsScreenLockActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion

        //public string UiSoftwareVersion
        //{
        //    get { return GetProperty<string>(); }
        //    set { SetProperty(value); }
        //}

        public string Copyright
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsAdornerVisible
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool FlashingFaultScreen
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public eNightlyCleanStatus NightlyCleanStatus
        {
            get { return GetProperty<eNightlyCleanStatus>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<HamburgerDomain> HamburgerItems
        {
            get { return GetProperty<ObservableCollection<HamburgerDomain>>(); }
            set { SetProperty(value); }
        }

        public HamburgerDomain SelectedHamburgerItem
        {
            get { return GetProperty<HamburgerDomain>(); }
            set
            {
                SetProperty(value);
                if (value != null)
                    SetHamburger(value);
            }
        }

        public TitleBarViewModel TitleBarVm
        {
            get { return GetProperty<TitleBarViewModel>(); }
            set { SetProperty(value); }
        }

        public bool IsNavigationMenuOpen
        {
            get { return GetProperty<bool>(); }
            set
            {
                var changed = GetProperty<bool>() != value;
                SetProperty(value);
                if (changed)
                    MessageBus.Default.Publish(new Notification<bool>(value, MessageToken.MainNavMenuOpenCloseToken));
            }
        }

        public ContentControl CurrentContent
        {
            get { return GetProperty<ContentControl>(); }
            set { SetProperty(value); }
        }

        public bool IsAdminEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsMaintenanceEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Public Methods

        public void SetContent(BaseViewModel viewModel)
        {
            lock (_contentLock)
            {
                if (CurrentContent == null)
                {
                    _logger.Warn($"CurrentContent is NULL when going to {viewModel.GetType().Name} page. ");
                    //ValidateSilentAdmin(); // I am not sure what purpose this once had but it is causing issues with turning ON security
                }

                DispatcherHelper.ApplicationExecute(() =>
                {

                    if (CurrentContent == null) CurrentContent = new ContentControl { Content = viewModel, IsTabStop = false, Focusable = false };
                    else if (CurrentContent.Content != viewModel)
                    {
                        if (CurrentContent.Content != null && !((BaseViewModel)(CurrentContent.Content)).IsSingleton)
                            ((BaseViewModel)CurrentContent.Content)?.Dispose();
                        CurrentContent.Content = viewModel;
                    }
                });
            }
        }

        public void EnableDisableHamburgerMenu(bool enableMenu)
        {
            DispatcherHelper.ApplicationExecute(() => TitleBarVm.IsHamburgerEnable = enableMenu);
        }

        public void SetHamburgerItemEnable()
        {
            foreach (var item in HamburgerItems)
            {
                if (LoggedInUser.NoLoggedInUser())
                {
                    item.IsItemEnable = false;
                    continue;
                }

                switch (LoggedInUser.CurrentUser.RoleID)
                {
                    case UserPermissionLevel.eNormal:
                        SetHamburgerOptionForNormalUser(item);
                        break;
                    case UserPermissionLevel.eElevated:
                        SetHamburgerOptionForAdvanceUser(item);
                        break;
                    case UserPermissionLevel.eAdministrator:
                        SetHamburgerOptionForAdminUser(item);
                        break;
                    case UserPermissionLevel.eService:
                        SetHamburgerOptionForServiceUser(item);
                        break;
                }
            }

            SelectedHamburgerItem = _isUserLocked ? null : HamburgerItems.FirstOrDefault();
        }

        public async void OnShutDown(string reason, bool isRestart = false)
        {
            BAFW.BAppFW.Shutdown();
            if (_aoExportMgr != null)
            {
                _aoExportMgr.Shutdown();
                _aoExportMgr = null;
            }
            if (_aoLogger != null)
            {
                _aoLogger.Shutdown();
                _aoLogger = null;
            }

            if (_shutdownInProgress)
            {
                return;
            }

            _shutdownInProgress = true;
            _systemStatusToken.Cancel();
            DialogEventBus.ExitUiDialog(this, new ExitUiDialogEventArgs(reason));

            UserModel.ShutDown();

            var token = new CancellationTokenSource();
            token.CancelAfter(TimeSpan.FromSeconds(UISettings.ShutDownTimeoutSecond));

            try
            {
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                            return Task.FromCanceled(token.Token);
                        if (UserModel.IsShutdownComplete())
                            return Task.CompletedTask;

                        await Task.Delay(1000, token.Token);
                    }
                }, token.Token);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error during loop to call IsShutdownComplete (#5100)");
            }

            if (isRestart)
            {
                DispatcherHelper.ApplicationExecute(() =>
                {
                    // The call to shut down method will trigger "Application.Exit" event with "ApplicationRestartCode"
                    var applicationRestartCode = int.MaxValue;
                    Application.Current.Shutdown(applicationRestartCode);
                });
            }

            await Task.Delay(500).ContinueWith(t =>
            {
                if (Application.Current == null)
                {
#if DEBUG
                    DumpThreads();
#endif
                    Environment.Exit(0);
                }
                else
                {
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        Application.Current?.Shutdown(0);
#if DEBUG
                        //DumpThreads();
#endif
                        Environment.Exit(0);
                    });
                }
            });
        }

#if DEBUG
        private void DumpThreads()
        {
            var currentThreads = Process.GetCurrentProcess().Threads;
            _logger.Debug($"Native Threads still running after shutdown: (number of threads = {currentThreads.Count}");
            foreach (ProcessThread nativeThread in currentThreads)
            {
                _logger.Debug($"Id: {nativeThread.Id}, ThreadState {nativeThread.ThreadState}");
            }

            _logger.Debug($"CLR Threads still running after shutdown: (number of threads = {currentThreads.Count}");
            using (DataTarget target = DataTarget.AttachToProcess(
                Process.GetCurrentProcess().Id, false))
            {
                ClrRuntime runtime = target.ClrVersions.First().CreateRuntime();
                _logger.Debug($"CLR Threads still running after shutdown: (number of threads = {runtime.Threads.Count()}");
                foreach (var clrThread in runtime.Threads)
                {
                    _logger.Debug($"Managed Id: {clrThread.ManagedThreadId}, OS Id: {clrThread.OSThreadId}, Name: {clrThread}");
                }
            }
        }
#endif

        public void OnSwitchOffLiveImage()
        {
            SwitchOffLiveImage?.Invoke();
        }

        public void UpdateSystemStatus(bool isFromInstrumentServiceStatus = false)
        {
            try
            {
                if (TitleBarVm.SystemStatusDomain == null)
                    return;

                if (isFromInstrumentServiceStatus)
                {
                    NightlyCleanStatus = TitleBarVm.SystemStatusDomain.NightlyCleanStatus;
                    DisableHamburgerItemsWhileRunning();
                    return;
                }

                MessageBus.Default.Publish(TitleBarVm.SystemStatusDomain.CarouselDetect);
                MessageBus.Default.Publish(new StageStatus
                {
                    IsCarouselStatus = TitleBarVm.SystemStatusDomain.CarouselDetect,
                    SamplePosition = TitleBarVm.SystemStatusDomain.SamplePosition
                });
                MessageBus.Default.Publish(new MotorFocusPosition(TitleBarVm.SystemStatusDomain.DefinedFocusPosition));

                TitleBarVm.SetSystemStatus();

                if (TitleBarVm.SystemStatusDomain.SystemStatus == SystemStatus.Faulted && !_redStatusBarHasBeenClicked)
                {
                    FlashingFaultScreen = true;
                    _redStatusBarHasBeenClicked = false;
                }

                DisableHamburgerItemsWhileRunning();
                NightlyCleanStatus = TitleBarVm.SystemStatusDomain.NightlyCleanStatus;
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_GET_SYSTEM_STATUS"));
            }
        }

        private void DisableHamburgerItemsWhileRunning()
        {
            var exit = HamburgerItems.FirstOrDefault(i => i.Item == HamburgerItem.Exit);
            var maintenance = HamburgerItems.FirstOrDefault(i => i.Item == HamburgerItem.Maintenance);
            if (exit != null && maintenance != null)
            {
                var isNotRunning = TitleBarVm.SystemStatusDomain.SystemStatus == SystemStatus.Idle ||
                                   TitleBarVm.SystemStatusDomain.SystemStatus == SystemStatus.Faulted ||
                                   TitleBarVm.SystemStatusDomain.SystemStatus == SystemStatus.Stopped;
                var isEnable = isNotRunning && !_lockManager.IsLocked();
                EnableDisableHamburgerOptionFromHamburgerMenu(maintenance, IsServiceUser, isEnable && IsServiceUser);
                EnableDisableHamburgerOptionFromHamburgerMenu(exit, true, isEnable && IsAdminOrServiceUser);
            }

        }

        public void EnableDisableHamburgerItemsWhileLocked(bool isLocked)
        {
            var exit = HamburgerItems.FirstOrDefault(i => i.Item == HamburgerItem.Exit);
            var maintenance = HamburgerItems.FirstOrDefault(i => i.Item == HamburgerItem.Maintenance);
            if (exit != null && maintenance != null)
            {
                EnableDisableHamburgerOptionFromHamburgerMenu(maintenance, IsServiceUser, !isLocked && IsServiceUser);
                EnableDisableHamburgerOptionFromHamburgerMenu(exit, IsAdminOrServiceUser, !isLocked && IsAdminOrServiceUser);
            }
        }

        public void OnLogOut()
        {
            SetContent(_viewModelFactory.CreateSignInViewModel(_isUserLocked));
            TitleBarVm.IsUserButtonVisible = IsAdminEnabled = IsMaintenanceEnabled = false;
            foreach (var item in HamburgerItems)
            {
                item.IsItemVisible = item.Item == HamburgerItem.Lock;
            }
            _isUserLocked = false;
            IsInActivityTimeOutActive = false;
        }

        public bool OnSilentAdminLogin()
        {
            var result = UserModel.LoginSilentAdmin();
            if (!result)
            {
                _logger.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_FieldBlank"));
                var args = new DialogBoxEventArgs(LanguageResourceHelper.Get("LID_Label_SilentLoginFailed"),
                    LanguageResourceHelper.Get("LID_Label_InstrumentError"), icon: MessageBoxImage.Error);
                DialogEventBus.DialogBox(this, args);
            }

            return result;
        }

        #endregion

        #region Private Methods

        private void HandleGenericNotifications(Notification msg)
        {
            if (string.IsNullOrEmpty(msg?.Token))
                return;

            switch (msg.Token)
            {
                case MessageToken.UpdateInactivityTimeout:
                    UpdateInactiveTimeOut();
                    break;
            }
        }

        private void LoadHamburger()
        {
            HamburgerItems = new ObservableCollection<HamburgerDomain>
            {
                new HamburgerDomain
                {
                    Item = HamburgerItem.Home,
                    Title = LanguageResourceHelper.Get("LID_HB_Home"),
                    Image = HamburgerIcons.Home,
                    IsActive = true,
                    IsItemVisible = false
                },
                new HamburgerDomain
                {
                    Item = HamburgerItem.CellTypes,
                    Title = LanguageResourceHelper.Get("LID_GridLabel_CellTypes"),
                    Image = HamburgerIcons.CellType,
                    IsActive = true,
                    IsItemVisible = false
                },
                new HamburgerDomain
                {
                    Item = HamburgerItem.QualityControl,
                    Title = LanguageResourceHelper.Get("LID_GridLabel_QualityControl"),
                    Image = HamburgerIcons.QualityControls,
                    IsActive = true,
                    IsItemVisible = false
                },
                new HamburgerDomain
                {
                    Item = HamburgerItem.Settings,
                    Title = LanguageResourceHelper.Get("LID_HB_Settings"),
                    Image = HamburgerIcons.Settings,
                    IsActive = true,
                    IsItemVisible = false
                },
                new HamburgerDomain
                {
                    Item = HamburgerItem.Maintenance,
                    Title = LanguageResourceHelper.Get("LID_HB_Maintenance"),
                    Image = HamburgerIcons.Maintenance,
                    IsActive = true,
                    IsItemVisible = false
                },
                new HamburgerDomain
                {
                    Item = HamburgerItem.Reports,
                    Title = LanguageResourceHelper.Get("LID_TabItem_Reports"),
                    Image = HamburgerIcons.Report,
                    IsActive = true,
                    IsItemVisible = false
                },
                new HamburgerDomain
                {
                    Item = HamburgerItem.Signout,
                    Title = LanguageResourceHelper.Get("LID_HB_Signout"),
                    Image = HamburgerIcons.SignOut,
                    IsActive = true,
                    IsItemVisible = false
                },
                new HamburgerDomain
                {
                    Item = HamburgerItem.Lock,
                    Title = LanguageResourceHelper.Get("LID_HB_Lock"),
                    Image = HamburgerIcons.Lock,
                    IsItemVisible = true,
                    IsActive = false
                },
                new HamburgerDomain
                {
                    Item = HamburgerItem.Exit,
                    Title = LanguageResourceHelper.Get("LID_HB_Exit"),
                    Image = HamburgerIcons.Exit,
                    IsActive = false,
                    IsItemVisible = false
                }
            };
        }

        private void ValidateSilentAdmin()
        {
            if (IsSecurityTurnedOn)
            {
                CurrentContent = new ContentControl { Content = _viewModelFactory.CreateSignInViewModel(_isUserLocked), IsTabStop = false, Focusable = false };
            }
            else if (OnSilentAdminLogin())
            {
                OnSilentUserSuccessfullyLogin();
            }
        }

        private void SetHamburger(HamburgerDomain selectedItem)
        {
            IsAdornerVisible = true;
            
            if (! LoggedInUser.NoLoggedInUser())
                HamburgerSelection(selectedItem);
            
            IsNavigationMenuOpen = false;
            IsAdornerVisible = false;
        }

        private void SetHamburgerSelection()
        {
            if (SelectedHamburgerItem == null)
                return;

            var item = HamburgerItems.FirstOrDefault(i => i.Item == SelectedHamburgerItem.Item);

            if (item != null)
                item.IsItemSelected = false;
        }

        private void SetHamburgerOptionForServiceUser(HamburgerDomain item)
        {
            if (!IsSecurityTurnedOn)
            {
                if (item.Item.Equals(HamburgerItem.Lock))
                {
                    item.IsActive = true;
                    item.IsItemVisible = false;
                }
                else
                {
                    EnableDisableHamburgerOptionFromHamburgerMenu(item, true, true);
                }
            }
            else
            {
                EnableDisableHamburgerOptionFromHamburgerMenu(item, true, true);
            }
        }

        private void SetHamburgerOptionForAdminUser(HamburgerDomain item)
        {
            if (IsSilentAdminActive)
            {
                if (item.Item.Equals(HamburgerItem.Signout) || item.Item.Equals(HamburgerItem.Lock) || item.Item.Equals(HamburgerItem.Maintenance))
                {
                    EnableDisableHamburgerOptionFromHamburgerMenu(item, false,false);
                }
                else
                {
                    EnableDisableHamburgerOptionFromHamburgerMenu(item, true,true);
                }
            }
            else
            {
                EnableDisableHamburgerOptionFromHamburgerMenu(item, !item.Item.Equals(HamburgerItem.Maintenance), !item.Item.Equals(HamburgerItem.Maintenance));
            }
        }

        private void SetHamburgerOptionForAdvanceUser(HamburgerDomain item)
        {
            switch (item.Item)
            {
                case HamburgerItem.Maintenance:
                    EnableDisableHamburgerOptionFromHamburgerMenu(item, false,false);
                    break;
                default:
                    EnableDisableHamburgerOptionFromHamburgerMenu(item, true,true);
                    break;
            }
        }

        private void SetHamburgerOptionForNormalUser(HamburgerDomain item)
        {
            switch (item.Item)
            {
                case HamburgerItem.CellTypes:
                    item.IsItemVisible = false;
                    break;
                case HamburgerItem.Maintenance:
                    EnableDisableHamburgerOptionFromHamburgerMenu(item, false,false);
                    break;
                case HamburgerItem.Exit:
                    EnableDisableHamburgerOptionFromHamburgerMenu(item, true, false);
                    break;
                default:
                    EnableDisableHamburgerOptionFromHamburgerMenu(item, true,true);
                    break;
            }
        }

        private void EnableDisableHamburgerOptionFromHamburgerMenu(HamburgerDomain item, bool isVisible, bool isEnable)
        {
            item.IsActive = isVisible;
            item.IsItemVisible = isVisible;
            item.IsItemEnable = isEnable;
        }

        private void SetNewNavigationItemAsSelected(HamburgerDomain selectedItem)
        {
            foreach (var item in HamburgerItems)
            {
                item.IsItemSelected = false;
            }
            selectedItem.IsItemSelected = true;
        }

        private void HamburgerSelection(HamburgerDomain selectedItem)
        {
            IsNavigationMenuOpen = !IsNavigationMenuOpen;
            OnSwitchOffLiveImage();

            switch (selectedItem.Item)
            {
                case HamburgerItem.Home:
                    SetContent(_viewModelFactory.CreateHomeViewModel());
                    TitleBarVm.ShowGenericLoginUserButton = false;
                    SetNewNavigationItemAsSelected(selectedItem);
                    return;

                case HamburgerItem.CellTypes:
                    var cellTypeViewModel = _viewModelFactory.CreateCellTypeViewModel(LoggedInUser.CurrentUser);
                    SetContent(cellTypeViewModel);
                    SetNewNavigationItemAsSelected(selectedItem); 
                    return;

                case HamburgerItem.QualityControl:
                    var qualityControlViewModel = _viewModelFactory.CreateQualityControlViewModel();
                    SetContent(qualityControlViewModel);
                    SetNewNavigationItemAsSelected(selectedItem);
                    return;

                case HamburgerItem.Maintenance:
                    if (_isInstrumentSelection)
                    {
                        var isServiceUser = LoggedInUser.CurrentUser.RoleID.Equals(UserPermissionLevel.eService);
                        MessageBus.Default.Publish(new Notification<bool>(isServiceUser, nameof(ServiceViewModel), nameof(ServiceViewModel.IsManualControlsVisible)));
                        MessageBus.Default.Publish(new Notification<bool>(true, nameof(ServiceViewModel), nameof(ServiceViewModel.SelectedStandardCalibration)));
                        MessageBus.Default.Publish(new Notification<bool>(true, nameof(ServiceViewModel), nameof(ServiceViewModel.SelectedACupCalibration)));
                        MessageBus.Default.Publish(new Notification<bool>(false, nameof(ServiceViewModel), nameof(ServiceViewModel.SelectedManual)));

                        _isInstrumentSelection = false;
                    }
                    else
                    {
                        MessageBus.Default.Publish(new Notification<bool>(true, nameof(ServiceViewModel), nameof(ServiceViewModel.IsManualControlsVisible)));
                    }

                    MessageBus.Default.Publish(new Notification<bool>(true, nameof(ServiceViewModel), nameof(ServiceViewModel.SelectedManual)));
                    MessageBus.Default.Publish(new Notification<bool>(true, nameof(ServiceViewModel), nameof(ServiceViewModel.SelectedManualOptics)));

                    SetContent(_viewModelFactory.CreateServiceViewModel());

                    SetNewNavigationItemAsSelected(selectedItem);
                    return;

                case HamburgerItem.Reports:
                    var reportViewModel = _viewModelFactory.CreateReportsPanelViewModel();
                    if (_isInstrumentSelection)
                    {
                        reportViewModel.SelectedTabItem = 1;
                        _isInstrumentSelection = false;
                    }
                    SetContent(reportViewModel);
                    SetNewNavigationItemAsSelected(selectedItem);
                    return;

                case HamburgerItem.Settings:
                    SetContent(_viewModelFactory.CreateSettingsViewModel());
                    SetNewNavigationItemAsSelected(selectedItem);
                    return;

                case HamburgerItem.Signout:
                    selectedItem.IsItemSelected = false;
                    IsInActivityTimeOutActive = false;
                    TitleBarVm.ShowGenericLoginUserButton = true;
                    if (IsSecurityTurnedOn)
                    {
                        UserModel.LogOutUser();
                        OnLogOut();
                    }
                    else
                    {
                        UserModel.LogOutUser();
                        if (OnSilentAdminLogin())
                        {
                            OnSilentUserSuccessfullyLogin();
                            // Under the previous implementation posting this message was not of much of use
                            // as the MessageHub was being cleared unconditionally in the next line;
                            // currently this line is commented out as handling of MessageHub was removed
                            // from this class, but left here until the dust will settle over this change
                            PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_ServiceUserSignOut"));
                        }
                    }

                    // TitleBarVm.ClearMessageHub();
                    SetNewNavigationItemAsSelected(selectedItem);
                    return;

                case HamburgerItem.Lock:
                    SetLockScreen();
                    return;

                case HamburgerItem.Exit:
                    if (DialogEventBus.DialogBoxYesNo(this, 
                            LanguageResourceHelper.Get("LID_ERRMSGBOX_ExitApp")) != true)
                    {
                        return;
                    }

                    IsAdornerVisible = false;
                    _applicationStateService.PublishStateChange(ApplicationStateEnum.Shutdown);
                    return;
            }
        }

        private void OnSilentUserSuccessfullyLogin()
        {
            CurrentContent = new ContentControl {IsTabStop = false, Focusable = false};
            SetContent(_viewModelFactory.CreateHomeViewModel());
        }
        
        private void SetLockScreen()
        {
            _isUserLocked = true;
            IsInActivityTimeOutActive = false;

            var displayedUsername = LoggedInUser.CurrentUserId;
            var loginArgs = new LoginEventArgs(displayedUsername, displayedUsername, LoginState.LockScreen);
            var breakLoop = false;
            while (true)
            {
                LoginResult? result = DialogEventBus.Login(this, loginArgs);
                switch (result)
                {
                    case LoginResult.AdminUnlockFailed:
                    case LoginResult.CurrentUserLoginFailed:
                    case LoginResult.SwapUserLoginFailed:
                        DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_AcoountDisable"));
                        break;

                    case LoginResult.AdminUnlockSuccess:
                    case LoginResult.CurrentUserLoginSuccess:
                        breakLoop = true;
                        _isUserLocked = false;
                        IsInActivityTimeOutActive = true;
                        break;

                    case LoginResult.SwapUserLoginSuccess:
                        breakLoop = true;
                        _isUserLocked = false;
                        break;

                    case LoginResult.Cancelled:
                    case LoginResult.UserHasBeenDisabled:
                        breakLoop = true;
                        break;
                }

                if (breakLoop)
                {
                    TitleBarVm.NotifyAllPropertiesChanged();
                    break;
                }
            }
        }

        private void SetCultureForHamburger()
        {
            HamburgerItems.ToList().ForEach(s =>
            {
                switch (s.Item)
                {
                    case HamburgerItem.Home:
                        s.Title = LanguageResourceHelper.Get("LID_HB_Home");
                        break;
                    case HamburgerItem.CellTypes:
                        s.Title = LanguageResourceHelper.Get("LID_GridLabel_CellTypes");
                        break;
                    case HamburgerItem.QualityControl:
                        s.Title = LanguageResourceHelper.Get("LID_GridLabel_QualityControl");
                        break;
                    case HamburgerItem.Settings:
                        s.Title = LanguageResourceHelper.Get("LID_HB_Settings");
                        break;
                    case HamburgerItem.Maintenance:
                        s.Title = LanguageResourceHelper.Get("LID_HB_Maintenance");
                        break;
                    case HamburgerItem.Reports:
                        s.Title = LanguageResourceHelper.Get("LID_TabItem_Reports");
                        break;
                    case HamburgerItem.Signout:
                        s.Title = LanguageResourceHelper.Get("LID_HB_Signout");
                        break;
                    case HamburgerItem.Lock:
                        s.Title = LanguageResourceHelper.Get("LID_HB_Lock");
                        break;
                    case HamburgerItem.Exit:
                        s.Title = LanguageResourceHelper.Get("LID_HB_Exit");
                        break;
                }
            });
        }

        #endregion

        protected override void DisposeUnmanaged()
        {
            _lockStateSubscriber?.Dispose();
            _instrumentServiceStatusSubscriber?.Dispose();
            _applicationStateServiceSubscription?.Dispose();
            //probably a good place to clean up the ApiEventBroker
            ApiEventBroker.Instance.Dispose();
            base.DisposeUnmanaged();
        }

        #region Timer Stuff

        private void UpdateInactiveTimeOut()
        {
            if (!IsScreenLockActive)
            {
                SetCountDownInactivityTimeSpan();
                _userInactivityTimer.Start();
            }
        }

        private void UserInactivityTimerTick(object sender, EventArgs e)
        {
            try
            {
                if (IsInActivityTimeOutActive)
                {
                    TimeSpan elapsed = DateTime.Now - LastUserActivity;
                    var withinThritySecs = (elapsed.TotalSeconds + 30) >= InactivityTimeSpan.TotalSeconds;
                    if (withinThritySecs && !IsScreenLockActive)
                    {
                        IsScreenLockActive = true;
                        if (DialogEventBus.InactivityDialog(this, new InactivityEventArgs()) != true)
                        {
                            IsScreenLockActive = false;
                            SetCountDownInactivityTimeSpan();
                            SetLockScreen();
                            _logger.Debug("LogInactivityTimeOut:: screen locked due to inactivity for " + InactivityTimeSpan + " mins");
                        }
                        else
                        {
                            IsScreenLockActive = false;
                            UpdateInactiveTimeOut();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_USER_TIMER_ERROR"));
            }
        }

        private void SetCountDownInactivityTimeSpan()
        {
            _userInactivityTimer.Stop();
            LastUserActivity = DateTime.Now;
            using (var securitySettingsModel = new SecuritySettingsModel())
            {
                InactivityTimeSpan = TimeSpan.FromMinutes(securitySettingsModel.GetUserInactivityTimeout());
            }
        }

        #endregion
    }
}
