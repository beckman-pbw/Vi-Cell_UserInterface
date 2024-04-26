using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.DataTransferObjects;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Review;
using ScoutModels.Service;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.EventDomain;
using ScoutUtilities.Events;
using ScoutUtilities.Structs;
using ScoutUtilities.UIConfiguration;
using ScoutViewModels.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ScoutModels.Interfaces;

namespace ScoutViewModels.ViewModels.Service
{
    public class ManualControlsOpticsViewModel : BaseViewModel
    {
        #region Constructor

        public ManualControlsOpticsViewModel(IInstrumentStatusService instrumentStatusService) : base()
        {
            _instrumentStatusService = instrumentStatusService;
            OpticsManualServiceModel = new OpticsManualServiceModel();
            Initialize();
        }

        private void Initialize()
        {
            MainWindowViewModel.Instance.SwitchOffLiveImage = OnSwitchOffLiveImage;
            SetManualControlOpticsValue();
            if (LiveImageList.Any()) SelectedOption = LiveImageList[0];
            ValueUpdateAction += ValueUpdateCallBack;

            // Subscribed MotorFocusPosition for to reflect the updated value in UI.
            _focusPositionSubscriber = MessageBus.Default.Subscribe<MotorFocusPosition>((s) => CurrentFocusPosition = s.FocusPosition);
        }

        protected override void DisposeUnmanaged()
        {
            ValueUpdateAction -= ValueUpdateCallBack;
            OpticsManualServiceModel?.Dispose();
            MessageBus.Default.UnSubscribe(ref _focusPositionSubscriber);
            base.DisposeUnmanaged();
        }

        #endregion

        #region Private Property    

        private string _resultHeader;
        private bool _isFlyOutOpen;
        private bool _isFlyOutChecked;
        private bool _isSingleImageAvailable;
        private bool _enableSingleAnalysisListener;
        private bool _enableContinuousAnalysisListener;
        private bool _enableLiveImageListener;
        private bool _response;
        private bool _isLiveImageActive;
        private bool _isExpandedResultListVisible;
        private readonly IInstrumentStatusService _instrumentStatusService;
        private Subscription<MotorFocusPosition> _focusPositionSubscriber;

        private ICommand _setFocusCommand;
        
        private ICommand _diagnosticsCommand;
        
        private ICommand _dustReferenceCommand;

        private bool _isFlowCellDepthActive = true;

        private const double MaxFlowCellDepth = 0.066;

        private const double MinFlowCellDepth = 0.059;

        private ICommand _flowCellDepthCommand;

        private OpticsManualServiceModel _opticsManualServiceModel;
        
        private AdjustValue _adjustState;

        private bool _isContShotActive = true;

        private double _brightField;

        private bool _isImageAvailable;

        private OpticsAction _opticsAction;

        private double _flowCellDepth;

        private double _currentFocusPosition;

        private ICommand _stopContShotCommand;
        
        private ICommand _restoreFocusCommand;

        private bool _isFinalImgEnable;

        private ICommand _opticsCommand;

        private ICommand _imageCommand;

        private SampleImageRecordDomain _sampleImageDomain;

        private ICommand _imageExpandCommand;

        private bool _isLiveImageOn;

        public bool IsLiveImageOn
        {
            get { return _isLiveImageOn; }
            set
            {
                _isLiveImageOn = value;
                NotifyPropertyChanged(nameof(IsLiveImageOn));
            }
        }

        private ICommand _displayNextBgResultCommand;

        private bool _isBrightFieldDisabled;

        #endregion

        #region Public Property  

        public string ResultHeader
        {
            get { return _resultHeader; }
            set
            {
                _resultHeader = value;
                NotifyPropertyChanged(nameof(ResultHeader));
            }
        }

        private bool _isLiveImageFreeze;

        public bool IsLiveImageFreeze
        {
            get { return _isLiveImageFreeze; }
            set
            {
                _isLiveImageFreeze = value;
                NotifyPropertyChanged("IsLiveImageFreeze");
            }
        }

        public ICommand ImageExpandCommand => _imageExpandCommand ??
                                              (_imageExpandCommand = new RelayCommand(OnExpandImage, null));

        public ICommand DisplayNextBgResultCommand => _displayNextBgResultCommand ??
                                                      (_displayNextBgResultCommand =
                                                          new RelayCommand(OnDisplayNextBgResultCommand, null));

        public bool IsContShotActive
        {
            get { return _isContShotActive; }
            set
            {
                _isContShotActive = value;
                NotifyPropertyChanged(nameof(IsContShotActive));
            }
        }

        public bool IsFinalImgEnable
        {
            get { return _isFinalImgEnable; }
            set
            {
                _isFinalImgEnable = value;
                NotifyPropertyChanged(nameof(IsFinalImgEnable));
            }
        }

        public bool IsLampOn
        {
            get { return GetCameraLamp(); }
            set
            {
                if (IsSetCameraLampStateSucceed(value, BrightField))
                    NotifyPropertyChanged(nameof(IsLampOn));
            }
        }

        private ObservableCollection<LiveImageDataDomain> _liveImageDataList;

        public ObservableCollection<LiveImageDataDomain> LiveImageDataList
        {
            get { return _liveImageDataList ?? (_liveImageDataList = new ObservableCollection<LiveImageDataDomain>()); }
            set
            {
                _liveImageDataList = value;
                NotifyPropertyChanged(nameof(LiveImageDataList));
            }
        }

        private bool _isSingleAnalyseActive = true;

        public bool IsSingleAnalyseActive
        {
            get { return _isSingleAnalyseActive; }
            set
            {
                _isSingleAnalyseActive = value;
                NotifyPropertyChanged(nameof(IsSingleAnalyseActive));
            }
        }


        public bool IsFlyOutChecked
        {
            get { return _isFlyOutChecked; }
            set
            {
                if (_isFlyOutChecked == value)
                    return;
                _isFlyOutChecked = value;
                NotifyPropertyChanged(nameof(IsFlyOutChecked));
                OnFlyOutOpen(value);
            }
        }

        private void OnFlyOutOpen(bool isChecked)
        {
            if (IsLiveImageActive)
                IsLiveImageResultFlyOutOpen = isChecked;
            else if (IsSingleImageAvailable)
                IsFlyOutOpen = isChecked;
        }

        private bool _isLiveImageResultFlyOutOpen;

        public bool IsLiveImageResultFlyOutOpen
        {
            get { return _isLiveImageResultFlyOutOpen; }
            set
            {
                if (_isLiveImageResultFlyOutOpen == value)
                    return;
                _isLiveImageResultFlyOutOpen = value;
                NotifyPropertyChanged(nameof(IsLiveImageResultFlyOutOpen));
                if (value)
                    return;
                IsFlyOutChecked = false;
                if (IsLiveImageFreeze)
                {
                    IsLiveImageFreeze = false;
                    OnStartLiveImage();
                }
            }
        }


        public bool IsFlyOutOpen
        {
            get { return _isFlyOutOpen; }
            set
            {
                if (_isFlyOutOpen == value)
                    return;
                _isFlyOutOpen = value;
                NotifyPropertyChanged(nameof(IsFlyOutOpen));
                if (!value)
                    IsFlyOutChecked = false;
            }
        }

        public bool IsSingleImageAvailable
        {
            get { return _isSingleImageAvailable; }
            set
            {
                _isSingleImageAvailable = value;
                NotifyPropertyChanged(nameof(IsSingleImageAvailable));
            }
        }

        public ICommand ImageCommand => _imageCommand ??
                                        (_imageCommand = new RelayCommand(SwitchToImage, null));

   
        public SampleImageRecordDomain SampleImageDomain
        {
            get { return _sampleImageDomain ?? (_sampleImageDomain = new SampleImageRecordDomain()); }
            set
            {
                _sampleImageDomain = value;
                NotifyPropertyChanged(nameof(SampleImageDomain));
            }
        }

        private List<KeyValuePair<int, string>> _liveImageList;

        public List<KeyValuePair<int, string>> LiveImageList
        {
            get
            {
                return _liveImageList ?? (_liveImageList =
                           new List<KeyValuePair<int, string>>(OpticsManualServiceModel.LiveImageList));
            }
            set
            {
                _liveImageList = value;
                NotifyPropertyChanged(nameof(LiveImageList));
            }
        }

        private KeyValuePair<int, string> _selectedOption;

        public KeyValuePair<int, string> SelectedOption
        {
            get { return _selectedOption; }
            set
            {
                _selectedOption = value;
                NotifyPropertyChanged(nameof(SelectedOption));
            }
        }

        public AdjustValue AdjustState
        {
            get { return _adjustState; }
            set
            {
                _adjustState = value;
                NotifyPropertyChanged(nameof(AdjustState));
            }
        }

        public ICommand OpticsCommand => _opticsCommand ??
                                         (_opticsCommand = new RelayCommand(OnOpticsFunction, null));


        public ICommand StopContShotCommand => _stopContShotCommand ??
                                               (_stopContShotCommand = new RelayCommand(OnStopContAnalyze, null));


        public OpticsManualServiceModel OpticsManualServiceModel
        {
            get { return _opticsManualServiceModel; }
            set
            {
                _opticsManualServiceModel = value;
                if (_opticsManualServiceModel != null)
                {
                    _opticsManualServiceModel.SetFocus.ServiceLiveImageOccurred += HandleLiveImageOccurred;
                    _opticsManualServiceModel.ServiceResultAnalysisOccurred += HandleServiceResultAnalysisOccurred;
                }
            }
        }

        public bool IsImageAvailable
        {
            get { return _isImageAvailable; }
            set
            {
                _isImageAvailable = value;
                NotifyPropertyChanged(nameof(IsImageAvailable));
            }
        }


        public bool IsLiveImageActive
        {
            get { return _isLiveImageActive; }
            set
            {
                if (_isLiveImageActive == value)
                    return;
                _isLiveImageActive = value;
                NotifyPropertyChanged(nameof(IsLiveImageActive));
                OnLiveImageActive(value);
            }
        }

        public bool IsFlowCellDepthActive
        {
            get { return _isFlowCellDepthActive; }
            set
            {
                _isFlowCellDepthActive = value;
                NotifyPropertyChanged(nameof(IsFlowCellDepthActive));
            }
        }

        public ICommand SetFlowCellDepthCommand => _flowCellDepthCommand ??
            (_flowCellDepthCommand = new RelayCommand(OnSetFlowCellDepth, null));

        public void OnSetFlowCellDepth()
        {   
            if(ValidateFlowCellDepth())
            {
                SetFlowCellDepth(_flowCellDepth);
            }
               
        }
     
     
        public bool ValidateFlowCellDepth()
        {

            if (_flowCellDepth > MaxFlowCellDepth || _flowCellDepth < MinFlowCellDepth)
            {
                var minFlowCellDepth = String.Format("{0:0.0000}", MinFlowCellDepth);
                var maxFlowCellDepth = String.Format("{0:0.0000}", MaxFlowCellDepth);

                var msg = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_FlowCellDepthRangeMsg"), minFlowCellDepth, maxFlowCellDepth);
                DialogEventBus.DialogBoxOk(this, msg);
                return false;
            }

            return true;
        }

     
        public double FlowCellDepth
        {
            get { return _flowCellDepth; }
            set
            {
                _flowCellDepth = value;                   
                NotifyPropertyChanged(nameof(FlowCellDepth));                                  
            }
        }

    
        public double BrightField
        {
            get { return _brightField; }
            set
            {
                _brightField = value;
                NotifyPropertyChanged(nameof(BrightField));
            }
        }

      
        public OpticsAction OpticsAction
        {
            get { return _opticsAction; }
            set
            {
                _opticsAction = value;
                NotifyPropertyChanged(nameof(OpticsAction));
            }
        }


        public double CurrentFocusPosition
        {
            get { return _currentFocusPosition; }
            set
            {
                _currentFocusPosition = value;
                NotifyPropertyChanged(nameof(CurrentFocusPosition));
            }
        }

        public ICommand SetFocusCommand => _setFocusCommand ??
                                           (_setFocusCommand = new RelayCommand(OnSetFocus, null));


        public ICommand DiagnosticsCommand => _diagnosticsCommand ??
                                              (_diagnosticsCommand = new RelayCommand(Diagnostics, null));

        public ICommand DustReferenceCommand => _dustReferenceCommand ??
                                                (_dustReferenceCommand = new RelayCommand(OpenDustReference, null));

        public ICommand RestoreFocusCommand => _restoreFocusCommand ??
                                               (_restoreFocusCommand =
                                                   new RelayCommand(RestoredDefaultCameraPosition, null));

     
        public bool IsBrightFieldDisabled
        {
            get { return _isBrightFieldDisabled; }
            set
            {
                _isBrightFieldDisabled = value;
                NotifyPropertyChanged(nameof(IsBrightFieldDisabled));
            }
        }

        public Action<OpticsAction, double> ValueUpdateAction { get; private set; }

        #endregion

        #region Private Method

      
        private void ValueUpdateCallBack(OpticsAction opticsAction, double obj)
        {
            OpticsAction = opticsAction;
            OnUpdateOpticsAdjust(obj);
        }

        private void OnLiveImageActive(bool value)
        {
            try
            {
                IsSingleImageAvailable = false;
                IsFlyOutChecked = false;
                if (value)
                {
                    IsLiveImageFreeze = false;
                    if (!IsContShotActive)
                        OnStopContAnalyze();
                    IsLiveImageOn = true;
                    OnStartLiveImage();
                }
                else
                {
                    StopLiveImage();
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_LIVE_IMAGE_ACTIVE"));
            }
        }

        private void OpenDustReference()
        {
            IsLiveImageActive = false;
            DialogEventBus.DustReferenceDialog(this, new DustReferenceEventArgs());
        }

        public void OnSwitchOffLiveImage()
        {
            IsLiveImageActive = false;
        }

        public void OnSetFocus()
        {
            IsLiveImageActive = false;
            DialogEventBus.SetFocusDialog(this, new SetFocusEventArgs());
            if (LoggedInUser.CurrentUserRoleId.Equals(UserPermissionLevel.eService))
            {
                GetCurrentFocusPos();
            }
        }

        private void OnStartLiveImage()
        {
            _isExpandedResultListVisible = false;
            _enableLiveImageListener = true;
            var liveImageStatus = OpticsManualServiceModel.SetFocus.StartLiveImageFeed();
            if (liveImageStatus.Equals(HawkeyeError.eSuccess))
            {
                IsImageAvailable = true;
            }
            else
            {
                _enableLiveImageListener = false;
                DisplayErrorDialogByApi(liveImageStatus);
            }
        }

      
        /// This event handler is called for images coming from ApiEventType.Service_Live_Image events.
        /// The API's StartLiveImage stimulates this event handler.
        private void HandleLiveImageOccurred(object sender, ApiEventArgs<HawkeyeError, ImageDto, List<LiveImageDataDomain>> e)
        {
            if (!_enableLiveImageListener)
            {
                return;
            }

            Log.Info("Live Image Status:- " + e.Arg1);
            if (e.Arg1.Equals(HawkeyeError.eSuccess))
            {
                if (!IsLiveImageFreeze && e.Arg2 != null)
                {
                    SampleImageDomain = new SampleImageRecordDomain
                    {
                        ImageSet = new ImageSetDto { BrightfieldImage = e.Arg2 }
                    };
                    LiveImageDataList = new ObservableCollection<LiveImageDataDomain>(e.Arg3);
                }
            }
            else
            {
                ApiHawkeyeMsgHelper.ErrorCommon(e.Arg1);
                StopLiveImage();
            }
        }


        public void StopLiveImage()
        {
            var loadStatus = OpticsManualServiceModel.SetFocus.StopLiveImageFeed();
            if (loadStatus.Equals(HawkeyeError.eSuccess))
            {
                SampleImageDomain = new SampleImageRecordDomain();
                IsImageAvailable = false;
            }
            else
                DisplayErrorDialogByApi(loadStatus);
        }

    
        public void OnOpticsFunction(object obj)
        {
            try
            {
                string sampleOpticsOption = (string) obj;
                if (sampleOpticsOption != null)
                {
                    switch (sampleOpticsOption)
                    {
                        case "Load":
                            LoadOpticsSample();
                            break;
                        case "Nudge":
                            NudgeOpticsSample();
                            break;
                        case "Expel":
                            ExpelOpticsSample();
                            break;
                        case "Analyze":
                            if (!IsContShotActive)
                                OnStopContAnalyze();
                            _isExpandedResultListVisible = true;
                            IsFlyOutChecked = false;
                            AnalyzeOpticsSample();
                            ResultHeader = LanguageResourceHelper.Get("LID_ButtonContent_Analyze_Results");
                            break;
                        case "ContAnalyze":
                            IsFlyOutChecked = false;
                            _isExpandedResultListVisible = true;
                            ContAnalyzeOpticsSample();
                            ResultHeader =
                                LanguageResourceHelper.Get("LID_ButtonContent_ContAnalyze_Results");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_OPTICS_FUNCTION"));
            }
        }


        public void SetManualControlOpticsValue()
        {
            try
            {
                FlowCellDepth = OpticsManualServiceModel.svc_GetFlowCellDepthSettingInMillimeters();
                GetCameraLamp();
                GetCurrentFocusPos();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_SET_MANUAL_CONTROL_OPTICS_VALUE"));
            }
        }

   
        private bool GetCameraLamp()
        {
            bool lampState;
            float intensity;
            var camLampStatus = OpticsManualServiceModel.svc_GetCameraLampState(out lampState, out intensity);
            if (camLampStatus.Equals(HawkeyeError.eSuccess))
                BrightField = intensity;
            else
                DisplayErrorDialogByApi(camLampStatus);

            return lampState;
        }

    
        public void GetCurrentFocusPos()
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                MainWindowViewModel.Instance.UpdateSystemStatus();
            });
            Log.Info("GetCurrentFocusPos:: DefinedFocusPosition: " + _instrumentStatusService.SystemStatusDom.DefinedFocusPosition);
            CurrentFocusPosition = _instrumentStatusService.SystemStatusDom.DefinedFocusPosition;
        }

    
        public void LoadOpticsSample()
        {
            var loadStatus = OpticsManualServiceModel.svc_ManualSampleLoad();
            if (loadStatus.Equals(HawkeyeError.eSuccess))
                PostToMessageHub(LanguageResourceHelper.Get("LID_ButtonContent_LoadSuccessful"));
            else
                ApiHawkeyeMsgHelper.ErrorCommon(loadStatus);
        }

   
        public void NudgeOpticsSample()
        {
            var nudgeStatus = OpticsManualServiceModel.svc_ManualSampleNudge();
            if (nudgeStatus.Equals(HawkeyeError.eSuccess))
                PostToMessageHub(LanguageResourceHelper.Get("LID_ButtonContent_NudgeSuccessful"));
            else
                ApiHawkeyeMsgHelper.ErrorCommon(nudgeStatus);
        }


        public void ExpelOpticsSample()
        {
            var expelStatus = OpticsManualServiceModel.svc_ManualSampleExpel();
            if (expelStatus.Equals(HawkeyeError.eSuccess))
                PostToMessageHub(LanguageResourceHelper.Get("LID_ButtonContent_ExpelSuccessful"));
            else
                ApiHawkeyeMsgHelper.ErrorCommon(expelStatus);
        }

    
        public void AnalyzeOpticsSample()
        {
            IsLiveImageOn = IsLiveImageActive = false;
            _enableSingleAnalysisListener = true;
            var singleShotStatus = OpticsManualServiceModel.svc_GenerateSingleShotAnalysis();
            if (singleShotStatus.Equals(HawkeyeError.eSuccess))
            {
                IsSingleAnalyseActive = IsImageAvailable = true;
                IsSingleImageAvailable = true;
            }
            else
            {
                _enableSingleAnalysisListener = false;
                DisplayLiveImageErrorByAPi(singleShotStatus);
            }
        }

        public void ContAnalyzeOpticsSample()
        {
            IsLiveImageOn = true;
            IsLiveImageActive = false;
            _enableContinuousAnalysisListener = true;
            var contShotStatus = OpticsManualServiceModel.svc_GenerateContinuousAnalysis();
            if (contShotStatus.Equals(HawkeyeError.eSuccess))
            {
                IsSingleImageAvailable = true;
                IsImageAvailable = true;
                IsContShotActive = false;
            }
            else
            {
                _enableContinuousAnalysisListener = false;
                DisplayLiveImageErrorByAPi(contShotStatus);
            }
        }

        private void OnDisplayNextBgResultCommand()
        {
            IsLiveImageFreeze = !IsLiveImageFreeze;
            if (IsLiveImageFreeze)
            {
                var loadStatus = OpticsManualServiceModel.SetFocus.StopLiveImageFeed();
                if (!loadStatus.Equals(HawkeyeError.eSuccess))
                    DisplayErrorDialogByApi(loadStatus);
            }
            else
            {
                OnStartLiveImage();
            }
        }

        /// This event handler is called for images coming from ApiEventType.Service_Analysis_Result events.
        /// The API's GenerateSingleShotAnalysis and GenerateContinuousShotAnalysis both stimulate this event handler.
        private void HandleServiceResultAnalysisOccurred(object sender,
            ApiEventArgs<HawkeyeError, BasicResultAnswers, ImageDto> args)
        {
            if (!_enableSingleAnalysisListener && !_enableContinuousAnalysisListener)
            {
                return;
            }

            _enableSingleAnalysisListener = false;
            Log.Info("Image Analysis Status:- " + args.Arg1);
            if (args.Arg1.Equals(HawkeyeError.eSuccess))
            {
                if (args.Arg3 != null)
                {
                    var imageResult = args.Arg2.MarshalToBasicResultDomain();
                    SampleImageDomain = new SampleImageRecordDomain
                    {
                        ImageSet = new ImageSetDto { BrightfieldImage = args.Arg3 },
                        ResultPerImage = imageResult
                    };
                }
            }
            else
                ApiHawkeyeMsgHelper.ErrorCommon(args.Arg1);

            IsSingleAnalyseActive = true;
        }

        private void OnExpandImage()
        {
            // the View doesn't have a Expand button to open this dialog. Leaving in for the time being -- JDT
            var args = new ExpandedImageGraphEventArgs(ImageType.Annotated, ApplicationConstants.ImageViewRightClickMenuImageFitSize, null, SampleImageDomain);
            args.IsExpandedResultListVisible = _isExpandedResultListVisible;
            args.IsHorizontalPaginationVisible = false;
            DialogEventBus.ExpandedImageGraph(this, args);
        }

        private void OnStopContAnalyze()
        {
            _enableContinuousAnalysisListener = false;
            var stopStatus = OpticsManualServiceModel.svc_StopContinuousAnalysis();
            if (stopStatus.Equals(HawkeyeError.eSuccess))
            {
                IsContShotActive = true;
            }
            else
                DisplayLiveImageErrorByAPi(stopStatus);
        }

     
        public void SwitchToImage(object param)
        {
            if (param == null)
                return;
            switch (param.ToString())
            {
                case "Final":
                    IsFinalImgEnable = false;
                    break;
                case "ImgList":
                    IsFinalImgEnable = true;
                    break;
            }
        }

   
        public void OnUpdateOpticsAdjust(double newValue)
        {
            try
            {
                switch (AdjustState)
                {
                    case AdjustValue.Idle:
                        break;
                    case AdjustValue.Left:
                        OnPerformLeftRight(newValue, false, true);
                        break;
                    case AdjustValue.Right:
                        OnPerformLeftRight(newValue, true, true);
                        break;
                    case AdjustValue.LeftForward:
                        OnPerformLeftRightForward(newValue,false,false);
                        break;
                    case AdjustValue.RightForward:
                        OnPerformLeftRightForward(newValue, true, false);
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_UPDATE_OPTICS_ADJUST"));
            }
        }

        public void SetLoadingIndicator(bool status)
        {
            MessageBus.Default.Publish(new Notification<bool>(status, MessageToken.AdornerVisible));
        }

      
        private void OnPerformLeftRight(double newValue, bool up, bool fineAdjustment)
        {
            switch (OpticsAction)
            {
                case OpticsAction.FocusPosition:
                    SetCurrentFocusPosition(up, fineAdjustment);
                    break;
                case OpticsAction.BrightField:
                    SetCameraLampState(newValue);
                    break;
            }
        }

   
        private void OnPerformLeftRightForward(double newValue, bool up, bool fineAdjustment)
        {
            switch (OpticsAction)
            {
                case OpticsAction.FocusPosition:
                    SetCurrentFocusPosition(up, fineAdjustment);
                    break;
                case OpticsAction.BrightField:
                    SetCameraLampState(newValue);
                    break;
            }
        }

        public DispatcherTimer Timer;


        private void SetCurrentFocusPosition(bool up, bool fineAdjustment)
        {
            try
            {
                var thread = new Thread(()=> SetFocus(up,fineAdjustment));
                var responseTimeOutSeconds = UISettings.ResponseTimeOutSeconds;
                SetLoadingIndicator(true);
                Timer = new DispatcherTimer(new TimeSpan(0, 0, responseTimeOutSeconds), DispatcherPriority.Normal,
                    delegate
                    {
                        if (!_response)
                        {
                            Timer.Stop();
                            SetLoadingIndicator(false);
                            DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_ResponseTimeOut"));
                            thread.Abort();
                        }
                    }, Application.Current.Dispatcher);
                Timer.Start();
                thread.Start();
            }
            catch (Exception ex)
            {
                SetLoadingIndicator(false);
                SetDefaultValues();
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_RESTORED_DEFAULT_CAMERA_POSITION"));
            }
        }

        private void SetFocus(bool up, bool fineAdjustment)
        {
            var cameraFocusAdjustStatus = OpticsManualServiceModel.svc_CameraFocusAdjust(up, fineAdjustment);
            _response = true;
            SetDefaultValues();
            if (cameraFocusAdjustStatus.Equals(HawkeyeError.eSuccess))
            {
                GetCurrentFocusPos();
                PostToMessageHub(LanguageResourceHelper.Get("LID_Label_CurFocusSuccessful"));
            }
            else
                ApiHawkeyeMsgHelper.ErrorCommon(cameraFocusAdjustStatus);

            SetLoadingIndicator(false);
        }

        private void SetDefaultValues()
        {
            Timer.Stop();
            _response = false;
        }

        private async void SetFlowCellDepth(double flowCell)
        {
            try
            {
                var flowCellStatus = await Task.Run(() => OpticsManualServiceModel.svc_SetFlowCellDepthSetting(flowCell));
                if (flowCellStatus.Equals(HawkeyeError.eSuccess))
                {
                    FlowCellDepth = OpticsManualServiceModel.svc_GetFlowCellDepthSettingInMillimeters();
                    PostToMessageHub(LanguageResourceHelper.Get("LID_MSGBOX_FlowCellDepthSuccessful"));
                }
                else
                {
                    ApiHawkeyeMsgHelper.ErrorInvalid(flowCellStatus);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_SET_FLOWCELL_DEPTH"));
            }            
        }

        private async void SetCameraLampState(double newValue)
        {
            try
            {
                IsBrightFieldDisabled = true;
                if (await Task.Run(() => IsSetCameraLampStateSucceed(IsLampOn, newValue)))
                {
                    var cameraIntensityStatus = await Task.Run(() => OpticsManualServiceModel.svc_SetCameraLampIntensity((float)newValue));
                    if (cameraIntensityStatus.Equals(HawkeyeError.eSuccess))
                    {
                        GetCameraLamp();
                        PostToMessageHub(LanguageResourceHelper.Get("LID_Label_BrightfieldSuccessfulBrightfield"));
                    }
                    else
                        DisplayErrorDialogByApi(cameraIntensityStatus);
                }

                IsBrightFieldDisabled = false;
            }
            catch (Exception ex)
            {
               ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_SET_CAMERA_LAMP_STATE"));
            }            
        }

        private bool IsSetCameraLampStateSucceed(bool lampState, double intensityValue)
        {
            var cameraStateStatus = OpticsManualServiceModel.svc_SetCameraLampState(lampState, (float)intensityValue);
            if (cameraStateStatus.Equals(HawkeyeError.eSuccess))
                return true;

            DisplayErrorDialogByApi(cameraStateStatus);
            return false;
        }

     
        private void RestoredDefaultCameraPosition()
        {
            try
            {
                var thread = new Thread(svc_CameraFocusRestoreToSavedLocation);
                var responseTimeOutSeconds = UISettings.ResponseTimeOutSeconds;
                SetLoadingIndicator(true);
                Timer = new DispatcherTimer(new TimeSpan(0, 0, responseTimeOutSeconds), DispatcherPriority.Normal, delegate
                {
                    if (!_response)
                    {
                        Timer.Stop();
                        SetLoadingIndicator(false);
                        DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_ResponseTimeOut"));
                        thread.Abort();
                    }
                }, Application.Current.Dispatcher);
                Timer.Start();
                thread.Start();
            }
            catch (Exception ex)
            {
                SetLoadingIndicator(false);
                SetDefaultValues();
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_RESTORED_DEFAULT_CAMERA_POSITION"));
            }
        }

        private void svc_CameraFocusRestoreToSavedLocation()
        {
            var hawkeyeError = OpticsManualServiceModel.svc_CameraFocusRestoreToSavedLocation();
            _response = true;
            SetDefaultValues();
            if (hawkeyeError.Equals(HawkeyeError.eSuccess))
            {
                GetCurrentFocusPos();
                PostToMessageHub(LanguageResourceHelper.Get("LID_ButtonContent_RestoreFocusSuccessful"));
            }
            else
                ApiHawkeyeMsgHelper.ErrorCommon(hawkeyeError);

            SetLoadingIndicator(false);

        }

        public void Diagnostics()
        {
            try
            {
                IsLiveImageActive = false;
                var args = new SelectCellTypeEventArgs(LanguageResourceHelper.Get("LID_POPUPHeader_SelectTempCellType"), true);
                var result = DialogEventBus.SelectCellTypeDialog(this, args);

                if (result == true && args.SelectedCellTypeDomain is CellTypeDomain selectedCellType)
                {
                    // todo: move this to the Model (this could be hard due to handling OpticsManualServiceModel events)
                    try
                    {
                        var setCellTypeStatus = OpticsManualServiceModel.svc_SetTemporaryCellTypeFromExisting(selectedCellType.CellTypeIndex);
                        if (setCellTypeStatus.Equals(HawkeyeError.eSuccess))
                        {
                            var setAnalysisStatus = OpticsManualServiceModel.svc_SetTemporaryAnalysisDefinition(selectedCellType.AnalysisDomain);
                            if (setAnalysisStatus.Equals(HawkeyeError.eSuccess))
                            {
                                PostToMessageHub(LanguageResourceHelper.Get("LID_MSGBOX_SetTempCellType"));
                                Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_SetTempCellType"));
                            }
                            else
                            {
                                ApiHawkeyeMsgHelper.ErrorCommon(setAnalysisStatus);
                            }
                        }
                        else
                        {
                            ApiHawkeyeMsgHelper.ErrorCommon(setCellTypeStatus);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_SET_TEMP_CELL_TYPE"));
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_DIAGNOSTICS"));
            }
        }

        #endregion
    }
}
