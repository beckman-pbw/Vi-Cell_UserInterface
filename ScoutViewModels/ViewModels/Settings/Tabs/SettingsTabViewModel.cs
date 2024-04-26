using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutServices.Enums;
using ScoutServices.Interfaces;
using ScoutServices.Watchdog;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutViewModels.Interfaces;
using ScoutModels.Interfaces;
using ScoutViewModels.ViewModels.Tabs.SettingsPanel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;


namespace ScoutViewModels.ViewModels.Tabs
{
    public class SettingsTabViewModel : BaseViewModel
    {
        private IWatchdog _watchDog;
        private ILockManager _lockManager;
        private IOpcUaCfgManager _opcUaCfgManager;
        private readonly IInstrumentStatusService _instrumentStatusService;

        #region Constructor

        public SettingsTabViewModel(IScoutViewModelFactory viewModelFactory, IWatchdog watchDog, ILockManager lockManager, IOpcUaCfgManager opcUaCfgManager, IInstrumentStatusService instrumentStatusService) : base()
        {
            _viewModelFactory = viewModelFactory;
            _watchDog = watchDog;
            _lockManager = lockManager;
            _lockStateSubscriber = _lockManager.SubscribeStateChanges().Subscribe(LockStatusChanged);
            _opcUaCfgManager = opcUaCfgManager;
            _instrumentStatusService = instrumentStatusService;

            IsSingleton = true;
            CurrentSettingContent = new ContentControl {Focusable = false, IsTabStop = false, };

            _systemStatusSubscriber = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe((OnSystemStatusChanged));
            _languageSettingsSubscriber = MessageBus.Default.Subscribe<LanguageSettingsViewModel>(OnLanguageSettingsChanged);
        }

        protected override void DisposeUnmanaged()
        {
            if (SettingsPanels != null)
            {
                foreach (var panel in SettingsPanels)
                    panel.Dispose();
            }
            _systemStatusSubscriber?.Dispose();
            _lockStateSubscriber?.Dispose();
            MessageBus.Default.UnSubscribe(ref _languageSettingsSubscriber);
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties & Fields

        private readonly IScoutViewModelFactory _viewModelFactory;

        private Subscription<LanguageSettingsViewModel> _languageSettingsSubscriber;
        private IDisposable _systemStatusSubscriber;
        private IDisposable _lockStateSubscriber;

        public ObservableCollection<BaseSettingsPanel> SettingsPanels
        {
            get { return GetProperty<ObservableCollection<BaseSettingsPanel>>(); }
            set { SetProperty(value); }
        }

        public BaseSettingsPanel SelectedSettingsPanel
        {
            get { return GetProperty<BaseSettingsPanel>(); }
            set
            {
                SetProperty(value);
                CurrentSettingContent.Content = value;
                value?.SetDefaultSettings();
            }
        }

        public ContentControl CurrentSettingContent
        {
            get { return GetProperty<ContentControl>(); }
            set { SetProperty(value); }
        }

        public UserDomain SelectedUser
        {
            get { return GetProperty<UserDomain>(); }
            set { SetProperty(value); }
        }

        public bool IsPanelEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Event Handlers

        public void OnViewLoaded()
        {
            var panels = new List<BaseSettingsPanel>();
            panels.Add(_viewModelFactory.CreateInstrumentSettingsViewModel());
            panels.Add(_viewModelFactory.CreateLanguageSettingsViewModel());
            panels.Add(_viewModelFactory.CreateRunOptionSettingsViewModel());
            if (IsSecurityTurnedOn)
            {
                panels.Add(_viewModelFactory.CreateSignatureSettingsViewModel());
            }

            SettingsPanels = new ObservableCollection<BaseSettingsPanel>(panels);
        }

        private void OnLanguageSettingsChanged(LanguageSettingsViewModel vm)
        {
            foreach (BaseSettingsPanel s in SettingsPanels)
                s.UpdateListItemLabel();
            NotifyPropertyChanged(nameof(SettingsPanels));
        }

        private void OnSystemStatusChanged(SystemStatusDomain sysStatus)
        {
            if (sysStatus == null) return;

            IsPanelEnable = _instrumentStatusService.IsNotRunning() && !_lockManager.IsLocked();
        }

        private void LockStatusChanged(LockResult res)
        {
            if (res == LockResult.Locked)
            {
                DispatcherHelper.ApplicationExecute(() =>
                {
                    SelectedSettingsPanel = null;
                });
            }
        }
        #endregion
    }
}