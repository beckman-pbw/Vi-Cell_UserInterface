using ApiProxies.Misc;
using HawkeyeCoreAPI.Facade;
using ScoutDataAccessLayer.DAL;
using ScoutDomains;
using ScoutDomains.Common;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.ExpandedSampleWorkflow;
using ScoutModels.Home.QueueManagement;
using ScoutModels.Service;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Structs;
using ScoutViewModels.ViewModels.Common;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using ScoutDomains.RunResult;
using ScoutModels.Review;
using ScoutServices.Interfaces;

// ReSharper disable once RedundantUsingDirective
using ScoutUtilities.UIConfiguration; // do NOT remove this using - required for release build - used in CanPerformPlay(), CanPerformPause(), CanPerformStop(), CanPerformEject()
using ScoutViewModels.ViewModels.Home.ExpandedSampleWorkflow; // do NOT remove this using - required for release build - used in CanPerformPlay(), CanPerformPause(), CanPerformStop(), CanPerformEject()
using ScoutModels.Interfaces;
using Notification = ScoutUtilities.Events.Notification;
using Precession = ScoutUtilities.Enums.Precession;
using SampleRecordDomain = ScoutDomains.SampleRecordDomain;
using SampleStatus = ScoutUtilities.Enums.SampleStatus;
using SubstrateType = ScoutUtilities.Enums.SubstrateType;
using ScoutViewModels.Interfaces;
using ScoutServices.Enums;
using System.Threading;

namespace ScoutViewModels.ViewModels.Home
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel(ILockManager lockManager, IScoutViewModelFactory viewModelFactory, 
            ISampleProcessingService sampleProcessingService, IInstrumentStatusService instrumentStatusService,
            IWorkListModel workListModel, ISampleResultsManager sampleResultsManager)
        {
            _lockManager = lockManager;
            _lockStateSubscriber = _lockManager.SubscribeStateChanges().Subscribe(LockStatusChanged);
            _viewModelFactory = viewModelFactory;
            _sampleProcessingService = sampleProcessingService;
            _instrumentStatusService = instrumentStatusService;
            _workListModel = workListModel;
            _sampleResultsManager = sampleResultsManager;
            _sampleSetIndexCounter = 1;
            _runOptionSettingsDictionary = new Dictionary<string, RunOptionSettingsModel>();
            IsSingleton = true;
            SampleSets = new ObservableCollection<SampleSetViewModel>();
            SampleSets.CollectionChanged += OnSampleSetsOnCollectionChanged;
            WorkListSampleTemplate = _viewModelFactory.CreateUserSampleTemplateViewModel();
            OrphanSampleTemplate = _viewModelFactory.CreateOrphanSampleTemplateViewModel();
            OrphanSampleTemplate.CopyFrom(WorkListSampleTemplate);
            VCRButtonsAreVisible = lockManager.IsLocked() ? false : true;
            WorkListSampleTemplateIsVisible = true;
            _worklistMutex = new EventWaitHandle (false, EventResetMode.ManualReset);
            SettingsModel.UseCarouselSimulation(UISettings.UseCarouselSimulation);

            _sampleSetSubscriber = MessageBus.Default.Subscribe<Notification<SampleSetChangedEventArgs>>(OnSampleSetChangedAsync);
            _notificationMessageSubscriber = MessageBus.Default.Subscribe<Notification>(OnNotificationMessageReceived);
            _systemStatusSubscriber = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe((OnSystemStatusChanged));

            _sampleProcessingService = sampleProcessingService;
            _sampleStatusSubscriber = _sampleProcessingService.SubscribeToSampleStatusCallback().Subscribe(OnSampleStatusCallback);
            _sampleCompleteSubscriber = _sampleProcessingService.SubscribeToSampleCompleteCallback().Subscribe(OnSampleCompleteCallback);
            _imageResultSubscriber = _sampleProcessingService.SubscribeToImageResultCallback().Subscribe(OnImageResultCallback);
            _workListCompleteSubscriber = _sampleProcessingService.SubscribeToWorkListCompleteCallback().Subscribe(OnWorkListCompleteCallback);
            _workListStatusChangedSubscriber = _sampleProcessingService.SubscribeToWorkListStatusChangedCallback().Subscribe(WorkListStatusUpdated);
            _deleteSampleSubscriber = _sampleResultsManager.SubscribeSamplesDeleted().Subscribe(DeleteSamplesFromCallback);
        }

        protected override void DisposeUnmanaged()
        {
            if (SampleSets != null)
            {
                foreach (var sample in SampleSets)
                    sample?.Dispose();
            }
            WorkListSampleTemplate?.Dispose();
            CurrentOrphanSampleSet?.Dispose();
            _lockStateSubscriber?.Dispose();
            _systemStatusSubscriber?.Dispose();
            _sampleStatusSubscriber?.Dispose();
            _sampleCompleteSubscriber?.Dispose();
            _imageResultSubscriber?.Dispose();
            _workListCompleteSubscriber?.Dispose();
            _workListStatusChangedSubscriber?.Dispose();
            _deleteSampleSubscriber?.Dispose();
            _worklistMutex?.Dispose();
            MessageBus.Default.UnSubscribe(ref _sampleSetSubscriber);
            MessageBus.Default.UnSubscribe(ref _notificationMessageSubscriber);
            base.DisposeUnmanaged();
        }

        #region Properties & Fields

        private readonly ISampleProcessingService _sampleProcessingService;
        private readonly ISampleResultsManager _sampleResultsManager;
        private readonly IDisposable _systemStatusSubscriber;
        private readonly IDisposable _lockStateSubscriber;
        private readonly IDisposable _sampleStatusSubscriber;
        private readonly IDisposable _sampleCompleteSubscriber;
        private readonly IDisposable _imageResultSubscriber;
        private readonly IDisposable _workListCompleteSubscriber;
        private readonly IDisposable _workListStatusChangedSubscriber;
        private readonly IDisposable _deleteSampleSubscriber;
        private Subscription<Notification<SampleSetChangedEventArgs>> _sampleSetSubscriber;
        private Subscription<Notification> _notificationMessageSubscriber;
        private List<uuidDLL> _samplesToDelete;
        
        private readonly Dictionary<string, RunOptionSettingsModel> _runOptionSettingsDictionary;
        private ushort _sampleSetIndexCounter; // the counter for the identifier used in the current worklist's sampleSets
        private bool _carouselIsInstalled;
        private SystemStatus _previousSystemStatus = SystemStatus.Idle;

        /* PC3549-5669...below mutex synchronizes the worklistStatus updating with the sampleStatus updating.  A race condition occurs with
         * an A-Cup sample where the OnSampleStatusCallback completes before the WorklistStatusUpdated callback completes causing the
         * wrong VCR buttons to be enabled/disabled.  
         */ 
        private EventWaitHandle _worklistMutex;

        private readonly ILockManager _lockManager;
        private readonly IScoutViewModelFactory _viewModelFactory;
        private readonly IInstrumentStatusService _instrumentStatusService;
        private readonly IWorkListModel _workListModel;

        public ObservableCollection<SampleSetViewModel> SampleSets
        {
            get { return GetProperty<ObservableCollection<SampleSetViewModel>>(); }
            set { SetProperty(value); }
        }

        public SampleSetViewModel SelectedSampleSet
        {
            get { return GetProperty<SampleSetViewModel>(); }
            set { SetProperty(value); }
        }

        public SampleSetViewModel CurrentOrphanSampleSet
        {
            get { return GetProperty<SampleSetViewModel>(); }
            set { SetProperty(value); }
        }

        public UserSampleTemplateViewModel WorkListSampleTemplate
        {
            get => GetProperty<UserSampleTemplateViewModel>();
            set => SetProperty(value);
        }

        public OrphanSampleTemplateViewModel OrphanSampleTemplate
        {
            get => GetProperty<OrphanSampleTemplateViewModel>();
            set => SetProperty(value);
        }

        public bool WorkListSampleTemplateIsVisible
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public bool VCRButtonsAreVisible
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        #endregion

        #region Event Handlers

        public void OnViewLoaded()
        {
            // filter the sample sets on the page using the current session filter args
            var curUser = LoggedInUser.CurrentUser;
            var curUserSession = curUser.Session;
            var args = new FilterSampleSetsEventArgs();
            var ctQc = curUserSession.GetVariable<CellTypeQualityControlGroupDomain>(SessionKey.FilterDialog_SelectedCellTypeOrQualityControlGroup);
            args.CellTypeOrQualityControlName = ctQc?.Name ?? string.Empty;
            args.IsAllCellTypesSelected = curUserSession.GetVariable<bool>(SessionKey.FilterDialog_IsAllSelected);
            args.FilteringItem = curUserSession.GetVariable<eFilterItem>(SessionKey.FilterDialog_FilteringItem);
            args.FromDate = curUserSession.GetVariable<DateTime>(SessionKey.FilterDialog_FromDate);
            args.ToDate = curUserSession.GetVariable<DateTime>(SessionKey.FilterDialog_ToDate);
            args.SearchString = curUserSession.GetVariable<string>(SessionKey.FilterDialog_SearchString);
            args.TagSearchString = curUserSession.GetVariable<string>(SessionKey.FilterDialog_TagSearchString);
            args.User = curUserSession.GetVariable<string>(SessionKey.FilterDialog_SelectedUser);
            FilterResultsAsync(args);
        }

        private void WorkListStatusUpdated(WorkListStatus e)
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                try
                {
                    WorkListSampleTemplateIsVisible = e == WorkListStatus.Idle;
                    if ((e == WorkListStatus.Idle) || (e == WorkListStatus.Paused))
                    {
                        _worklistMutex.Reset();
                        _sampleProcessingService.RunStateChangeInProgress = false;
                        UpdateButtons();
                    }
                    else if (e == WorkListStatus.Running)
                    {
                        /* Enable Pause/Stop if Running with no pending samples so user doesn't have to wait for entire carousel to be scanned.
                         * Also have seen Auto Resume function rarely go from Pausing to Paused to Running... Usually it goes from Pausing to Running.
                         * The check for !RunStateChangeInProgress will update the buttons in the Pausing -> Paused -> Running case.
                         */
                        if (!_sampleProcessingService.HasPendingDeviceSamples || !_sampleProcessingService.RunStateChangeInProgress)
                        {
                            _sampleProcessingService.RunStateChangeInProgress = false;
                            UpdateButtons();
                        }
                        _worklistMutex.Set(); // let OnSampleStatusCallback know it's okay to update VCR buttons now
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("WorkListStatusUpdated error", ex);
                }
            });
        }

        private void OnSampleSetsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.Action == NotifyCollectionChangedAction.Add ||
                    e.Action == NotifyCollectionChangedAction.Remove ||
                    e.Action == NotifyCollectionChangedAction.Reset)
                {
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        _sampleProcessingService.HasPendingDeviceSamples = GetPendingSampleSets().Count > 0;
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error("OnSampleSetsOnCollectionChanged error", ex);
            }
        }

        /// <summary>
        /// Called when the backend has a status update for a particular sample
        /// </summary>
        /// <param name="args"></param>
        private void OnSampleStatusCallback(ApiEventArgs<SampleEswDomain> args)
        {
            try
            {
                if(args?.Arg1 == null)
                {
                    Log.Error("OnSampleStatusCallback - null args <exit>");
                    return;
                }
                
                var sampleEswDomain = args.Arg1;

                if (sampleEswDomain.SampleStatus == ScoutUtilities.Enums.SampleStatus.NotProcessed)
                {
	                Log.Debug($"sampleEswDomain: {sampleEswDomain}");
                }
                else
                {
	                Log.Debug($"sampleEswDomain.SampleStatus: {sampleEswDomain.SampleStatus}");
                }

				var sampleSet = sampleEswDomain.SampleSetIndex == ApplicationConstants.OrphanSampleSetIndex
                    ? CurrentOrphanSampleSet
                    : GetSampleSetForProcessing(sampleEswDomain);

                var foundResult = GetSampleViewModelFromCallback(sampleSet, sampleEswDomain);
                if (!foundResult.Item1)
                {
                    Log.Error($"Unable to find SampleViewModel in HomeViewModel.SampleSets");
                    return;
                }
                var sampleViewModel = foundResult.Item2;
                sampleSet = GetSampleSetForProcessing(sampleEswDomain);

                if(!CheckIndexAndUuid(sampleSet, sampleEswDomain, sampleViewModel))
                {
                    Log.Error("OnSampleStatusCallback - CheckIndexAndUuid failed <exit>");
                    return;
                }

                // 1 - update the status
                sampleViewModel.SampleStatus = sampleEswDomain.SampleStatus;

                // 2 - update the carousel wells (if carousel)
                if (sampleEswDomain.SamplePosition.IsCarousel() &&
                    (sampleEswDomain.SampleStatus == SampleStatus.Completed ||
                     sampleEswDomain.SampleStatus == SampleStatus.SkipManual ||
                     sampleEswDomain.SampleStatus == SampleStatus.SkipError))
                {
                    CarouselModel.Instance.Remove(sampleViewModel.SamplePosition.Column);
                }

                // 3 - update the set status if it hasn't already been updated
                if (sampleSet.SampleSetStatus != SampleSetStatus.Running &&
                    sampleSet.SampleSetStatus != SampleSetStatus.Cancelled &&
                    sampleSet.SampleSetStatus != SampleSetStatus.Complete &&
                    sampleSet.SampleSetStatus != SampleSetStatus.Paused)
                {
                    sampleSet.SampleSetStatus = SampleSetStatus.Running;
                }

                // 4 - Create a SampleRecord and attach it to the SampleViewModel, if one doesn't exist, so the
                // Image Callbacks have something add their image objects to.
                if (sampleViewModel.SampleRecord == null)
                {
                    var ctQcName = string.IsNullOrEmpty(sampleEswDomain.CellTypeQcName)
                        ? WorkListSampleTemplate.QcCellTypes.GetCellTypeQualityControlByIndex(sampleEswDomain.CellTypeIndex)?.Name ?? string.Empty
                        : sampleEswDomain.CellTypeQcName;

                    string uname = sampleEswDomain.Username;
                    sampleViewModel.Username = uname;

                    var list = new List<KeyValuePair<string, string>>(ResultRecordHelper.GetShowParameterList(new GenericDataDomain(), uname));
                    sampleViewModel.SampleRecord = sampleEswDomain.GenerateSampleRecordDomainFrom(list, ctQcName);
                    sampleViewModel.SampleRecord.UserId = uname;
                }

                if (sampleEswDomain.SampleStatus == SampleStatus.InProcessAspirating)
                {
                    // don't want to wait here forever... only seen < 1 second between this call and worklist changing to Running
                    if (_worklistMutex.WaitOne(2000))
                    {
                        _sampleProcessingService.RunStateChangeInProgress = false;
                        UpdateButtons();
                    }
                    else
                    {
                        Log.Warn("OnSampleStatusCallback _worklistMutex timeout");
                    }
                }
                // Keep for debugging: Log.Debug($"Sample Status Callback [HomeViewModel] -- EXIT::args.Arg1:{argMsg}");
            }
            catch (Exception ex)
            {
                Log.Error("Exception: ", ex);
            }
        }

        /// <summary>
        /// Called when the backend has completed a sample (and it's image analysis).
        /// NOTE: Sometimes the last image or 2 is not completed until after this message is received.
        /// </summary>
        /// <param name="args"></param>
        private void OnSampleCompleteCallback(ApiEventArgs<SampleEswDomain, SampleRecordDomain> args)
        {
            try
            {
                if ((args?.Arg1 == null) || (args?.Arg2 == null))
                {
                    Log.Warn($"OnSampleCompleteCallback <exit> args null");
                    return;
                }

                var argMsg = args.Arg1.ToString();
                var arg2Msg = args.Arg2?.ToString();
                Log.Debug($"args.Arg1:{argMsg}");

                var sampleEswDomain = args.Arg1;
                var sampleRecord = args.Arg2;
                if (sampleRecord == null)
                    Log.Warn($"sampleRecord is null");
                else if (sampleRecord.SelectedResultSummary == null)
                    Log.Warn($"sampleRecord.SelectedResultSummary is null");
                else if (sampleRecord.SelectedResultSummary.UUID.IsNullOrEmpty())
                    Log.Warn($"sampleRecord.SelectedResultSummary.UUID is null");
                
                var sampleSet = sampleEswDomain.SampleSetIndex == ApplicationConstants.OrphanSampleSetIndex
                    ? CurrentOrphanSampleSet
                    : GetSampleSetForProcessing(sampleEswDomain);

                var foundResult = GetSampleViewModelFromCallback(sampleSet, sampleEswDomain);
                if (!foundResult.Item1)
                {
                    Log.Error($"EXIT, Unable to find SampleViewModel in HomeViewModel.SampleSets::args.Arg1");
                    return;
                }
                var sampleViewModel = foundResult.Item2;
                sampleSet = GetSampleSetForProcessing(sampleEswDomain);

                if(!CheckIndexAndUuid(sampleSet, sampleEswDomain, sampleViewModel))
                {
                    Log.Error($"EXIT, CheckIndexAndUuid failed");
                    return;
                }

                var sampleDomain = sampleViewModel.GenerateSampleDomain(sampleSet.PlatePrecession == Precession.RowMajor);
                if (string.IsNullOrEmpty(sampleDomain?.SampleID))
                {
                    Log.Error($"EXIT, Unable to create SampleDomain from args.Arg1");
                    return;
                }

                // update the sampleViewModel
                if (sampleEswDomain.SampleStatus == SampleStatus.SkipError)
                {
                    QueueResultModel.LogDetailsAndClearSkippedSample(sampleDomain);
                    sampleViewModel.SampleStatus = sampleEswDomain.SampleStatus;
                }
                else if (sampleEswDomain.SampleStatus == SampleStatus.SkipManual)
                {
                    // Don't log an error for a missing sample 
                    sampleDomain.Clear();
                    sampleDomain.SampleStatusColor = SampleStatusColor.Skip;
                    sampleViewModel.SampleStatus = sampleEswDomain.SampleStatus;
                }
                else
                {
                    sampleViewModel.SampleRecord = sampleRecord;
                    sampleViewModel.SampleStatus = SampleStatus.Completed;
                }

                // update the completed sample's SampleSet, if necessary
                if (sampleSet.Samples.All(s => s.SampleStatus == SampleStatus.SkipError ||
                                               s.SampleStatus == SampleStatus.SkipManual))
                {
                    sampleSet.SampleSetStatus = SampleSetStatus.Cancelled;
                }
                else if (sampleSet.Samples.All(s => s.SampleStatus == SampleStatus.Completed ||
                                                    s.SampleStatus == SampleStatus.SkipError ||
                                                    s.SampleStatus == SampleStatus.SkipManual))
                {
                    if (!sampleSet.IsOrphanSet)
                        sampleSet.SampleSetStatus = SampleSetStatus.Complete;
                }

                // handle any auto-export settings
                if ((null != sampleViewModel.SampleRecord) && (sampleViewModel.SampleStatus == SampleStatus.Completed))
                {
                    if (sampleRecord != null)
                    {
#if DEBUG
						Log.Debug("sampleRecord.SelectedResultSummary.UUID (before): " + sampleRecord?.SelectedResultSummary?.UUID);
#endif
	                    sampleRecord.SubstrateType = sampleEswDomain.SubstrateType;
	                    ExportSample(sampleViewModel);
#if DEBUG
	                    Log.Debug("sampleRecord.SelectedResultSummary.UUID  (after): " + sampleRecord?.SelectedResultSummary?.UUID);
#endif
                    }
                    else
                    {
                        Log.Warn($"Failed to retrieve the sample record for the sample data {sampleEswDomain.SampleDataUuid}. Export of sample data not performed");
                    }
                }

                DispatcherHelper.ApplicationExecute(() =>
                {
                    if ((sampleSet.SampleSetStatus == SampleSetStatus.Complete ||
                         sampleSet.SampleSetStatus == SampleSetStatus.Cancelled) && !sampleSet.IsExpanderEnabled)
                    {
                        // User can't expand, which means they are a normal user and this is not their sample set,
                        // so they should not see it if it is "Idle".
                        SampleSets.Remove(sampleSet);
                        sampleSet.Dispose();
                    }
                    else
                    {
                        // reset the ImageView
                        sampleSet.ImageViewModel?.Dispose();
                        sampleSet.ImageViewModel = new ImageViewModel(new ResultRecordHelper(sampleSet.RunByUser));

                        // refresh the GUI
                        sampleSet.UpdateButtons();

                        // Keep for debugging: Log.Debug($"Sample Complete Callback [HomeViewModel] -- END::args.Arg1:{argMsg}{Environment.NewLine}args.Arg2:{arg2Msg}");
                    }
                    _sampleProcessingService.HasPendingDeviceSamples = GetPendingSampleSets().Count > 0;
                    if (sampleSet.SampleSetStatus != SampleSetStatus.Running &&
                        sampleSet.SampleSetStatus != SampleSetStatus.Pending &&
                        sampleSet.SampleSetStatus != SampleSetStatus.Paused)
                    {
                        UpdateButtons();
                    }
                });
            }
            catch (Exception e)
            {
                Log.Error("OnSampleCompleteCallback error", e);
            }
        }

        /// <summary>
        /// Called when the backend has completed all the samples in the work list.
        /// NOTE: Sometimes the last image or 2 is not completed until after this message is received.
        /// </summary>
        /// <param name="args"></param>
        private void OnWorkListCompleteCallback(ApiEventArgs<List<uuidDLL>> args)
        {
            try
            {
                var argMsg = args?.Arg1 == null ? "Arg1 was null" : string.Join(", ", args.Arg1);
                Log.Debug($"WorkList UUID: {argMsg}");
                _sampleSetIndexCounter = 1; // reset the counter for the next worklist -- 0 reserved for orphan set

                if (CurrentOrphanSampleSet != null)
                {
                    CurrentOrphanSampleSet.SampleSetStatus = SampleSetStatus.Complete;
                    CurrentOrphanSampleSet = null;
                    UpdateButtons();
                }

                _sampleProcessingService.HasPendingDeviceSamples = false;
            }
            catch (Exception e)
            {
                Log.Error("Exception: ", e);
            }
        }

        private void OnImageResultCallback(ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers> args)
        {
			ThreadPool.QueueUserWorkItem(new WaitCallback(OnImageResultCallbackThread), args);
        }

        private void OnImageResultCallbackThread(object obj)
        {
            try
            {
                if (obj == null)
                {
                    Log.Warn($"OnImageResultCallbackThread <exit>, obj is null");
                    return;
                }

                ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers> args = obj as ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>;
                var sampleEswDomain = args.Arg1;
                var imageSequenceNumber = args.Arg2; // EG: 14 out of 100
                var cumulativeResults = args.Arg3;
                var imageSet = args.Arg4;
                var imageResult = args.Arg5;

                if ((args?.Arg1 == null) || (args?.Arg2 == null) || (args?.Arg3 == null) ||
                    (args?.Arg4 == null) || (args?.Arg5 == null))
                {
                    Log.Warn($"OnImageResultCallbackThread <exit> args null");
                    return;
                }

#if DEBUG
                Log.Debug($"imageSequenceNumber: {imageSequenceNumber}");
#endif

                var sampleSet = sampleEswDomain.SampleSetIndex == ApplicationConstants.OrphanSampleSetIndex
                    ? CurrentOrphanSampleSet
                    : GetSampleSetForProcessing(sampleEswDomain);

                var foundResult = GetSampleViewModelFromCallback(sampleSet, sampleEswDomain);
                if (!foundResult.Item1)
                {
                    //Log.Debug($"OnImageResultCallback::Unable to find SampleViewModel in HomeViewModel.SampleSets");
                    return;
                }

                var sampleViewModel = foundResult.Item2;
                sampleSet = GetSampleSetForProcessing(sampleEswDomain);

                if(!CheckIndexAndUuid(sampleSet, sampleEswDomain, sampleViewModel))
                {
                    Log.Warn($"OnImageResultCallbackThread <exit>, CheckIndexAndUuid failed");
                    return;
                }

                if (sampleViewModel.SampleRecord == null)
                {
                    Log.Warn($"OnImageResultCallback:: sampleViewModel.SampleRecord is null: sampleViewModel: {sampleViewModel.SampleName} - Uuid: {sampleViewModel.Uuid}");
                    return;
                }

                if (string.IsNullOrEmpty(sampleViewModel.SampleRecord?.SampleIdentifier))
                {
                    Log.Warn($"OnImageResultCallback:: sampleViewModel.SampleRecord?.SampleIdentifier is NULL");
                    return;
                }

                // SetResultSampleRecord call needs to be on the same thread as the SelectedImageIndex assignment or else we get race conditions
                SetResultSampleRecord(sampleEswDomain, cumulativeResults, imageResult, imageSet, imageSequenceNumber, sampleViewModel.SampleRecord);

                try
                {
                    sampleViewModel.Concentration = cumulativeResults.concentration_general;
                    sampleViewModel.ViableConcentration = cumulativeResults.concentration_ofinterest;
                    sampleViewModel.TotalCells = cumulativeResults.count_pop_general;
                    sampleViewModel.AverageDiameter = cumulativeResults.avg_diameter_pop;
                    sampleViewModel.TotalViability = cumulativeResults.percent_pop_ofinterest;
                }
                catch (Exception e)
                {
                    Log.Warn($"OnImageResultCallback:: Exception thrown when updating SampleViewModel with Image Result Callback sample results", e);
                }

                sampleSet.ImageViewModel.SelectedImageType = ImageType.Annotated;
                sampleSet.ImageViewModel.SelectedSampleFromList = sampleViewModel.SampleRecord;
                sampleSet.ImageViewModel.SelectedImageIndex = sampleViewModel.SampleRecord.ImageIndexList
                        .FirstOrDefault(kvp => kvp.Key == imageSequenceNumber);
            }
            catch (Exception e)
            {
                Log.Error("OnImageResultCallback error", e);
            }
        }

        private async void OnSampleSetChangedAsync(Notification<SampleSetChangedEventArgs> msg)
        {
            if (string.IsNullOrEmpty(msg?.Token) || msg.Token != MessageToken.SampleSetChanged ||
                SampleSets == null || !SampleSets.Any())
            {
                return;
            }

            var changedSet = msg.Target.SampleSetUid.IsEmpty()
                ? GetSampleSetForProcessing(msg.Target.SampleSetIndex)
                : GetSampleSetForProcessing(msg.Target.SampleSetUid)
                  ?? GetSampleSetForProcessing(msg.Target.SampleSetIndex);
            if (changedSet?.Samples == null || !changedSet.Samples.Any()) return;

            if (!string.IsNullOrEmpty(msg?.Message) && msg.Message.Equals(MessageToken.CancelSampleSet))
            {
                if (_sampleProcessingService.GetWorkListStatus() == WorkListStatus.Idle)
                {
                    // WorkList hasn't been started yet - just remove the sample set. We do not need to notify _sampleProcessingService.
                    // We will not decrement the _sampleSetIndexCounter; we will simply not use that set index -- this is safer
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        changedSet.CancelSampleSet();
                        SampleSets.Remove(changedSet);
                    });
                }
                else
                {
                    var result = await _sampleProcessingService.CancelSampleSetAsync(changedSet.SetIndex);
                    if (result)
                    {
                        if (_sampleProcessingService.GetWorkListStatus() == WorkListStatus.Running)
                            _sampleProcessingService.RunStateChangeInProgress = true;
                        DispatcherHelper.ApplicationExecute(() =>
                        {
                            changedSet.CancelSampleSet();
                            _sampleProcessingService.HasPendingDeviceSamples = GetPendingSampleSets().Count > 0;

                        });
                    }
                }
                UpdateButtons();
            }
            else
            {
                await Task.Run(() => ReloadSampleSet(changedSet.Uuid));
            }
        }

        private void OnSystemStatusChanged(SystemStatusDomain sysStatus)
        {
            if (sysStatus == null) return;
            _carouselIsInstalled = sysStatus.CarouselDetect == eSensorStatus.ssStateActive;

            // Detect Stopping from OPC so we can update VCR buttons if they choose to unlock during this state change.
            // It's ok that this code will also run if Stopping from the UI b/c RunStateChangeInProgress should be true anyways
            if (sysStatus.SystemStatus != _previousSystemStatus && sysStatus.SystemStatus == SystemStatus.Stopping)
                _sampleProcessingService.RunStateChangeInProgress = true;

            _previousSystemStatus = sysStatus.SystemStatus;
        }

        private void OnNotificationMessageReceived(Notification msg)
        {
            if (string.IsNullOrEmpty(msg?.Token)) return;

            if (msg.Token == MessageToken.RunSampleSettingsChanged || msg.Token == MessageToken.UserDefaultCellTypeChanged)
            {
                DispatcherHelper.ApplicationExecute(() => WorkListSampleTemplate.SetForUser());
            }

            if (msg.Token == MessageToken.RecordsDeleted)
            {
                // re-filter results by whatever the current filter is set to
                DispatcherHelper.ApplicationExecute(() => FilterResultsAsync(new FilterSampleSetsEventArgs()));
            }
        }

        #endregion

        #region Private Methods

        private async Task<bool> PauseAsync(string username, string password)
        {
            var pauseSuccess = await Task.Run(() => _sampleProcessingService.PauseProcessing(username, password));
            if (!pauseSuccess)
            {
                return false;
            }

            foreach (var sampleSet in SampleSets.Where(s => s.SampleSetStatus == SampleSetStatus.Running))
            {
                sampleSet.Pause();
            }

            return true;
        }

        private static bool CheckIndexAndUuid(SampleSetViewModel sampleSet, SampleEswDomain sampleEswDomain, SampleViewModel sampleViewModel)
        {
            var strData = Environment.NewLine +
                          $"sampleEswDomain:: name: '{sampleEswDomain.SampleName}' - Uuid: '{sampleEswDomain.Uuid}' - SampleDataUuid: '{sampleEswDomain.SampleDataUuid}' - SampleSetUuid: '{sampleEswDomain.SampleSetUid}' - SampleSetIndex: '{sampleEswDomain.SampleSetIndex}' - SampleIndex: '{sampleEswDomain.Index}'{Environment.NewLine}" +
                          $"sampleViewModel:: name: '{sampleViewModel.SampleName}' - Uuid: '{sampleViewModel.Uuid}' - SampleDataUuid: '{sampleViewModel.SampleDataUid}' - SampleSetUuid: '{sampleViewModel.SampleSetUid}' - SampleSetIndex: '{sampleViewModel.SampleSetIndex}' - SampleIndex: '{sampleViewModel.SampleIndex}'{Environment.NewLine}" +
                          $"sampleSet:: name: '{sampleSet.SampleSetName}' - Uuid: '{sampleSet.Uuid}' - SampleSetIndex: '{sampleSet.SetIndex}'{Environment.NewLine}";

            if (sampleSet.SetIndex == sampleEswDomain.SampleSetIndex)
            {
                if (sampleViewModel.Uuid.IsEmpty()) sampleViewModel.Uuid = sampleEswDomain.Uuid;
                if (sampleViewModel.SampleDataUid.IsEmpty()) sampleViewModel.SampleDataUid = sampleEswDomain.SampleDataUuid;
                if (sampleSet.Uuid.IsEmpty()) sampleSet.Uuid = sampleEswDomain.SampleSetUid;

                if (!sampleSet.Uuid.Equals(sampleEswDomain.SampleSetUid))
                {
                    Log.Warn($"sampleSet Uuid ({sampleSet.Uuid.ToString()}) does not match " +
                             $"sampleEswDomain.SampleSetUid ({sampleEswDomain.SampleSetUid.ToString()}).{strData}");
                    return false;
                }

                if (!sampleViewModel.Uuid.Equals(sampleEswDomain.Uuid))
                {
                    Log.Warn($"sampleViewModel Uuid ({sampleViewModel.Uuid.ToString()}) does not match " +
                             $"sampleEswDomain.Uuid ({sampleEswDomain.Uuid.ToString()}).{strData}");
                    return false;
                }
            }
            else
            {
                Log.Warn($"sampleSet.SetIndex ({sampleSet.SetIndex}) does not match " +
                         $"sampleEswDomain.SampleSetIndex ({sampleEswDomain.SampleSetIndex}).{strData}");
                return false;
            }

            return true;
        }

        private Tuple<bool, SampleViewModel> GetSampleViewModelFromCallback(SampleSetViewModel sampleSet, 
            SampleEswDomain sampleEswDomain)
        {
            SampleViewModel sampleViewModel = null;

            var automationLocked = _lockManager.IsLocked();
            if (sampleSet == null && !automationLocked)
            {
                //Log.Debug($"GetSampleViewModelFromCallback:: Input parameter 'sampleSet' is null");
                return new Tuple<bool, SampleViewModel>(false, null);
            }

            if (sampleSet == null)
            {
                // The incoming sampleEswDomain is coming in from automation. We need to create
                // the SampleSetViewModel and SampleViewModel for this new data set.

                Log.Info($"Creating a SampleSetViewModel and SampleViewModel for the Automation Sample(s) that the backend sent that the current ViewModel's are unaware of.");

                var result = _sampleResultsManager.GetSampleSet(sampleEswDomain.SampleSetUid, false, out var sampleSetDomain);
                if (result != HawkeyeError.eSuccess)
                {
                    Log.Warn($"Failed to get SampleSet data from backend for set '{sampleEswDomain.SampleSetUid}'");
                    return new Tuple<bool, SampleViewModel>(false, null);
                }

                sampleSet = new SampleSetViewModel(_lockManager, _instrumentStatusService, _viewModelFactory, sampleSetDomain, false, false, true);
                sampleSet.CreatedByUser = sampleEswDomain.Username;
                sampleSet.RunByUser = sampleEswDomain.Username;
                sampleSet.SetSampleSetIndex(sampleEswDomain.SampleSetIndex);

                sampleViewModel = sampleSet.Samples.FirstOrDefault(s =>
                    s.Uuid.Equals(sampleEswDomain.Uuid) ||
                    (s.SampleIndex == sampleEswDomain.Index &&
                     s.SampleSetIndex == sampleEswDomain.SampleSetIndex));

                DispatcherHelper.ApplicationExecute(() =>
                {
                    SampleSets.Insert(0, sampleSet);
                });

                return new Tuple<bool, SampleViewModel>(true, sampleViewModel);
            }

            sampleViewModel = sampleSet.GetSampleViewModel(sampleEswDomain);
            if (sampleViewModel != null) return new Tuple<bool, SampleViewModel>(true, sampleViewModel);

            if (!sampleSet.IsOrphanSet)
            {
                Log.Debug($"Unable to find the SampleViewModel from the Sample Status Callback Item. " +
                          $"Current sample set is not orphan set. {sampleEswDomain}");
                return new Tuple<bool, SampleViewModel>(false, null);
            }

            // New orphan is being processed. Create and add it:
            Log.Debug($"New orphan found. Adding to SampleSet. {sampleEswDomain}");

            var runOptions = OrphanSampleTemplate.AdvancedSampleSettings.CurrentRunOptions;
            var tempSampleVm = new SampleViewModel(sampleEswDomain, sampleSet.SampleSetName, true, runOptions);

            tempSampleVm.AdvancedSampleSettings = OrphanSampleTemplate.AdvancedSampleSettings.CreateCopy();
            tempSampleVm.QcOrCellType = CurrentOrphanSampleSet.SampleTemplate.QcCellType;
            DispatcherHelper.ApplicationExecute(() =>
            {
                sampleSet.Samples.Add(tempSampleVm);
                sampleSet.UpdateIsVisible(); // make the Orphan set appear visible
            });

            sampleViewModel = tempSampleVm;
            return new Tuple<bool, SampleViewModel>(true, sampleViewModel);
        }

        private void DeleteSamplesFromCallback(SamplesDeletedEventArgs args)
        {
            try
            {
                if (args.PercentComplete <= 0) //start list of samples to delete from home view
                {
                    _samplesToDelete = new List<uuidDLL>();
                    return;
                }
                else
                {
                    _samplesToDelete.Add(args.SampleUuid);
                    if (args.PercentComplete < 100)
                        return;
                    foreach (var uuid in _samplesToDelete) //delete operation complete, remove samples from home view
                    {
                        try
                        {
                            bool foundSample = false;
                            for (int i = SampleSets.Count - 1; i >= 0; i--)
                            {
                                var set = SampleSets[i];
                                for (int j = set.Samples.Count - 1; j >= 0; j--)
                                {
                                    try
                                    {
                                        var sample = set.Samples[j];

                                        if (sample.SampleRecord?.UUID.Equals(uuid) == true)
                                        {
                                            DispatcherHelper.ApplicationExecute(() =>
                                            {
                                                set.Samples.Remove(sample);
                                                if (set.Samples.Count <= 0)
                                                    SampleSets.Remove(set);
                                            });
                                            foundSample = true;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                         Log.Debug("HomeViewModel :: DeleteSamplesFromCallback :: " + e.Message);
                                    }
                                }
                                if (foundSample)
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error($"DeleteSamplesFromCallback remove sample from view exception ", e);
                        }
                    }
                    _samplesToDelete.Clear();
                }
            }
            catch (Exception e)
            {
                Log.Error($"DeleteSamplesFromCallback exception ", e);
            }
        }

        private RunOptionSettingsModel GetRunOptionSettingsModel(string username)
        {
            if (_runOptionSettingsDictionary.ContainsKey(username))
            {
                var runOptionSettings = _runOptionSettingsDictionary[username];
                return runOptionSettings;
            }

            var runOptionsModel = new RunOptionSettingsModel(XMLDataAccess.Instance, username);
            _runOptionSettingsDictionary.Add(username, runOptionsModel);
            return runOptionsModel;
        }

        private async Task ReloadSampleSet(uuidDLL sampleSetUid)
        {
            if (SampleSets == null) return;

            var oldSampleSetViewModel = SampleSets.FirstOrDefault(ss => ss.Uuid.Equals(sampleSetUid));

            var reloaded = await Task.Run(() => HomeModel.GetSampleSet(sampleSetUid));
            if (reloaded == null)
                return;

            var reloadedSampleSetViewModel = _viewModelFactory.CreateSampleSetViewModel(reloaded, oldSampleSetViewModel?.IsOrphanSet ?? false);
            var ssIndex = SampleSets.IndexOf(oldSampleSetViewModel);
            DispatcherHelper.ApplicationExecute(() => SampleSets[ssIndex] = reloadedSampleSetViewModel);
        }

        private void UpdateButtons()
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                PlayButtonCommand.RaiseCanExecuteChanged();
                PauseButtonCommand.RaiseCanExecuteChanged();
                StopButtonCommand.RaiseCanExecuteChanged();
                EjectButtonCommand.RaiseCanExecuteChanged();
                AddSamplesCommand.RaiseCanExecuteChanged();
                NotifyPropertyChanged(nameof(SampleSets));
            });
        }

        private void ExportSample(SampleViewModel svm)
        {
            if (svm == null) return;      
            if(svm.AdvancedSampleSettings.ExportSamples && svm.AdvancedSampleSettings.ExportSamplesAsPdf)
            {
                ExportHelper.AutoExportPdf(svm.SampleRecord, svm.AdvancedSampleSettings.ExportSampleDirectory);
            }
            ExportHelper.ExportCompletedRunResult(svm);
        }


        private void SetResultSampleRecord(SampleEswDomain sampleEsw, BasicResultAnswers cumulativeResults,
            BasicResultAnswers imageResult, ImageSetDto imageSet, int imageSequence, SampleRecordDomain sampleRecordDomain)
        {
            var resultSummary = new ResultSummaryDomain();
            var allCellTypes = CellTypeFacade.Instance.GetAllCellTypes_BECall();
            if (allCellTypes == null || allCellTypes.Count == 0)
            {
                Log.Debug($"No cell types");
                return;
            }
            resultSummary.CellTypeDomain = allCellTypes.FirstOrDefault(c => c.CellTypeIndex == sampleEsw.CellTypeIndex);
            var newCumulative = cumulativeResults.MarshalToBasicResultDomain();

            if (newCumulative.TotalCumulativeImage > sampleRecordDomain.NumImageSets || sampleRecordDomain.SelectedResultSummary == null)
            {
                resultSummary.CumulativeResult = newCumulative;
                sampleRecordDomain.NumImageSets = resultSummary.CumulativeResult.TotalCumulativeImage;
            }
            else
            {
                // Use the most-recent cumulative result
                resultSummary.CumulativeResult = sampleRecordDomain.SelectedResultSummary.CumulativeResult;
            }

            sampleRecordDomain.ResultSummaryList.Add(resultSummary);
            sampleRecordDomain.SelectedResultSummary = resultSummary;

            var sampleImageResult = new SampleImageRecordDomain
            {
				ImageSet = imageSet,
                SequenceNumber = (uint)imageSequence,
                ImageID = sampleEsw.SamplePosition.Column,
                ResultPerImage = imageResult.MarshalToBasicResultDomain()
			};

            sampleRecordDomain.SampleImageList.Add(sampleImageResult);

            var kvp = new KeyValuePair<int, string>(imageSequence, Misc.ConvertToString(imageSequence));
            sampleRecordDomain.ImageIndexList.Add(kvp);

            sampleRecordDomain.SelectedSampleImageRecord = sampleImageResult; // this appears to do nothing - JDT

            foreach (var item in sampleRecordDomain.SampleImageList)
            {
                item.TotalCumulativeImage = sampleRecordDomain.SampleImageList.Count;
            }

            var imageSuccessCount = sampleRecordDomain.SampleImageList.Count(x =>
                x?.ResultPerImage?.ProcessedStatus != null &&
                x.ResultPerImage.ProcessedStatus.Equals(E_ERRORCODE.eSuccess));
            sampleRecordDomain.NumImageSets = (uint)imageSuccessCount;
            sampleRecordDomain.UpdateSampleBubbleStatus();
        }

        private IList<SampleSetViewModel> GetPendingAndRunningSampleSets()
        {
            return SampleSets.Where(s => (s.SampleSetStatus == SampleSetStatus.Running ||
                                          s.SampleSetStatus == SampleSetStatus.Pending ||
                                          s.SampleSetStatus == SampleSetStatus.Paused) &&
                                         !s.IsOrphanSet).ToList();
        }

        private IList<SampleSetViewModel> GetPendingAndPausedSampleSets()
        {
            return SampleSets.Where(s =>
                (s.SampleSetStatus == SampleSetStatus.Pending || s.SampleSetStatus == SampleSetStatus.Paused) &&
                !s.IsOrphanSet).ToList();
        }

        private IList<SampleSetViewModel> GetPendingSampleSets()
        {
            return SampleSets.Where(s => (s.SampleSetStatus == SampleSetStatus.Paused || s.SampleSetStatus == SampleSetStatus.Pending)  && !s.IsOrphanSet).ToList();
        }

        private SampleSetViewModel GetSampleSetForProcessing(SampleEswDomain sampleEswDomain)
        {
            if (!sampleEswDomain.SampleSetUid.IsEmpty())
            {
                return GetSampleSetForProcessing(sampleEswDomain.SampleSetUid) ?? GetSampleSetForProcessing(sampleEswDomain.SampleSetIndex);
            }

            return GetSampleSetForProcessing(sampleEswDomain.SampleSetIndex);
        }

        private SampleSetViewModel GetSampleSetForProcessing(uuidDLL sampleSetUuid)
        {
            return SampleSets.FirstOrDefault(ss => ss.Uuid.Equals(sampleSetUuid));
        }

        private SampleSetViewModel GetSampleSetForProcessing(ushort sampleSetIndex)
        {
            if (sampleSetIndex == ApplicationConstants.OrphanSampleSetIndex)
                return CurrentOrphanSampleSet;

            return SampleSets.FirstOrDefault(ss => ss.SetIndex == sampleSetIndex &&
                                                   ss.SampleSetStatus != SampleSetStatus.Cancelled &&
                                                   ss.SampleSetStatus != SampleSetStatus.Complete &&
                                                   ss.SampleSetStatus != SampleSetStatus.SampleSetTemplate);
        }

        private async void FilterResultsAsync(FilterSampleSetsEventArgs args)
        {
            MainWindowViewModel.Instance.IsAdornerVisible = true;

            // remove the displayed completed sets
            var initialCount = SampleSets.Count;
            for (var i = initialCount - 1; i >= 0; i--)
            {
                var set = SampleSets[i];
                if (set.SampleSetStatus == SampleSetStatus.Complete ||
                    set.SampleSetStatus == SampleSetStatus.Cancelled)
                {
                    set.Dispose();
                    SampleSets.RemoveAt(i);
                }
            }

            if (LoggedInUser.NoLoggedInUser())
            {
                return;
            }

            try
            {
                // query for new sample sets
                var sampleSets = await HomeModel.GetSampleSetsAsync(args);
                foreach (var set in sampleSets)
                {
                    // add to the end of the list
                    SampleSets.Add(_viewModelFactory.CreateSampleSetViewModel(set, false));
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to filter sample sets", ex);
                PostToMessageHub(LanguageResourceHelper.Get("LID_Message_FilterSampleSetsFailed"));
            }
            finally
            {
                MainWindowViewModel.Instance.IsAdornerVisible = false;
            }
        }

        private void LockStatusChanged(LockResult res)
        {
            VCRButtonsAreVisible = (res == LockResult.Locked ? false : true);
            UpdateButtons();
        }

        #endregion

        #region Commands

        #region Add Samples Command

        private RelayCommand _addSamplesCommand;
        public RelayCommand AddSamplesCommand => _addSamplesCommand ?? (_addSamplesCommand = new RelayCommand(OpenAddSamplesDialogAsync, CanAddSamplesDialog));

        private bool CanAddSamplesDialog()
        {
            var weHaveNonCarouselSet = GetPendingAndRunningSampleSets().Any(s =>
                s.SubstrateType == SubstrateType.Plate96 ||
                s.SubstrateType == SubstrateType.AutomationCup);
            
            return !weHaveNonCarouselSet && !_lockManager.IsLocked() && !_sampleProcessingService.RunStateChangeInProgress;
        }

        private async void OpenAddSamplesDialogAsync()
        {
            var addSamplesDialogCausedPause = false;
            var currentUserId = LoggedInUser.CurrentUserId;

            if (_sampleProcessingService.GetWorkListStatus() == WorkListStatus.Running || 
                _sampleProcessingService.GetWorkListStatus() == WorkListStatus.Paused)
            {
                addSamplesDialogCausedPause = true;
                _sampleProcessingService.RunStateChangeInProgress = true;
                UpdateButtons();
                var result = await PauseAsync(currentUserId, "");
                if (!result)
                {
                    _sampleProcessingService.RunStateChangeInProgress = false;
                    UpdateButtons();
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_Error_UnableToPauseAddSamples"));
                    return;
                }
            }

            var ss = _viewModelFactory.CreateSampleSetViewModel(new ResultRecordHelper(currentUserId), WorkListSampleTemplate,
                    currentUserId, currentUserId,
                string.Format(LanguageResourceHelper.Get("LID_Default_SampleSetName"),
                    Misc.ConvertToSampleSetNameDefaultDateOnlyFormat(DateTime.Now)));
            var allowSubstrateToChange = SampleSets.All(set =>
                set.SampleSetStatus == SampleSetStatus.Complete ||
                set.SampleSetStatus == SampleSetStatus.Cancelled);

            var args = new CreateSampleSetEventArgs<SampleSetViewModel>(ss, ss.CreatedByUser, ss.RunByUser,
                ss.SampleSetName, SampleSetStatus.NoSampleSetStatus, allowSubstrateToChange, _sampleProcessingService.LastSubstrate, 
                _sampleProcessingService.GetWorkListStatus());

            if (DialogEventBus<SampleSetViewModel>.CreateSampleSetDialog(this, args) == true)
            {
                args.NewSampleSet.SetSampleSetIndex(_sampleSetIndexCounter++);
                args.NewSampleSet.SampleSetStatus = SampleSetStatus.Pending;
                
                if (_sampleProcessingService.GetWorkListStatus() != WorkListStatus.Idle)
                {
                    // WorkList is already started -- add set to the backend for processing
                    var sampleSetDomain = args.NewSampleSet.GetSampleSetDomain(XMLDataAccess.Instance, LoggedInUser.CurrentUserId);
                    var result = await _sampleProcessingService.AddSampleSetAsync(sampleSetDomain);
                    if (!result)
                    {
                        // clear the wells from the Carousel
                        if (args.NewSampleSet.SubstrateType == SubstrateType.Carousel)
                        {
                            foreach (var s in args.NewSampleSet.Samples)
                            {
                                CarouselModel.Instance.Remove(s.SamplePosition.Column);
                            }
                        }
                        DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_Error_UnableToAddSamples"));
                        return;
                    }

                    args.NewSampleSet.IsExpanded = false;
                    SampleSets.Insert(1, args.NewSampleSet);
                }
                else
                {
                    // insert at the top of the GUI list -- Orphan set hasn't been created yet
                    args.NewSampleSet.IsExpanded = true;
                    SampleSets.Insert(0, args.NewSampleSet);
                }
                _sampleProcessingService.LastSubstrate = args.NewSampleSet.SubstrateType;

                // Resume/Play the work list if this dialog was the reason for the Pause
                if (_sampleProcessingService.GetWorkListStatus() == WorkListStatus.Paused && addSamplesDialogCausedPause)
                {
                    PerformPlayAsync();
                }
                else
                {
                    UpdateButtons();
                }
            }
            else
            {
                _sampleProcessingService.RunStateChangeInProgress = false;
                UpdateButtons();
            }
        }

        #endregion

        #region Filter Command

        private RelayCommand _filterCommand;
        public RelayCommand FilterCommand => _filterCommand ?? (_filterCommand = new RelayCommand(PerformFilter, CanPerformFilter));

        private bool CanPerformFilter()
        {
            return true;
        }

        private void PerformFilter()
        {
            var args = new FilterSampleSetsEventArgs();
            if (DialogEventBus.FilterSampleSetsDialog(this, args) == true)
            {
                FilterResultsAsync(args);
            }
        }

        #endregion

        #region Stop Button Command

        private RelayCommand _stopButtonCommand;
        public RelayCommand StopButtonCommand => _stopButtonCommand ?? (_stopButtonCommand = new RelayCommand(PerformStop, CanPerformStop));

        private bool CanPerformStop()
        {
#if !DEBUG
            // Block the ability for customers to stop sample sets when running in offline mode (PC3549-2750)
            if (!UISettings.IsFromHardware && !UISettings.EnableSampleSetVcrControlsInOfflineMode)
                return false;
#endif
            return (_sampleProcessingService.GetWorkListStatus() != WorkListStatus.Idle) && !_sampleProcessingService.RunStateChangeInProgress; 
        }

        private void PerformStop()
        {
            _sampleProcessingService.RunStateChangeInProgress = true;
            UpdateButtons();
            PerformStopAsync(true);
        }

        private async void PerformStopAsync(bool promptUser)
        {
            if (promptUser && DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_MSGBOX_QueueManagementAbort")) != true)
            {
                _sampleProcessingService.RunStateChangeInProgress = false;
                UpdateButtons();
                return;
            }

            var result = await Task.Run(() => _sampleProcessingService.StopProcessing("",""));
            if (!result)
            {
                return;
            }

            var setsToStop = SampleSets.Where(ss =>
                ss.SampleSetStatus == SampleSetStatus.Pending ||
                ss.SampleSetStatus == SampleSetStatus.Running ||
                ss.SampleSetStatus == SampleSetStatus.Paused);
            foreach (SampleSetViewModel ss in setsToStop)
                ss.Stop();
            // wait for the WorkList OnComplete to set the worklist back to IDLE
            // -- which will come from the SampleProcessingService (not the backend) w/o UUID
            UpdateButtons();
        }

        #endregion

        #region Eject Button Command

        private RelayCommand _ejectButtonCommand;
        public RelayCommand EjectButtonCommand => _ejectButtonCommand ?? (_ejectButtonCommand = new RelayCommand(PerformEjectAsync, CanPerformEject));

        private bool CanPerformEject()
        {
#if !DEBUG
            // Block the ability for customers to eject sample sets when running in offline mode (PC3549-2750)
            if (!UISettings.IsFromHardware && !UISettings.EnableSampleSetVcrControlsInOfflineMode)
                return false;
#endif
            var wlStatus = _sampleProcessingService.GetWorkListStatus();
            return (wlStatus == WorkListStatus.Idle || wlStatus == WorkListStatus.Paused) && !_sampleProcessingService.RunStateChangeInProgress;
        }

        private async void PerformEjectAsync()
        {
            var username = ScoutModels.LoggedInUser.CurrentUserId;
            await Task.Run(() => _sampleProcessingService.EjectStage(username, "", true));
        }

        #endregion

        #region Pause Button Command

        private RelayCommand _pauseButtonCommand;
        public RelayCommand PauseButtonCommand => _pauseButtonCommand ?? (_pauseButtonCommand = new RelayCommand(PerformPause, CanPerformPause));

        private bool CanPerformPause()
        {
#if !DEBUG
            // Block the ability for customers to pause sample sets when running in offline mode (PC3549-2750)
            if (!UISettings.IsFromHardware && !UISettings.EnableSampleSetVcrControlsInOfflineMode)
                return false;
#endif
            return _sampleProcessingService.GetWorkListStatus() == WorkListStatus.Running && !_sampleProcessingService.RunStateChangeInProgress;
        }

        private void PerformPause()
        {
            _sampleProcessingService.RunStateChangeInProgress = true;
            UpdateButtons();
            var username = ScoutModels.LoggedInUser.CurrentUserId;
            PauseAsync(username, "");
        }

        #endregion

        #region Play Button Command

        private RelayCommand _playButtonCommand;
        public RelayCommand PlayButtonCommand => _playButtonCommand ?? (_playButtonCommand = new RelayCommand(PerformPlayAsync, CanPerformPlay));

        private bool CanPerformPlay()
        {
#if !DEBUG
            // Block the ability for customers to play sample sets when running in offline mode (PC3549-2750)
            if (!UISettings.IsFromHardware && !UISettings.EnableSampleSetVcrControlsInOfflineMode)
                return false;
#endif
            return _sampleProcessingService.GetWorkListStatus() != WorkListStatus.Running && !_sampleProcessingService.RunStateChangeInProgress;
        }

        private async void PerformPlayAsync()
        {
            var username = LoggedInUser.CurrentUserId; // todo: should be injected
            var runOptionsDataAccess = XMLDataAccess.Instance; // todo: should be injected
            _sampleProcessingService.RunStateChangeInProgress = true;
            UpdateButtons();


            var resumeResult = await Task.Run(() => _sampleProcessingService.ResumeProcessing(username, ""));
            if (!resumeResult)
            {
                _sampleProcessingService.RunStateChangeInProgress = false;
                UpdateButtons();
            }

            var template = WorkListSampleTemplate.GetSampleSetTemplateDomain();
            if (string.IsNullOrWhiteSpace(template.SampleId))
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_SampleIDBlank"));
                _sampleProcessingService.RunStateChangeInProgress = false;
                UpdateButtons();
                return;
            }
            var sampleSets = new List<SampleSetDomain>();
            foreach (var sampleSetViewModel in GetPendingAndPausedSampleSets())
            {
                sampleSets.Add(sampleSetViewModel.GetSampleSetDomain(runOptionsDataAccess, LoggedInUser.CurrentUserId));
            }

            var carrier = sampleSets.FirstOrDefault(s => s.SampleSetStatus == SampleSetStatus.Pending)
                                    ?.Carrier ?? SubstrateType.Carousel;

            var hasPlateSet = sampleSets.Any(ss => ss.Carrier == SubstrateType.Plate96);

            if (_carouselIsInstalled && hasPlateSet)
            {
                // ask user to install 96 well plate
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_Run96WellPlate"));
                _sampleProcessingService.RunStateChangeInProgress = false;
                UpdateButtons();
                return;
            }

            if (!_carouselIsInstalled && !hasPlateSet && carrier != SubstrateType.AutomationCup)
            {
                // ask user to install carousel
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_RunCarousel"));
                _sampleProcessingService.RunStateChangeInProgress = false;
                UpdateButtons();
                return;
            }

            DiskSpaceModel.GetDiskSpace(AppDomain.CurrentDomain.BaseDirectory, out var totalSize, out var totalFreeSpace);
            if (totalFreeSpace <= 50000000000)
            {
                // ask user to free more storage space before we continue.
                ScoutUtilities.Events.MessageBus.Default.Publish(new SystemMessageDomain
                {
                    IsMessageActive = true,
                    Message = LanguageResourceHelper.Get("LID_API_SystemErrorCode_Failure_Storagenearcapacity"),
                    MessageType = MessageType.Warning
                });

                _sampleProcessingService.RunStateChangeInProgress = false;
                UpdateButtons();
                return;
            }
           
            var numSamplesNotRun = _sampleProcessingService.NumSamplesNotYetRun(sampleSets);
            _workListModel.CheckReagentsAndWasteTray(numSamplesNotRun, carrier, out _, out _);

            var isPaused = sampleSets.Any(ss => ss.SampleSetStatus == SampleSetStatus.Paused)
                            || _sampleProcessingService.GetWorkListStatus() == WorkListStatus.Paused;

            // If worklist is paused, CanProcessSamples() will fail, so don't call it. This appears to be expected behavior as the
            // worklist has already been validated and set. Don't call ProcessSamples() if the worklist is paused. Autoresume
            // in the backend will continue processing where it left off
            if (!isPaused)
            {
                var validationResult = _sampleProcessingService.CanProcessSamples(username, sampleSets, _carouselIsInstalled);
                if (validationResult.IsValid())
                {
                    var startResult = _sampleProcessingService.RunStateChangeInProgress = await Task.Run(() =>
                        _sampleProcessingService.ProcessSamples(sampleSets, username, template, runOptionsDataAccess));
                    if (startResult)
                    {
                        // update the VM as needed
                        OrphanSampleTemplate.CopyFrom(WorkListSampleTemplate);

                        CurrentOrphanSampleSet = _viewModelFactory.CreateSampleSetViewModel(new ResultRecordHelper(username),
                            OrphanSampleTemplate,
                            username, username, LanguageResourceHelper.Get("LID_OrphanSetName"), true);
                        CurrentOrphanSampleSet.SampleSetStatus = SampleSetStatus.Pending;
                        CurrentOrphanSampleSet.SetSampleSetIndex(ApplicationConstants.OrphanSampleSetIndex);
                        CurrentOrphanSampleSet.DateTimeStarted = DateTime.Now;
                        CurrentOrphanSampleSet.IsExpanded = true;

                        SampleSets.Insert(ApplicationConstants.OrphanSampleSetIndex, CurrentOrphanSampleSet);
                    }
                }
                else
                {
                    _sampleProcessingService.RunStateChangeInProgress = false;
                }
            }

            _runOptionSettingsDictionary.Clear();
            UpdateButtons();
        }

        #endregion // Play Button Command

        #endregion // Commands
    }
}
