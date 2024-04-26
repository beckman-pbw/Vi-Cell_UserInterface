using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HawkeyeCoreAPI.Facade;
using Ninject;
using ScoutDataAccessLayer.DAL;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Service;
using ScoutModels.Settings;
using ScoutServices;
using ScoutServices.Interfaces;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using ScoutViewModels.Interfaces;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;

namespace ScoutViewModels.ViewModels.Home.ExpandedSampleWorkflow
{
    /// <summary>
    /// This is the sample template for a user. It is initially populated from the values
    /// define in the Settings, Run Options. The user may customize these settings and then
    /// create multiple sample sets to run. The OnUserChange method is implemented, causing
    /// this data to refresh with the newly logged in user's Run Options. The
    /// SampleTemplateUserControl is populated by this view model. The controls are editable.
    /// When samples are being run, this control is hidden, and the same control, but populated
    /// by the OrphanSampleTemplateViewModel is visible. When the system is in an Idle state,
    /// this control is visible and the orphan control is hidden.
    /// </summary>
    public class UserSampleTemplateViewModel : SampleTemplateViewModel
    {
        private readonly CellTypeFacade _cellTypeManager;
        private readonly ISettingsService _settingsService;
        private Subscription<Notification> _notificationMessageSubscriber;
        private Subscription<Notification<UserDomain>> _userChangedSubscriber;

        [Inject] 
        public UserSampleTemplateViewModel(CellTypeFacade cellTypeManager, ISettingsService settingsService, RunOptionSettingsModel runOptions, ISampleProcessingService sampleProcessingService, IScoutViewModelFactory viewModelFactory) :
            base(runOptions, sampleProcessingService, viewModelFactory,cellTypeManager)
        {
            _cellTypeManager = cellTypeManager;
            _settingsService = settingsService;
            SetForUser();
            _notificationMessageSubscriber = MessageBus.Default.Subscribe<Notification>(OnNotificationMessageReceived);
            _userChangedSubscriber = MessageBus.Default.Subscribe<Notification<UserDomain>>(OnUserChanged);
            IsEnabled = true;
        }

        //only used for unit tests
        public UserSampleTemplateViewModel(SettingsService settingsService, ObservableCollection<CellTypeQualityControlGroupDomain> qcCellTypes,
            CellTypeQualityControlGroupDomain qcCellType, uint dilution, ObservableCollection<SamplePostWash> washes,
            SamplePostWash wash, string tag, bool useSequencing, SequentialNamingSetViewModel sequentialNamingSet,
            AdvancedSampleSettingsViewModel advancedSampleSettings, RunOptionSettingsModel runOptions, UserDomain user, ISampleProcessingService sampleProcessingService, IScoutViewModelFactory viewModelFactory, CellTypeFacade cellTypeFacade) :
            base( qcCellTypes, qcCellType, dilution,  washes, wash, tag,  useSequencing, sequentialNamingSet,
            advancedSampleSettings,runOptions, user,  sampleProcessingService,  viewModelFactory, cellTypeFacade)
        {
           
        }
        protected override void DisposeUnmanaged()
        {
            SequentialNamingItems?.Dispose();
            MessageBus.Default.UnSubscribe(ref _notificationMessageSubscriber);
            MessageBus.Default.UnSubscribe(ref _userChangedSubscriber);
            base.DisposeUnmanaged();
        }

        public override object Clone()
        {
            var newObject = _viewModelFactory.CreateUserSampleTemplateViewModel();
            CopyValues(newObject);
            return newObject;
        }

        #region Event Handlers

        #endregion

        /// <summary>
        /// Initialize all properties based on the current user.
        /// </summary>
        public void SetForUser()
        {
            UpdateCellTypeProperties();

            var curUser = LoggedInUser.CurrentUser;
            var username = curUser?.UserID;
            var runOptionsModel = _settingsService.GetRunOptionSettingsModel(_runOptionSettingsModel.DataAccess, username);
            var runOptionsSettings = runOptionsModel.RunOptionsSettings;

            uint.TryParse(runOptionsSettings.DefaultDilution, out var dilution);
            Dilution = dilution;
            WashTypes = Enum.GetValues(typeof(SamplePostWash)).Cast<SamplePostWash>().ToObservableCollection();
            WashType = runOptionsSettings.DefaultWash;
            SampleTag = string.Empty;
            UseSequencing = false;
            SequentialNamingItems = new SequentialNamingSetViewModel(runOptionsSettings.DefaultSampleId);
            AdvancedSampleSettings = new AdvancedSampleSettingsViewModel(runOptionsModel);
            User = curUser;

            NotifyPropertyChanged(nameof(DisplayedSampleName));

        }

        /// <summary>
        /// All values are replaced with the current logged in user's Run Options when the OnUserChanged method is called.
        /// </summary>
        /// <param name="msg"></param>
        private void OnUserChanged(Notification<UserDomain> msg)
        {
            if (string.IsNullOrEmpty(msg?.Token)) return;
            if (msg.Token.Equals(NotificationClasses.NewCurrentUser) && LoggedInUser.IsConsoleUserLoggedIn())
            {
                SetForUser();
            }
        }

        private void OnNotificationMessageReceived(Notification msg)
        {
            if (string.IsNullOrEmpty(msg?.Token)) return;

            if (msg.Token.Equals(MessageToken.CellTypesUpdated) || msg.Token.Equals(MessageToken.QualityControlsUpdated))
            {
                DispatcherHelper.ApplicationExecute(UpdateCellTypeProperties);
            }
        }

        private void UpdateCellTypeProperties()
        {
            var runOptionsSettings = _settingsService.GetRunOptions(_runOptionSettingsModel.DataAccess);
            var selectedName = QcCellType?.Name;
            List<CellTypeDomain> _cellTypes = new List<CellTypeDomain>();
            List<QualityControlDomain> _qcs = new List<QualityControlDomain>();
            List<CellTypeQualityControlGroupDomain> ctqcgroup = new List<CellTypeQualityControlGroupDomain>();
            CellTypeFacade.Instance.GetAllowedCtQc(LoggedInUser.CurrentUserId, ref _cellTypes, ref _qcs, ref ctqcgroup);
            QcCellTypes = ctqcgroup.ToObservableCollection();
            if (!string.IsNullOrEmpty(selectedName))
                QcCellType = QcCellTypes.GetCellTypeQualityControlByName(selectedName);
            QcCellType = QcCellTypes.GetCellTypeQualityControlByIndex(runOptionsSettings.DefaultBPQC);
            if (null != QcCellType)
            {
                QcCellType.IsSelectionActive = QcCellType != null;
            }
            else if (QcCellTypes.Count > 0)
            {
                QcCellType = QcCellTypes.First().CellTypeQualityControlChildItems.FirstOrDefault(c => c.CellTypeIndex == (uint)CellTypeIndex.BciDefault);
            }
        }
    }
}