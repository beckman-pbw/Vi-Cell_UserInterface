using ScoutDataAccessLayer.IDAL;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.Common;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.ExpandedSampleWorkflow;
using ScoutModels.Review;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Interfaces;
using ScoutUtilities.Structs;
using ScoutViewModels.Common.Helper;
using ScoutViewModels.ViewModels.Common;
using ScoutViewModels.ViewModels.Home.ExpandedSampleWorkflow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using HawkeyeCoreAPI;
using System.Threading.Tasks;
using ScoutDataAccessLayer.DAL;
using ScoutServices.Interfaces;
using ScoutViewModels.Interfaces;
using ScoutServices.Enums;
using ScoutModels.Interfaces;
using ScoutModels.Settings;
using HawkeyeCoreAPI.Facade;
using SampleSet = HawkeyeCoreAPI.SampleSet;
using SystemStatus = ScoutUtilities.Enums.SystemStatus;

namespace ScoutViewModels.ViewModels.ExpandedSampleWorkflow
{
    public class SampleSetViewModel : BaseViewModel, IEquatable<SampleSetViewModel>, IUpdateColumns
    {
        #region Constructor & Dispose

        // ToDo: Should we remove the sampleSetName? It is defaulted to the application default language value and not the one for the user.
        public SampleSetViewModel(ILockManager lockManager, IInstrumentStatusService instrumentStatusService, IScoutViewModelFactory viewModelFactory, ResultRecordHelper resultRecordHelper, 
            SampleTemplateViewModel template, string createdByUser, string runByUser, string sampleSetName, bool isOrphanSet = false, bool isExpanded = false)
        {
            _lockManager = lockManager;
            _lockStateSubscriber = _lockManager.SubscribeStateChanges().Subscribe(LockStatusChanged);
            _instrumentStatusService = instrumentStatusService;
            _viewModelFactory = viewModelFactory;
            _isExpanded = isExpanded;
            Samples = new ObservableCollection<SampleViewModel>();
            SubstrateType = ScoutUtilities.Enums.SubstrateType.Carousel;
            PlatePrecession = Precession.RowMajor;
            SampleTemplate = (SampleTemplateViewModel) template.Clone();
            CreatedByUser = createdByUser;
            RunByUser = runByUser;
            SampleSetName = sampleSetName;
            IsOrphanSet = isOrphanSet;
            DateTimeStarted = DateTime.Now;
            RunningImagesAreDisplayed = false;
            ImageViewModel = new ImageViewModel(resultRecordHelper);
            ColumnSettings = ColumnSettingViewModel.GetAll();
            NotifyPropertyChanged(nameof(IsExpanderEnabled));
            NotifyPropertyChanged(nameof(IsExpanded));

            _statusSubscriber = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe(OnSystemStatusChanged);
        }

        public SampleSetViewModel(ILockManager lockManager, IInstrumentStatusService instrumentStatusService,
            IScoutViewModelFactory viewModelFactory, SampleSetDomain setDomain, bool isOrphanSet, bool isExpanded = false, bool createForAutomationSampleSet = false)
        {
            _lockManager = lockManager;
            _lockStateSubscriber = _lockManager.SubscribeStateChanges().Subscribe(LockStatusChanged);
            _instrumentStatusService = instrumentStatusService;
            _viewModelFactory = viewModelFactory;
            _isExpanded = isExpanded;
            IsOrphanSet = isOrphanSet;

            Uuid = setDomain.Uuid;
            SampleSetName = setDomain.SampleSetName;
            CreatedByUser = setDomain.Username;
            RunByUser = setDomain.Username;
            DateTimeStarted = DateTimeConversionHelper.FromSecondUnixToDateTime(setDomain.Timestamp);
            SampleSetStatus = setDomain.SampleSetStatus;
            SubstrateType = setDomain.Carrier;
            SetIndex = setDomain.Index;

            RunningImagesAreDisplayed = false;
            var resultRecordHelper = new ResultRecordHelper(RunByUser);
            ImageViewModel = new ImageViewModel(resultRecordHelper);
            ColumnSettings = ColumnSettingViewModel.GetAll();
            Samples = new ObservableCollection<SampleViewModel>();
            foreach (var sampleDomain in setDomain.Samples)
            {
                if (sampleDomain.SampleRecord != null && sampleDomain.SampleRecord.ResultSummaryList.Count > 1)
                {
                    var flatSampleResults = sampleDomain.SampleRecord.FlattenResultSummaries_wrappedInDuplicateSamples();
                    ReviewModel.SetSampleHierarchy(flatSampleResults);
                    foreach (var result in flatSampleResults)
                    {
                        var s = new SampleViewModel(result, sampleDomain, setDomain.SampleSetName, result.SampleHierarchy,
                            resultRecordHelper.RunOptionModel);
                        Samples.Add(s);
                    }
                }
                else
                {
                    if (createForAutomationSampleSet)
                    {
                        var runOptions = new RunOptionSettingsModel(XMLDataAccess.Instance, setDomain.Username);
                        var ctQcList = new List<CellTypeQualityControlGroupDomain>();
                        var ctList = new List<CellTypeDomain>();
                        var qcList = new List<QualityControlDomain>();
                        CellTypeFacade.Instance.GetAllowedCtQc(setDomain.Username, ref ctList, ref qcList, ref ctQcList);
                        Samples.Add(new SampleViewModel(sampleDomain, setDomain.SampleSetName, true, runOptions, ctQcList));
                    }
                    else
                    {
                        Samples.Add(new SampleViewModel(sampleDomain,
                            setDomain.SampleSetName, false, resultRecordHelper.RunOptionModel));
                    }
                }
            }

            NotifyPropertyChanged(nameof(IsExpanderEnabled));
            NotifyPropertyChanged(nameof(IsExpanded));

            _statusSubscriber = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe((OnSystemStatusChanged));
        }

        protected override void DisposeUnmanaged()
        {
            _statusSubscriber?.Dispose();
            _lockStateSubscriber?.Dispose();
            ImageViewModel?.Dispose();
            if (Samples != null)
            {
                foreach (var view in Samples)
                    view.Dispose();
            }
            if (ColumnSettings != null)
            {
                foreach (var value in ColumnSettings)
                    value.Value.Dispose();
            }
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties & Fields
        private bool _isExpanded;

        private bool _sampleSetApiCalled;

        private readonly ILockManager _lockManager;
        private readonly IInstrumentStatusService _instrumentStatusService;
        private readonly IScoutViewModelFactory _viewModelFactory;

        private IDisposable _statusSubscriber;
        private IDisposable _lockStateSubscriber;

        private SystemStatus _systemStatus;

        public ushort SetIndex { get; private set; } // the identifier used before the Uuid is generated in the backend

        public bool IsVisible => !IsOrphanSet || (IsOrphanSet && Samples.Any());

        public uuidDLL Uuid
        {
            get { return GetProperty<uuidDLL>(); }
            set { SetProperty(value); }
        }

        public string SampleSetName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsOrphanSet
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(IsVisible));
                CancelSet.RaiseCanExecuteChanged();
            }
        }

        public DateTime DateTimeStarted
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public string CreatedByUser
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string RunByUser
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public SampleSetStatus SampleSetStatus
        {
            get { return GetProperty<SampleSetStatus>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(ShowCancelButton));
                DispatcherHelper.ApplicationExecute(() => CancelSet.RaiseCanExecuteChanged());

                ShowImagesToggleButton = value != SampleSetStatus.Cancelled && value != SampleSetStatus.Complete;
            }
        }

        public bool ShowImagesToggleButton
        {
            get { return GetProperty<bool>(); }
            set
            {
                var changed = value != GetProperty<bool>();
                SetProperty(value);
                if (changed)
                {
                    if (!value) RunningImagesAreDisplayed = false;
                    ImageViewModel?.NotifyAllPropertiesChanged();
                }
            }
        }

        public SubstrateType SubstrateType
        {
            get { return GetProperty<SubstrateType>(); }
            set { SetProperty(value); }
        }

        public Precession PlatePrecession
        {
            get { return GetProperty<Precession>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<SampleViewModel> Samples
        {
            get { return GetProperty<ObservableCollection<SampleViewModel>>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(IsVisible));
                CancelSet.RaiseCanExecuteChanged();
            }
        }

        public SampleViewModel SelectedSample
        {
            get { return GetProperty<SampleViewModel>(); }
            set
            {
                SetProperty(value);
                UpdateButtons();
                NotifyPropertyChanged(nameof(ShowCancelButton));
            }
        }

        public SampleTemplateViewModel SampleTemplate
        {
            get { return GetProperty<SampleTemplateViewModel>(); }
            set { SetProperty(value); }
        }

        public bool RunningImagesAreDisplayed
        {
            get { return GetProperty<bool>(); }
            set
            {
                var changed = GetProperty<bool>() != value;
                SetProperty(value);

                if (changed)
                {
                    MessageBus.Default.Publish(new Notification<bool>(value, MessageToken.ShowRunningImagesToggleButtonToken));
                }
            }
        }

        public bool ShowCancelButton
        {
            get
            {
                return SampleSetStatus == SampleSetStatus.Paused ||
                       SampleSetStatus == SampleSetStatus.Pending ||
                       SampleSetStatus == SampleSetStatus.Running;

            }
        }

        /// <summary>
        /// For a normal user:
        /// <ol>
        /// <li>All other idle samples from other users should not be displayed.</li>
        /// <li>All other running or pending samples from other users should be collapsed and not expandable.</li>
        /// </ol>
        /// </summary>
        public bool IsExpanderEnabled
        {
            get
            {
                return LoggedInUser.IsConsoleUserLoggedIn();
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded && IsExpanderEnabled;
            set
            {
                _isExpanded = value;
                NotifyPropertyChanged(nameof(IsExpanded));
                if (value)
                {
                    OnSampleSetExpanded();
                }
            }
        }

        public ImageViewModel ImageViewModel
        {
            get { return GetProperty<ImageViewModel>(); }
            set { SetProperty(value); }
        }

        public Dictionary<string, ColumnSettingViewModel> ColumnSettings
        {
            get { return GetProperty<Dictionary<string, ColumnSettingViewModel>>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Enabling the expander depends on the logged in user and whether the control is enabled is dependent on whether the expander is enabled.
        /// </summary>
        public override void OnUserChanged(UserDomain newUser)
        {
            base.OnUserChanged(newUser);
            if (null != newUser)
            {
                NotifyPropertyChanged(nameof(IsExpanderEnabled));
                NotifyPropertyChanged(nameof(IsExpanded));
            }
        }

        private void OnSystemStatusChanged(SystemStatusDomain systemStatus)
        {
            _systemStatus = systemStatus.SystemStatus;
            DispatcherHelper.ApplicationExecute(() =>
            {
                SignSelectedSample.RaiseCanExecuteChanged();
                CancelSet.RaiseCanExecuteChanged();
            });
        }

        private async void OnSampleSetExpanded()
        {
            var setIsComplete = SampleSetStatus == SampleSetStatus.Complete || SampleSetStatus == SampleSetStatus.Cancelled;
            if (_sampleSetApiCalled || !setIsComplete) return; // no need to call the backend

            try
            {
                MainWindowViewModel.Instance.IsAdornerVisible = true;

                var ssvm = await SampleSetHelper.GetAsync(Uuid, _viewModelFactory);
                _sampleSetApiCalled = true;

                IsOrphanSet = false;
                ShowImagesToggleButton = false;
                RunningImagesAreDisplayed = false;

                if (ssvm != null)
                {
                    SampleSetName = ssvm.SampleSetName;
                    DateTimeStarted = ssvm.DateTimeStarted;
                    CreatedByUser = ssvm.CreatedByUser;
                    RunByUser = ssvm.RunByUser;
                    SampleSetStatus = ssvm.SampleSetStatus;
                    SubstrateType = ssvm.SubstrateType;
                    PlatePrecession = ssvm.PlatePrecession;
                    Samples = ssvm.Samples;
                    SelectedSample = Samples.FirstOrDefault(s => s.Uuid.Equals(SelectedSample.Uuid)); // select the same sample as before
                }
                else
                {
                    PostToMessageHub(ScoutLanguageResources.LanguageResourceHelper.Get("LID_Report_NoData"), MessageType.Warning);
                }

                UpdateButtons();
            }
            finally
            {
                MainWindowViewModel.Instance.IsAdornerVisible = false;
            }
        }

        #endregion

        #region Commands

        #region Show Sample Results Command

        private RelayCommand _showSampleResultsCommand;
        public RelayCommand ShowSampleResultsCommand => _showSampleResultsCommand ?? (_showSampleResultsCommand = new RelayCommand(ShowSampleResultsExecute, CanShowSampleResults));

        private bool CanShowSampleResults()
        {
            return Samples != null && Samples.Any() && SelectedSample != null &&
                   SelectedSample.SampleStatus == SampleStatus.Completed;
        }

        private async void ShowSampleResultsExecute()
        {
            SetLoadingIndicator(true);
            try
            {
                await Task.Run(ShowSampleResults);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
            finally
            {
                SetLoadingIndicator(false);
            }
        }

        private void ShowSampleResults()
        {
            var sampleClone = (SampleViewModel) SelectedSample.Clone();
            var result = DialogEventBus<SampleViewModel>.SampleResultsDialog(this,
                new SampleResultDialogEventArgs<SampleViewModel>(sampleClone, _instrumentStatusService.SystemStatus));
            if (result == true)
            {
                SelectedSample.Reload();
                var args = new SampleSetChangedEventArgs(Uuid, SampleSetName);
                MessageBus.Default.Publish(new Notification<SampleSetChangedEventArgs>(args, MessageToken.SampleSetChanged));
            }
            SetLoadingIndicator(false);
        }

        #endregion

        #region Export Selected Sample Command

        private RelayCommand _exportSelectedSampleCommand;
        public RelayCommand ExportSelectedSampleCommand => _exportSelectedSampleCommand ?? (_exportSelectedSampleCommand = new RelayCommand(ExportSelectedSample, CanExportSelectedSample));

        private bool CanExportSelectedSample()
        {
            try
            {
                if ((Samples == null) || !Samples.Any())
                    return false;
                if (SelectedSample?.SampleRecord?.SelectedResultSummary == null)
                    return false;
                if (string.IsNullOrEmpty(SelectedSample.SampleRecord.SampleIdentifier))
                    return false;

                return SelectedSample.SampleStatus == SampleStatus.Completed;
            }
            catch (Exception e)
            {
                Log.Warn("CanExportSelectedSample exception", e);
            }
            return false;
        }

        private void ExportSelectedSample()
        {
            try
            {
                if (SelectedSample?.SampleRecord?.SelectedResultSummary == null)
                    return;
                if (string.IsNullOrEmpty(SelectedSample.SampleRecord.SampleIdentifier))
                    return;
                var sampleFileName = SelectedSample.SampleRecord.SampleIdentifier;
                var dateStr = Misc.ConvertToFileNameFormat(SelectedSample.SampleRecord.SelectedResultSummary.RetrieveDate);
                var defaultFileName = $"{sampleFileName}_{dateStr}";

                var sampleList = new List<SampleRecordDomain> { SelectedSample.SampleRecord };
                var first = ResultRecordHelper.ExportCompleteRunResult(sampleList).FirstOrDefault();

                ExportModel.PromptAndExportSampleToCsv(first, SelectedSample.SampleRecord, defaultFileName);
            }
            catch (Exception e)
            {
                Log.Warn("ExportSelectedSample exception", e);
            }
        }

        #endregion

        #region Export Sample Set Command

        private RelayCommand _exportSampleSetCommand;
        public RelayCommand ExportSampleSetCommand => _exportSampleSetCommand ?? (_exportSampleSetCommand = new RelayCommand(ExportSampleSet, CanExportSampleSet));

        private bool CanExportSampleSet()
        {
            return Samples != null && Samples.Any() && 
                   Samples.All(s => s.SampleStatus == SampleStatus.Completed ||
                                    s.SampleStatus == SampleStatus.SkipError ||
                                    s.SampleStatus == SampleStatus.SkipManual);
        }

        private void ExportSampleSet()
        {
            var setFileName = SampleSetName;
            var dateFileName = Misc.ConvertToFileNameFormat(DateTime.Now);
            var defaultFileName = $"{ApplicationConstants.SummaryExportFileNameAppendant}{setFileName}_{dateFileName}";

            var sampleList = Samples.Select(s => s.SampleRecord).ToList();
            if ((sampleList == null) || sampleList.Count == 0)
            {
                Log.Error("ExportSampleSet no samples found");
                ExportModel.ExportFailedMessage();
                return;
            }

            var records = ResultRecordHelper.ExportCompleteRunResult(sampleList);
            if ((records == null) || records.Count == 0)
            {
                Log.Error("ExportSampleSet no records found");
                ExportModel.ExportFailedMessage();
                return;
            }

            string filename = ExportModel.PromptAndExportSamplesToCsv(ref sampleList, defaultFileName);
            if (!String.IsNullOrEmpty(filename))
            {
                var outdir = Path.GetDirectoryName(filename);
                var summaryCsvFilename = Path.GetFileNameWithoutExtension(filename);

                ExportManager.EvExportCsvReq.Publish(LoggedInUser.CurrentUserId, "", sampleList,
                    outdir, outdir, summaryCsvFilename, "", true, false, false, false);

                return;
            }
        }

        #endregion

        #region Sign Selected Sample Command

        private RelayCommand _signSelectedSample;
        public RelayCommand SignSelectedSample => _signSelectedSample ?? (_signSelectedSample = new RelayCommand(PerformSignSample, CanPerformSignSample));

        private bool CanPerformSignSample()
        {
            return IsSecurityTurnedOn &&
                   SelectedSample != null &&
                   SelectedSample.SampleStatus == SampleStatus.Completed &&
                   !LoggedInUser.CurrentUserId.Equals(ApplicationConstants.ServiceUser);
        }

        private void PerformSignSample()
        {
            var args = new AddSignatureEventArgs(ReviewModel.RetrieveSignatureDefinitions());
            if (DialogEventBus.AddSignature(this, args) == true)
            {
                OnAddSignature(args.SignatureSelected);
            }
        }

        private void OnAddSignature(ISignature selectedSign)
        {
            var signStatus = ReviewModel.SignResultRecord(SelectedSample.SampleRecord.SelectedResultSummary.UUID,
                selectedSign.SignatureIndicator, (ushort)selectedSign.SignatureIndicator.Length);
            if (signStatus.Equals(HawkeyeError.eSuccess))
            {
                var resultSummary = ReviewModel.RetrieveResultSummary(SelectedSample.SampleRecord.SelectedResultSummary.UUID);
                SelectedSample.SampleRecord.SelectedResultSummary.SignatureList = resultSummary.SignatureList;
                SelectedSample.SampleRecord.SelectedResultSummary.SelectedSignature = resultSummary.SignatureList.Last();

                // todo: check these events and see which ones are actually necessary after the backend work is done
                SelectedSample.SampleRecord.SelectedResultSummary.NotifyAllPropertiesChanged(false);
                SelectedSample.SampleRecord.NotifyAllPropertiesChanged(false);
                SelectedSample.NotifyAllPropertiesChanged(false);
                NotifyAllPropertiesChanged();

                var str = LanguageResourceHelper.Get("LID_StatusBar_ResultRecordSigned");
                PostToMessageHub(str);
                Log.Debug(str);
            }
            else
            {
                DisplayErrorDialogByApi(signStatus);
            }
        }

        #endregion

        #region Cancel Set Command

        private RelayCommand _cancelSet;
        public RelayCommand CancelSet => _cancelSet ?? (_cancelSet = new RelayCommand(PerformCancelSet, CanPerformCancelSet));

        private bool CanPerformCancelSet()
        {
            var isAutomationLocked = _lockManager.IsLocked();
            var userHasPermission = LoggedInUser.CurrentUserId.Equals(CreatedByUser) ||
                                    LoggedInUser.CurrentUserRoleId != UserPermissionLevel.eNormal;
            return userHasPermission &&
                   !IsOrphanSet &&
                   (Samples.Any(s => (s.SampleStatus > SampleStatus.NotProcessed) && (s.SampleStatus < SampleStatus.Completed)) ||
                    SampleSetStatus == SampleSetStatus.Pending && Samples.All(s => s.SampleStatus == SampleStatus.NotProcessed) ||
                    _systemStatus == SystemStatus.Paused || _systemStatus == SystemStatus.Idle) &&
                   _systemStatus != SystemStatus.Pausing &&
                   !isAutomationLocked; // don't allow the GUI to cancel the automation sample set 
        }

        private void PerformCancelSet()
        {
            var args = new SampleSetChangedEventArgs(Uuid, SetIndex, SampleSetName);
            var msg = new Notification<SampleSetChangedEventArgs>(args, MessageToken.SampleSetChanged, MessageToken.CancelSampleSet);
            MessageBus.Default.Publish(msg); // HomeViewModel will handle the cancellation
        }

        #endregion

        #region Show Running Images Command

        private RelayCommand _showRunningImagesCommand;
        public RelayCommand ShowRunningImagesCommand => _showRunningImagesCommand ?? (_showRunningImagesCommand = new RelayCommand(ShowRunningImages));

        private void ShowRunningImages()
        {
            
        }

        #endregion

        #region Open Data Columns Settings Command

        private RelayCommand _openDataColumnsSettingsCommand;
        public RelayCommand OpenDataColumnsSettingsCommand => _openDataColumnsSettingsCommand ??
                                                              (_openDataColumnsSettingsCommand = new RelayCommand(OpenDataColumnSettings));

        private void OpenDataColumnSettings()
        {
            var args = new SampleSetSettingsDialogEventArgs<Dictionary<string, ColumnSettingViewModel>>(ColumnSettings);
            var result = DialogEventBus<Dictionary<string, ColumnSettingViewModel>>.SampleSetSettingsDialog(this, args);
            if (result == true)
            {
                // changes were made -- update the datagrid
                ColumnSettings = args.ColumnSettingViewModelDictionary;
            }
        }

        #endregion

        #endregion

        #region Methods

        public void UpdateIsVisible()
        {
            NotifyPropertyChanged(nameof(IsVisible));
        }

        public void UpdateButtons()
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                ShowSampleResultsCommand.RaiseCanExecuteChanged();
                ExportSampleSetCommand.RaiseCanExecuteChanged();
                ExportSelectedSampleCommand.RaiseCanExecuteChanged();
                SignSelectedSample.RaiseCanExecuteChanged();
                CancelSet.RaiseCanExecuteChanged();
                ShowRunningImagesCommand.RaiseCanExecuteChanged();
                OpenDataColumnsSettingsCommand.RaiseCanExecuteChanged();
            });
        }

        public void UpdateColumns(string propertyName, double newColWidth)
        {
            if (ColumnSettings.ContainsKey(propertyName))
            {
                ColumnSettings[propertyName].ColumnWidth = newColWidth;
                if (!ColumnSettingsModel.SaveColumnSetting(LoggedInUser.CurrentUserId, propertyName, ColumnSettings[propertyName].IsVisible, newColWidth))
                {
                    Log.Warn($"Failed to save column setting. propertyName: {propertyName}; newColWidth: {newColWidth}");
                    PostToMessageHub(LanguageResourceHelper.Get("LID_Warning_FailedToSaveColumnSetting"), MessageType.Warning);
                }
            }
        }

        public void SetLoadingIndicator(bool status)
        {
            MessageBus.Default.Publish(new Notification<bool>(status, MessageToken.AdornerVisible));
        }

        public override string ToString()
        {
            return Misc.ObjectToString(this);
        }

        public SampleViewModel GetSampleViewModel(SampleEswDomain sample)
        {
            var sampleVmForUuid = Samples.FirstOrDefault(s => s.Uuid.Equals(sample.Uuid));
            if (sampleVmForUuid != null)
                return sampleVmForUuid;

            var sampleVmForIndex = Samples.FirstOrDefault(s => s.SampleIndex == sample.Index);
            return sampleVmForIndex;
        }


        public SampleSetDomain GetSampleSetDomain(IDataAccess dataAccess, string username)
        {
            var set = new SampleSetDomain();

            set.Uuid = Uuid;
            set.Carrier = SubstrateType;
            set.Index = SetIndex;
            set.SampleSetName = SampleSetName;
            set.SampleSetStatus = SampleSetStatus;
            set.Username = username;
            set.Timestamp = DateTimeConversionHelper.DateTimeToUnixSecondRounded(DateTimeStarted);
            
            set.Samples = new List<SampleEswDomain>();
            foreach (var sample in Samples)
            {
                set.Samples.Add(sample.GetSampleEswDomain(dataAccess, username));
            }

            return set;
        }

        public void Start()
        {
            foreach (SampleViewModel s in Samples)
            {
                if (s != null) s.SampleStatus = SampleStatus.NotProcessed;
            }
            SampleSetStatus = SampleSetStatus.Running;
        }

        public void Pause()
        {
            SampleSetStatus = SampleSetStatus.Paused;
        }

        public void UnPause()
        {
            SampleSetStatus = SampleSetStatus.Running;
        }

        public void SkipRemaining()
        {
            foreach (var sample in Samples)
            {
                if (sample.SampleStatus == SampleStatus.NotProcessed)
                {
                    sample.SampleStatus = SampleStatus.SkipManual;
                }
            }

            SampleSetStatus = Samples.All(s => s.SampleStatus == SampleStatus.SkipManual)
                ? SampleSetStatus.Cancelled
                : SampleSetStatus.Complete;
        }

        public void CancelSampleSet()
        {
            if (SubstrateType == SubstrateType.Carousel || SubstrateType == SubstrateType.Plate96)
            {
                foreach (var sample in Samples)
                {
                    if (sample.SampleStatus == SampleStatus.NotProcessed)
                    {
                        CarouselModel.Instance.Remove(sample.SamplePosition.Column);
                        sample.SampleStatus = SampleStatus.SkipManual;
                        sample.NotifyAllPropertiesChanged();
                    }
                }

                SampleSetStatus = Samples.All(s => s.SampleStatus == SampleStatus.SkipManual)
                    ? SampleSetStatus.Cancelled
                    : SampleSetStatus.Complete;
                
                NotifyAllPropertiesChanged();
            }
            else
            {
                SampleSetStatus = SampleSetStatus.Complete;
            }
        }

        public void Stop()
        {
            foreach (var sample in Samples)
            {
                if (sample.SampleStatus == SampleStatus.NotProcessed)
                {
                    sample.SampleStatus = SampleStatus.SkipManual;
                }
            }

            SampleSetStatus = SampleSetStatus.Cancelled;
            CarouselModel.Instance.ClearAll();
        }
        
        public void SetSampleSetIndex(ushort index)
        {
            SetIndex = index;
            foreach(SampleViewModel s in Samples)
                s.SetSampleSetIndex(index);
        }

        private void LockStatusChanged(LockResult res)
        {
            UpdateButtons();
        }

        #endregion

        #region IEquatable

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is SampleSetViewModel)) return false;
            return Equals((SampleSetViewModel)obj);
        }

        public bool Equals(SampleSetViewModel other)
        {
            if (other == null) return false;
            return GetHashCode().Equals(other.GetHashCode());
        }

        public override int GetHashCode()
        {
            var str = Uuid.ToString() +
                      SetIndex.ToString() +
                      SampleSetName +
                      DateTimeStarted.Ticks.ToString() +
                      Samples?.Count.ToString() ?? "0";
            return str.GetHashCode();
        }

        #endregion
    }
}