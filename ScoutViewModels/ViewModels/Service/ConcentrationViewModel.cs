using ApiProxies.Generic;
using ApiProxies.Misc;
using Ninject;
using ScoutDomains;
using ScoutDomains.Common;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Home.QueueManagement;
using ScoutModels.Service;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.EventDomain;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using ScoutUtilities.Structs;
using ScoutViewModels.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ScoutModels.Interfaces;
using ScoutServices.Interfaces;
using Application = System.Windows.Application;

namespace ScoutViewModels.ViewModels.Service
{
    public class ConcentrationViewModel : BaseViewModel
    {
        #region Fields

        private readonly IRunningWorkListModel _runningWorkListModel;
        private readonly IInstrumentStatusService _instrumentStatusService;
        private readonly ICapacityManager _capacityManager;

        private IDisposable _statusSubscriber;

        #endregion

        public ConcentrationViewModel(IRunningWorkListModel runningWorkListModel, IInstrumentStatusService instrumentStatusService, ICapacityManager capacityManager, IRunSampleHelper runSampleHelper)
        {
            _runningWorkListModel = runningWorkListModel;
            _instrumentStatusService = instrumentStatusService;
            _capacityManager = capacityManager;
            IsListViewActive = true;
            IsImageAvailable = true;
            IsConcentrationCompleted = true;
            IsSampleRunning = true;
            PlayPauseImagePath = "/Images/Play.png";
            SkipImagePath = "/Images/Forward_Ash.png";
            StopImagePath = "/Images/StopAsh.png";
            ConcentrationFormDate = DateTime.Now.AddDays(-30);
            ConcentrationToDate = DateTime.Now;

            RunSampleHelper = runSampleHelper; // used to store image and result data -- can be refactored

            _workQueueResultRecord = new WorkQueueRecordDomain();
            SelectedSampleRecord = new SampleRecordDomain();
            SampleImageResultList = new List<SampleImageRecordDomain>();
            DataGridExpanderColumnConcHeaderList = new ObservableCollection<DataGridExpanderColumnHeader>();
            DataGridExpanderColumnSizeHeaderList = new ObservableCollection<DataGridExpanderColumnHeader>();
            ConcentrationList = CalibrationModel.GetStandardConcentrationList().ToObservableCollection();
            CarouselSamples = QueueManagementModel.CreateEmptyCarousel().ToObservableCollection();

            ResetGraphList();
            SetActiveView(ViewType.ListView);
            ResetDataGridExpanderColumnConcHeaderList();
            SetDefaultPlayPause();

            // Set up event handlers
            _statusSubscriber = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe(OnSystemStatusChanged);

            _runningWorkListModel.ConcentrationWorkListItemStatusUpdated += OnWorkListItemStatusCallback;
            _runningWorkListModel.ConcentrationWorkListItemCompleted += OnWorkListItemCompleteCallback;
            _runningWorkListModel.ConcentrationWorkListCompleted += OnWorkListCompleteCallback;
            _runningWorkListModel.ConcentrationWorkListImageResultOccurred += OnWorkListImageResultCallback;
        }

        protected override void DisposeUnmanaged()
        {
            _runningWorkListModel.ConcentrationWorkListItemStatusUpdated -= OnWorkListItemStatusCallback;
            _runningWorkListModel.ConcentrationWorkListItemCompleted -= OnWorkListItemCompleteCallback;
            _runningWorkListModel.ConcentrationWorkListCompleted -= OnWorkListCompleteCallback;
            _runningWorkListModel.ConcentrationWorkListImageResultOccurred -= OnWorkListImageResultCallback;

            _statusSubscriber?.Dispose();
            base.DisposeUnmanaged();
        }

        #region Event Handlers

        private void OnSystemStatusChanged(SystemStatusDomain systemStatusDomain)
        {
            if (systemStatusDomain == null)
                return;
            _systemStatusDomain = systemStatusDomain;
            
            // update the carousel position
            OnCarouselStatusChange(new StageStatus
            {
                IsCarouselStatus = _systemStatusDomain.CarouselDetect,
                SamplePosition = _systemStatusDomain.SamplePosition
            });

            // only change things if the state has actually changed
            var changed = _systemStatus != _systemStatusDomain.SystemStatus;
            if (!changed) return;
            
            _systemStatus = _systemStatusDomain.SystemStatus;

            DispatcherHelper.ApplicationExecute(() =>
            {
                AbortStatusContent = string.Empty;
                PlayPauseStatusContent = string.Empty;
            });

            switch (_systemStatus)
            {
                case SystemStatus.Pausing:
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        IsEnablePlayPause = IsStopActive = false;
                        PlayPauseStatusContent = LanguageResourceHelper.Get("LID_MSGBOX_PausingPleaseWait");
                    });
                    break;

                case SystemStatus.Paused:
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        IsPauseActive = false;
                        IsEnablePlayPause = IsStopActive = true;
                        if (!IsPaused)
                        {
                            IsPaused = true;
                            if (_isConcentrationRunning) // PC3549-5134
                                InvokeSystemErrorDialog(LanguageResourceHelper.Get("LID_MSGBOX_PAUSEDSAMPLE"));
                        }

                        PlayPauseStatusContent = LanguageResourceHelper.Get("LID_Label_Resume");
                    });

                    PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_WorkQueuePaused"));
                    ClearRunProgressUI();
                    break;

                case SystemStatus.Stopping:
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        IsEnablePlayPause = IsStopActive = false;
                        AbortStatusContent = LanguageResourceHelper.Get("LID_Label_Aborting");
                    });
                    break;

                case SystemStatus.Stopped:
                    DispatcherHelper.ApplicationExecute(AbortAfterSampleRun);
                    break;

                case SystemStatus.Faulted:
                    var showMes = _isConcentrationRunning; // AbortAfterSampleRun sets this to FALSE and the user needs to know what happened
                    DispatcherHelper.ApplicationExecute(AbortAfterSampleRun);
                    if (showMes)
                    {
                        InvokeSystemErrorDialog(LanguageResourceHelper.Get("LID_MSGBOX_ABORTEDSAMPLE"));
                    }
                    break;

                case SystemStatus.SearchingTube:
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        IsLoadingIndicatorActive = true;
                        SetSampleProgressStatusColor(SampleProgressStatus.IsReady,
                            SampleProgressStatus.IsReady,
                            SampleProgressStatus.IsReady,
                            SampleProgressStatus.IsReady,
                            LanguageResourceHelper.Get("LID_ProgressIndication_FindingTubes"));
                    });
                    break;

                case SystemStatus.ProcessingSample:
                    DispatcherHelper.ApplicationExecute(() => IsPauseActive = IsStopActive = true);
                    break;
            }
        }

        private void OnCarouselStatusChange(StageStatus stageStatus)
        {
            // Updated with -1 to remove highlighted position in carousel for invalid position from API.
            var samplePosition = stageStatus.SamplePosition;
            var newPosition = samplePosition.IsValid() ? samplePosition.Column : -1;
            if (PlayItemPosition != newPosition &&
                stageStatus.IsCarouselStatus == eSensorStatus.ssStateActive)
            {
                DispatcherHelper.ApplicationExecute(() => PlayItemPosition = newPosition);
            }
        }

        private void OnWorkListItemStatusCallback(object sender, ApiEventArgs<SampleEswDomain> args)
        {
            var sampleEswDomain = args.Arg1;
            Log.Debug($"OnWorkListItemStatusCallback:: {sampleEswDomain.SampleName}, status: {sampleEswDomain.SampleStatus}");

            ChangeRunProgressUI(sampleEswDomain.SampleStatus, sampleEswDomain);
            UpdateStartDataGridExpanderColumnHeader(sampleEswDomain);
        }

        private void OnWorkListItemCompleteCallback(object sender, ApiEventArgs<SampleEswDomain> args)
        {
            try
            {
                var sampleEswDomain = args.Arg1;

                if (sampleEswDomain.SampleStatus == SampleStatus.SkipManual ||
                    sampleEswDomain.SampleStatus == SampleStatus.SkipError) return;

                _queueId = sampleEswDomain.SampleDataUuid;
                Log.Debug("OnWorkListItemCompleteCallback:: " + sampleEswDomain.SampleName);

                foreach (var sample in CarouselSamples
                    .Where(s => s.SamplePosition.Column == sampleEswDomain.SamplePosition.Column))
                {
                    sample.SampleStatusColor = SampleStatusColor.Completed;
                }

                DispatcherHelper.ApplicationExecute(() =>
                {
                    ChangesDialogResultCarouselPlate = true;
                    ChangesDialogResultCarouselPlate = false;
                });

                ClearRunProgressUI();

                // Update the reagent status
                var msg = new Notification<ReagentContainerStateDomain>(
                    ReagentModel.GetReagentContainerStatusAll().FirstOrDefault(),
                    MessageToken.RefreshReagentStatus, "");
                MessageBus.Default.Publish(msg);

            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_HANDLE_WORK_QUEUE_ITEM_COMPLETED"));
            }
        }

        private void OnWorkListCompleteCallback(object sender, ApiEventArgs<uuidDLL> args)
        {
            try
            {
                var workListUuid = args.Arg1;
                Log.Debug($"OnWorkListCompleteCallback - {workListUuid}");
                _queueId = workListUuid;

                ClearCarousel();

                var tubes = ConcentrationList.Sum(x => x.NumberOfTubes);
                if (CarouselSamples.Count(x => x.SampleStatusColor == SampleStatusColor.Completed) != tubes)
                {
                    IsConcAcceptButtonEnable = false;
                }

                Graph.NotifyAllPropertiesChanged();
                NotifyAllPropertiesChanged();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_HANDLE_WORK_QUEUE_COMPLETED"));
            }
        }

        private void OnWorkListImageResultCallback(object sender,
            ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers> args)
        {
            var sampleEswDomain = args.Arg1;
            var imageSequenceNumber = args.Arg2; // EG: 14 out of 100
            var cumulativeResults = args.Arg3;
            var imageSet = args.Arg4;
            var imageResults = args.Arg5;

            try
            {
                RunSampleHelper.AddResultSample(sampleEswDomain, cumulativeResults, imageResults, imageSet, imageSequenceNumber);
                _workQueueResultRecord = RunSampleHelper.WorkQueueResultRecord;
                SelectedSampleRecord = RunSampleHelper.SelectedSampleRecord;
                UpdateDataGridExpanderColumnHeader(sampleEswDomain, cumulativeResults);
                IsImageAvailable = false;
            }
            catch (ArgumentNullException ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_CONCENTRATE_IMAGE_RESULT_QUEUE"));
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_CONCENTRATE_IMAGE_RESULT_QUEUE"));
            }
        }

        #endregion

        #region Properties

        private uuidDLL _queueId;
        private WorkQueueRecordDomain _workQueueResultRecord;
        private bool _isConcentrationRunning;
        private bool _isConcentrationCompleted;
        private SystemStatus _systemStatus;
        private SystemStatusDomain _systemStatusDomain;

        public ObservableCollection<ICalibrationConcentrationListDomain> ConcentrationList
        {
            get { return GetProperty<ObservableCollection<ICalibrationConcentrationListDomain>>(); }
            private set { SetProperty(value); }
        }

        public ObservableCollection<SampleDomain> CarouselSamples
        {
            get { return GetProperty<ObservableCollection<SampleDomain>>(); }
            set { SetProperty(value); }
        }

        public string AbortStatusContent
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string PlayPauseStatusContent
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public int PlayItemPosition
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public bool IsEnablePlayPause
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsPaused
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsPauseActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsUsingCarousel
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsStopActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsExportViewActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsLoadingIndicatorActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public double CalibrationIntercept
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double CalibrationSlope
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double CalibrationR2
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public bool IsConcentrationCompleted
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsImageAvailable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string LastConcentrationSlope
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public DateTime LastConcentrationSlopeDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public string ConcentrationComment
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsSampleRunning
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsSampleStatusCompleted
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsConcentrationUpdateActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsPaginationButtonEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsConcAcceptButtonEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsStatusCompleted
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsCalibrationCompleted
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string GraphCalibrationPrimaryTrendLabel
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string RunSampleStatus
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsListAvailable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public IRunSampleHelper RunSampleHelper
        {
            get { return GetProperty<IRunSampleHelper>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<CalibrationActivityLogDomain> ConcentrationOverTimeList
        {
            get { return GetProperty<ObservableCollection<CalibrationActivityLogDomain>>(); }
            set { SetProperty(value); }
        }

        public SampleProgressStatus AspirationBrush
        {
            get { return GetProperty<SampleProgressStatus>(); }
            set { SetProperty(value); }
        }

        public SampleProgressStatus MixingDyeBrush
        {
            get { return GetProperty<SampleProgressStatus>(); }
            set { SetProperty(value); }
        }

        public SampleProgressStatus CleaningBrush
        {
            get { return GetProperty<SampleProgressStatus>(); }
            set { SetProperty(value); }
        }

        public SampleProgressStatus ImageAnalysisBrush
        {
            get { return GetProperty<SampleProgressStatus>(); }
            set { SetProperty(value); }
        }

        public string PlayPauseImagePath
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool SkipQueueEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool StopQueueEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string SkipImagePath
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string StopImagePath
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public DateTime ConcentrationFormDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public DateTime ConcentrationToDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public bool IsClearingCarousel
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<DataGridExpanderColumnHeader> DataGridExpanderColumnConcHeaderList
        {
            get { return GetProperty<ObservableCollection<DataGridExpanderColumnHeader>>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<DataGridExpanderColumnHeader> DataGridExpanderColumnSizeHeaderList
        {
            get { return GetProperty<ObservableCollection<DataGridExpanderColumnHeader>>(); }
            set { SetProperty(value); }
        }

        public bool ChangesDialogResultCarouselPlate
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsOverTimeActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsListViewActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsImageViewActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsGraphViewActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsImageGraphActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsImageSelectionChanged
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsPlayingSample
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                OnPlayingSampleChanged();
            }
        }

        public bool IsConcentrationTabActive
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                if (!value) return; // we're done here
                SetConcentrationDefaultValue("concentration");
                if (IsSampleRunning)
                    UpdateConcentrationValue();
                SetLogEnable(calibration_type.cal_Concentration, 0, 0);
                SetLatestSlopeData();
            }
        }

        #region Graph

        public List<LineGraphDomain> GraphViewList
        {
            get { return GetProperty<List<LineGraphDomain>>(); }
            set { SetProperty(value); }
        }

        public LineGraphDomain Graph
        {
            get { return GetProperty<LineGraphDomain>(); }
            set { SetProperty(value); }
        }

        public SampleRecordDomain SelectedSampleRecord
        {
            get { return GetProperty<SampleRecordDomain>(); }
            set { SetProperty(value); }
        }

        public List<SampleImageRecordDomain> SampleImageResultList
        {
            get { return GetProperty<List<SampleImageRecordDomain>>(); }
            set { SetProperty(value); }
        }

        public DataGridExpanderColumnHeader SelectedConcentrationSampleItem
        {
            get { return GetProperty<DataGridExpanderColumnHeader>(); }
            set
            {
                SetProperty(value);

                if (SampleImageResultList.Count > 0) IsImageSelectionChanged = false;
                IsImageSelectionChanged = true;
            }
        }

        #endregion

        #endregion

        #region Commands

        #region Result Carousel Rotate

        private RelayCommand _resultCarouselRotateCommand;
        public RelayCommand ResultCarouselRotateCommand
        {
            get { return _resultCarouselRotateCommand ?? (_resultCarouselRotateCommand = new RelayCommand(RotateResultCarousel)); }
            set { _resultCarouselRotateCommand = value; } // Needs to be here for a custom control
        }

        private void RotateResultCarousel()
        {
            // Updated with -1 to remove highlighted position in carousel for invalid position from API.
            var samplePosition = _systemStatusDomain.SamplePosition;
            PlayItemPosition = samplePosition.IsValid() ? samplePosition.Column : -1;

            var temp = PlayItemPosition;
            if (temp == 0) temp = 2;
            else temp += 1;

            var newSamplePosition = SamplePosition.Parse(temp.ToString());
            var hawkeyeError = QueueManagementModel.RotateCarousel(ref newSamplePosition);
            if (hawkeyeError != HawkeyeError.eSuccess)
            {
                ApiHawkeyeMsgHelper.ErrorCommon(hawkeyeError);
                return;
            }

            if (newSamplePosition.IsValid()) PlayItemPosition = newSamplePosition.Column;
            PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_CarouselRotatedOnePosition"));
        }

        #endregion

        #region Start/Stop/Eject Command

        private RelayCommand _executeSampleCommand;
        public RelayCommand ExecuteSampleCommand => _executeSampleCommand ?? (_executeSampleCommand = new RelayCommand(ExecuteSamples));

        private void ExecuteSamples(object parameter)
        {
            try
            {
                if (parameter == null) return;

                switch (parameter.ToString())
                {
                    case "Start": if (ValidateConcentrationValues()) StartSamplesAsync(); break;
                    case "Stop": StopSamplesAsync(); break;
                    case "Eject": EjectSampleStageAsync(); break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_WHILE_EXECUTING_SAMPLE"));
            }
        }

        private async void StartSamplesAsync()
        {
            if (IsPauseActive)
            {
                PauseSamplesAsync();
                return;
            }

            if (IsPaused)
            {
                ResumeSamplesAsync();
                return;
            }

            _capacityManager.InsufficientReagentPackUsesLeft(ApplicationConstants.NumberOfTubesInConcentration);
            if (IsUsingCarousel)
            {
                _capacityManager.InsufficientDisposalTrayCapacity(ApplicationConstants.NumberOfTubesInConcentration);
            }

            ClearConcentrationSizeList();
            var wld = CalibrationModel.GetConcentrationWorkListDomain(ConcentrationComment);

            var result = await Task.Run(() => _runningWorkListModel.SetWorkList(wld, WorkListSource.Concentration));
            if (result)
            {
                await Task.Run(StartWorkList);
            }
        }

        private async void StopSamplesAsync()
        {
            if (DialogEventBus.DialogBoxYesNo(this,
                    LanguageResourceHelper.Get("LID_MSGBOX_QueueManagementAbort")) == true)
            {
                var username = LoggedInUser.CurrentUserId;
                await Task.Run(() => _runningWorkListModel.StopProcessing(username, string.Empty));
            }
        }

        private async void EjectSampleStageAsync()
        {
            var username = LoggedInUser.CurrentUserId;
            if (IsPaused)
            {
                if (DialogEventBus.DialogBoxOk(this,
                        LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_DiscardRunSample")) == true)
                {
                    var result = await Task.Run(() => _runningWorkListModel.StopProcessing(username, string.Empty));
                    if (result)
                    {
                        await _runningWorkListModel.EjectSampleStageAsync(username, string.Empty);
                    }
                }
            }
            else
            {
                await _runningWorkListModel.EjectSampleStageAsync(username, string.Empty);
            }
        }

        #endregion

        #region Search Log Command

        private RelayCommand _searchLog;
        public RelayCommand SearchLog => _searchLog ?? (_searchLog = new RelayCommand(OnSearchLogFile));

        private void OnSearchLogFile(object param)
        {
            try
            {
                var startTime = DateTimeConversionHelper.DateTimeToUnixSecondRounded(ConcentrationFormDate);
                var endTime = DateTimeConversionHelper.DateTimeToEndOfDayUnixSecondRounded(ConcentrationToDate);

                var overTimeStatus = param.ToString();
                switch (overTimeStatus)
                {
                    case "CleanConcentration":
                        if (DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_MSGBOX_DeleteConcentrationSlopeHistory")) == true)
                        {
                            var args = new LoginEventArgs(LoggedInUser.CurrentUserId,
                                LoggedInUser.CurrentUserId, LoginState.ValidateCurrentUserOnly,
                                DialogLocation.CenterApp, false, true);

                            if (DialogEventBus.Login(this, args) == LoginResult.CurrentUserLoginSuccess)
                            {
                                var status = CalibrationModel.ClearCalibrationActivityLog(calibration_type.cal_Concentration, endTime, args.Password, false);
                                if (status.Equals(HawkeyeError.eSuccess))
                                {
                                    PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_ConLoghasBeenCleared"));
                                }
                                else
                                {
                                    ApiHawkeyeMsgHelper.ErrorValidate(status);
                                }
                                SetLogEnable(calibration_type.cal_Concentration, startTime, endTime);
                            }
                        }
                        break;
                    case "SearchConcentration":
                        SetLogEnable(calibration_type.cal_Concentration, startTime, endTime);
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_CONCENTRATE_SEARCH_LOG"));
            }
        }

        #endregion

        #region Select Tab command

        private RelayCommand _concentrationCommand;
        public RelayCommand ConcentrationCommand => _concentrationCommand ?? (_concentrationCommand = new RelayCommand(OnConcentrationCall));

        private void OnConcentrationCall(object type)
        {
            string conType = (string)type;
            switch (conType)
            {
                case "OverTime":
                    SetActiveView(ViewType.OverTimeView);
                    if (ConcentrationOverTimeList != null && ConcentrationOverTimeList.Any())
                    {
                        ConcentrationFormDate = ConcentrationOverTimeList.Min(a => a.Date);
                        ConcentrationToDate = ConcentrationOverTimeList.Max(a => a.Date);
                    }
                    break;
                case "List":
                    SetActiveView(ViewType.ListView);
                    break;
                case "Camera":
                    SetActiveView(ViewType.ImageView);
                    break;
                case "Graph":
                    SetActiveView(ViewType.GraphView);
                    break;
            }
        }

        #endregion

        #region Image Expand command

        private RelayCommand _imageExpandCommand;
        public RelayCommand ImageExpandCommand => _imageExpandCommand ?? (_imageExpandCommand = new RelayCommand(OnExpandImage, null));

        private void OnExpandImage()
        {
            var imageList = SelectedSampleRecord?.SampleImageList?.Cast<object>()?.ToList();
            var index = SelectedSampleRecord?.SelectedSampleImageRecord == null ? -1 : (int)SelectedSampleRecord.SelectedSampleImageRecord.SequenceNumber;
            var args = new ExpandedImageGraphEventArgs(ImageType.Annotated, ApplicationConstants.ImageViewRightClickMenuImageFitSize, imageList, SelectedSampleRecord, index);
            DialogEventBus.ExpandedImageGraph(this, args);
        }

        #endregion

        #region Graph Expand command

        private RelayCommand _graphExpandCommand;
        public RelayCommand GraphExpandCommand => _graphExpandCommand ?? (_graphExpandCommand = new RelayCommand(OnExpandGraph, null));

        private void OnExpandGraph()
        {
            var graphList = GraphViewList?.Cast<object>()?.ToList();
            var index = GraphViewList?.IndexOf(Graph) ?? -1;
            var args = new ExpandedImageGraphEventArgs(ExpandedContentType.ScatterChart, ApplicationConstants.ImageViewRightClickMenuImageFitSize, graphList, Graph, index);
            DialogEventBus.ExpandedImageGraph(this, args);
        }

        #endregion

        #region Update Concentration command

        private RelayCommand _updateConcentrationCommand;
        public RelayCommand UpdateConcentrationCommand => _updateConcentrationCommand ?? (_updateConcentrationCommand = new RelayCommand(OnUpdateConcentrationDetails));

        private void OnUpdateConcentrationDetails(object obj)
        {
            try
            {
                switch (obj.ToString())
                {
                    case "Export":
                        ExportConcentrationSlopeDetails();
                        break;

                    case "CancelConcentration":
                        if (DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_MSGBOX_CancelConcentration")) == true)
                        {
                            IsEnablePlayPause = true;
                            SetHamburgerEnableDisable(true);
                            ResetConcentration();
                            IsCalibrationCompleted = false;

                            _isConcentrationCompleted = true;
                            _isConcentrationRunning = false;
                        }
                        break;

                    case "UpdateConcentration":
                        AcceptNewConcentration();

                        _isConcentrationCompleted = true;
                        _isConcentrationRunning = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_UPDATE_CONCENTRATION_DETAILS"));
            }
        }

        #endregion

        #region Concentration Loaded command

        private RelayCommand _concentrationLoadedCommand;
        public RelayCommand ConcentrationLoadedCommand => _concentrationLoadedCommand ?? (_concentrationLoadedCommand = new RelayCommand(OnLoaded));

        private void OnLoaded()
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                ChangesDialogResultCarouselPlate = true;
                ChangesDialogResultCarouselPlate = false;
            });
        }

        #endregion

        #endregion

        #region Private Methods

        private void ResetGraphList()
        {
            GraphViewList = new List<LineGraphDomain> { new LineGraphDomain(), new LineGraphDomain(), new LineGraphDomain(), new LineGraphDomain() };
            Graph = GraphViewList.FirstOrDefault();
            Graph?.NotifyAllPropertiesChanged();
        }

        private bool ValidateConcentrationValues()
        {
            foreach (var item in ConcentrationList)
            {
                var assayValue = item.AssayValue;
                if (assayValue <= 0.0)
                {
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_AssayValueFieldBlank"));
                    return false;
                }

                // todo: actually check the assay value ranges here

                switch (item.AssayValueType)
                {
                    case AssayValueEnum.M2:
                        if (!item.IsCorrectAssayValue)
                        {
                            var msg = string.Format(LanguageResourceHelper.Get("LID_ERRMSGBOX_TheAssayValueIsNotWithinRange"), ScoutUtilities.Misc.ConvertToString(2));
                            DialogEventBus.DialogBoxOk(this, msg);
                            return false;
                        }
                        break;

                    case AssayValueEnum.M4:
                        if (!item.IsCorrectAssayValue)
                        {
                            var msg = string.Format(LanguageResourceHelper.Get("LID_ERRMSGBOX_TheAssayValueIsNotWithinRange"), ScoutUtilities.Misc.ConvertToString(4));
                            DialogEventBus.DialogBoxOk(this, msg);
                            return false;
                        }
                        break;

                    case AssayValueEnum.M10:
                        if (!item.IsCorrectAssayValue)
                        {
                            var msg = string.Format(LanguageResourceHelper.Get("LID_ERRMSGBOX_TheAssayValueIsNotWithinRange"), ScoutUtilities.Misc.ConvertToString(10));
                            DialogEventBus.DialogBoxOk(this, msg);
                            return false;
                        }
                        break;
                }

                if (string.IsNullOrEmpty(item.Lot))
                {
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_LotNumberFieldBlank"));
                    return false;
                }

                if (item.ExpiryDate.Date < DateTime.Now.Date)
                {
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_Calibration_ExpirationDate"));
                    return false;
                }

                var reagentContainerStatus = ReagentModel.GetReagentContainerStatusAll()?.FirstOrDefault();
                if(reagentContainerStatus == null || reagentContainerStatus.EventsRemaining == null || reagentContainerStatus.EventsRemaining <= 17)
                {
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ReagentStatus_LReagentInsufficientRemainingFail"));
                    return false;
                }

            }
            return true;
        }

        private void ClearConcentrationSizeList()
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                if (DataGridExpanderColumnConcHeaderList != null && DataGridExpanderColumnConcHeaderList.Count > 0)
                    DataGridExpanderColumnConcHeaderList.Clear();
            });
        }

        private void StartWorkList()
        {
            IsSampleRunning = false;
            DispatcherHelper.ApplicationExecute(() =>
            {
                IsPaginationButtonEnable = true;
                IsPaginationButtonEnable = false;
                IsConcAcceptButtonEnable = false;
            });

            SampleImageResultList = new List<SampleImageRecordDomain>();
            var username = LoggedInUser.CurrentUserId;
            var result = _runningWorkListModel.StartProcessing(username, string.Empty);
            if (result)
            {
                PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_WorkQueueStart"));
                SetResultCarouselDefault(false);
                SetHamburgerEnableDisable(false);

                _isConcentrationCompleted = false;
                _isConcentrationRunning = true;

                IsConcentrationCompleted = false;
                IsCalibrationCompleted = false;
                IsConcAcceptButtonEnable = false;
            }
            else
            {
                SetResultCarouselDefault(true);
                _isConcentrationCompleted = true;
            }
        }

        private void SetResultCarouselDefault(bool status)
        {
            MessageBus.Default.Publish(new Notification<bool>(status, nameof(ServiceViewModel), nameof(ServiceViewModel.IsMotorRunning)));
        }

        private async void PauseSamplesAsync()
        {
            var username = LoggedInUser.CurrentUserId;
            var result = await Task.Run(() => _runningWorkListModel.PauseProcessing(username, string.Empty));
            if (result)
            {
                IsPaused = true;
            }
        }

        private async void ResumeSamplesAsync()
        {
            var username = LoggedInUser.CurrentUserId;
            var result = await Task.Run(() => _runningWorkListModel.ResumeProcessing(username, string.Empty));
            if (result)
            {
                SetHamburgerEnableDisable(false);
                PlayPauseStatusContent = string.Empty;
                PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_QueueResumed"));
                IsPaused = false;
            }
        }

        private void ClearCarousel()
        {
            IsConcentrationCompleted = true;
            IsCalibrationCompleted = true;
            ClearRunProgressUI();
            IsEnablePlayPause = IsStopActive = IsPauseActive = false;
            IsSampleRunning = true;
            SetResultCarouselDefault(true);
            CalculateConcentrationGraphData();
            FindCV();
            IsConcAcceptButtonEnable = true;

            DispatcherHelper.ApplicationExecute(() =>
            {
                IsPaginationButtonEnable = false;
                IsPaginationButtonEnable = true;
                IsConcentrationUpdateActive = !IsConcentrationUpdateActive;
            });
        }

        private void CalculateAdjustValue(out CalibrationData totalCells, out CalibrationData originalConcentration,
            out CalibrationData adjustedConcentration)
        {
            totalCells = new CalibrationData();
            originalConcentration = new CalibrationData();
            adjustedConcentration = new CalibrationData();

            if (_workQueueResultRecord?.SampleRecords == null) return;

            foreach (SampleRecordDomain record in _workQueueResultRecord.SampleRecords)
            {
                var concentrationList = ConcentrationList.Where(x =>
                    x.StartPosition <= record.Position.Column
                    && x.EndPosition >= record.Position.Column);
                double assayValue = 0;
                var concentration = concentrationList.FirstOrDefault();
                if (concentration != null)
                {
                    var assayValueTemp = concentration.AssayValue;
                    switch (concentration.AssayValueType)
                    {
                        case AssayValueEnum.M2:
                            assayValue = assayValueTemp * 1000000;
                            break;
                        case AssayValueEnum.M4:
                            assayValue = assayValueTemp * 1000000;
                            break;
                        case AssayValueEnum.M10:
                            // If the provided value is >= 2, we assume the unit is 10^6/uL;
                            // otherwise, we assume 10^7/uL.
                            if (assayValueTemp >= 2.0)
                            {
                                assayValue = assayValueTemp * 1000000;
                            }
                            else
                            {
                                assayValue = assayValueTemp * 10000000;
                            }

                            break;
                    }
                }

                // Normalize the total count to handle invalid images.
                var adjustedCount = record.SelectedResultSummary.CumulativeResult.TotalCells *
                                    (100.0 / record.SelectedResultSummary.CumulativeResult.TotalCumulativeImage);
                totalCells.Data.Add(new KeyValuePair<double, double>(assayValue, adjustedCount));
                originalConcentration.Data.Add(new KeyValuePair<double, double>(assayValue,
                    record.SelectedResultSummary.CumulativeResult.ConcentrationML));
            }

            totalCells.CalculateSlope_averageOverAssays();
            originalConcentration.CalculateSlope_averageOverAssays();

            foreach (KeyValuePair<double, double> pair in totalCells.Data)
            {
                var adjConcValue = (pair.Value * totalCells.Slope) + totalCells.Intercept;
                adjustedConcentration.Data.Add(new KeyValuePair<double, double>(pair.Key, adjConcValue));
            }

            adjustedConcentration.CalculateSlope_averageOverAssays();
            IsConcentrationUpdateActive = !IsConcentrationUpdateActive;
        }

        private void FindCV()
        {
            var data = new CalibrationData();
            if (DataGridExpanderColumnConcHeaderList == null)
                return;
            var headerList = DataGridExpanderColumnConcHeaderList.GroupBy(x => x.AssayValue);
            foreach (var assayGroup in headerList)
            {
                var cellCount = new List<double>();
                foreach (var datagrid in assayGroup)
                {
                    cellCount.Add(datagrid.TotCount);
                }
                var cv = data.CalculateCV(cellCount, assayGroup.ElementAt(0).AvgTotCount);
                assayGroup.ElementAt(0).PercentCV = ScoutUtilities.Misc.UpdateTrailingPoint(cv, TrailingPoint.One);

            }
        }

        private void CalculateGraph(CalibrationData totalCells, CalibrationData originalConcentration,
            CalibrationData adjustedConcentration)
        {
            GraphCalibrationPrimaryTrendLabel = GetStraightLineEquation(totalCells.Slope);

            ResetGraphList();

            var index = 0;
            GraphViewList.ForEach(g =>
            {
                g.XAxisName = LanguageResourceHelper.Get("LID_GraphLabel_AssayConcentration");
                g.IsExpandableView = false;
                switch (index)
                {
                    case 0:
                        g.GraphDetailList = new ObservableCollection<KeyValuePair<dynamic, double>>();
                        foreach (KeyValuePair<double, double> item in totalCells.Data)
                        {
                            g.GraphDetailList.Add(
                                new KeyValuePair<dynamic, double>(item.Key / Math.Pow(10, 6), item.Value));
                        }

                        g.PrimaryTrendPoints = GetTrendPair(totalCells);

                        SetTrendLabel(g, totalCells, 3);
                        SetGraphData(LanguageResourceHelper.Get("LID_GraphLabel_Totalcells"),
                            LanguageResourceHelper.Get("LID_GraphLabel_Totalcells"), g);
                        break;
                    case 1:
                        SetPrimaryTrend(originalConcentration, g);
                        SetTrendLabel(g, originalConcentration, 3);
                        SetGraphData(
                            LanguageResourceHelper.Get("LID_GraphLabel_OriginalConcentration"),
                            LanguageResourceHelper.Get("LID_CheckBox_TotalCellConcentration"), g);
                        break;
                    case 2:
                        SetPrimaryTrend(adjustedConcentration, g);
                        SetTrendLabel(g, adjustedConcentration, 3);
                        SetGraphData(LanguageResourceHelper.Get("LID_Graph_AdjustConc"),
                            LanguageResourceHelper.Get("LID_CheckBox_TotalCellConcentration"), g);

                        break;
                    case 3:
                        g.GraphName = LanguageResourceHelper.Get("LID_Graph_Original");
                        g.YAxisName =
                            LanguageResourceHelper.Get("LID_CheckBox_TotalCellConcentration");
                        g.IsMultiAxisEnable = true;
                        g.PrimaryLegendName = LanguageResourceHelper.Get("LID_Graph_OrigConc");
                        g.PrimaryTrendLegendName =
                            LanguageResourceHelper.Get("LID_Graph_LinerOrigConc");
                        g.SecondaryLegendName =
                            LanguageResourceHelper.Get("LID_Graph_AdjustConc");
                        g.SecondaryTrendLegendName =
                            LanguageResourceHelper.Get("LID_Graph_LinerAdjustConc");

                        //Primary Axis
                        SetPrimaryTrend(originalConcentration, g);
                        SetTrendLabel(g, originalConcentration, 4);
                        //Secondary Axis
                        SetMultiTrend(adjustedConcentration, g);
                        SetTrendLabel(g, adjustedConcentration, 4);
                        break;
                }
                index++;
            });

            Graph = GraphViewList.LastOrDefault();
            Graph = GraphViewList.FirstOrDefault();
            NotifyAllPropertiesChanged();
        }

        private void SetTrendLabel(LineGraphDomain graph, CalibrationData value, int decimalPoint)
        {
            decimal decR2 = double.IsNaN(value.R2) ? 0 : decimal.Round((decimal)value.R2, decimalPoint);
            graph.PrimaryTrendLabel = GetStraightLineEquation(value.Slope) + "\n" +
                                      LanguageResourceHelper.Get("LID_GridLabel_R2") +
                                      "\u00b2" + " = " + decR2;
        }

        private ObservableCollection<KeyValuePair<dynamic, double>> GetTrendPair(CalibrationData data)
        {
            var trendPair = new ObservableCollection<KeyValuePair<dynamic, double>>();
            trendPair.Add(new KeyValuePair<dynamic, double>(data.Intercept / Math.Pow(10, 6), 0.0));
            double largestPair = trendPair.Max(item => item.Key);
            // TODO PC3527-3219 validate the adjustments here. The trend line should look reasonable.
            trendPair.Add(new KeyValuePair<dynamic, double>(
                largestPair / Math.Pow(10, 6),
                // *divide* the assay value by the slope to determine the expected cell count
                largestPair / data.Slope));
            return trendPair;
        }

        private void SetPrimaryTrend(CalibrationData data, LineGraphDomain graph)
        {
            graph.GraphDetailList = new ObservableCollection<KeyValuePair<dynamic, double>>();
            foreach (KeyValuePair<double, double> item in data.Data)
            {
                graph.GraphDetailList.Add(
                    new KeyValuePair<dynamic, double>(item.Key / Math.Pow(10, 6), GetConcentrationPoint(item.Value)));
            }

            graph.PrimaryTrendPoints = GetTrendPair(data);

        }

        private void SetMultiTrend(CalibrationData data, LineGraphDomain graph)
        {
            graph.MultiGraphDetailList = new ObservableCollection<KeyValuePair<dynamic, double>>();
            foreach (KeyValuePair<double, double> item in data.Data)
            {
                graph.MultiGraphDetailList.Add(
                    new KeyValuePair<dynamic, double>(item.Key / Math.Pow(10, 6), GetConcentrationPoint(item.Value)));
            }

            graph.SecondaryTrendPoints = GetTrendPair(data);
        }

        private double GetConcentrationPoint(double graphValue)
        {
            return Misc.UpdateDecimalPoint(Misc.ConvertToPowerSix(graphValue));
        }

        private void CalculateConcentrationGraphData()
        {
            CalculateAdjustValue(out var totalCells, out var originalConcentration, out var adjustedConcentration);

            if (adjustedConcentration.Data.Count > 0 && DataGridExpanderColumnConcHeaderList.Count == adjustedConcentration.Data.Count)
            {
                for (var index = 0; index < DataGridExpanderColumnConcHeaderList.Count; index++)
                {
                    DataGridExpanderColumnConcHeaderList[index].Adjusted = adjustedConcentration.Data[index].Value;
                }
            }

            DispatcherHelper.ApplicationExecute(() =>
            {
                CalculateGraph(totalCells, originalConcentration, adjustedConcentration);
                // Set Button enable/ disable based on Status updated
                //IsConcAcceptButtonEnable = !DataGridExpanderColumnConcHeaderList.Any(x => !x.IsStatusUpdated && !x.Validate);
                CalibrationSlope = totalCells.Slope;
                CalibrationR2 = totalCells.R2;
                CalibrationIntercept = totalCells.Intercept;
                IsSampleStatusCompleted = false;
                IsSampleStatusCompleted = true;
            });
        }

        private string GetStraightLineEquation(double slope)
        {
            double localslope = double.IsNaN(slope) ? 0 : slope;
            string equation = "y = " + decimal.Round((decimal)localslope, 4) + "x";
            return equation;
        }

        private void SetGraphData(string graphName, string yAxis, LineGraphDomain graph)
        {
            graph.GraphName = graphName;
            graph.YAxisName = yAxis;
        }

        private void UpdateDataGridExpanderColumnHeader(SampleEswDomain sample, BasicResultAnswers cumulativeResults)
        {

            if (DataGridExpanderColumnConcHeaderList == null)
                DataGridExpanderColumnConcHeaderList = new ObservableCollection<DataGridExpanderColumnHeader>();
            var preDataGridColumn = DataGridExpanderColumnConcHeaderList.FirstOrDefault(x => x.SampleId == sample.SampleName);

            if (preDataGridColumn == null)
            {
                UpdateStartDataGridExpanderColumnHeader(sample);
            }
            else
            {
                preDataGridColumn.Original = cumulativeResults.concentration_general;
                preDataGridColumn.TotCount = cumulativeResults.count_pop_general;
                SetAvgValues();
                IsConcentrationUpdateActive = !IsConcentrationUpdateActive;
            }
        }

        private void SetAvgValues()
        {
            if (DataGridExpanderColumnConcHeaderList == null)
                return;
            var itemsAverageValues =
                from assay in DataGridExpanderColumnConcHeaderList
                group assay by assay.AssayValue
                into assayGroup
                select new
                {
                    Assayvalue = assayGroup.Key,
                    AverageOriginal = assayGroup.Average(x => x.Original),
                    AverageTotCount = assayGroup.Average(x => x.TotCount),
                    AverageAssayValue = assayGroup.Average(x => x.AssayValue)
                };
            var a = DataGridExpanderColumnConcHeaderList.GroupBy(x => x.AssayValue);
            foreach (var obj in itemsAverageValues)
            {
                foreach (var b in a)
                {
                    foreach (var c in b)
                    {
                        if (!(Math.Abs(b.Key - obj.Assayvalue) <= 0))
                            continue;
                        c.AvgTotCount = ScoutUtilities.Misc.UpdateDecimalPoint(obj.AverageTotCount, null);
                        c.AvgOriginal = ScoutUtilities.Misc.UpdateDecimalPoint(obj.AverageOriginal);
                    }
                }
            }
        }

        private void UpdateStartDataGridExpanderColumnHeader(SampleEswDomain sample)
        {
            if (DataGridExpanderColumnConcHeaderList == null)
                DataGridExpanderColumnConcHeaderList = new ObservableCollection<DataGridExpanderColumnHeader>();

            var preDataGridColumn = DataGridExpanderColumnConcHeaderList.FirstOrDefault(x => x.SampleId == sample.SampleName);
            if (preDataGridColumn == null)
            {
                var position = (int)sample.SamplePosition.Column;
                preDataGridColumn = new DataGridExpanderColumnHeader();
                var concentrationType = ConcentrationList.FirstOrDefault(x =>
                    x.StartPosition <= position && x.EndPosition >= position);
                if (concentrationType != null)
                {
                    preDataGridColumn.DilutionFactor = (int)sample.Dilution;
                    preDataGridColumn.AssayValue = ConvertStringAssayValueToDouble(concentrationType);
                    preDataGridColumn.ExpirationDate = concentrationType.ExpiryDate;
                    preDataGridColumn.KnownConcentration = concentrationType.KnownConcentration;
                    preDataGridColumn.Lot = concentrationType.Lot;
                    preDataGridColumn.Original = double.NaN;
                    preDataGridColumn.Adjusted = double.NaN;
                    preDataGridColumn.AvgAdjusted = double.NaN;
                    preDataGridColumn.AvgOriginal = double.NaN;
                }
                preDataGridColumn.SampleId = sample.SampleName;
                DispatcherHelper.ApplicationExecute(() =>
                {
                    DataGridExpanderColumnConcHeaderList.Add(preDataGridColumn);
                });
            }
        }

        private void ResetDataGridExpanderColumnConcHeaderList()
        {
            if (DataGridExpanderColumnConcHeaderList == null)
                DataGridExpanderColumnConcHeaderList = new ObservableCollection<DataGridExpanderColumnHeader>();
            DataGridExpanderColumnConcHeaderList.Clear();
            foreach (var concentration in ConcentrationList)
            {
                DataGridExpanderColumnConcHeaderList.Add(new DataGridExpanderColumnHeader
                {
                    AssayValue = ConvertStringAssayValueToDouble(concentration),
                    Original = double.NaN,
                    Adjusted = double.NaN
                });
            }

            IsConcentrationUpdateActive = true;
            IsConcentrationUpdateActive = false;
        }

        private double ConvertStringAssayValueToDouble(ICalibrationConcentrationListDomain calibrationConcentration)
        {
            double assayValue = calibrationConcentration.AssayValue;
            if (calibrationConcentration.AssayValueType == AssayValueEnum.M10)
            {
                if (assayValue == 0)
                {
                    assayValue = 0;
                }
                else if (assayValue >= 2)
                {
                    assayValue = (assayValue * Math.Pow(10, 6));
                }
                else
                    assayValue = (assayValue * Math.Pow(10, 7));
            }
            else
                assayValue = (assayValue * Math.Pow(10, 6));

            return assayValue;
        }

        private void SetSampleProgressStatusColor(SampleProgressStatus aspirationBrush, SampleProgressStatus mixingDyeBrush,
            SampleProgressStatus imageAnalysisBrush, SampleProgressStatus cleaningBrush, string runSampleStatus)
        {
            AspirationBrush = aspirationBrush;
            MixingDyeBrush = mixingDyeBrush;
            ImageAnalysisBrush = imageAnalysisBrush;
            CleaningBrush = cleaningBrush;
            RunSampleStatus = runSampleStatus;
        }

        private void SetLatestSlopeData()
        {
            if (ConcentrationOverTimeList == null || !ConcentrationOverTimeList.Any()) return;

            var resultConcentration = ConcentrationOverTimeList.OrderByDescending(t => t.Date).First();
            if (resultConcentration != null)
            {
                LastConcentrationSlopeDate = resultConcentration.Date;
                LastConcentrationSlope = ScoutUtilities.Misc.ConvertToString(resultConcentration.Slope);
            }
        }

        private void ExportConcentrationSlopeDetails()
        {
            if (ExportModel.OpenCsvSaveFileDialog(string.Empty, out var fullFilePath))
            {
                MessageBus.Default.Publish(new Notification<bool>(true, MessageToken.AdornerVisible));

                ExportModel.ExportConcentrationSlopeDetails(ConcentrationList,
                    GetSlopeInterceptValues(), DataGridExpanderColumnConcHeaderList.ToList(), FileType.Csv,
                    Path.GetDirectoryName(fullFilePath), Path.GetFileName(fullFilePath));

                MessageBus.Default.Publish(new Notification<bool>(false, MessageToken.AdornerVisible));
            }
        }

        private List<KeyValuePair<string, double>> GetSlopeInterceptValues()
        {
            return new List<KeyValuePair<string, double>>
            {
                new KeyValuePair<string, double>("Slope", CalibrationSlope),
                new KeyValuePair<string, double>("Intercept", CalibrationIntercept),
                new KeyValuePair<string, double>("R-squared", CalibrationR2)
            };
        }

        private void AcceptNewConcentration()
        {
            uint calibrationImageCount = 100;
            var consumablesid = new List<CalibrationConcentrationOverTimeDomain>();
            foreach (var item in ConcentrationList)
            {
                var consumable = new CalibrationConcentrationOverTimeDomain();
                consumable.AssayValue = ConvertStringAssayValueToDouble(item);
                consumable.ConsumableLabel = item.KnownConcentration;
                consumable.ConsumableLotId = item.Lot;
                consumable.Date = item.ExpiryDate;
                consumablesid.Add(consumable);
            }

            var num_consumables = Convert.ToUInt16(consumablesid.Count());

            if (DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_MSGBOX_SetSlope")) != true)
            {
                return;
            }

            IsCalibrationCompleted = false;
            var result = CalibrationModel.SetConcentrationCalibration(calibration_type.cal_Concentration,
                CalibrationSlope, CalibrationIntercept, calibrationImageCount, _queueId, num_consumables,
                consumablesid);
            if (result.Equals(HawkeyeError.eSuccess))
            {
                OnConcentrationSlopeSetSuccess();
                PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_NewConcentrationSlopeSet"));
            }
            else
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_SetSlopeFailed"));
            }
        }

        private void SetHamburgerEnableDisable(bool status)
        {
            DispatcherHelper.ApplicationExecute(() => 
                MainWindowViewModel.Instance.EnableDisableHamburgerMenu(status));
        }

        private void OnConcentrationSlopeSetSuccess()
        {
            SetActiveView(ViewType.OverTimeView);
            SetLogEnable(calibration_type.cal_Concentration, 0, 0);
            SetLatestSlopeData();
            IsEnablePlayPause = true;
            SetHamburgerEnableDisable(true);
            ResetConcentration();
        }

        private void ResetConcentration()
        {
            ClearConcentrationSizeList();
            UpdateConcentrationValue();
            _workQueueResultRecord = null;
            SelectedSampleRecord = null;
            ResetGraphList();
            CalibrationSlope = 0;
            CalibrationR2 = 0;
            IsConcAcceptButtonEnable = false;
            ConcentrationComment = string.Empty;
            IsImageAvailable = true;
        }

        private void OnPlayingSampleChanged()
        {
            if (IsPlayingSample)
            {
                IsSampleRunning = false;
                SkipQueueEnable = StopQueueEnable = true;
                PlayPauseImagePath = "/Images/Pause.png";
                SkipImagePath = "/Images/Forward.png";
                StopImagePath = "/Images/Stop.png";
            }
            else
            {
                SkipQueueEnable = false;
                PlayPauseImagePath = "/Images/Play.png";
                SkipImagePath = "/Images/Forward_Ash.png";
            }
        }

        private void SetDefaultPlayPause()
        {
            IsEnablePlayPause = true;
            IsStopActive = IsPauseActive = false;
        }

        private void AbortAfterSampleRun()
        {
            IsPaused = false;
            IsCalibrationCompleted = false;
            IsConcentrationCompleted = true;
            IsImageAvailable = true;

            _isConcentrationCompleted = true;
            _isConcentrationRunning = false;

            ConcentrationComment = string.Empty;
            ClearRunProgressUI();
            ResetConcentration();
            SetResultCarouselDefault(true);
            SetDefaultPlayPause();

            var msg = LanguageResourceHelper.Get("LID_StatusBar_WorkQueueStopped");
            PostToMessageHub(msg, MessageType.Warning);
            Log.Warn(msg);
            SetHamburgerEnableDisable(true);
        }

        private void InvokeSystemErrorDialog(string message)
        {
            DialogEventBus.DialogBoxOk(this, message);
        }

        #endregion

        #region Public Methods

        public void SetActiveView(ViewType viewType)
        {
            switch (viewType)
            {
                case ViewType.ListView:
                    IsOverTimeActive = IsGraphViewActive = IsImageGraphActive = IsImageViewActive = false;
                    IsListViewActive = true;
                    break;
                case ViewType.ImageView:
                    IsListViewActive = IsOverTimeActive = IsGraphViewActive = false;
                    IsImageViewActive = IsImageGraphActive = true;
                    if (!_isConcentrationRunning && _isConcentrationCompleted) ResetDataGridExpanderColumnConcHeaderList();
                    break;
                case ViewType.GraphView:
                    IsListViewActive = IsOverTimeActive = IsImageViewActive = false;
                    IsGraphViewActive = IsImageGraphActive = true;
                    if (!_isConcentrationRunning && _isConcentrationCompleted) ResetDataGridExpanderColumnConcHeaderList();
                    break;
                case ViewType.OverTimeView:
                    IsListViewActive = IsImageViewActive = IsGraphViewActive = IsImageGraphActive = false;
                    IsOverTimeActive = true;
                    break;
            }

            UpdateExportEnable(!IsConcentrationTabActive, viewType);
        }

        // todo: simplify and move to a model class:
        public void UpdateConcentrationValue()
        {
            if (!IsEnablePlayPause) return;

            foreach (SampleDomain sample in CarouselSamples)
                sample.SampleStatusColor = SampleStatusColor.Empty;

            var count = 0;
            var startTubesNo = 1;
            var startPosition = 0;

            foreach (var concentration in ConcentrationList)
            {
                count++;
                concentration.Lot = string.Empty;
                concentration.ExpiryDate = DateTime.Now;
                var tubeStartNo = startTubesNo;

                switch (count)
                {
                    case 1:
                        concentration.Status = SampleStatusColor.ConcentrationType1;
                        concentration.AssayValueType = AssayValueEnum.M2;
                        concentration.IsCorrectAssayValue = true;
                        for (var i = startTubesNo; i < tubeStartNo + concentration.NumberOfTubes; i++)
                        {
                            foreach (var sample in CarouselSamples.Where(x => x.SamplePosition.Column == i))
                            {
                                sample.SampleStatusColor = SampleStatusColor.ConcentrationType1;
                                sample.SampleID = $"{ApplicationConstants.ConcSlope2M}.{sample.SetCarouselPositionToSampleId(sample.SamplePosition)}";
                            }

                            if (startTubesNo == tubeStartNo) concentration.StartPosition = i;
                            startTubesNo++;
                            startPosition++;
                            if (startTubesNo == tubeStartNo + concentration.NumberOfTubes) concentration.EndPosition = i;
                        }
                        break;

                    case 2:
                        concentration.Status = SampleStatusColor.ConcentrationType2;
                        concentration.AssayValueType = AssayValueEnum.M4;
                        concentration.IsCorrectAssayValue = true;
                        concentration.StartPosition = startPosition++;
                        for (var i = startTubesNo; i < tubeStartNo + concentration.NumberOfTubes; i++)
                        {
                            foreach (var sample in CarouselSamples.Where(x => x.SamplePosition.Column == i))
                            {
                                sample.SampleStatusColor = SampleStatusColor.ConcentrationType2;
                                sample.SampleID = $"{ApplicationConstants.ConcSlope4M}.{sample.SetCarouselPositionToSampleId(sample.SamplePosition)}";
                            }

                            if (startTubesNo == tubeStartNo) concentration.StartPosition = i;
                            startTubesNo++;
                            startPosition++;
                            if (startTubesNo == tubeStartNo + concentration.NumberOfTubes) concentration.EndPosition = i;
                        }
                        break;

                    case 3:
                        concentration.Status = SampleStatusColor.ConcentrationType3;
                        concentration.AssayValueType = AssayValueEnum.M10;
                        concentration.IsCorrectAssayValue = true;
                        concentration.StartPosition = startPosition++;
                        for (var i = startTubesNo; i < tubeStartNo + concentration.NumberOfTubes; i++)
                        {
                            foreach (SampleDomain sample in CarouselSamples.Where(r => r.SamplePosition.Column == i))
                            {
                                sample.SampleStatusColor = SampleStatusColor.ConcentrationType3;
                                sample.SampleID = $"{ApplicationConstants.ConcSlope10M}.{sample.SetCarouselPositionToSampleId(sample.SamplePosition)}";
                            }

                            if (startTubesNo == tubeStartNo) concentration.StartPosition = i;
                            startTubesNo++;
                            startPosition++;
                            if (startTubesNo == tubeStartNo + concentration.NumberOfTubes) concentration.EndPosition = i;
                        }
                        break;
                }
            }

            DispatcherHelper.ApplicationExecute(() =>
            {
                ChangesDialogResultCarouselPlate = true;
                ChangesDialogResultCarouselPlate = false;
            });
        }

        public void SetConcentrationDefaultValue(string tabType)
        {
            IsListViewActive = true;
            SetActiveView(ViewType.ListView);
        }

        public void SetLogEnable(calibration_type calibrationType, ulong startTime, ulong endTime)
        {
            try
            {
                CalibrationModel.RetrieveCalibrationActivityLogRange(calibrationType, startTime, endTime, out var calibrationErrorLog);
                ConcentrationOverTimeList = new ObservableCollection<CalibrationActivityLogDomain>(calibrationErrorLog);
                IsListAvailable = ConcentrationOverTimeList.Count == 0;
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_SET_LOG_ENABLE"));
            }
        }

        public void ChangeRunProgressUI(SampleStatus Status, SampleEswDomain item)
        {
            string runSampleStatus;
            switch (Status)
            {
                case SampleStatus.NotProcessed:
                    IsLoadingIndicatorActive = true;
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eNotProcessed));
                    SetSampleProgressStatusColor(SampleProgressStatus.IsReady, SampleProgressStatus.IsReady,
                        SampleProgressStatus.IsReady, SampleProgressStatus.IsReady, runSampleStatus);
                    break;
                case SampleStatus.InProcessAspirating:
                    IsLoadingIndicatorActive = true;
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eInProcess_Aspirating));
                    SetSampleProgressStatusColor(SampleProgressStatus.IsRunning, SampleProgressStatus.IsInActive,
                        SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive, runSampleStatus);
                    break;
                case SampleStatus.InProcessMixing:
                    IsLoadingIndicatorActive = true;
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eInProcess_Mixing));
                    SetSampleProgressStatusColor(SampleProgressStatus.IsActive, SampleProgressStatus.IsRunning,
                        SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive, runSampleStatus);
                    break;
                case SampleStatus.InProcessImageAcquisition:
                    IsLoadingIndicatorActive = true;
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eInProcess_ImageAcquisition));
                    SetSampleProgressStatusColor(SampleProgressStatus.IsActive, SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsRunning, SampleProgressStatus.IsInActive, runSampleStatus);
                    break;
                case SampleStatus.InProcessCleaning:
                    IsLoadingIndicatorActive = true;
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eInProcess_Cleaning));
                    SetSampleProgressStatusColor(SampleProgressStatus.IsActive, SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsActive, SampleProgressStatus.IsRunning, runSampleStatus);
                    break;
                case SampleStatus.AcquisitionComplete:
                    IsLoadingIndicatorActive = true;
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eAcquisition_Complete));
                    SetSampleProgressStatusColor(SampleProgressStatus.IsActive, SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsActive, SampleProgressStatus.IsRunning, runSampleStatus);
                    break;
                case SampleStatus.Completed:
                    IsLoadingIndicatorActive = false;
                    runSampleStatus = string.Format(LanguageResourceHelper.Get("LID_Status_Completed_POS"), item.SamplePosition.Column);
                    SetSampleProgressStatusColor(SampleProgressStatus.IsActive, SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsActive, SampleProgressStatus.IsActive, runSampleStatus);
                    break;
            }
        }

        public void ClearRunProgressUI()
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                IsLoadingIndicatorActive = false;
                SetSampleProgressStatusColor(SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive,
                    SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive, string.Empty);
            });
        }

        public void UpdateExportEnable(bool sizeType, ViewType IsImageType)
        {
            if (sizeType)
            {
                switch (IsImageType)
                {
                    case ViewType.ListView:
                        IsExportViewActive = true;
                        break;
                    case ViewType.ImageView:
                        var count = DataGridExpanderColumnSizeHeaderList.ToList().Count();
                        IsExportViewActive = count > 0;
                        break;
                    case ViewType.GraphView:
                        count = DataGridExpanderColumnSizeHeaderList.ToList().Count();
                        IsExportViewActive = count > 0;
                        break;
                    case ViewType.OverTimeView:
                        count = ConcentrationOverTimeList.ToList().Count();
                        IsExportViewActive = count > 0;
                        break;
                    default:
                        IsExportViewActive = false;
                        break;
                }
            }
            else
            {
                switch (IsImageType)
                {
                    case ViewType.ListView:
                        IsExportViewActive = true;
                        break;
                    case ViewType.ImageView:
                        var count = DataGridExpanderColumnConcHeaderList.ToList().Count();
                        IsExportViewActive = count > 0;
                        break;
                    case ViewType.GraphView:
                        count = DataGridExpanderColumnConcHeaderList.ToList().Count();
                        IsExportViewActive = count > 0;
                        break;
                    case ViewType.OverTimeView:
                        count = ConcentrationOverTimeList.ToList().Count();
                        IsExportViewActive = count > 0;
                        break;
                    default:
                        IsExportViewActive = false;
                        break;
                }
            }
        }

        #endregion
    }
}