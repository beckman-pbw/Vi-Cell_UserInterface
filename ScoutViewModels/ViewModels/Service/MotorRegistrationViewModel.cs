using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutDomains;
using ScoutDomains.Common;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Home.QueueManagement;
using ScoutModels.Service;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.EventDomain;
using ScoutUtilities.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ScoutModels.Interfaces;

namespace ScoutViewModels.ViewModels.Service
{
    public class MotorRegistrationViewModel : BaseViewModel
    {
        public eSensorStatus CarouselDetect => _instrumentStatusService.SystemStatusDom.CarouselDetect;
        public eSensorStatus TubeDetect => _instrumentStatusService.SystemStatusDom.TubeDetect;
        public eSensorStatus RadiusHome => _instrumentStatusService.SystemStatusDom.RadiusHome;
        public eSensorStatus ThetaHome => _instrumentStatusService.SystemStatusDom.ThetaHome;
        public eSensorStatus ProbeHome => _instrumentStatusService.SystemStatusDom.ProbeHome;
        public string StagePosition => _instrumentStatusService.SystemStatusDom.StagePositionString;
        public int MotorProbePosition => _instrumentStatusService.SystemStatusDom.MotorProbePosition;
        public int MotorRadiusPosition => _instrumentStatusService.SystemStatusDom.MotorRadiusPosition;
        public int MotorThetaPosition => _instrumentStatusService.SystemStatusDom.MotorThetaPosition;

        public eSensorStatus IsCarouselStatus
        {
            get { return _instrumentStatusService.SystemStatusDom.CarouselDetect; }
            set
            {
                if (_instrumentStatusService.SystemStatusDom.CarouselDetect != value)
                {
                    _instrumentStatusService.SystemStatusDom.CarouselDetect = value;
                    NotifyPropertyChanged(nameof(IsCarouselStatus));
                }
            }
        }

        private bool _isMotorProcessRunning;

        private bool _isMotorRegEnable;

        public bool IsMotorRegEnable
        {
            get { return _isMotorRegEnable; }
            set
            {
                _isMotorRegEnable = value;
                NotifyPropertyChanged(nameof(IsMotorRegEnable));
            }
        }

        private bool _isNotchVisible;

        public bool IsNotchVisible
        {
            get { return _isNotchVisible; }
            set
            {
                _isNotchVisible = value;
                NotifyPropertyChanged(nameof(IsNotchVisible));
            }
        }

        private bool _isProbeBtnEnabled;

        public bool IsProbeBtnEnabled
        {
            get { return _isProbeBtnEnabled; }
            set
            {
                _isProbeBtnEnabled = value;
                NotifyPropertyChanged(nameof(IsProbeBtnEnabled));
            }
        }

        private readonly IInstrumentStatusService _instrumentStatusService;
        private IDisposable _statusSubscriber;
        private Subscription<StageStatus> _stageStatusSubscriber;

        #region Constructor

        QueueManagementModel QueueManagementModel { get; set; }
        QueueResultModel QueueResultModel { get; set; }

        public MotorRegistrationViewModel(IInstrumentStatusService instrumentStatusService) : base()
        {
            _instrumentStatusService = instrumentStatusService;
            MotorRegistrationModel = new MotorRegistrationModel();
            QueueManagementModel = new QueueManagementModel();
            QueueResultModel = new QueueResultModel();
            Initialize();
        }

        private void Initialize()
        {
            StepOneTabItemSelected = true;
            MotorRegistrationModel.CarouselMotorCalibrationStateChanged += HandleCarouselMotorCalibrationStateChanged;
            MotorRegistrationModel.PlateMotorCalibrationStateChanged += HandlePlateMotorCalibrationStateChanged;
            MotorRegistrationModel.MotorCalibrationCancelled += HandleCancelMotorCalibrationStateChanged;
            _stageStatusSubscriber = MessageBus.Default.Subscribe<StageStatus>(OnCarouselStatusChange);
            _statusSubscriber = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe((OnSystemStatusChanged));
            SetCarouselStatus(MotorRegHighlightType.Black);
            IsProbeBtnEnabled = true;
        }

        protected override void DisposeUnmanaged()
        {
            _statusSubscriber?.Dispose();
            QueueResultModel?.Dispose();
            QueueManagementModel?.Dispose();
            MessageBus.Default.UnSubscribe(ref _stageStatusSubscriber);
            base.DisposeUnmanaged();
        }

        private void OnSystemStatusChanged(SystemStatusDomain systemStatusDomain)
        {
            if (systemStatusDomain == null) return;

            NotifyAllPropertiesChanged();
        }

        private void OnCarouselStatusChange(StageStatus obj)
        {
            try
            {
                if (_instrumentStatusService.SystemStatusDom.CarouselDetect == obj.IsCarouselStatus)
                    return;

                if (_isMotorProcessRunning)
                {
                    switch (obj.IsCarouselStatus)
                    {
                        case eSensorStatus.ssStateUnknown:
                            IsMotorRegEnable = false;
                            break;
                        case eSensorStatus.ssStateInactive:
                            DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_MotorCarouselRegError"));
                            CancelCarouselCalibration();
                            break;
                        case eSensorStatus.ssStateActive:
                            DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_MotorPlateRegError"));
                            CancelCarouselCalibration();
                            break;
                    }
                }
                else
                {
                    IsMotorRegEnable = !obj.IsCarouselStatus.Equals(eSensorStatus.ssStateUnknown);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CAROUSEL_STATUS_CHANGE"));
            }
        }

        private void CancelCarouselCalibration()
        {
            CancelCalibration();
            _isMotorProcessRunning = false;
            UpdateTabOnMotorRegProcess(true);
            IsMotorRegEnable = true;
            SetGridCarouselStep("1");
        }

        #endregion

        #region Private Property   

        private bool _isCarousel;
        
        private string _motorRegType;

        private Visibility _motorRegStep2Visibility;

        private Visibility _motorRegStep3Visibility;

        private bool _progressBarStatus;
        
        private bool _changesDialogResult;

        private int _gridAngle;
        
        private bool _isClearingCarousel;
        
        private int _highLightedSample;
        
        private int _playItemPosition;

        private MotorRegistrationModel _motorRegModel;

        private bool _stepTwoTabItemSelected;
        
        private bool _stepThreeTabItemSelected;

        private bool _stepOneTabItemSelected;

        private bool _stepFourTabItemSelected;

        private KeyValuePair<int, string> _selectedGridRowPosition;

        private KeyValuePair<int, string> _selectedColumnPosition;
        
        private KeyValuePair<int, string> _selectedPosition;

        private List<KeyValuePair<int, string>> _gridRowPositionList;

        private List<KeyValuePair<int, string>> _gridColumnPositionList;

        private List<KeyValuePair<int, string>> _positionList;

        private bool _isCarouselGridActive;

        private bool _isStepNotchActive;
        private bool _isStepEllipseActive;

        #endregion

        #region Public Property      

        public bool IsCarousel
        {
            get { return _isCarousel; }
            set
            {
                _isCarousel = value;
                NotifyPropertyChanged(nameof(IsCarousel));
            }
        }

     
        public Visibility MotorRegStep2Visibility
        {
            get { return _motorRegStep2Visibility; }
            set
            {
                _motorRegStep2Visibility = value;
                NotifyPropertyChanged(nameof(MotorRegStep2Visibility));
            }
        }

        public Visibility MotorRegStep3Visibility
        {
            get { return _motorRegStep3Visibility; }
            set
            {
                _motorRegStep3Visibility = value;
                NotifyPropertyChanged(nameof(MotorRegStep3Visibility));
            }
        }

        public bool ProgressBarStatus
        {
            get { return _progressBarStatus; }
            set
            {
                _progressBarStatus = value;
                NotifyPropertyChanged(nameof(ProgressBarStatus));
            }
        }

        public string MotorRegType
        {
            get { return _motorRegType; }
            set
            {
                _motorRegType = value;
                NotifyPropertyChanged(nameof(MotorRegType));
            }
        }

    
        public bool ChangesDialogResult
        {
            get { return _changesDialogResult; }
            set
            {
                _changesDialogResult = value;
                NotifyPropertyChanged(nameof(ChangesDialogResult));
            }
        }

        public int GridAngle
        {
            get { return _gridAngle; }
            set
            {
                _gridAngle = value;
                NotifyPropertyChanged(nameof(GridAngle));
            }
        }

    
        public bool IsClearingCarousel
        {
            get { return _isClearingCarousel; }
            set
            {
                _isClearingCarousel = value;
                NotifyPropertyChanged(nameof(IsClearingCarousel));
            }
        }

      
        public int HighLightedSample
        {
            get { return _highLightedSample; }
            set
            {
                _highLightedSample = value;
                NotifyPropertyChanged(nameof(HighLightedSample));
            }
        }

        public int PlayItemPosition
        {
            get { return _playItemPosition; }
            set
            {
                _playItemPosition = value;
                NotifyPropertyChanged(nameof(PlayItemPosition));
            }
        }

        public List<KeyValuePair<int, string>> GridRowPositionList
        {
            get
            {
                return _gridRowPositionList ??
                       (_gridRowPositionList = new List<KeyValuePair<int, string>>(SetGridRowPosition()));
            }
            set { _gridRowPositionList = value; }
        }

        public KeyValuePair<int, string> SelectedGridRowPosition
        {
            get { return _selectedGridRowPosition; }
            set
            {
                _selectedGridRowPosition = value;
                NotifyPropertyChanged(nameof(SelectedGridRowPosition));
            }
        }

        public KeyValuePair<int, string> SelectedColumnPosition
        {
            get { return _selectedColumnPosition; }
            set
            {
                _selectedColumnPosition = value;
                NotifyPropertyChanged(nameof(SelectedColumnPosition));
            }
        }

        public KeyValuePair<int, string> SelectedPosition
        {
            get { return _selectedPosition; }
            set
            {
                _selectedPosition = value;
                NotifyPropertyChanged(nameof(SelectedPosition));
            }
        }

     
        public List<KeyValuePair<int, string>> GridColumnPositionList
        {
            get
            {
                return _gridColumnPositionList ?? (_gridColumnPositionList =
                           new List<KeyValuePair<int, string>>(SetGridColumnPosition()));
            }
            set { _gridColumnPositionList = value; }
        }

     
        public List<KeyValuePair<int, string>> PositionList
        {
            get { return _positionList ?? (_positionList = new List<KeyValuePair<int, string>>(SetPosition())); }
            set { _positionList = value; }
        }

      
        public MotorRegistrationModel MotorRegistrationModel
        {
            get { return _motorRegModel; }
            set
            {
                _motorRegModel = value;
                NotifyPropertyChanged(nameof(MotorRegistrationModel));
            }
        }

     
        public bool StepOneTabItemSelected
        {
            get { return _stepOneTabItemSelected; }
            set
            {
                _stepOneTabItemSelected = value;
                NotifyPropertyChanged(nameof(StepOneTabItemSelected));
            }
        }

   
        public bool StepTwoTabItemSelected
        {
            get { return _stepTwoTabItemSelected; }
            set
            {
                _stepTwoTabItemSelected = value;
                NotifyPropertyChanged(nameof(StepTwoTabItemSelected));
            }
        }

   
        public bool StepThreeTabItemSelected
        {
            get { return _stepThreeTabItemSelected; }
            set
            {
                _stepThreeTabItemSelected = value;
                NotifyPropertyChanged(nameof(StepThreeTabItemSelected));
            }
        }

    
        public bool StepFourTabItemSelected
        {
            get { return _stepFourTabItemSelected; }
            set
            {
                _stepFourTabItemSelected = value;
                NotifyPropertyChanged(nameof(StepFourTabItemSelected));
            }
        }


        public bool IsCarouselGridActive
        {
            get { return _isCarouselGridActive; }
            set
            {
                _isCarouselGridActive = value;
                NotifyPropertyChanged(nameof(IsCarouselGridActive));
            }
        }


        public bool IsStepNotchActive
        {
            get { return _isStepNotchActive; }
            set
            {
                _isStepNotchActive = value;
                NotifyPropertyChanged(nameof(IsStepNotchActive));
            }
        }

        public bool IsStepEllipseActive
        {
            get { return _isStepEllipseActive; }
            set
            {
                _isStepEllipseActive = value;
                NotifyPropertyChanged(nameof(IsStepEllipseActive));
            }
        }
        #endregion

        #region private Command

        private ICommand _probeUpDownCommand;

        private ICommand _nextCommand;

        #endregion

        #region Public Command

        public ICommand NextCommand => _nextCommand ?? 
                                       (_nextCommand = new RelayCommand(MotorRegStepPosition));

        public ICommand ProbUpDownCommand => _probeUpDownCommand ?? 
                                             (_probeUpDownCommand = new RelayCommand(OnChangeProbeUpDown));

        #endregion

        #region Private Method

     
        private void MotorRegStepPosition(object parameter)
        {
            try
            {
                IsNotchVisible = false;
                IsStepEllipseActive = false;
                IsStepNotchActive = false;
                string stepPosition = parameter.ToString();

                var carouselDetected = _instrumentStatusService.SystemStatusDom.CarouselDetect;

                switch (stepPosition)
                {
                    case "1":
                        if (carouselDetected == eSensorStatus.ssStateInactive)
                            UpdateGridAngle(false);
                        break;
                    
                    case "2":
                        if (carouselDetected == eSensorStatus.ssStateActive)
                        {
                            var carouselResult = PerformCarouselCalibration();
                            if (carouselResult.Equals(HawkeyeError.eSuccess))
                            {
                                SetCarouselStatus(MotorRegHighlightType.Yellow);
                                IsNotchVisible = true;
                            }
                        }
                        else if (carouselDetected == eSensorStatus.ssStateInactive)
                        {
                            var plateResult = PerformPlateCalibration();
                            if (plateResult.Equals(HawkeyeError.eSuccess))
                            {
                                UpdateGridAngle(false);
                                IsStepEllipseActive = true;
                            }
                        }
                        _isMotorProcessRunning = true;
                        break;
                    
                    case "3":
                        if (carouselDetected == eSensorStatus.ssStateActive)
                        {
                            var carouselResult = PerformCarouselCalibration();
                            if (carouselResult.Equals(HawkeyeError.eSuccess))
                            {
                                ClearCarousel();
                                SetCarouselStatus(MotorRegHighlightType.Border);
                            }
                        }
                        else if (carouselDetected == eSensorStatus.ssStateInactive)
                        {
                            var plateResult = PerformPlateCalibration();
                            if (plateResult.Equals(HawkeyeError.eSuccess))
                            {
                                UpdateGridAngle(true);
                                IsStepNotchActive = true;
                            }
                        }
                        _isMotorProcessRunning = true;
                        break;
                    
                    case "4":
                        if (carouselDetected == eSensorStatus.ssStateInactive)
                        {
                            var plateResult = PerformPlateCalibration();
                            if (plateResult.Equals(HawkeyeError.eSuccess))
                            {
                                UpdateGridAngle(false);
                                IsCarouselGridActive = false;
                            }
                        }
                        _isMotorProcessRunning = true;
                        break;
                    
                    case "Repeat":
                        if (carouselDetected == eSensorStatus.ssStateActive)
                        {
                            CancelCalibration();
                            SetGridCarouselStep("1");
                            SetCarouselStatus(MotorRegHighlightType.Black);
                        }
                        else if (carouselDetected == eSensorStatus.ssStateInactive)
                        {
                            UpdateGridAngle(false);
                            CancelCalibration();
                            SetGridCarouselStep("1");
                            IsCarouselGridActive = false;
                        }
                        _isMotorProcessRunning = false;
                        break;
                    
                    case "Ok":
                        if (carouselDetected == eSensorStatus.ssStateActive)
                        {
                            var carouselResult = PerformCarouselCalibration();
                            if (carouselResult.Equals(HawkeyeError.eSuccess))
                                SetCarouselStatus(MotorRegHighlightType.Black);
                        }
                        else if (carouselDetected == eSensorStatus.ssStateInactive)
                        {
                            var plateResult = PerformPlateCalibration();
                            if (plateResult.Equals(HawkeyeError.eSuccess))
                            {
                                UpdateGridAngle(false);
                                SetGridCarouselStep("1");
                                IsCarouselGridActive = false;
                            }
                        }
                        _isMotorProcessRunning = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_MOTOR_REG_STEP_POSITION"));
            }
        }

        private void SetGridCarouselStep(string step)
        {
            switch (step)
            {
                case "1":
                    StepOneTabItemSelected = true;
                    StepTwoTabItemSelected = false;
                    StepThreeTabItemSelected = false;
                    StepFourTabItemSelected = false;
                    break;
                case "2":
                    StepOneTabItemSelected = false;
                    StepTwoTabItemSelected = true;
                    StepThreeTabItemSelected = false;
                    StepFourTabItemSelected = false;
                    break;
                case "3":
                    StepOneTabItemSelected = false;
                    StepTwoTabItemSelected = false;
                    StepThreeTabItemSelected = true;
                    StepFourTabItemSelected = false;
                    break;
                case "4":
                    StepOneTabItemSelected = false;
                    StepTwoTabItemSelected = false;
                    StepThreeTabItemSelected = false;
                    StepFourTabItemSelected = true;
                    break;
            }
        }

        private List<KeyValuePair<int, string>> SetGridRowPosition()
        {
            var list = new List<KeyValuePair<int, string>>();
            for (int i = 1; i < 13; i++)
                list.Add(new KeyValuePair<int, string>(i, ScoutUtilities.Misc.ConvertToString(i)));
            return list;
        }

    
        private List<KeyValuePair<int, string>> SetGridColumnPosition()
        {
            var list = new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(1, "A"),
                new KeyValuePair<int, string>(2, "B"),
                new KeyValuePair<int, string>(3, "C"),
                new KeyValuePair<int, string>(4, "D"),
                new KeyValuePair<int, string>(5, "E"),
                new KeyValuePair<int, string>(6, "F"),
                new KeyValuePair<int, string>(7, "G"),
                new KeyValuePair<int, string>(8, "H")
            };
            return list;
        }

        //hard code
        private List<KeyValuePair<int, string>> SetPosition()
        {
            var list = new List<KeyValuePair<int, string>>();

            for (int i = 1; i < 25; i++)
                list.Add(new KeyValuePair<int, string>(i, ScoutUtilities.Misc.ConvertToString(i)));

            return list;
        }

        private void SetGridCarouselPosition(KeyValuePair<int, string> selectedRowPosition,
            KeyValuePair<int, string> selectedColumnPosition)
        {
            if (string.IsNullOrEmpty(selectedRowPosition.Value) || string.IsNullOrEmpty(selectedColumnPosition.Value))
            {
                return;
            }

            MotorRegistrationModel.SetSampleWellPosition(char.Parse(selectedColumnPosition.Value), uint.Parse(selectedRowPosition.Value));
            var samplePosition = QueueManagementModel.GetSampleWellPosition();
            if (!samplePosition.IsValid())
                return;

            ClearGridCarousel();

            var gridSampleList = QueueResultModel.PlateSamples.Where(x =>
                x.SamplePosition.Row == char.Parse(selectedColumnPosition.Value) &&
                x.SamplePosition.Column == byte.Parse(selectedRowPosition.Value));

            ClearGridRunning();

            foreach (var item in gridSampleList)
            {
                item.IsRunning = true;
            }
        }

        private void ClearGridCarousel()
        {
            QueueResultModel.PlateSamples.Select(x =>
            {
                x.SampleStatusColor = SampleStatusColor.Empty;
                return x;
            }).ToList();
        }

            private void ClearGridRunning()
        {
            QueueResultModel.PlateSamples.Select(x =>
            {
                x.IsRunning = false;
                return x;
            }).ToList();
        }

        private void UpdateGridAngle(bool isRotate)
        {
            GridAngle = isRotate ? 90 : 0;
        }

   
        private void ClearCarousel()
        {
            QueueResultModel.CarouselSamples.ForEach(x => { x.SampleStatusColor = SampleStatusColor.Empty; });
            IsClearingCarousel = true;
            IsClearingCarousel = false;
        }

     
        private void SetSelectedCarouselPosition(int position)
        {
            MotorRegistrationModel.SetSampleWellPosition('Z', Convert.ToUInt32(position));
            var samplePosition = QueueManagementModel.GetSampleWellPosition();

            var selectedPosition = PositionList.FirstOrDefault(x => x.Value == Misc.ConvertToString(samplePosition.Column));

            if (selectedPosition.Key != 0)
                SelectedPosition = selectedPosition;

            if (samplePosition.IsValid())
            {
                QueueResultModel.CarouselSamples.ForEach(x => x.SampleStatusColor = SampleStatusColor.Empty);
                foreach (SampleDomain sample in QueueResultModel.CarouselSamples.Where(r => (r.SamplePosition.Column == samplePosition.Column)))
                    sample.SampleStatusColor = SampleStatusColor.Selected;
                HighLightedSample = PlayItemPosition;
                PlayItemPosition = samplePosition.Column;
            }
        }

        private void SetCarouselStep(string value)
        {
            SetGridCarouselStep(value);
            ProgressBarStatus = false;
        }

        private async void OnChangeProbeUpDown(object param)
        {
            try
            {
                if (param == null)
                    return;
                switch (param.ToString())
                {
                    case "Top":
                        IsProbeBtnEnabled = false;
                        var moveProbeTopStatus = await Task.Run(() => MotorRegistrationModel.MoveProbe(true));
                        if (moveProbeTopStatus.Equals(HawkeyeError.eSuccess))
                            PostToMessageHub(
                                LanguageResourceHelper.Get("LID_StatusBar_MovingProbeSuccessful"));
                        else
                            ApiHawkeyeMsgHelper.ErrorCommon(moveProbeTopStatus);
                        IsProbeBtnEnabled = true;
                        break;
                    case "Bottom":
                        IsProbeBtnEnabled = false;
                        var moveProbeBottomStatus = await Task.Run(() => MotorRegistrationModel.MoveProbe(false));
                        if (moveProbeBottomStatus.Equals(HawkeyeError.eSuccess))
                            PostToMessageHub(
                                LanguageResourceHelper.Get("LID_StatusBar_MovingProbeSuccessful"));
                        else
                            ApiHawkeyeMsgHelper.ErrorCommon(moveProbeBottomStatus);
                        IsProbeBtnEnabled = true;
                        break;
                    case "StepUp":
                        IsProbeBtnEnabled = false;
                        var moveProbeStepUpStatus = await Task.Run(() => MotorRegistrationModel.SetProbePosition(true, ApplicationConstants.ProbePositionValue));
                        if (moveProbeStepUpStatus.Equals(HawkeyeError.eSuccess))
                            PostToMessageHub(
                                LanguageResourceHelper.Get("LID_StatusBar_MovingProbeSuccessful"));
                        else
                            ApiHawkeyeMsgHelper.ErrorCommon(moveProbeStepUpStatus);
                        IsProbeBtnEnabled = true;
                        break;
                    case "StepDown":
                        IsProbeBtnEnabled = false;
                        var moveProbeStepDownStatus = await Task.Run(() => MotorRegistrationModel.SetProbePosition(false, ApplicationConstants.ProbePositionValue));
                        if (moveProbeStepDownStatus.Equals(HawkeyeError.eSuccess))
                            PostToMessageHub(
                                LanguageResourceHelper.Get("LID_StatusBar_MovingProbeSuccessful"));
                        else
                            ApiHawkeyeMsgHelper.ErrorCommon(moveProbeStepDownStatus);
                        IsProbeBtnEnabled = true;
                        break;
                    case "GridPosition":
                        await Task.Run(() => SetGridCarouselPosition(SelectedGridRowPosition, SelectedColumnPosition));
                        break;
                    case "Position":
                        await Task.Run(() => SetSelectedCarouselPosition(SelectedPosition.Key));
                        break;
                }
            }
            catch (Exception ex)
            {
                IsProbeBtnEnabled = true;
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CHANGE_PROBE_UP_DOWN"));
            }
        }

        private void SetCarouselStatus(MotorRegHighlightType motorRegType)
        {
            HighLightedSample = 1;
            switch (motorRegType)
            {
                case MotorRegHighlightType.Black:
                    PlayItemPosition = 1;
                    MotorRegType = "Black";
                    HighLightedSample = 24;
                    break;
                case MotorRegHighlightType.Border:
                    MotorRegType = "Border";
                    HighLightedSample = 24;
                    break;
                case MotorRegHighlightType.Yellow:
                    MotorRegType = "Yellow";
                    HighLightedSample = 24;
                    break;
                default:
                    MotorRegType = "";
                    break;
            }
        }

        #endregion

        #region Public Method

        public void UpdateTabOnMotorRegProcess(bool isTabEnable)
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                MainWindowViewModel.Instance.EnableDisableHamburgerMenu(isTabEnable);
                MessageBus.Default.Publish(new Notification<bool>(isTabEnable, nameof(ServiceViewModel),
                    nameof(ServiceViewModel.IsMotorRunning)));
            });
        }


        public HawkeyeError PerformPlateCalibration()
        {
            _enablePlateMotorStateListener = true;
            var plateResult = MotorRegistrationModel.PerformPlateMotorCalibration();
            if (plateResult.Equals(HawkeyeError.eSuccess))
            {
                UpdateTabOnMotorRegProcess(false);
                PostToMessageHub(
                    LanguageResourceHelper.Get("LID_StatusBar_PerformPlateProcess"));
                ProgressBarStatus = true;
            }
            else
            {
                _enablePlateMotorStateListener = false;
                ApiHawkeyeMsgHelper.ErrorCommon(plateResult);
                UpdateTabOnMotorRegProcess(true);
            }
            return plateResult;
        }

        private bool _enableCarouselMotorStateListener;
        private bool _enablePlateMotorStateListener;

        public HawkeyeError PerformCarouselCalibration()
        {
            _enableCarouselMotorStateListener = true;
            var carouselResult = MotorRegistrationModel.PerformCarouselMotorCalibration();
            if (carouselResult.Equals(HawkeyeError.eSuccess))
            {
                UpdateTabOnMotorRegProcess(false);
                PostToMessageHub(
                    LanguageResourceHelper.Get("LID_StatusBar_PerformCarouselProcess"));
                ProgressBarStatus = true;
            }
            else
            {
                _enableCarouselMotorStateListener = false;
                ApiHawkeyeMsgHelper.ErrorCommon(carouselResult);
                UpdateTabOnMotorRegProcess(true);
            }

            return carouselResult;
        }

  
        public void CancelCalibration()
        {
            var cancelResult = MotorRegistrationModel.CancelMotorCalibration();
            if (cancelResult == HawkeyeError.eSuccess)
            {
                UpdateTabOnMotorRegProcess(true);
                PostToMessageHub(
                    LanguageResourceHelper.Get("LID_StatusBar_MotorRegProcessCan"));
            }
            else
                ApiHawkeyeMsgHelper.ErrorCommon(cancelResult);
        }

        private void HandlePlateMotorCalibrationStateChanged(object sender, ApiEventArgs<CalibrationState> e)
        {
            if (!_enablePlateMotorStateListener)
            {
                return;
            }

            switch (e.Arg1)
            {
                case CalibrationState.eHomingRadiusTheta:
                    DispatcherHelper.ApplicationExecute(() => { SetCarouselStep("2"); });
                    break;
                case CalibrationState.eCalibrateRadius:
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        SetCarouselStep("3");
                    });
                    break;
                case CalibrationState.eCalibrateTheta:
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        SetCarouselStep("4");
                        OnUpdateCarouselTypePosition(_instrumentStatusService.SystemStatusDom.CarouselDetect,
                            _instrumentStatusService.SystemStatusDom.SamplePosition);
                    });
                    break;
                case CalibrationState.eCompleted:
                    _enablePlateMotorStateListener = false;
                    DispatcherHelper.ApplicationExecute(() => { SetCarouselStep("1"); });
                    UpdateTabOnMotorRegProcess(true);
                    break;
                case CalibrationState.eFault:
                    _enablePlateMotorStateListener = false;
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_FailToPerformReg"));
                    DispatcherHelper.ApplicationExecute(() => { SetCarouselStep("1"); });

                    _isMotorProcessRunning = ProgressBarStatus = false;
                    UpdateTabOnMotorRegProcess(true);
                    break;
            }
        }

        private void HandleCancelMotorCalibrationStateChanged(object sender, ApiEventArgs<CalibrationState> e)
        {
            Log.Info($"HandleCancelMotorCalibrationStateChanged(): Cancel Motor Calibration State = {e.Arg1}");
        }

        private void HandleCarouselMotorCalibrationStateChanged(object sender, ApiEventArgs<CalibrationState> e)
        {
            if (!_enableCarouselMotorStateListener)
            {
                return;
            }

            Log.Debug("HandleCarouselMotorCalibrationStateChanged:: New Calibration State:" + e.Arg1.ToString());

            switch (e.Arg1)
            {
                case CalibrationState.eHomingRadiusTheta:
                    DispatcherHelper.ApplicationExecute(() => {
                        PlayItemPosition = 1;
                        SetCarouselStep("2");
                    });
                    break;
                case CalibrationState.eCalibrateRadiusTheta:
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        SetCarouselStep("3");
                        OnUpdateCarouselTypePosition(_instrumentStatusService.SystemStatusDom.CarouselDetect,
                            _instrumentStatusService.SystemStatusDom.SamplePosition);
                    });
                    break;
                case CalibrationState.eCompleted:
                    _enableCarouselMotorStateListener = false;
                    DispatcherHelper.ApplicationExecute(() => { SetCarouselStep("1"); PlayItemPosition = 1; });
                    UpdateTabOnMotorRegProcess(true);
                    break;
                case CalibrationState.eFault:

                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_FailToPerformReg"));
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        SetCarouselStep("1");
                        PlayItemPosition = 1;
                    });

                    _isMotorProcessRunning = ProgressBarStatus = false;
                    UpdateTabOnMotorRegProcess(true);
                    break;
            }
        }

        private void OnUpdateCarouselTypePosition(eSensorStatus sensorcarouseldetect,
            SamplePosition sampleStageLocation)
        {
            switch (sensorcarouseldetect)
            {
                case eSensorStatus.ssStateUnknown:
                    break;

                case eSensorStatus.ssStateActive:
                    if (sampleStageLocation.IsValid())
                        PlayItemPosition = sampleStageLocation.Column;
                    SetCarouselPosition(sampleStageLocation);
                    break;

                case eSensorStatus.ssStateInactive:
                    if (sampleStageLocation.IsValid())
                        QueueManagementModel.PlateQueueDesignCollection.Where(sample =>
                                sample.SamplePosition.Row == sampleStageLocation.Row &&
                                sample.SamplePosition.Column == sampleStageLocation.Column)
                            .Select(sample => sample.IsRunning = true);
                    SetGridCarouselPosition(sampleStageLocation);
                    break;
            }
        }

        private void SetCarouselPosition(SamplePosition sampleStageLocation)
        {
            if (!sampleStageLocation.IsValid())
                return;
            var carouselColumn = PositionList.FirstOrDefault(columnPosition =>
                columnPosition.Value == sampleStageLocation.Row.ToString());
            if (!(string.IsNullOrEmpty(carouselColumn.Value)))
            {
                SelectedPosition = carouselColumn;
            }
        }

        private void SetGridCarouselPosition(SamplePosition sampleStageLocation)
        {
            if (!sampleStageLocation.IsValid())
                return;
            var gridColumn = GridColumnPositionList.FirstOrDefault(columnPosition =>
                columnPosition.Value == sampleStageLocation.Row.ToString());
            var gridRow = GridRowPositionList.FirstOrDefault(rowPosition =>
                rowPosition.Value == ScoutUtilities.Misc.ConvertToString(sampleStageLocation.Column));
            if (string.IsNullOrEmpty(gridColumn.Value) || string.IsNullOrEmpty(gridColumn.Value))
                return;
            SelectedGridRowPosition = gridRow;
            SelectedColumnPosition = gridColumn;
        }

        #endregion
    }
}