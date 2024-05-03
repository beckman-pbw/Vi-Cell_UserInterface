using ApiProxies.Misc;
using Ninject.Extensions.Logging;
using ScoutDataAccessLayer.IDAL;
using ScoutDomains;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutModels.Interfaces;
using ScoutModels.Reports;
using ScoutModels.Settings;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ScoutModels;
using ScoutModels.Admin;

namespace ScoutServices
{
    public class SampleProcessingService : Disposable, ISampleProcessingService
    {
        #region Constructor

        public SampleProcessingService(IRunningWorkListModel runningWorkListModel, ICellTypeManager cellTypeManager,
            ILogger logger, IWorkListModel workListModel, IScoutModelsFactory scoutModelsFactory, IAutomationSettingsService automationSettingsService,
            IInstrumentStatusService instrumentStatusService, ISecurityService securityService)
        {
            WorkListStatus = WorkListStatus.Idle;
            HasPendingDeviceSamples = false;
            _parameterListCache = new List<KeyValuePair<string, string>>();
            _cellTypeManager = cellTypeManager;
            _log = logger;
            _workListModel = workListModel;
            _scoutModelsFactory = scoutModelsFactory;
            _automationSettingsService = automationSettingsService;
            _instrumentStatusService = instrumentStatusService;
            _securityService = securityService;
            _runningWorkListModel = runningWorkListModel;

            _runningWorkListModel.SampleStatusUpdated += OnSampleStatusCallback;
            _runningWorkListModel.SampleCompleted += OnSampleCompleteCallback;
            _runningWorkListModel.WorkListCompleted += OnWorkListCompleteCallback;
            _runningWorkListModel.ImageResultOccurred += OnImageResultCallback;

            _sampleStatusCallbackSubject = new Subject<ApiEventArgs<SampleEswDomain>>();
            _sampleCompleteCallbackSubject = new Subject<ApiEventArgs<SampleEswDomain, SampleRecordDomain>>();
            _imageResultCallbackSubject = new Subject<ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>>();
            _workListCompleteCallbackSubject = new Subject<ApiEventArgs<List<uuidDLL>>>();
            _workListStatusChangedCallbackSubject = new Subject<WorkListStatus>();
            _instrumentStatusServiceSubject = _instrumentStatusService.SubscribeToSystemStatusCallback()
                .Subscribe(OnSystemStatusChanged);
        }

        protected override void DisposeManaged()
        {
            _sampleStatusCallbackSubject?.OnCompleted();
            _sampleCompleteCallbackSubject?.OnCompleted();
            _imageResultCallbackSubject?.OnCompleted();
            _workListCompleteCallbackSubject?.OnCompleted();
            _workListStatusChangedCallbackSubject?.OnCompleted();
            base.DisposeManaged();
        }

        protected override void DisposeUnmanaged()
        {
            if (_runningWorkListModel != null)
            {
                _runningWorkListModel.SampleStatusUpdated -= OnSampleStatusCallback;
                _runningWorkListModel.SampleCompleted -= OnSampleCompleteCallback;
                _runningWorkListModel.WorkListCompleted -= OnWorkListCompleteCallback;
                _runningWorkListModel.ImageResultOccurred -= OnImageResultCallback;
            }
            _instrumentStatusServiceSubject?.Dispose();
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties & Fields

        private List<KeyValuePair<string, string>> _parameterListCache;

        private readonly ILogger _log;
        private readonly IRunningWorkListModel _runningWorkListModel;
        private readonly ICellTypeManager _cellTypeManager;
        private readonly IWorkListModel _workListModel;
        private readonly IScoutModelsFactory _scoutModelsFactory;
        private readonly IAutomationSettingsService _automationSettingsService;
        private readonly IInstrumentStatusService _instrumentStatusService;
        private readonly ISecurityService _securityService;

        private WorkListStatus _workListStatus;
        protected WorkListStatus WorkListStatus
        {
            get { return _workListStatus;}
            set
            {
                var og = _workListStatus;
                var changed = _workListStatus != value;
                _workListStatus = value;
                if (changed)
                {
                    _log.Debug($"WorkListStatus changed [SERVICE] from '{og}' to '{value}'");
                    PublishWorkListStatusChangedCallback(value);
                }
            }
        }

        public bool HasPendingDeviceSamples { get; set; }
        public bool RunStateChangeInProgress { get; set; }

        public SubstrateType LastSubstrate { get; set; } = SubstrateType.Carousel;
        #endregion

        #region Sample Processing Callbacks

        #region Sample Status Callback Subscription

        private readonly object _sampleStatusLock = new object();
        private readonly Subject<ApiEventArgs<SampleEswDomain>> _sampleStatusCallbackSubject;

        public IObservable<ApiEventArgs<SampleEswDomain>> SubscribeToSampleStatusCallback()
        {
            lock (_sampleStatusLock)
            {
                return _sampleStatusCallbackSubject;
            }
        }

        public void PublishSampleStatusCallback(ApiEventArgs<SampleEswDomain> sampleStatusArgs)
        {
            try
            {
                lock (_sampleStatusLock)
                {
                    _sampleStatusCallbackSubject.OnNext(sampleStatusArgs);
                }
            }
            catch (Exception e)
            {
                _log.Error(e, $"Failed to publish sample status callback");
                
                lock (_sampleStatusLock)
                {
                    _sampleStatusCallbackSubject.OnError(e);
                }
            }
        }

        #endregion

        #region Sample Complete Callback Subscription

        private readonly object _sampleCompleteLock = new object();
        private readonly Subject<ApiEventArgs<SampleEswDomain, SampleRecordDomain>> _sampleCompleteCallbackSubject;

        public IObservable<ApiEventArgs<SampleEswDomain, SampleRecordDomain>> SubscribeToSampleCompleteCallback()
        {
            lock (_sampleCompleteLock)
            {
                return _sampleCompleteCallbackSubject;
            }
        }

        public void PublishSampleCompleteCallback(ApiEventArgs<SampleEswDomain, SampleRecordDomain> sampleCompleteArgs)
        {
            try
            {
                lock (_sampleCompleteLock)
                {
                    _sampleCompleteCallbackSubject.OnNext(sampleCompleteArgs);
                }
            }
            catch (Exception e)
            {
                _log.Error(e, $"Failed to publish sample complete callback");

                lock (_sampleCompleteLock)
                {
                    _sampleCompleteCallbackSubject.OnError(e);
                }
            }
        }

        #endregion

        #region WorkList Complete Callback Subscription

        private readonly object _workListCompleteLock = new object();
        private readonly Subject<ApiEventArgs<List<uuidDLL>>> _workListCompleteCallbackSubject;

        public IObservable<ApiEventArgs<List<uuidDLL>>> SubscribeToWorkListCompleteCallback()
        {
            lock (_workListCompleteLock)
            {
                return _workListCompleteCallbackSubject;
            }
        }

        public void PublishWorkListCompleteCallback(ApiEventArgs<List<uuidDLL>> workListCompleteArgs)
        {
            try
            {
                lock (_workListCompleteLock)
                {
                    _workListCompleteCallbackSubject.OnNext(workListCompleteArgs);
                }
            }
            catch (Exception e)
            {
                _log.Error(e, $"Failed to publish worklist complete callback");

                lock (_workListCompleteLock)
                {
                    _workListCompleteCallbackSubject.OnError(e);
                }
            }
        }

        #endregion

        #region Image Result Callback Subscription

        private readonly object _imageResultLock = new object();
        private readonly Subject<ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>> _imageResultCallbackSubject;

        public IObservable<ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>> SubscribeToImageResultCallback()
        {
            lock (_imageResultLock)
            {
                return _imageResultCallbackSubject;
            }
        }

        public void PublishImageResultCallback(
            ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers> imageResultArgs)
        {
            try
            {
                lock (_imageResultLock)
                {
                    _imageResultCallbackSubject.OnNext(imageResultArgs);
                }
            }
            catch (Exception e)
            {
                _log.Error(e, $"Failed to publish image result callback");

                lock (_imageResultLock)
                {
                    _imageResultCallbackSubject.OnError(e);
                }
            }
        }

        #endregion

        #region WorkListStatus Changed Callback Subscription

        private readonly object _workListStatusChangedLock = new object();
        private readonly Subject<WorkListStatus> _workListStatusChangedCallbackSubject;
        private readonly IDisposable _instrumentStatusServiceSubject;

        public IObservable<WorkListStatus> SubscribeToWorkListStatusChangedCallback()
        {
            lock (_workListStatusChangedLock)
            {
                return _workListStatusChangedCallbackSubject;
            }
        }

        public void PublishWorkListStatusChangedCallback(WorkListStatus workListCompleteArgs)
        {
            try
            {
                lock (_workListStatusChangedLock)
                {
                    _workListStatusChangedCallbackSubject.OnNext(workListCompleteArgs);
                }
            }
            catch (Exception e)
            {
                _log.Error(e, $"Failed to publish worklist status changed callback");

                lock (_workListStatusChangedLock)
                {
                    _workListStatusChangedCallbackSubject.OnError(e);
                }
            }
        }

        #endregion

        private void OnSampleStatusCallback(object sender, ApiEventArgs<SampleEswDomain> args)
        {
            // Keep for debugging:var argMsg = args.Arg1.ToString();
            // Keep for debugging: _log.Debug($"Sample Status Callback [SERVICE]::args.Arg1:{argMsg}");
            PublishSampleStatusCallback(args);
        }

        private void OnSampleCompleteCallback(object sender, ApiEventArgs<SampleEswDomain> args)
        {
            var argMsg = args.Arg1.ToString();
            _log.Debug($"Sample Complete Callback (w/ async) [SERVICE]::args.Arg1:{argMsg}");

            // We are going to make the call to retrieve the sample result here before allowing the service user to handle the callback
            SampleRecordDomain sampleRecord = null;
            var sampleDataUuid = args?.Arg1?.SampleDataUuid ?? new uuidDLL();
            if (!sampleDataUuid.IsNullOrEmpty())
            {
                sampleRecord = ResultModel.RetrieveSampleRecord(sampleDataUuid);

                var sampleEswDomain = args.Arg1;
                sampleRecord.Position = sampleEswDomain.SamplePosition;
                if (args?.Arg1 != null) args.Arg1.SampleRecord = sampleRecord;
            }

            if (sampleRecord == null)
            {
                _log.Debug($"Sample Complete Callback [SERVICE]::Failed to retrieve the sample record for the sample data '{sampleDataUuid}'. Export of sample data not performed::args.Arg1:{args?.Arg1}");
            }

            PublishSampleCompleteCallback(new ApiEventArgs<SampleEswDomain, SampleRecordDomain>(args?.Arg1, sampleRecord));
        }

        private void OnWorkListCompleteCallback(object sender, ApiEventArgs<List<uuidDLL>> args)
        {
            var argMsg = args.Arg1.ToString();
            PublishWorkListCompleteCallback(args);
        }

        private void OnImageResultCallback(object sender,
            ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers> args)
        {
            PublishImageResultCallback(args);
        }

        #endregion

        #region ISampleProcessingService Methods

        public WorkListStatus GetWorkListStatus()
        {
            return WorkListStatus;
        }

        public List<KeyValuePair<string, string>> GetParameterList()
        {
            return _parameterListCache;
        }

        public int NumSamplesNotYetRun(IList<SampleSetDomain> sampleSets)
        {
            if (sampleSets == null || !sampleSets.Any()) return 0;

            return sampleSets.Sum(ss => ss.NumSamplesNotYetRun());
        }

        public bool EjectStage(string username, string password, bool showDialogPrompt)
        {
            var result = HawkeyeCoreAPI.Service.EjectSampleStageAPI(username, password);
            if (result != HawkeyeError.eSuccess)
            {
                _log.Error($"Failed to EjectSampleStage(): {result}");
                ApiHawkeyeMsgHelper.ErrorCommon(result, null, showDialogPrompt);
            }
            else
            {
                _log.Debug($"EjectSampleStage() success");
            }

            return result == HawkeyeError.eSuccess;
        }

        public bool StopProcessing(string username, string password)
        {
            var result = _runningWorkListModel.StopProcessing(username, password);
            return result;
        }

        public async Task<bool> CancelSampleSetAsync(ushort setIndex)
        {
            var result = await _runningWorkListModel.CancelSampleSetAsync(setIndex);
            return result;
        }

        public bool PauseProcessing(string username, string password)
        {
            var result = _runningWorkListModel.PauseProcessing(username, password);
            return result;
        }

        public bool ResumeProcessing(string username, string password)
        {
            if (WorkListStatus != WorkListStatus.Paused)
                return true;

            var result = _runningWorkListModel.ResumeProcessing(username, password);
            return result;
        }

        /// <summary>
        /// Detects when the backend pauses sample processing independent of the UI.
        /// </summary>
        /// <param name="systemStatusDomain"></param>
        private void OnSystemStatusChanged(SystemStatusDomain systemStatusDomain)
        {
            if (WorkListStatus != WorkListStatus.Paused && _instrumentStatusService.SystemStatus == SystemStatus.Paused)
            {
                WorkListStatus = WorkListStatus.Paused;
            }
            else if (WorkListStatus != WorkListStatus.Running && _instrumentStatusService.IsRunning())
            {
                WorkListStatus = WorkListStatus.Running;
            }
            else if (WorkListStatus != WorkListStatus.Idle && _instrumentStatusService.IsNotRunning())
            {
                WorkListStatus = WorkListStatus.Idle;
            }
        }

        public SampleSetDomain CreateSampleSetFromAutomation(IList<SampleEswDomain> samples, string username, 
            string setName, bool usingStartMultipleSamplesMethod,
            out SampleProcessingValidationResult validationResult,
            ushort setIndex = ApplicationConstants.FirstNonOrphanSampleSetIndex)
        {
            validationResult = SampleProcessingValidationResult.Valid; // assume all is well

            var set = new SampleSetDomain
            {
                SampleSetStatus = SampleSetStatus.Pending,
                SampleSetName = setName,
                Index = setIndex,
                Username = username,
                Carrier = SubstrateType.NoType // only set this after checking the samples (see below)
            };
            set.SampleSetName = set.SampleSetName.Trim();

            if (samples == null || samples.Count < 1)
            {
                _log.Error($"CreateSampleSetFromAutomation::No samples to process");
                validationResult |= SampleProcessingValidationResult.NoSamplesDefined;
                return set;
            }

            if (!AutomationSamplePositionIsValid(samples, usingStartMultipleSamplesMethod, ref validationResult))
            {
                _log.Error($"CreateSampleSetFromAutomation::AutomationSamplePosition Is NOT Valid for the given OPC method");
                return set;
            }

            var carrier = samples.Count == 1 && samples.FirstOrDefault().SamplePosition.IsAutomationCup()
                ? SubstrateType.AutomationCup
                : SubstrateType.Plate96;

            if (carrier == SubstrateType.Plate96)
            {
                if (samples.Any(s => !s.SamplePosition.Is96WellPlate()))
                {
                    _log.Error($"CreateSampleSetFromAutomation::Carrier defined as 96WellPlate but at least 1 sample is not a valid 96WellPlate position");
                    validationResult |= SampleProcessingValidationResult.InvalidSamplePosition;
                    return set;
                }
            }
            
            set.Samples = samples;
            set.Carrier = carrier;
            
            ushort sampleIndex = 0;
            foreach (var s in set.Samples)
            {
//TODO: Remove this restriction to allow the automation system to invoke a second sampling workflow for a-cup samples
                if (SubstrateType.AutomationCup == s.SubstrateType && SamplePostWash.FastWash == s.WashType)
                {
                    // A Cup sample cannot be FastWash
                    _log.Error($"CreateSampleSetFromAutomation::WashType cannot be FastWash for A Cup sample.");
                    validationResult |= SampleProcessingValidationResult.InvalidAutomationCupSample;
                    return set;
                }

                s.Index = sampleIndex;
                s.SampleStatus = SampleStatus.NotProcessed;
                s.SampleSetIndex = setIndex;
                s.Username = username;
                s.SubstrateType = carrier;
                s.SampleName = s.SampleName.Trim();

                if (s.IsQualityControl)
                {
                    var qc = _cellTypeManager.GetQualityControlDomain(username, "", s.CellTypeQcName);
                    if (qc == null)
                    {
                        _log.Error($"CreateSampleSetFromAutomation::Invalid QC / Cell Type.");
                        validationResult |= SampleProcessingValidationResult.InvalidAutomationCupSample;
                        return set;
                    }
                    s.CellTypeIndex = qc.CellTypeIndex;
                }
                else
                {
                    // Find the index 
                    var ct = _cellTypeManager.GetCellTypeDomain(username, "", s.CellTypeQcName);
                    if (ct == null)
                    {
                        _log.Error($"CreateSampleSetFromAutomation::Invalid Cell Type.");
                        validationResult |= SampleProcessingValidationResult.InvalidAutomationCupSample;
                        return set;
                    }
                    s.CellTypeIndex = ct.CellTypeIndex;
                }

                sampleIndex++;
            }

            return set;
        }

        public SampleProcessingValidationResult CanProcessSamples(string username, IList<SampleSetDomain> sampleSets, bool carouselIsInstalled)
        {
            var validationResult = SampleProcessingValidationResult.Valid; // assume everything is cool

            if (WorkListStatus != WorkListStatus.Idle)
            {
                _log.Info($"CanProcessSamples: WorkListStatus is '{WorkListStatus}' and not IDLE");
                validationResult |= SampleProcessingValidationResult.WorkListStatusNotIdle;
                return validationResult; // don't check anything else; just return
            }

            if (sampleSets.Any(s => string.IsNullOrWhiteSpace(s.SampleSetName)))
            {
                _log.Info($"CanProcessSamples: SampleSet name is empty");
                validationResult |= SampleProcessingValidationResult.InvalidSampleOrSampleSetName;
            }

            if (sampleSets.Any(s => Misc.ContainsInvalidCharacter(s.SampleSetName)))
            {
                _log.Info($"CanProcessSamples: SampleSet name is invalid");
                validationResult |= SampleProcessingValidationResult.InvalidSampleOrSampleSetName;
            }

            var carrier = sampleSets.FirstOrDefault(s => 
                s.SampleSetStatus == SampleSetStatus.Pending)?.Carrier ?? SubstrateType.Carousel;
            var hasPlateSet = sampleSets.Any(ss => ss.Carrier == SubstrateType.Plate96);
            var allSamples = sampleSets.SelectMany(ss => 
                ss.Samples ?? new List<SampleEswDomain>()).ToList();

            if (!allSamples.Any() && carrier != SubstrateType.Carousel)
            {
                _log.Info($"CanProcessSamples: No samples defined");
                validationResult |= SampleProcessingValidationResult.NoSamplesDefined;
            }

            if (allSamples.Any(s => string.IsNullOrWhiteSpace(s.SampleName)))
            {
                _log.Info($"CanProcessSamples: Sample name is empty");
                validationResult |= SampleProcessingValidationResult.InvalidSampleOrSampleSetName;
            }

            if (allSamples.Any(s => Misc.ContainsInvalidCharacter(s.SampleName)))
            {
                _log.Info($"CanProcessSamples: Sample name is invalid");
                validationResult |= SampleProcessingValidationResult.InvalidSampleOrSampleSetName;
            }

            if (carrier == SubstrateType.NoType)
            {
                _log.Info($"CanProcessSamples: Carrier not defined");
                validationResult |= SampleProcessingValidationResult.CarrierNotDefined;
            }

            if (!allSamples.All(s => s.SamplePosition.IsValid()))
            {
                _log.Info($"CanProcessSamples: At least one sample position is invalid");
                validationResult |= SampleProcessingValidationResult.InvalidSamplePosition;
            }

            var userRecord = _securityService.GetUserRecord(username);
            if (!userRecord.AllowFastMode && allSamples.Any(s => s.WashType == SamplePostWash.FastWash))
            {
                _log.Info($"CanProcessSamples: Requested samples contain a wash type of fast mode and the user does not have access.");
                validationResult |= SampleProcessingValidationResult.InvalidPermissions;
            }

            if (UsesSameSamplePositionInMultipleSamples(allSamples))
            {
                _log.Info($"CanProcessSamples: duplicate sample position found");
                validationResult |= SampleProcessingValidationResult.MultipleSamplesSharePosition;
            }
            
            if (carouselIsInstalled && hasPlateSet)
            {
                _log.Info($"CanProcessSamples: Carousel found for Plate SampleSet");
                validationResult |= SampleProcessingValidationResult.CarouselInstalledAndSetIs96WellPlate;
            }

            if (!carouselIsInstalled && !hasPlateSet && carrier != SubstrateType.AutomationCup)
            {
                _log.Info($"CanProcessSamples: Carousel NOT installed AND SampleSet carrier NOT Plate AND SampleSet carrier NOT A-Cup");
                validationResult |= SampleProcessingValidationResult.InvalidStageAndCarrier;
            }

            if (carrier == SubstrateType.AutomationCup)
            {
                var automationConfig = _automationSettingsService.GetAutomationConfig();
                var aCupEnabled = Misc.ByteToBool(automationConfig.ACupIsEnabled);
                if (!aCupEnabled)
                {
                    _log.Info($"CanProcessSamples: A-Cup not installed");
                    validationResult |= SampleProcessingValidationResult.ACupNotInstalled;
                }
                else
                {
                    var validACup = IsValidForAutomationCup(username, sampleSets);
                    if (validACup != SampleProcessingValidationResult.Valid)
                    {
                        validationResult |= validACup;
                        validationResult |= SampleProcessingValidationResult.InvalidAutomationCupSample;
                    }
                }
               
            }
            else if (carrier == SubstrateType.Plate96)
            {
                var validWellPlate = IsValidFor96WellPlate(username, sampleSets);
                if (validWellPlate != SampleProcessingValidationResult.Valid)
                {
                    validationResult |= validWellPlate;
                    validationResult |= SampleProcessingValidationResult.Invalid96WellPlateSample;
                }
            }

            if (_workListModel.CheckReagentsAndWasteTray(
				NumSamplesNotYetRun(sampleSets), carrier, out var needMoreReagent, out var needMoreWasteTray, false))
            {
				if (needMoreReagent)
					validationResult |= SampleProcessingValidationResult.InsufficientReagents;
				if (needMoreWasteTray)
					validationResult |= SampleProcessingValidationResult.InsufficientWasteTubeCapacity;
            }

            return validationResult;
        }

        public bool ProcessSamples(IList<SampleSetDomain> sampleSets, string username,
            SampleSetTemplateDomain sampleSetTemplate, IDataAccess runOptionsSettingsDataAccess)
        {
            if (sampleSets == null)
                sampleSets = new List<SampleSetDomain>(); // User might want to just process Orphan Samples

            // Starting a new run, remember the carrier type. It is referenced later to set the carrier type when creating 
            // a new Sample Set (this helps the user to not have to re-select the last carrier type).
            LastSubstrate = sampleSets.FirstOrDefault(s =>
                    (s.SampleSetStatus == SampleSetStatus.Pending) || s.SampleSetStatus == SampleSetStatus.Paused)
                                       ?.Carrier ?? SubstrateType.Carousel;

            _parameterListCache = ResultRecordHelper.GetShowParameterList(new GenericDataDomain(), username);

            // The backend is expecting an Orphan Sample Set for any samples it finds/processes that are not a part of 
            // the defined samples. Event carrier types that cannot have orphan sample sets need to give the Orphan Sample
            // Set to the backend (it is just disregarded). This Orphan Sample Set is expected to be the first set in the
            // collection.
            var currentOrphanSampleSet = CreateOrphanSampleSet(username, LastSubstrate);
            sampleSets.Insert(ApplicationConstants.OrphanSampleSetIndex, currentOrphanSampleSet);

            // We need to clean the \Instrument\Software\Target directory of previously run sample images.
            CleanImageTargetDirectory();

            var wlDomain = CreateWorkListDomain(username, sampleSetTemplate, sampleSets, runOptionsSettingsDataAccess, LastSubstrate);
            var setWorkListResult = _runningWorkListModel.SetWorkList(wlDomain);
            if (!setWorkListResult)
            {
                currentOrphanSampleSet.SampleSetStatus = SampleSetStatus.Complete;  // mark the orphan complete before removing it
                sampleSets.RemoveAt(ApplicationConstants.OrphanSampleSetIndex); // remove from the same place as the insert above
                
                return false;
            }

            var startProcessingResult = _runningWorkListModel.StartProcessing(username, "password");
            if (!startProcessingResult)
            {
                StopProcessing(username, "password");
                return false;
            }

            try
            {
                if ((LoggedInUser.IsConsoleUserLoggedIn()) && (LoggedInUser.CurrentUser?.Session != null))
                {
                    // When starting a sample set, set session filter from and to date to be relative to today
                    // Set the end time to the future so that this sample shows up whenn done
                    // Add a whole day to handle starting on one day and finishing the next day
                    var currentUserSession = LoggedInUser.CurrentUser.Session;
                    var now = DateTime.Now;
                    currentUserSession.SetVariable(SessionKey.FilterDialog_FromDate,
                        now.AddDays(ApplicationConstants.DefaultFilterFromDaysToSubtract));
                    currentUserSession.SetVariable(SessionKey.FilterDialog_ToDate, now.AddDays(1));
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to update filter settings", e);
            }

            return true;
        }

        public async Task<bool> AddSampleSetAsync(SampleSetDomain sampleSetDomain)
        {
            var result = await _scoutModelsFactory.CreateSecuredTask().Run(() => HawkeyeCoreAPI.WorkList.AddSampleSetAPI(sampleSetDomain));

            if (result != HawkeyeError.eSuccess)
            {
                _log.Error($"AddSampleSetAsync failed: {result}{Environment.NewLine}{sampleSetDomain}");
                ApiHawkeyeMsgHelper.ErrorCommon(result, showDialogPrompt: false);
            }

            return result == HawkeyeError.eSuccess;
        }

        #endregion

        #region Private Methods

        private SampleProcessingValidationResult IsValidForAutomationCup(string username, IList<SampleSetDomain> sampleSets)
        {
            var validationResult = SampleProcessingValidationResult.Valid;

            var setsContainSamples = sampleSets.SelectMany(s =>
                s.Samples ?? new List<SampleEswDomain>()).Any();

            if (!setsContainSamples)
            {
                _log.Info($"CanProcessSamples: Carrier is AutomationCup and there is no sample definition");
                validationResult |= SampleProcessingValidationResult.NoSamplesDefined;
                return validationResult;
            }

            var aCupSampleSet = sampleSets.FirstOrDefault();
            if (aCupSampleSet?.Samples == null)
            {
                _log.Info($"CanProcessSamples: Carrier is AutomationCup and there is unexpected data (first sampleSet is NULL or first sampleSet's Samples is NULL)");
                validationResult |= SampleProcessingValidationResult.NoSamplesDefined;
                return validationResult;
            }

            if (aCupSampleSet.Samples.Count != 1)
            {
                _log.Info($"CanProcessSamples: Carrier is AutomationCup and there is not exactly 1 sample (there are '{aCupSampleSet.Samples.Count}' samples)");
                validationResult |= SampleProcessingValidationResult.InvalidAutomationCupNumberOfSamples;
            }

            var aCupSample = aCupSampleSet.Samples.FirstOrDefault();
            if (aCupSample == null)
            {
                _log.Info($"CanProcessSamples: Carrier is AutomationCup and aCupSample is NULL");
                validationResult |= SampleProcessingValidationResult.NoSamplesDefined;
                return validationResult;
            }

            if (aCupSample.Dilution < ApplicationConstants.MinimumDilutionFactor ||
                aCupSample.Dilution > ApplicationConstants.MaximumDilutionFactor)
            {
                _log.Info($"CanProcessSamples: Carrier is AutomationCup and aCupSample.Dilution is out of range (value: '{aCupSample.Dilution}')");
                validationResult |= SampleProcessingValidationResult.InvalidDilution;
            }

			// Only need to check the max value since the minimum number is zero.
            if (aCupSample.SaveEveryNthImage > ApplicationConstants.MaximumNumberOfNthImages)
            {
                _log.Info($"CanProcessSamples: Carrier is AutomationCup and aCupSample.SaveEveryNthImage is out of range (value: '{aCupSample.SaveEveryNthImage}')");
                validationResult |= SampleProcessingValidationResult.InvalidSaveNthImage;
            }

            if (!aCupSample.SamplePosition.IsAutomationCup())
            {
                _log.Info($"CanProcessSamples: Carrier is AutomationCup and aCupSample.SamplePosition " +
                          $"is not valid for Automation Cup (value: '{aCupSample.SamplePosition.Row}{aCupSample.SamplePosition.Column}')");
                validationResult |= SampleProcessingValidationResult.InvalidSamplePosition;
            }

            if (string.IsNullOrEmpty(aCupSample.CellTypeQcName))
            {
                _log.Info($"CanProcessSamples: Carrier is AutomationCup and aCupSample.CellTypeQcName is NULL or empty");
                validationResult |= SampleProcessingValidationResult.InvalidCellTypeOrQcName;
            }

            var ctLookup = _cellTypeManager.GetCellTypeDomain(sampleSets[0].Username, "password", aCupSample.CellTypeQcName);
            var qcLookup = _cellTypeManager.GetQualityControlDomain(sampleSets[0].Username, "password", Misc.GetBaseQualityControlName(aCupSample.CellTypeQcName));
            if (ctLookup == null && qcLookup == null)
            {
                _log.Info(
                    $"CanProcessSamples: Carrier is AutomationCup and aCupSample.CellTypeQcName does not match any existing Cell Type or Quality Control name that this user has access to");
                validationResult |= SampleProcessingValidationResult.InvalidCellTypeOrQcName;
            }

            if (aCupSample.IsQualityControl && qcLookup == null)
            {
                _log.Info(
                    $"CanProcessSamples: Carrier is AutomationCup and aCupSample.IsQualityControl is TRUE and aCupSample.CellTypeQcName does not match any existing Quality Control name that this user has access to");
                validationResult |= SampleProcessingValidationResult.InvalidCellTypeOrQcName;
            }

            if (aCupSample.IsQualityControl && qcLookup?.NotExpired == false)
            {
                _log.Info($"CanProcessSamples: Carrier is AutomationCup and aCupSample.IsQualityControl is TRUE and aCupSample.CellTypeQcName is expired.");
                validationResult |= SampleProcessingValidationResult.QcExpired;
            }

            if (!aCupSample.IsQualityControl && ctLookup == null)
            {
                _log.Info(
                    $"CanProcessSamples: Carrier is AutomationCup and aCupSample.IsQualityControl is FALSE and aCupSample.CellTypeQcName does not match any existing Cell Type name that this user has access to");
                validationResult |= SampleProcessingValidationResult.InvalidCellTypeOrQcName;
            }

            return validationResult;
        }
        
        private SampleProcessingValidationResult IsValidFor96WellPlate(string username, IList<SampleSetDomain> sampleSets)
        {
            var validationResult = SampleProcessingValidationResult.Valid;

            var setsContainSamples = sampleSets.SelectMany(s =>
                s.Samples ?? new List<SampleEswDomain>()).Any();

            if (!setsContainSamples)
            {
                _log.Info($"CanProcessSamples: Carrier is 96WellPlate and there is no sample definition");
                validationResult |= SampleProcessingValidationResult.NoSamplesDefined;
                return validationResult;
            }

            var plateSampleSet = sampleSets.FirstOrDefault();
            if (plateSampleSet?.Samples == null)
            {
                _log.Info($"CanProcessSamples: Carrier is 96WellPlate and there is unexpected data (first sampleSet is NULL or first sampleSet's Samples is NULL)");
                validationResult |= SampleProcessingValidationResult.NoSamplesDefined;
                return validationResult;
            }

            if (plateSampleSet.Samples.Count < 1)
            {
                _log.Info($"CanProcessSamples: Carrier is 96WellPlate and there is less than 1 sample (there are '{plateSampleSet.Samples.Count}' samples)");
                validationResult |= SampleProcessingValidationResult.NoSamplesDefined;
                return validationResult;
            }

            foreach (var sample in plateSampleSet.Samples)
            {
                if (sample.Dilution < ApplicationConstants.MinimumDilutionFactor ||
                    sample.Dilution > ApplicationConstants.MaximumDilutionFactor)
                {
                    _log.Info($"CanProcessSamples: Carrier is 96WellPlate and contains at least 1 sample where Dilution is out of range (value: '{sample.Dilution}')");
                    validationResult |= SampleProcessingValidationResult.InvalidDilution;
                }

                if (sample.SaveEveryNthImage < ApplicationConstants.MinimumNumberOfNthImages ||
                    sample.SaveEveryNthImage > ApplicationConstants.MaximumNumberOfNthImages)
                {
                    _log.Info($"CanProcessSamples: Carrier is 96WellPlate and contains at least 1 sample where SaveEveryNthImage is out of range (value: {sample.SaveEveryNthImage})");
                    validationResult |= SampleProcessingValidationResult.InvalidSaveNthImage;
                }

                if (!sample.SamplePosition.Is96WellPlate())
                {
                    _log.Info($"CanProcessSamples: Carrier is 96WellPlate and contains at least 1 sample where SamplePosition is invalid (not a 96 Well Plate sample position. Value = '{sample.SamplePosition}')");
                    validationResult |= SampleProcessingValidationResult.InvalidSamplePosition;
                }

                if (string.IsNullOrEmpty(sample.CellTypeQcName))
                {
                    _log.Info($"CanProcessSamples: Carrier is 96WellPlate and sample.CellTypeQcName is NULL or empty");
                    validationResult |= SampleProcessingValidationResult.InvalidCellTypeOrQcName;
                }

                var cellType = _cellTypeManager.GetCellTypeDomain(username, "password", sample.CellTypeQcName);
                var qc = _cellTypeManager.GetQualityControlDomain(username, "password", Misc.GetBaseQualityControlName(sample.CellTypeQcName));

                if (cellType == null && qc == null)
                {
                    _log.Info($"CanProcessSamples: Carrier is 96WellPlate and sample.CellTypeQcName does not match any existing Cell Type or Quality Control name that this user has access to");
                    validationResult |= SampleProcessingValidationResult.InvalidCellTypeOrQcName;
                }

                if (sample.IsQualityControl && qc == null)
                {
                    _log.Info($"CanProcessSamples: Carrier is 96WellPlate and sample.IsQualityControl is TRUE and sample.CellTypeQcName does not match any existing Quality Control name that this user has access to");
                    validationResult |= SampleProcessingValidationResult.InvalidCellTypeOrQcName;
                }

                if (sample.IsQualityControl && qc?.NotExpired == false)
                {
                    _log.Info($"CanProcessSamples: Carrier is 96WellPlate and sample.IsQualityControl is TRUE and sample.CellTypeQcName is expired.");
                    validationResult |= SampleProcessingValidationResult.QcExpired;
                }

                if (!sample.IsQualityControl && cellType == null)
                {
                    _log.Info($"CanProcessSamples: Carrier is 96WellPlate and sample.IsQualityControl is FALSE and sample.CellTypeQcName does not match any existing Cell Type name that this user has access to");
                    validationResult |= SampleProcessingValidationResult.InvalidCellTypeOrQcName;
                }
            }

            return validationResult;
        }

        /// <summary>
        /// Checks to ensure that:
        /// - StartSingleSample is only used with Automation Cup
        /// - StartMultiSample is only used with 96 well plate
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="usingStartMultipleSamplesMethod"></param>
        /// <param name="validationResult"></param>
        /// <returns></returns>
        private bool AutomationSamplePositionIsValid(IList<SampleEswDomain> samples, bool usingStartMultipleSamplesMethod,
            ref SampleProcessingValidationResult validationResult)
        {
            if (usingStartMultipleSamplesMethod)
            {
                var result = samples.All(s => s.SamplePosition.Is96WellPlate());

                if (!result)
                    validationResult |= SampleProcessingValidationResult.InvalidSamplePosition;

                return result;
            }
            else
            {
                var result = samples.Count == 1 && samples.All(s => s.SamplePosition.IsAutomationCup());

                if (!result)
                    validationResult |= SampleProcessingValidationResult.InvalidSamplePosition;

                return result;
            }
        }

        private static SampleSetDomain CreateOrphanSampleSet(string username, SubstrateType carrier)
        {
            var currentOrphanSampleSet = new SampleSetDomain();
            currentOrphanSampleSet.Carrier = carrier;
            currentOrphanSampleSet.Index = ApplicationConstants.OrphanSampleSetIndex;
            currentOrphanSampleSet.SampleSetName = LanguageResourceHelper.Get("LID_OrphanSetName");
            currentOrphanSampleSet.Username = username;
            currentOrphanSampleSet.Samples = new List<SampleEswDomain>();
            currentOrphanSampleSet.SampleSetStatus = SampleSetStatus.Pending;
            currentOrphanSampleSet.Timestamp = DateTimeConversionHelper.DateTimeToUnixSecondRounded(DateTime.Now);
            return currentOrphanSampleSet;
        }

        private static WorkListDomain CreateWorkListDomain(string username, SampleSetTemplateDomain sampleSetTemplate,
            IList<SampleSetDomain> sampleSets, IDataAccess runOptionsSettingsDataAccess, SubstrateType carrier)
        {
            var wlDomain = new WorkListDomain();
            wlDomain.Carrier = carrier;
            foreach (var sampleSet in sampleSets)
            {
                if (sampleSet.Samples.Any())
                {
                    wlDomain.Precession = sampleSet.Samples.First().PlatePrecession;
                    break;
                }
            }
            wlDomain.Label = string.Empty;
            wlDomain.RunByUsername = username;
            wlDomain.SampleSets = sampleSets;
            wlDomain.TimeStamp = DateTimeConversionHelper.DateTimeToUnixSecondRounded(DateTime.Now);
            wlDomain.Username = username;

            if (sampleSetTemplate == null)
            {
                var settings = new RunOptionSettingsModel(runOptionsSettingsDataAccess, username);
                wlDomain.CellTypeIndex = settings.DefaultBPQC;
                wlDomain.DilutionFactor = settings.Dilution;
                wlDomain.QualityControlName = string.Empty;
                wlDomain.Tag = string.Empty;
                wlDomain.Wash = settings.DefaultWash;
                wlDomain.SaveEveryNthImage = ApplicationConstants.SaveEveryNthImage;
                wlDomain.SequenceParameters = new SequenceParametersDomain();
                wlDomain.SequenceParameters.UseSequencing = false;
                wlDomain.SequenceParameters.SequencingBaseLabel = settings.DefaultSampleId;
                wlDomain.SequenceParameters.SequencingNumberOfDigits = 0;
                wlDomain.SequenceParameters.SequencingStartingDigit = 0;
                wlDomain.SequenceParameters.SequencingTextFirst = true;
            }
            else
            {
                wlDomain.CellTypeIndex = sampleSetTemplate.CellTypeIndex;
                wlDomain.DilutionFactor = sampleSetTemplate.Dilution;
                wlDomain.QualityControlName = sampleSetTemplate.QualityControlName;
                wlDomain.Tag = sampleSetTemplate.SampleTag;
                wlDomain.Wash = sampleSetTemplate.WashType;
                wlDomain.SaveEveryNthImage = sampleSetTemplate.SaveEveryNthImage;
                wlDomain.SequenceParameters = new SequenceParametersDomain();
                wlDomain.SequenceParameters.UseSequencing = sampleSetTemplate.UseSequencing;
                wlDomain.SequenceParameters.SequencingBaseLabel = sampleSetTemplate.SequencingBaseLabel;
                wlDomain.SequenceParameters.SequencingNumberOfDigits = (ushort)sampleSetTemplate.SequencingNumberOfDigits;
                wlDomain.SequenceParameters.SequencingStartingDigit = (ushort)sampleSetTemplate.SequencingStartingDigit;
                wlDomain.SequenceParameters.SequencingTextFirst = sampleSetTemplate.SequencingTextFirst;
            }

            return wlDomain;
        }

        private void CleanImageTargetDirectory()
        {
            var targetDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ApplicationConstants.TargetFolderName);
            FileSystem.DeleteAllFilePath(targetDir);
        }

        private bool UsesSameSamplePositionInMultipleSamples(List<SampleEswDomain> samples)
        {
            var positions = samples.Select(s => s.SamplePosition.ToString());
            var hash = new Hashtable(); // using hash so we only have 1 loop iteration
            foreach (var pos in positions)
            {
                if (hash.ContainsKey(pos))
                {
                    return true;
                }

                hash.Add(pos, pos);
            }

            return false;
        }

        #endregion
    }
}