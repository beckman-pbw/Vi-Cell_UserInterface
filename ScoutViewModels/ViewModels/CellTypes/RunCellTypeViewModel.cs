using ApiProxies.Generic;
using ApiProxies.Misc;
using Microsoft.Practices.ServiceLocation;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.Common;
using ScoutDomains.DataTransferObjects;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Home.QueueManagement;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomControl;
using ScoutUtilities.Enums;
using ScoutUtilities.EventDomain;
using ScoutUtilities.Events;
using ScoutUtilities.Structs;
using ScoutUtilities.UIConfiguration;
using ScoutViewModels.Common;
using ScoutViewModels.Common.Helper;
using ScoutViewModels.ViewModels.Common;
using ScoutViewModels.ViewModels.Home.QueueManagement;
using ScoutViewModels.ViewModels.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;

namespace ScoutViewModels.ViewModels.CellTypes
{
    //todo: Find out what this class is supposed to do and if it can be removed. This feature was hidden for Scout
    public class RunCellTypeViewModel : BaseRunResult
    {

        #region Constructor

        public RunCellTypeViewModel(QueueManagementModel queueManagement, QueueResultModel queueResultModel) : base(queueManagement, queueResultModel)
        {
            RecordHelper = new ResultRecordHelper();
            RunSampleHelper = new RunSampleHelper();
            RunOptionSettingsModel = new RunOptionSettingsModel();

            _workQueueStatusTimer = new Timer(UISettings.WorkQueueStatusTimer);
            _workQueueStatusTimer.Elapsed += OnTimedEvent;
            _workQueueStatusTimer.AutoReset = true;

            Initialize();
        }

        public RunCellTypeViewModel(RunSampleHelper runSampleHelper, QueueResultModel queueResultModel, QueueManagementModel queueManagement, 
            RunOptionSettingsModel runOptionSettingsModel, ResultRecordHelper recordHelper) : base(queueManagement, queueResultModel)
        {
            RecordHelper = recordHelper;
            RunSampleHelper = runSampleHelper;
            RunOptionSettingsModel = runOptionSettingsModel;

            _workQueueStatusTimer = new Timer(UISettings.WorkQueueStatusTimer);
            _workQueueStatusTimer.Elapsed += OnTimedEvent;
            _workQueueStatusTimer.AutoReset = true;

            Initialize();
        }

        private void Initialize()
        {
            QueueManagementModel = ServiceLocator.Current.GetInstance<QueueManagementModel>("QueueManagementView"); // I think this line is trying to get it's own Queue and not use the singleton
            SetDefault();
            EventAggregator.Default.Subscribe<eSensorStatus>(SetCarouselType);
            EventAggregator.Default.Subscribe<StageStatus>(OnCarouselStatusChange);
            ImageType = ImageType.Annotated;
            IsSampleRunning = !IsCarouselStatus || QueueManagementModel.GridAddSamples.Any(x => x.SampleStatus == SampleStatus.Selected);
        }

        #endregion

        #region Private Properties

        private List<SampleCarouselDomain> PlaySamplesList;
        private bool _isPauseRunningSamples;
        private QueueManagementModel _queueManagementModel;
        private List<SampleCarouselDomain> _samples;
        private readonly Timer _workQueueStatusTimer;
        private DateTime _startWorkQueueDateTime;
        private bool _isSampleRunning = true;
        private int _index = 1;
        private static readonly Func<DateTime, WorkQueueItemDto, string> GetImageSavePathTail = (dt, wqi) => ScoutUtilities.Misc.ConvertToFileNameFormat(dt) + "_" + wqi.Label;

        #endregion

        #region Public Properties

        public RunOptionSettingsModel RunOptionSettingsModel;
        public ImageType ImageType { get; set; }
        public Func<bool, Tuple<CellTypeDomain, CellTypeDomain>> GetSelectedCell { get; set; }
        public ResultRecordHelper RecordHelper { get; set; }
        public string Message { get; set; }
        public RelayCommand UpdateGridCarousel { get; set; }

        public DilutionDomain SelectedDilution
        {
            get { return GetProperty<DilutionDomain>(); }
            set { SetProperty(value); }
        }

        public eSamplePostWash SelectedWash
        {
            get { return GetProperty<eSamplePostWash>(); }
            set { SetProperty(value); }
        }

        public CellTypeBpQcDomain BpQcCellType { get; set; }

        public SampleCarouselDomain SelectedDefaultSample
        {
            get { return GetProperty<SampleCarouselDomain>(); }
            set { SetProperty(value); }
        }

        public bool IsAbortSampleActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsSelectionChanged
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public List<SampleCarouselDomain> Samples
        {
            get { return _samples ?? (_samples = new List<SampleCarouselDomain>()); }
            set
            {
                _samples = value;
                NotifyPropertyChanged(nameof(Samples));
            }
        }

        public bool IsPlayPauseStatus
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool isPauseRunningSamples
        {
            get { return _isPauseRunningSamples; }
            set { _isPauseRunningSamples = value; }
        }

        public bool IsCarouselStatus
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public RunSampleHelper RunSampleHelper
        {
            get { return GetProperty<RunSampleHelper>(); }
            set { SetProperty(value); }
        }

        public new QueueManagementModel QueueManagementModel
        {
            get { return _queueManagementModel; }
            set
            {
                _queueManagementModel = value;
                NotifyPropertyChanged(nameof(QueueManagementModel));
                SubscribeToWorkQueueEvents(value);
            }
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

        public SampleProgressStatus ImageAnalysisBrush
        {
            get { return GetProperty<SampleProgressStatus>(); }
            set { SetProperty(value); }
        }

        public SampleProgressStatus CleaningBrush
        {
            get { return GetProperty<SampleProgressStatus>(); }
            set { SetProperty(value); }
        }

        public string RunSampleStatus
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsPlayPauseStopActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsClearingCarousel
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsLoadingIndicatorActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string ClearPausePosition
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsCarouselRunning
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsCreateByRunPlayTabActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool EnableWorkQueueListening { get; set; }

        public bool IsStopPlayingWorkQueueItem
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public int SelectedIndex
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public bool IsClearAddCarousel
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsSampleRunning
        {
            get { return _isSampleRunning; }
            set
            {
                _isSampleRunning = value;
                NotifyPropertyChanged(nameof(IsSampleRunning));
            }
        }

        public CellTypeDomain SelectedTempCellType { get; set; }
        public CellTypeDomain SelectedCellFromList { get; set; }
        public CellTypeBpQcDomain TempCellType { get; set; }
        public CellTypeDomain SelectedCellType { get; set; }

        #endregion

        #region Commands

        #region Run Sample Command

        private RelayCommand _runSampleCommand;

        public RelayCommand RunSampleCommand => _runSampleCommand ??
            (_runSampleCommand = new RelayCommand(RunSamples));

        private void RunSamples(object parameter)
        {
            try
            {
                if (parameter == null)
                    return;

                switch (parameter.ToString())
                {
                    case "Start":
                        ApiHawkeyeMsgHelper.InsufficientReagentPackUsesLeft((ushort)QueueResultModel.GetPlaySampleListSize());
                        ApiHawkeyeMsgHelper.InsufficientDisposalTrayCapacity((ushort)QueueResultModel.GetPlaySampleListSize());
                        PlaySample();
                        break;
                    case "Eject":
                        EjectQueue();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONRUNSAMPLES"));
            }
        }

        #endregion

        #region Expand Image & Graph Commands

        private RelayCommand _imageExpandCommand;

        public RelayCommand ImageExpandCommand => _imageExpandCommand ??
                                              (_imageExpandCommand = new RelayCommand(OnExpandImage, null));

        private void OnExpandImage()
        {
            try
            {
                OnExpandView(false);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONEXPANDIMAGE"));
            }
        }

        private RelayCommand _graphExpandCommand;

        public RelayCommand GraphExpandCommand => _graphExpandCommand ??
                                              (_graphExpandCommand = new RelayCommand(OnExpandGraph, null));

        private void OnExpandGraph()
        {
            try
            {
                OnExpandView(true);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONEXPANDGRAPH"));
            }
        }

        #endregion

        #region Create By Run Open Command

        private RelayCommand _createByRunOpenCommand;

        public RelayCommand CreateByRunOpenCommand => _createByRunOpenCommand ??
                                                  (_createByRunOpenCommand = new RelayCommand(OnCreateByRunOpen));

        private void OnCreateByRunOpen(object Obj)
        {
            try
            {
                string result = (string)Obj;
                switch (result)
                {
                    case "CreateByRunPlay":
                        IsCreateByRunPlayTabActive = true;
                        IsImageActive = false;
                        IsGraphActive = false;
                        break;
                    case "Graph":
                        if (QueueResultModel.SelectedSampleRecord.IsSampleCompleted)
                        {
                            CreateGraph();
                            IsCreateByRunPlayTabActive = false;
                            IsImageActive = false;
                            IsGraphActive = true;
                        }
                        break;
                    case "Image":
                        IsCreateByRunPlayTabActive = false;
                        IsImageActive = true;
                        IsGraphActive = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONCREATEBYRUNOPEN"));
            }
        }

        #endregion

        #endregion

        #region Private Methods

        private void OnTimedEvent(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            OnUpdateWorkQueueStatus();
        }

        private void OnUpdateWorkQueueStatus()
        {
            eWorkQueueStatus workQueueStatus = eWorkQueueStatus.eWQIdle;
            var workQueueItems = new List<WorkQueueItem>();

            var hawkeyeError = QueueManagementModel.GetWorkQueueStatus(ref workQueueStatus, ref workQueueItems);
            if (hawkeyeError.Equals(HawkeyeError.eSuccess))
            {
                Application.Current.Dispatcher.Invoke(() => { QueueResultModel.SetPausingAbortingStatus(workQueueStatus); });
                switch (workQueueStatus)
                {
                    case eWorkQueueStatus.eWQIdle:
                        ResetTimerAfterSampleStop();
                        ClearRunProgressUI();
                        break;
                    case eWorkQueueStatus.eWQStopped:
                        AbortAfterSampleRun();
                        break;
                    case eWorkQueueStatus.eWQFaulted:
                        AbortAfterSampleRun();
                        InvokeSystemErrorDialog(LanguageResourceHelper.Get("LID_MSGBOX_ABORTEDSAMPLE"));
                        break;
                    case eWorkQueueStatus.eWQSearchingTube:
                        IsLoadingIndicatorActive = true;
                        SetColor(SampleProgressStatus.IsReady, SampleProgressStatus.IsReady,
                            SampleProgressStatus.IsReady, SampleProgressStatus.IsReady,
                            LanguageResourceHelper.Get("LID_ProgressIndication_FindingTubes"));
                        break;
                }
            }
            else
            {
                if (!QueueResultModel.IsQueueRunning)
                    return;

                _workQueueStatusTimer.Stop();
                QueueResultModel.SetPausingAbortingStatus(eWorkQueueStatus.eWQIdle);
                ScoutUtilities.Misc.SampleState = RunSampleState.Idle;
            }
        }

        private void ResetTimerAfterSampleStop()
        {
            if (_workQueueStatusTimer.Enabled)
                _workQueueStatusTimer.Stop();
        }

        private void SetHamburgerEnableDisable(bool status)
        {
            ScoutUIMasterViewModel.Instance.IsHamburgerEnable = status;
        }

        private void InvokeSystemErrorDialog(string message)
        {
            DialogEventBus.DialogBoxOk(this, message);
        }

        private void SetDefault()
        {
            try
            {
                IsPlayPauseStopActive = true;
                IsCreateByRunPlayTabActive = true;
                QueueResultModel.ShowParameterList = new List<KeyValuePair<string, string>>(RecordHelper.GetShowParameterList(new GenericDataDomain(), LoggedInUser.CurrentUserId));
                UpdateGridCarousel = new RelayCommand(OnUpdateGridCarouselStatus);
                Samples = QueueManagementModel.Samples.ToList();
                SetDefaultDilutionWash();
                SetCarouselType(InstrumentStatusModel.Instance.SystemStatusDomain.CarouselDetect);
                QueueManagementModel.BpQcGroupList =
                    new ObservableCollection<CellTypeBpQcDomain>(RunSampleHelper.GetBpQcGroupList());
                if (InstrumentStatusModel.Instance.SystemStatusDomain.CarouselDetect == eSensorStatus.ssStateActive)
                {
                    QueueResultModel.PlayItemPosition = InstrumentStatusModel.Instance.SystemStatusDomain.SamplePosition.col;
                }
                else if (InstrumentStatusModel.Instance.SystemStatusDomain.CarouselDetect == eSensorStatus.ssStateInactive)
                {
                    QueueResultModel.PlateSamples.ForEach(x => x.IsRunning = false);
                    var sampleItem = QueueResultModel.PlateSamples.FirstOrDefault(sample =>
                        sample.SamplePosition.row == InstrumentStatusModel.Instance.SystemStatusDomain.SamplePosition.row &&
                        sample.SamplePosition.col == InstrumentStatusModel.Instance.SystemStatusDomain.SamplePosition.col);
                    if (sampleItem != null)
                        sampleItem.IsRunning = true;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONSETDEFAULT"));
            }
        }

        private void OnCarouselStatusChange(StageStatus obj)
        {
            if (obj.SamplePosition.isValid())
            {
                switch (obj.IsCarouselStatus)
                {
                    case eSensorStatus.ssStateActive:
                        QueueResultModel.PlayItemPosition = obj.SamplePosition.col;
                        break;
                    case eSensorStatus.ssStateInactive:
                        QueueResultModel.PlateSamples.ForEach(x => x.IsRunning = false);
                        var sampleItem = QueueResultModel.PlateSamples.FirstOrDefault(sample =>
                            sample.SamplePosition.row == obj.SamplePosition.row &&
                            sample.SamplePosition.col == obj.SamplePosition.col);
                        if (sampleItem != null)
                            sampleItem.IsRunning = true;
                        break;
                }
            }
            else
            {
                //Updated with -1 to remove highlighted position in carousel for invalid position from API.
                QueueResultModel.PlayItemPosition = -1;
                QueueResultModel.PlateSamples.ForEach(x => x.IsRunning = false);
            }
        }

        private void SetCarouselType(eSensorStatus sensor_carousel_detect)
        {
            //IsCarouselStatus True for 96well plate samples
            IsCarouselStatus = (sensor_carousel_detect == eSensorStatus.ssStateInactive);
        }

        private void GetPlaySampleList()
        {
            if (IsCarouselStatus) //Fetching 96well plate samples
            {
                PlaySamplesList = QueueResultModel.IsPlateOrderedByRow
                    ? QueueManagementHelper.Instance.GetGridRowSampleList(QueueManagementModel.GridAddSamples.ToList())
                    : QueueManagementHelper.Instance.GetGridColumnSampleList(QueueManagementModel.GridAddSamples.ToList());
            }
            else //Fetching carousel samples
                PlaySamplesList = GetCarouselSampleList(Samples);
        }

        private void StartWorkQueue()
        {
            try
            {
                if (true)
                {
                    OnEnableWorkQueueListening();
                    _index = 1;
                    QueueResultModel.WorkQueueResultRecord = new WorkQueueRecordDomain();
                    QueueResultModel.SelectedSampleRecord = new SampleRecordDomain();
                    BarGraphDomainList = new List<BarGraphDomain>();
                    BarGraphDomain = new BarGraphDomain();
                    IsChartVisible = false;
                    QueueResultModel.ShowParameterList =
                        new List<KeyValuePair<string, string>>(
                            RecordHelper.GetShowParameterList(new GenericDataDomain(), LoggedInUser.CurrentUserId));
                    var startWqResult = QueueManagementModel.StartWorkQueue(GenerateImageFilePath);
                    Log.Info("Output of StartWorkQueue. HawkeyeStatus: " + startWqResult);
                    if (startWqResult.Equals(HawkeyeError.eSuccess))
                    {
                        ScoutUtilities.Misc.SampleState = RunSampleState.Running;
                        PublishEventAggregator(
                            LanguageResourceHelper.Get("LID_StatusBar_WorkQueueStart"));
                        IsSampleRunning = false;

                    }
                    else
                    {
                        RunSampleHelper.IsPauseActive = false;
                        IsSampleRunning = true;
                        ApiHawkeyeMsgHelper.ErrorCommon(startWqResult);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_UNABLETOSTARTWORKQUEUE"));

            }
        }

        private string GenerateImageFilePath(
            Tuple<WorkQueueItemDto, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers> callbackResults)
        {
            var dateString = GetImageSavePathTail(_startWorkQueueDateTime, callbackResults.Item1);
            var basePath = AppDomain.CurrentDomain.BaseDirectory + "Target" + "\\" + dateString;

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);
            return basePath;
        }

        private void HandleWorkQueueItemStatusUpdated(object sender, ApiEventArgs<WorkQueueItemDto, uuidDLL> e)
        {
            if (!EnableWorkQueueListening)
                return;

            try
            {
                Log.Debug("---------------------------------------------------------");
                Log.Debug("- Output from onWQIStatus Callback: (*workqueue_status_callback)");

                IsCarouselRunning = true;
                QueueResultModel.LogWorkQueueItem(e.Arg1);
                RunSampleHelper.AddSampleRecord(e.Arg1);
                QueueResultModel.WorkQueueResultRecord = RunSampleHelper.WorkQueueResultRecord;
                QueueResultModel.SelectedSampleRecord = RunSampleHelper.SelectedSampleRecord;
                GridCarouselSampleRunningStatus(e.Arg1, "onWQIStatus");
                ClearAndSetWorkQueueResultRecord();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_UNABLETOUPDATESTATUS"));
            }
        }

        private void HandleWorkQueueItemCompleted(object sender, ApiEventArgs<WorkQueueItemDto, uuidDLL> e)
        {

            if (!EnableWorkQueueListening)
                return;

            if (e.Arg1.Status == eSampleStatus.eSkip_Manual || e.Arg1.Status == eSampleStatus.eSkip_Error)
            {
                LogDetailsOnSkippedSample();
                return;
            }
            try
            {
                Log.Debug("HandleWorkQueueItemCompleted::");
                RunSampleHelper.WriteLogForImage();
                QueueResultModel.LogWorkQueueItem(e.Arg1);
                RunSampleHelper.AddSampleRecord(e.Arg1);
                QueueResultModel.WorkQueueResultRecord = RunSampleHelper.WorkQueueResultRecord;
                QueueResultModel.SelectedSampleRecord = RunSampleHelper.SelectedSampleRecord;
                QueueResultModel.SelectedSampleRecord.IsSampleCompleted = true;
                if (IsGraphActive)
                    CreateGraph();
                if (QueueResultModel.SelectedSampleRecord != null)
                    SortImagesAndSelect(QueueResultModel.SelectedSampleRecord);
                if (PlaySamplesList.Count > 0)
                    GridCarouselSampleRunningStatus(e.Arg1, "onWQIComplete");

                if (IsAbortSampleActive)
                {
                    Log.Info("StopQueueExecution call Time-Stamp:" + ScoutUtilities.Misc.ConvertToString(DateTime.Now));
                    AbortAfterSampleRun();
                    Log.Info("StopQueueExecution after call Time-Stamp:" + ScoutUtilities.Misc.ConvertToString(DateTime.Now));
                    IsPlayPauseStopActive = true;
                }

                // Update the reagent status.
                ScoutUIMasterViewModel.Instance.SetReagentStatus(ReagentModel.GetReagentContainerStatusAll().First());

            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_UNABLETOUPDATESTATUS"));
            }
        }

        private void HandleWorkQueueCompleted(object sender, ApiEventArgs<uuidDLL> e)
        {
            if (!EnableWorkQueueListening)
                return;
            try
            {
                Log.Debug("---------------------------------------------------------");
                Log.Debug("- Output from onQComplete Callback: (*onWQComplete:Workqueueitem_completion_callback)");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsClearingCarousel = true;
                    IsClearingCarousel = false;
                });
                ClearRunProgressUI();
                IsCarouselRunning = RunSampleHelper.IsPauseActive = isPauseRunningSamples = false;
                SetHamburgerEnableDisable(true);
                IsSampleRunning = true;
                ScoutUtilities.Misc.SampleState = RunSampleState.Idle;
                EventAggregator.Default.Publish(ScoutUtilities.Misc.SampleState);
                if (IsGraphActive)
                    CreateGraph();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_UNABLETOUPDATESTATUS"));
            }
        }

        private void HandleWorkQueueImageResultOccurred(object sender,
            ApiEventArgs<WorkQueueItemDto, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers> e)
        {
            if (!EnableWorkQueueListening)
                return;
            try
            {
                foreach (var bpITem in QueueManagementModel.BpQcGroupList)
                {
                    BpQcCellType =
                        bpITem.CellTypeBpQcChildItems.FirstOrDefault(x => x.CellTypeIndex == e.Arg1.CelltypeIndex);
                    break;
                }

                if (string.IsNullOrEmpty(e.Arg4.BrightfieldImage.SavedFileName))
                    throw new ArgumentNullException("e.Arg4.BrightfieldImage.SavedFileName empty or null");
                RunSampleHelper.AddResultSample(e.Arg1, e.Arg3, e.Arg5, e.Arg4, e.Arg2);
                QueueResultModel.WorkQueueResultRecord = RunSampleHelper.WorkQueueResultRecord;
                QueueResultModel.SelectedSampleRecord = RunSampleHelper.SelectedSampleRecord;
                if (BpQcCellType != null)
                    QueueResultModel.SelectedSampleRecord.BpQcName = BpQcCellType.Name;

                ClearAndSetWorkQueueResultRecord();
                // Setting listView for result
                if (_index == 1)
                {
                    QueueResultModel.SelectedSampleRecord.SelectedResultSummary.CellTypeDomain = SelectedTempCellType;
                    SetShowParameterList();
                    _index++;
                }
                QueueResultModel.SelectedSampleRecord.UpdateSampleBubbleStatus();
                if (QueueResultModel.SelectedSampleRecord.IsSampleCompleted)
                {
                    SortImagesAndSelect(QueueResultModel.SelectedSampleRecord);
                }
            }
            catch (ArgumentNullException ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_CONCENTRATE_IMAGE_RESULT_QUEUE"));
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_UNABLETOHANDLEIMAGE"));
            }
        }

        private void GridCarouselSampleRunningStatus(WorkQueueItemDto wqi, string workQueueStatus)
        {
            var sampleData = new SampleCarouselDomain(); //Use implicit conversion
            if (IsCarouselStatus)
                sampleData = QueueResultModel.PlateSamples.Where(sample =>
                        sample.ColumnName == wqi.Location.row.ToString() && sample.Column == wqi.Location.col)
                    .FirstOrDefault();
            if (sampleData != null)
                switch (workQueueStatus)
                {
                    case "onWQIStatus":
                        if (IsCarouselStatus)
                        {
                            //################## For ProgressBar Indication ######################
                            PreChangeUI(wqi.Status, sampleData);
                            //#####################################################################
                            var nextSampleItem = PlaySamplesList.FirstOrDefault();
                            if (nextSampleItem != null)
                            {
                                nextSampleItem.ExecutionStatus = ExecutionStatus.Running;
                            }
                        }
                        else
                        {
                            var nextSampleItem = PlaySamplesList.FirstOrDefault();
                            if (nextSampleItem == null)
                                return;
                            //################## For ProgressBar Indication ######################
                            PreChangeUI(wqi.Status, nextSampleItem);
                            //#####################################################################
                            nextSampleItem.ExecutionStatus = ExecutionStatus.Running;
                        }
                        break;
                    case "onWQIComplete":
                        PlaySamplesList.RemoveAt(0);

                        if (PlaySamplesList.Count > 0)
                        {
                            var nextSampleItem = PlaySamplesList.FirstOrDefault();
                            //################## For ProgressBar Indication ######################
                            ClearRunProgressUI();
                            //#####################################################################
                            if (nextSampleItem != null)
                            {
                                nextSampleItem.ExecutionStatus = ExecutionStatus.Running;
                            }
                        }

                        if (IsCarouselStatus)
                        {
                            if (sampleData != null)
                            {
                                sampleData.ExecutionStatus = ExecutionStatus.Default;
                                ClearSample(sampleData);
                                sampleData.SampleStatus = SampleStatus.Completed;
                            }
                        }
                        else
                        {
                            ClearPausePosition = ScoutUtilities.Misc.ConvertToString(wqi.Location.col);
                            QueueManagementModel.Samples.Where(sample =>
                                sample.SamplePosition.col == wqi.Location.col
                                && sample.PlayStatusImage !=
                                GetEnumDescription.GetDescription(SampleStatusImage.SkipImage)
                                && sample.PlayStatusImage !=
                                GetEnumDescription.GetDescription(SampleStatusImage.ErrorImage)).Select(sample =>
                                {
                                    sample.ExecutionStatus = ExecutionStatus.Default;
                                    sample.PlayStatusImage = string.Empty;
                                    sample.SampleID = "(Empty)";
                                    sample.SelectedCellTypeBpQc = new CellTypeBpQcDomain();
                                    sample.SelectedDilution = string.Empty;
                                    sample.SelectedWash = 0;
                                    sample.Comment = string.Empty;
                                    sample.SampleStatus = SampleStatus.Empty;
                                    return sample;
                                }).ToList();


                            if (QueueManagementModel.Samples.All(
                                sample => sample.SampleStatus == SampleStatus.Empty))
                            {
                                Application.Current.Dispatcher.Invoke(() => { IsCarouselRunning = false; }
                                );
                            }
                        }

                        break;

                    default:
                        break;
                }
        }

        private void PreChangeUI(eSampleStatus status, SampleCarouselDomain item)
        {
            Application.Current.Dispatcher.Invoke(() => { ChangeRunProgressUI(status, item); });
        }

        private void SetColor(SampleProgressStatus aspirationBrush, SampleProgressStatus mixingDyeBrush,
            SampleProgressStatus imageAnalysisBrush, SampleProgressStatus cleaningBrush, string runSampleStatus)
        {
            AspirationBrush = aspirationBrush;
            MixingDyeBrush = mixingDyeBrush;
            ImageAnalysisBrush = imageAnalysisBrush;
            CleaningBrush = cleaningBrush;
            RunSampleStatus = runSampleStatus;
        }

        private void LogDetailsOnSkippedSample()
        {
            var runningSample = PlaySamplesList.FirstOrDefault();
            if (runningSample == null)
                return;
            var message = string.Format(LanguageResourceHelper.Get("LID_StatusBar_With"), runningSample.SampleID,
                runningSample.SelectedCellTypeBpQc.Name, runningSample.SampleRowPosition);
            Log.Warn(message);
            EventAggregator.Default.Publish(new SystemMessageDomain { IsMessageActive = true, Message = message, MessageType = MessageType.Warning });
        }

        private void ClearSample(SampleCarouselDomain sample)
        {
            sample.PlayStatusImage = string.Empty;
            sample.SampleID = "(Empty)";
            sample.SelectedDilution = string.Empty;
            sample.SelectedWash = 0;
            sample.Comment = string.Empty;
            sample.SampleStatus = SampleStatus.Empty;
            sample.SelectedCellTypeBpQc = new CellTypeBpQcDomain();
            sample.ExecutionStatus = ExecutionStatus.Default;
        }


        private void AbortAfterSampleRun()
        {
            var stopQueueResult = QueueManagementModel.StopQueueExecution();
            if (stopQueueResult.Equals(HawkeyeError.eSuccess))
            {
                ScoutUtilities.Misc.SampleState = RunSampleState.Aborted;
                CleaningRunProgress();
                EventAggregator.Default.Publish(ScoutUtilities.Misc.SampleState);
                IsCarouselRunning = false;
                IsAbortSampleActive = false;
                RunSampleHelper.IsPauseActive = false;
                PublishEventAggregator(LanguageResourceHelper.Get("LID_StatusBar_WorkQueueStopped"));
                ScoutUtilities.Misc.SampleState = RunSampleState.Idle;
            }
            else
                ApiHawkeyeMsgHelper.ErrorCommon(stopQueueResult);
        }

        private void CleaningRunProgress()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SetColor(SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive,
                    SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive, string.Empty);
            });
        }

        private void ClearAndSetWorkQueueResultRecord()
        {
            QueueResultModel.WorkQueueResultRecord.WorkQueueName = "";
        }

        #region private Graph Method

        private void CreateGraph()
        {
            var graphHelper = new GraphHelper();
            var graphList = graphHelper.CreateGraph(QueueResultModel.SelectedSampleRecord, BarGraphDomainList,
                new List<KeyValuePair<int, List<histogrambin_t>>>(), false);
            BarGraphDomainList = graphList;
            if (BarGraphDomainList != null && BarGraphDomainList.Any())
                BarGraphDomain = BarGraphDomainList.FirstOrDefault();
        }

        #endregion

        private void SubscribeToWorkQueueEvents(QueueManagementModel newModel)
        {
            //Unsubscribe from old model events
            if (_queueManagementModel != null)
            {
                _queueManagementModel.WorkQueueItemStatusUpdated -= HandleWorkQueueItemStatusUpdated;
                _queueManagementModel.WorkQueueItemCompleted -= HandleWorkQueueItemCompleted;
                _queueManagementModel.WorkQueueCompleted -= HandleWorkQueueCompleted;
                _queueManagementModel.WorkQueueImageResultOccurred -= HandleWorkQueueImageResultOccurred;
            }

            //Subscribe to new model events
            if (newModel != null)
            {
                newModel.WorkQueueItemStatusUpdated += HandleWorkQueueItemStatusUpdated;
                newModel.WorkQueueItemCompleted += HandleWorkQueueItemCompleted;
                newModel.WorkQueueCompleted += HandleWorkQueueCompleted;
                newModel.WorkQueueImageResultOccurred += HandleWorkQueueImageResultOccurred;
            }
        }

        private CellTypeBpQcDomain GetCellTypeBpQcDomainFromCellTypeDomain(CellTypeDomain cellTypeDomain)
        {
            var cellTypeBpQcDomain = new CellTypeBpQcDomain();
            if (cellTypeDomain != null)
            {
                cellTypeBpQcDomain.CellTypeIndex = (uint)cellTypeDomain.CellTypeIndex;
                cellTypeBpQcDomain.AppTypeIndex = cellTypeDomain.ApplicationTypeDomain.AnalysisIndex;
                cellTypeBpQcDomain.CellTypeBpQcChildItems = null;
                cellTypeBpQcDomain.SelectedCellTypeBpQcType = CellTypeBpQcTypes.CellType;
                cellTypeBpQcDomain.Name = cellTypeDomain.CellTypeName;
            }

            return cellTypeBpQcDomain;
        }

        private void SetShowParameterList()
        {
            QueueResultModel.SelectedSampleRecord.SelectedResultSummary.UserId = LoggedInUser.CurrentUserId;
            QueueResultModel.SelectedSampleRecord.SelectedResultSummary.RetrieveDate = DateTime.Now;
            QueueResultModel.SelectedSampleRecord.SelectedResultSummary.CellTypeDomain.CellTypeName =
                ApplicationConstants.TempCellTypeCharacter + QueueResultModel.SelectedSampleRecord.SelectedResultSummary.CellTypeDomain.CellTypeName;
            QueueResultModel.ShowParameterList = null;
            QueueResultModel.ShowParameterList =
                RecordHelper.SetListParameter(QueueResultModel.SelectedSampleRecord, "Celltype", LoggedInUser.CurrentUserId);
        }

        private void PlaySample()
        {
            ScoutUtilities.Misc.SampleState = RunSampleState.Idle;
            PlayRunSample();
            OnUpdateWorkQueueStatus();
            _workQueueStatusTimer.Start();
        }

        private void SetTempCell()
        {
            OnSelectedCell(SelectedCellFromList);
            TempCellType = GetCellTypeBpQcDomainFromCellTypeDomain(SelectedTempCellType);
        }

        private void OnSelectedCell(CellTypeDomain selectedCT)
        {
            selectedCT.CellTypeName = selectedCT.TempCellName;

            CellTypeModel.svc_SetTemporaryCellType(selectedCT);
            CellTypeModel.svc_SetTemporaryAnalysisDefinition(selectedCT.ApplicationTypeDomain);

            GetTempCellType();
        }

        private void GetTempCellType()
        {
            var tempCellType = CellTypeModel.svc_GetTemporaryCellType();
            SelectedTempCellType = tempCellType.FirstOrDefault();
        }

        private void SetDefaultPlaySample()
        {
            if (PlaySamplesList.Count > 0)
                PlaySamplesList.Select(x =>
                {
                    x.SampleID = "Default." + x.SampleRowPosition;
                    x.SelectedWash = SelectedWash;
                    x.SelectedDilution = SelectedDilution.DilutionValue;
                    x.SelectedCellTypeBpQc = TempCellType;
                    x.AnalysisIndex = (UInt16)SelectedTempCellType.ApplicationTypeDomain.AnalysisIndex;
                    x.Comment = "Test";
                    return x;
                }).ToList();
        }

        private void SetSelectedDefaultSample()
        {
            if (SelectedDefaultSample == null)
                SelectedDefaultSample = new SampleCarouselDomain();

            SelectedDefaultSample.SampleID = "ProRun";
            SelectedDefaultSample.SelectedWash = SelectedWash;
            SelectedDefaultSample.SelectedDilution = SelectedDilution.DilutionValue;
            SelectedDefaultSample.SelectedCellTypeBpQc = TempCellType;
            SelectedDefaultSample.AnalysisIndex = (UInt16)SelectedTempCellType.ApplicationTypeDomain.AnalysisIndex;
            SelectedDefaultSample.Comment = "Test";
        }

        private void SetDefaultDilutionWash()
        {
            RunOptionSettingsModel.GetRunOptionSettingsDomain(LoggedInUser.CurrentUserId);
            SelectedWash = RunOptionSettingsModel.RunOptionsSettings.DefaultWash;
            SelectedDilution = QueueManagementModel.DilutionList.FirstOrDefault(x =>
                x.DilutionValue == RunOptionSettingsModel.RunOptionsSettings.DefaultDilution);
        }

        private void OnUpdateGridCarouselStatus(object parameter)
        {
            try
            {
                var sendData = parameter as SampleCarouselDomain;
                if (sendData == null)
                    return;
                if (sendData.SampleStatus == SampleStatus.Selected)
                {
                    sendData.SampleStatus = SampleStatus.Empty;
                }
                else
                {
                    if (QueueManagementModel.GridAddSamples.Any(sample => sample.SampleStatus == SampleStatus.Selected))
                    {
                        DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_CreateByOpen_SelectOneSample"));
                        return;
                    }

                    sendData.SampleStatus = SampleStatus.Selected;
                }
                IsSampleRunning = QueueManagementModel.GridAddSamples.Any(x => x.SampleStatus == SampleStatus.Selected);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONUPDATECAROUSELSTATUS"));
            }
        }

        private void SetValueForCell()
        {
            SelectedCellFromList.TempCellName = SelectedCellType.TempCellName;
            SelectedCellFromList.CellTypeName = SelectedCellType.CellTypeName;
            SelectedCellFromList.IsUserDefineCellType = SelectedCellType.IsUserDefineCellType;
        }

        private void PlayRunSample()
        {
            var updatedCell = GetSelectedCell(true);
            SelectedCellType = updatedCell.Item2;
            SelectedCellFromList = updatedCell.Item1;
            if (SelectedCellType == null)
                return;
            if (SelectedCellFromList == null)
                return;
            SetValueForCell();
            if (!SaveCellTypeValidation(SelectedCellFromList))
                return;
            SetTempCell();
            GetPlaySampleList();
            foreach (var sample in PlaySamplesList)
            {
                sample.SelectedDilution = SelectedDilution.DilutionValue;
                sample.SelectedWash = SelectedWash;
                sample.SelectedCellTypeBpQc = TempCellType;
                sample.AnalysisIndex = (UInt16)SelectedTempCellType.ApplicationTypeDomain.AnalysisIndex;
            }

            if (!IsCarouselStatus && PlaySamplesList.Count < 1)
            {
                InvokeUnDefinedPositionDialogOnCarousel();
                return;
            }
            ProceedToRunSample();
        }

        private void ProceedToRunSample()
        {
            ClearResultData();
            SetDefaultPlaySample();
            SetSelectedDefaultSample();
            PlaySamplesList.ForEach(s =>
            {
                s.Comment = string.IsNullOrEmpty(s.Comment) ? null : s.Comment;
                s.BpQcName = string.IsNullOrEmpty(s.BpQcName) ? null : s.BpQcName;
            });

            RunSampleHelper.PlaySample(PlaySamplesList, SelectedDefaultSample, StartWorkQueue,
                QueueManagementHelper.Instance.SubstrateType(IsCarouselStatus),
                QueueManagementHelper.Instance.Precession(QueueResultModel.IsPlateOrderedByRow, IsCarouselStatus), IsCarouselStatus);
            SetHamburgerEnableDisable(false);
        }

        private void InvokeUnDefinedPositionDialogOnCarousel()
        {
            var asyncMsgBox = new AsynchMessageBox();
            Message = LanguageResourceHelper.Get("LID_MSGBOX_DoesNotContainDefinedSample");
            asyncMsgBox.DataContext = this;
            asyncMsgBox.btnOK.Click += (s, e) =>
            {
                asyncMsgBox.Close();
                ProceedToRunSample();
            };
            asyncMsgBox.btnSecond.Click += (s, e) => { asyncMsgBox.Close(); };
            asyncMsgBox.ShowDialog();
        }

        private void EjectQueue()
        {
            if (IsCarouselRunning)
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_QueueManagement_DiscardRunSample"));
            }
            else
            {
                var ejectStatus = QueueManagementModel.EjectSampleStage();
                if (!ejectStatus.Equals(HawkeyeError.eSuccess))
                    ApiHawkeyeMsgHelper.ErrorCommon(ejectStatus);
            }
        }

        private void OnExpandView(bool IsGraph)
        {
            if (QueueResultModel.SelectedSampleRecord == null)
                return;
            var viewModel = new ExpandedImageGraphViewModel();
            viewModel.IsFromGraph = IsGraph;
            viewModel.IsQMImage = true;
            viewModel.Filter += ViewModel_Filter;
            if (BarGraphDomainList != null && BarGraphDomainList.Count > 0)
            {
                viewModel.GraphViewList = BarGraphDomainList;
                viewModel.SelectedGraphItem = viewModel.Graph = BarGraphDomain;
            }

            if (QueueResultModel.SelectedSampleRecord.SampleImageList.Count > 0)
            {
                viewModel.SelectedSampleFromList = QueueResultModel.SelectedSampleRecord;
                viewModel.SampleImageResultList = QueueResultModel.SelectedSampleRecord.SampleImageList.ToList();
                viewModel.ImageIndexList = QueueResultModel.SelectedSampleRecord.ImageIndexList;
                viewModel.SampleImageDomain = QueueResultModel.SelectedSampleRecord.SelectedSampleImageRecord;
            }

            ScoutUIMasterViewModel.Instance.ShowUserPopUp(viewModel, "ExpandedImageGraph");
            BarGraphDomain = viewModel.SelectedGraphItem == null && BarGraphDomainList != null && BarGraphDomainList.Any()
                ? BarGraphDomainList.FirstOrDefault()
                : viewModel.SelectedGraphItem;
        }

        private void ViewModel_Filter(SampleImageRecordDomain obj)
        {
            try
            {
                if (QueueResultModel.SelectedSampleRecord != null)
                {
                    QueueResultModel.SelectedSampleRecord.SelectedSampleImageRecord = null;
                    QueueResultModel.SelectedSampleRecord.SelectedSampleImageRecord = obj;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONFILTER"));
            }
        }

        private void OnEnableWorkQueueListening()
        {
            ServiceLocator.Current.GetInstance<RunCellTypeViewModel>().EnableWorkQueueListening = true;
            ServiceLocator.Current.GetInstance<ConcentrationViewModel>().EnableWorkQueueListening = false;
            ServiceLocator.Current.GetInstance<QueueCreationViewModel>().EnableWorkQueueListening = false;
        }

        #endregion

        #region Public Methods

        public List<SampleCarouselDomain> GetCarouselSampleList(List<SampleCarouselDomain> gridRowSamples)
        {
            return gridRowSamples.Where(sample => sample.SampleStatus == SampleStatus.Selected).Select(sample =>
            {
                sample.RunSampleProgress = RunSampleProgressIndicator.eNotProcessed;
                return sample;
            }).ToList();
        }

        public void ClearRunProgressUI()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsPlayPauseStatus = IsLoadingIndicatorActive = false;
                SetColor(SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive,
                    SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive, string.Empty);
            });
        }

        public void ChangeRunProgressUI(eSampleStatus Status, SampleCarouselDomain item)
        {
            string runSampleStatus;
            switch (Status)
            {
                case eSampleStatus.eNotProcessed:
                    IsLoadingIndicatorActive = true;
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eNotProcessed));
                    SetColor(SampleProgressStatus.IsReady, SampleProgressStatus.IsReady,
                        SampleProgressStatus.IsReady, SampleProgressStatus.IsReady, runSampleStatus);
                    IsPlayPauseStatus = true;
                    break;
                case eSampleStatus.eInProcess_Aspirating:
                    IsLoadingIndicatorActive = true;
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eInProcess_Aspirating));
                    SetColor(SampleProgressStatus.IsRunning, SampleProgressStatus.IsInActive,
                        SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive, runSampleStatus);

                    item.RunSampleProgress = RunSampleProgressIndicator.eInProcess_Aspirating;
                    IsPlayPauseStatus = true;
                    break;
                case eSampleStatus.eInProcess_Mixing:
                    IsLoadingIndicatorActive = true;
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eInProcess_Mixing));
                    SetColor(SampleProgressStatus.IsActive, SampleProgressStatus.IsRunning,
                        SampleProgressStatus.IsInActive, SampleProgressStatus.IsInActive, runSampleStatus);

                    item.RunSampleProgress = RunSampleProgressIndicator.eInProcess_Mixing;
                    IsPlayPauseStatus = true;
                    break;
                case eSampleStatus.eInProcess_ImageAcquisition:
                    IsLoadingIndicatorActive = true;
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eInProcess_ImageAcquisition));
                    SetColor(SampleProgressStatus.IsActive, SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsRunning, SampleProgressStatus.IsInActive, runSampleStatus);

                    item.RunSampleProgress = RunSampleProgressIndicator.eInProcess_ImageAcquisition;
                    IsPlayPauseStatus = true;
                    break;
                case eSampleStatus.eInProcess_Cleaning:
                    IsLoadingIndicatorActive = true;
                    runSampleStatus = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RunSampleProgressIndicator.eInProcess_Cleaning));
                    SetColor(SampleProgressStatus.IsActive, SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsActive, SampleProgressStatus.IsRunning, runSampleStatus);

                    item.RunSampleProgress = RunSampleProgressIndicator.eInProcess_Cleaning;
                    IsPlayPauseStatus = true;
                    break;
                case eSampleStatus.eCompleted:
                    IsLoadingIndicatorActive = false;

                    runSampleStatus = IsCarouselStatus ? string.Format(LanguageResourceHelper.Get("LID_Status_Completed_POS"), item.RowWisePosition) : string.Format(LanguageResourceHelper.Get("LID_Status_Completed_POS"), item.SamplePosition.col);

                    SetColor(SampleProgressStatus.IsActive, SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsActive, SampleProgressStatus.IsActive, runSampleStatus);

                    item.RunSampleProgress = RunSampleProgressIndicator.eCompleted;
                    IsPlayPauseStatus = true;
                    break;
                case eSampleStatus.eSkip_Manual:
                case eSampleStatus.eSkip_Error:
                    LogDetailsOnSkippedSample();
                    break;
            }
        }

        #endregion

        #region Image Process Status

        public bool IsBubblePopupOpen
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public BubbleStatusViewModel BubbleStatusViewModel
        {
            get { return GetProperty<BubbleStatusViewModel>(); }
            set { SetProperty(value); }
        }

        private RelayCommand _bubbleStatusCommand;

        public RelayCommand BubbleStatusCommand =>
            _bubbleStatusCommand ?? (_bubbleStatusCommand = new RelayCommand(OnBubbleStatusUpdate, null));

        private void OnBubbleStatusUpdate()
        {
            if (IsBubblePopupOpen)
                UpdateBubblePopUp(false);
            else
            {
                UpdateBubblePopUp(true);
                if (BubbleStatusViewModel == null)
                    BubbleStatusViewModel = new BubbleStatusViewModel();
                BubbleStatusViewModel.UpdateBubbleStatus(QueueResultModel?.SelectedSampleRecord, true);
            }
        }

        private void UpdateBubblePopUp(bool isOpen)
        {
            IsBubblePopupOpen = isOpen;
        }
        #endregion
    }
}