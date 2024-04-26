using System;
using ScoutDomains;
using ScoutServices.Watchdog;
using ScoutUtilities;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.UIConfiguration;
using ScoutViewModels.ViewModels.Tabs;
using ScoutServices.Interfaces;
using ScoutViewModels.Interfaces;
using ScoutModels.Interfaces;

namespace ScoutViewModels.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel(IWatchdog watchdog, ILockManager lockManager, IOpcUaCfgManager opcUaCfgManager, IInstrumentStatusService instrumentStatusService, IScoutViewModelFactory viewModelFactory)
        {
            _watchDog = watchdog;
            _lockManager = lockManager;
            _viewModelFactory = viewModelFactory;
            _opcUaCfgManager = opcUaCfgManager;
            _instrumentStatusService = instrumentStatusService;
            IsSingleton = true;
            SecurityTabViewModel = new SecurityTabViewModel();
            StorageTabViewModel = viewModelFactory.CreateStorageTabViewModel();
            SettingsTabViewModel = viewModelFactory.CreateSettingsTabViewModel();
            ShowWebBrowserElement = true;
            _mainNavMenuSubscriber = MessageBus.Default.Subscribe<Notification<bool>>(OnMainNavMenuOpenClose);
            _systemStatusSubscriber = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe(OnSystemStatusChanged);
            SelectedTabItem = SettingsTab.Settings;
        }

        protected override void DisposeUnmanaged()
        {
            StorageTabViewModel?.Dispose();
            SettingsTabViewModel?.Dispose();
            SecurityTabViewModel?.Dispose();
            MessageBus.Default.UnSubscribe(ref _mainNavMenuSubscriber);
            _systemStatusSubscriber?.Dispose();
            base.DisposeUnmanaged();
        }

        #region Properties & Fields

        public string HelpHtmlPath => UISettings.HelpHtmlPath;
        public string LicensesHtmlPath => UISettings.LicensesHtmlPath;
        public string UiSoftwareVersion => Misc.GetUiVersionString();

        private IWatchdog _watchDog;
        private ILockManager _lockManager;
        private readonly IScoutViewModelFactory _viewModelFactory;
        private IOpcUaCfgManager _opcUaCfgManager;
        private IInstrumentStatusService _instrumentStatusService;

        private IDisposable _systemStatusSubscriber;
        private Subscription<Notification<bool>> _mainNavMenuSubscriber;

        public SecurityTabViewModel SecurityTabViewModel
        {
            get { return GetProperty<SecurityTabViewModel>(); }
            set { SetProperty(value); }
        }

        public StorageTabViewModel StorageTabViewModel
        {
            get { return GetProperty<StorageTabViewModel>(); }
            set { SetProperty(value); }
        }

        public SettingsTabViewModel SettingsTabViewModel
        {
            get { return GetProperty<SettingsTabViewModel>(); }
            set { SetProperty(value); }
        }

        public SettingsTab SelectedTabItem
        {
            get { return GetProperty<SettingsTab>(); }
            set { SetProperty(value); }
        }

        public bool ShowWebBrowserElement
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsTabEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value);}
        }

        #endregion

        #region Event Handlers

        private void OnMainNavMenuOpenClose(Notification<bool> obj)
        {
            if (string.IsNullOrEmpty(obj?.Token) || obj.Token != MessageToken.MainNavMenuOpenCloseToken) return;
            ShowWebBrowserElement = !obj.Target;
        }

        private void OnSystemStatusChanged(SystemStatusDomain sysStatus)
        {
            if (sysStatus == null) return;
            IsTabEnable = (_instrumentStatusService.IsNotRunning() || SystemStatus.Paused == sysStatus.SystemStatus) &&
                          !_lockManager.IsLocked();
            if(_lockManager.IsLocked())
            {
                SelectedTabItem = SettingsTab.Settings;
            }
        }

        #endregion
    }
}