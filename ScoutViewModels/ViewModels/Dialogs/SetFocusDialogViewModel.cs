using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutDomains;
using ScoutDomains.Common;
using ScoutDomains.DataTransferObjects;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Service;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Structs;
using ScoutUtilities.UIConfiguration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using ScoutModels.Interfaces;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class SetFocusDialogViewModel : BaseDialogViewModel
    {
        #region Constructor

        public SetFocusDialogViewModel(IInstrumentStatusService instrumentStatusService, SetFocusEventArgs args, Window parentWindow) : base(args, parentWindow)
        {
            ShowDialogTitleBar = true;
            DialogTitle = LanguageResourceHelper.Get("LID_POPUPHeader_SetFocus");

            _totalSetFocusTime = 0;
            _setFocusModel = new SetFocusModel();
            _setFocusTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, UISettings.SetFocusTimeOutSec) };
            _instrumentStatusService = instrumentStatusService;
            SampleImageDomain = new SampleImageRecordDomain();
            BarGraphList = new ObservableCollection<BarGraphDomain>();
            BarGraph = new BarGraphDomain();

            IsStepOneEnable = true;
            ImageViewType = ApplicationConstants.ImageViewRightClickMenuImageFitSize;

            SetPreviousPosition();

            _setFocusTimer.Tick += OnSetFocusTimer;
            _setFocusModel.AutoFocusTimerTick += HandleAutofocusTimerTick;
            _setFocusModel.AutoFocusStateChanged += HandleAutofocusStateChanged;
            _setFocusModel.ServiceLiveImageOccurred += HandleServiceLiveImageOccurred;

            _setFocusStatus = CallBackProgressStatus.IsStart;
            IsAcceptEnabled = false;
            _userCancelled = false;
        }

        protected override void DisposeUnmanaged()
        {
            if (_setFocusTimer != null)
            {
                _setFocusTimer.Stop();
                _setFocusTimer.Tick -= OnSetFocusTimer;

                _setFocusModel.AutoFocusStateChanged -= HandleAutofocusStateChanged;
                _setFocusModel.AutoFocusTimerTick -= HandleAutofocusTimerTick;
                _setFocusModel.ServiceLiveImageOccurred -= HandleServiceLiveImageOccurred;
                _setFocusModel.Dispose();
            }
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties & Fields

        private SetFocusModel _setFocusModel;
        private DispatcherTimer _setFocusTimer;
        private CallBackProgressStatus _setFocusStatus;
        private readonly IInstrumentStatusService _instrumentStatusService;

        private bool _enableAutoFocusListener;
        private bool _enableLiveImageListener;

        private bool _isCancelRequestAccepted;
        private bool _userCancelled = false;

        private int _totalSetFocusTime;
        private int _setFocusTimeRemaining;

        public bool IsAcceptEnabled
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                DispatcherHelper.ApplicationExecute(() =>
                {
                    SetFocusStepThreeCommand.RaiseCanExecuteChanged();
                    CancelSetFocus.RaiseCanExecuteChanged();
                });
            }
        }

        public bool IsCompleteAvailable
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                DispatcherHelper.ApplicationExecute(() => { AcceptCommand.RaiseCanExecuteChanged(); });
            }
        }

        public bool IsStepOneEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsStepTwoEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsSkipAvailable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsLiveImageOn
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsImageAvailable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsImageEnable
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                DispatcherHelper.ApplicationExecute(() => { ImageCommand.RaiseCanExecuteChanged(); });

            }
        }

        public bool IsImageActive
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                DispatcherHelper.ApplicationExecute(() => { ImageCommand.RaiseCanExecuteChanged(); });
            }
        }

        public bool IsGraphEnable
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                DispatcherHelper.ApplicationExecute(() => { GraphCommand.RaiseCanExecuteChanged(); });

            }
        }

        public bool IsGraphActive
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                DispatcherHelper.ApplicationExecute(() => { GraphCommand.RaiseCanExecuteChanged(); });
            }
        }

        public eAutofocusState AutoFocusStatus
        {
            get { return GetProperty<eAutofocusState>(); }
            set
            {
                SetProperty(value);

                IsStatusChecked1 |= value.Equals(eAutofocusState.af_PreparingSample);
                IsStatusChecked2 |= value.Equals(eAutofocusState.af_SampleToFlowCell);
                IsStatusChecked3 |= value.Equals(eAutofocusState.af_SampleSettlingDelay);
                IsStatusChecked4 |= value.Equals(eAutofocusState.af_AcquiringFocusData);
                IsStatusChecked5 |= value.Equals(eAutofocusState.af_WaitingForFocusAcceptance);
                IsStatusChecked6 |= value.Equals(eAutofocusState.af_FlushingSample);
                IsStatusChecked7 |= value.Equals(eAutofocusState.af_Idle);
            }
        }

        public bool IsStatusChecked1
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsStatusChecked2
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsStatusChecked3
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsStatusChecked4
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsStatusChecked5
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsStatusChecked6
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsStatusChecked7
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<BarGraphDomain> BarGraphList
        {
            get { return GetProperty<ObservableCollection<BarGraphDomain>>(); }
            set { SetProperty(value, NotifyType.Force); }
        }

        public BarGraphDomain BarGraph
        {
            get { return GetProperty<BarGraphDomain>(); }
            set { SetProperty(value, NotifyType.Force); }
        }

        public SampleImageRecordDomain SampleImageDomain
        {
            get { return GetProperty<SampleImageRecordDomain>(); }
            set { SetProperty(value, NotifyType.Force); }
        }

        public string ImageViewType
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string IntervalTime
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string NewFocusAcceptanceState
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public int PreviousPosition
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public SetFocusDomain SetFocusProperties
        {
            get { return _setFocusModel.SetFocusProperties; }
            set
            {
                _setFocusModel.SetFocusProperties = value;
                NotifyPropertyChanged(nameof(SetFocusProperties));
            }
        }

        #endregion

        #region Event Handlers

        private void OnSetFocusTimer(object sender, EventArgs e)
        {
            //_cancelTokenSource?.Cancel();

            Log.Debug(LanguageResourceHelper.Get("LID_MSGBOX_CancelAutoFocusError"));
            var result = DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_CancelAutoFocusError"));
            if (result != true) return;

            _setFocusTimer?.Stop();
            Close(null);
        }

        private void HandleAutofocusStateChanged(object sender, ApiEventArgs<eAutofocusState, AutofocusResultsDto> e)
        {
            Log.Debug($"SetFocusDVM::HandleAutofocusStateChanged: '{e.Arg1}'{Environment.NewLine}" +
                      $"_enableAutoFocusListener = '{_enableAutoFocusListener}'");
            
            if (!_enableAutoFocusListener) return;

            AutoFocusState(e.Arg1, e.Arg2);
        }

        private void HandleAutofocusTimerTick(object sender, ApiEventArgs<uint> e)
        {
            if (!_enableAutoFocusListener) return;

            try
            {
                _setFocusTimeRemaining = (int)e.Arg1;

                if (_totalSetFocusTime < 1) _totalSetFocusTime = _setFocusTimeRemaining;

                IntervalTime = $"{GetStringFromSeconds(_setFocusTimeRemaining)} {LanguageResourceHelper.Get("LID_QMgmtHEADER_WaitMins2")}";
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_HANDLE_AUTO_FOCUS_TIMER_TICK"));
            }
        }

        private void HandleServiceLiveImageOccurred(object sender, ApiEventArgs<HawkeyeError, ImageDto, List<LiveImageDataDomain>> e)
        {
            if (!_enableLiveImageListener) return;

            try
            {
                Log.Info("Live Image Status:- " + e.Arg1);
                if (e.Arg1 != HawkeyeError.eSuccess)
                {
                    ApiHawkeyeMsgHelper.ErrorCommon(e.Arg1);
                    CancelAutoFocus();
                    return;
                }

                if (e.Arg2 == null)
                {
                    Log.Info("Live Image Data:- " + e.Arg2);
                    CancelAutoFocus();
                }
                else
                {
                    SampleImageDomain = new SampleImageRecordDomain
                    {
                        ImageSet = new ImageSetDto { BrightfieldImage = e.Arg2 }
                    };
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_HANDLE_SERVICE_LIVE_IMAGE_OCCURRED"));
            }
        }

        #endregion

        #region Commands

        #region Accept Command

        public override bool CanAccept()
        {
            return IsCompleteAvailable;
        }

        #endregion

        #region Cancel Command

        protected override void OnCancel()
        {
            Log.Debug($"OnCancel()");

            if (_setFocusStatus.Equals(CallBackProgressStatus.IsRunning) && !_isCancelRequestAccepted)
            {
                // The 'Yes' touch press was right over the ManualControlOpticsViewModel 'Resume' button and the 'Resume' was getting
                // the touch event after this dialog was gone (see PC3549-4975). This change (location of UpAndLeftOfCenter) moves the
                // 'Yes' and 'No' buttons over an area with no active parts of the screen for the ManualControlOpticsViewModel
                if (DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_MSGBOX_SetFocusWarning"), null, DialogLocation.UpAndLeftOfCenter) != true)
                {
                    return;
                }

                _userCancelled = true;
                try
                {
                    CancelAutoFocus();
                }
                catch (Exception e)
                {
                    ExceptionHelper.HandleExceptions(e, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CANCEL_SET_FOCUS"));
                }
            }
            else if (_setFocusStatus.Equals(CallBackProgressStatus.IsCanceled))
            {
                base.OnCancel();
            }
            else if (_setFocusStatus.Equals(CallBackProgressStatus.IsFinish))
            {
                base.OnCancel();
            }
            else
            {
                _userCancelled = true;
            }
        }

        #endregion

        #region Image Expand

        private RelayCommand _imageExpandCommand;
        public RelayCommand ImageExpandCommand => _imageExpandCommand ?? (_imageExpandCommand = new RelayCommand(OnExpandImage));

        private void OnExpandImage()
        {
            var args = new ExpandedImageGraphEventArgs(ImageType.Annotated, ImageViewType, 
                new List<object> { SampleImageDomain }, SampleImageDomain, 0, false);
            args.IsHorizontalPaginationVisible = false;

            DialogEventBus.ExpandedImageGraph(this, args);

			//TODO: what does this code do???  (5652 pc3549 set focus large scale image does not expand)
            var newIndex = args.SelectedImageIndex + 1;
            ImageViewType = newIndex.ToString();
        }

        #endregion

        #region Graph Expand

        private RelayCommand _graphExpandCommand;
        public RelayCommand GraphExpandCommand => _graphExpandCommand ?? (_graphExpandCommand = new RelayCommand(OnExpandGraph));

        private void OnExpandGraph()
        {
            var args = new ExpandedImageGraphEventArgs(ExpandedContentType.BarChart, ImageViewType, BarGraphList?.Cast<object>()?.ToList(), BarGraph,
                BarGraphList?.IndexOf(BarGraph) ?? -1);
            args.IsHorizontalPaginationVisible = false;
            args.IsSetFocusEnable = true;
            args.IsListAvailable = false;

            DialogEventBus.ExpandedImageGraph(this, args);
        }

        #endregion

        #region Skip Delay

        private RelayCommand _skipDelayCommand;
        public RelayCommand SkipDelayCommand => _skipDelayCommand ?? (_skipDelayCommand = new RelayCommand(OnSkipDelay));

        private void OnSkipDelay()
        {
            try
            {
                var skipStatus = _setFocusModel.svc_CameraAutoFocus_ServiceSkipDelay();
                if (skipStatus.Equals(HawkeyeError.eSuccess))
                {
                    IsSkipAvailable = false;
                }
                else
                {
                    ApiHawkeyeMsgHelper.ErrorCommon(skipStatus);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_SKIP_DELAY"));
            }
        }

        #endregion

        #region Set Focus to Step 2

        private RelayCommand _setFocusStepTwoCommand;
        public RelayCommand SetFocusStepTwoCommand => _setFocusStepTwoCommand ?? (_setFocusStepTwoCommand = new RelayCommand(SetFocusStepTwo));

        private void SetFocusStepTwo()
        {
            try
            {
                var samplePosition = new SamplePosition('Z', 1);
                _enableAutoFocusListener = true;
                Log.Debug($"SetFocusDVM::SetFocusStepTwo()::Setting _enableAutoFocusListener to TRUE");
                var hawkeyeError = _setFocusModel.svc_CameraAutoFocus(samplePosition);
                _setFocusStatus = CallBackProgressStatus.IsStart;
                Log.Debug($"SetFocusDVM::SetFocusStepTwo()::Setting _setFocusStatus = CallBackProgressStatus.IsStart");

                if (hawkeyeError.Equals(HawkeyeError.eSuccess))
                {
                    _setFocusStatus = CallBackProgressStatus.IsRunning;
                    Log.Debug($"SetFocusDVM::SetFocusStepTwo()::Setting _setFocusStatus = CallBackProgressStatus.IsStart");
                    IsStepOneEnable = false;
                    IsStepTwoEnable = true;
                }
                else
                {
                    ApiHawkeyeMsgHelper.ErrorCommon(hawkeyeError);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_SET_FOCUS_STEP_TWO"));
            }
        }

        #endregion

        #region Set Focus to Step 3

        private RelayCommand _setFocusStepThreeCommand;
        public RelayCommand SetFocusStepThreeCommand => _setFocusStepThreeCommand ?? (_setFocusStepThreeCommand = new RelayCommand(OnAcceptSetFocus, CanAcceptSetFocus));

        private bool CanAcceptSetFocus()
        {
            return IsAcceptEnabled;
        }

        private void OnAcceptSetFocus()
        {
            try
            {
                var setFocusAccept = _setFocusModel.svc_CameraAutoFocus_FocusAcceptance(eAutofocusCompletion.afc_Accept);
                if (setFocusAccept.Equals(HawkeyeError.eSuccess))
                {
                    PostToMessageHub(LanguageResourceHelper.Get("LID_Icon_AcceptSetFocusSuccessful"));
                    IsAcceptEnabled = false;
                    _setFocusStatus = CallBackProgressStatus.IsFinish;
                    NewFocusAcceptanceState = LanguageResourceHelper.Get("LID_Label_Accepted");
                }
                else
                {
                    ApiHawkeyeMsgHelper.ErrorCommon(setFocusAccept);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_ACCEPT_SET_FOCUS"));
            }
        }

        #endregion

        #region Cancel Set Focus

        private RelayCommand _cancelSetFocus;
        public RelayCommand CancelSetFocus => _cancelSetFocus ?? (_cancelSetFocus = new RelayCommand(OnCancelSetFocus, CanCancelSetFocus));

        private bool CanCancelSetFocus()
        {
            return IsAcceptEnabled;
        }

        private void OnCancelSetFocus()
        {
            try
            {
                _userCancelled = true;
                var setFocusCancel = _setFocusModel.svc_CameraAutoFocus_FocusAcceptance(eAutofocusCompletion.afc_Cancel);
                if (setFocusCancel.Equals(HawkeyeError.eSuccess))
                {
                    PostToMessageHub(LanguageResourceHelper.Get("LID_Icon_CancelSetFocusSuccessful"));
                    IsAcceptEnabled = false;
                    NewFocusAcceptanceState = LanguageResourceHelper.Get("LID_Label_Rejected");
                }
                else
                {
                    ApiHawkeyeMsgHelper.ErrorCommon(setFocusCancel);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CANCEL_SET_FOCUS"));
            }
        }

        #endregion

        #region Image Command

        private RelayCommand _imageCommand;
        public RelayCommand ImageCommand => _imageCommand ?? (_imageCommand = new RelayCommand(SwitchToImage, CanSwitchToImage));

        private bool CanSwitchToImage()
        {
            return IsImageEnable;
        }

        private void SwitchToImage()
        {
            IsGraphActive = false;
            IsImageActive = true;
        }

        #endregion

        #region Graph Command

        private RelayCommand _graphCommand;
        public RelayCommand GraphCommand => _graphCommand ?? (_graphCommand = new RelayCommand(SwitchToGraph, CanSwitchToGraph));

        private bool CanSwitchToGraph()
        {
            return IsGraphEnable;
        }

        private void SwitchToGraph()
        {
            IsGraphActive = true;
            IsImageActive = false;
        }

        #endregion

        #endregion

        #region Private Methods

        private void SetPreviousPosition()
        {
            MainWindowViewModel.Instance.UpdateSystemStatus();
            PreviousPosition = _instrumentStatusService.SystemStatusDom.DefinedFocusPosition;
        }

        private void AutoFocusState(eAutofocusState status, AutofocusResultsDto results)
        {
            try
            {
                AutoFocusStatus = status;
                switch (status)
                {
                    case eAutofocusState.af_WaitingForSample:
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_WaitingForSample");
                        break;

                    case eAutofocusState.af_PreparingSample:
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_PreparingSample - Start");
                        NotifyPropertyChanged(nameof(IsStatusChecked1));
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_PreparingSample - Complete");
                        break;

                    case eAutofocusState.af_SampleToFlowCell:
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_SampleToFlowCell - Start");
                        NotifyPropertyChanged(nameof(IsStatusChecked2));
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_SampleToFlowCell - Complete");
                        break;

                    case eAutofocusState.af_SampleSettlingDelay:
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_SampleSettlingDelay - Start");
                        NotifyPropertyChanged(nameof(IsStatusChecked3));
                        StartLiveImageFeed();
                        IsSkipAvailable = LoggedInUser.CurrentUserRoleId.Equals(UserPermissionLevel.eService);
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_SampleSettlingDelay - Complete");
                        break;

                    case eAutofocusState.af_AcquiringFocusData:
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_AcquiringFocusData - Start");
                        NotifyPropertyChanged(nameof(IsStatusChecked4));
                        var elapsedTime = _totalSetFocusTime - _setFocusTimeRemaining;

                        if (_totalSetFocusTime == elapsedTime || _setFocusTimeRemaining <= 0)
                        {
                            Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_AcquiringFocusData - IF - 1");
                            IntervalTime = string.Format(LanguageResourceHelper.Get("LID_Label_CompletedInMins"), GetStringFromSeconds(_totalSetFocusTime));
                        }
                        else
                        {
                            Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_AcquiringFocusData - IF - 2");
                            IntervalTime = string.Format(LanguageResourceHelper.Get("LID_Label_CompletedOutOfMins"),
                                GetStringFromSeconds(elapsedTime),
                                (_totalSetFocusTime + 59) / 60);    // round up to the next min
                        }

                        StopLiveImage();
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_AcquiringFocusData - Complete");
                        break;

                    case eAutofocusState.af_WaitingForFocusAcceptance:
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_WaitingForFocusAcceptance - Start");
                        NotifyPropertyChanged(nameof(IsStatusChecked5));
                        if (results != null)
                        {
                            Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_WaitingForFocusAcceptance - if - results != null");
                            IsImageActive = false;
                            IsGraphEnable = IsImageEnable = IsGraphActive = true;
                            SetAutoFocusResult(results);
                        }

                        IsAcceptEnabled = true;
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_WaitingForFocusAcceptance - Complete");
                        break;

                    case eAutofocusState.af_FlushingSample:
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_FlushingSample - Start");
                        NotifyPropertyChanged(nameof(IsStatusChecked6));
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_FlushingSample - Complete");
                        break;

                    case eAutofocusState.af_Cancelling:
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_Cancelling - Start");
                        _setFocusStatus = CallBackProgressStatus.IsCanceling;
                        _setFocusTimer.Start();
                        IsSkipAvailable = false;
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_Cancelling - Complete");
                        break;

                    case eAutofocusState.af_Idle:
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_Idle - Start status: " + _setFocusStatus.ToString());

                        if (_setFocusStatus.Equals(CallBackProgressStatus.IsStart))
                        {
                            Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_Idle - _setFocusStatus = IsStart");
                            OnFaultError();
                            return;
                        }

                        if (_setFocusStatus.Equals(CallBackProgressStatus.IsError))
                        {
                            Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_Idle - _setFocusStatus = IsError");
                            OnFaultError();
                            return;
                        }

                        if (_setFocusStatus.Equals(CallBackProgressStatus.IsRunning) && !IsAcceptEnabled)
                        {
                            Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_Idle - _setFocusStatus = IsRunning AND !IsAcceptEnabled");
                            OnFaultError();
                            return;
                        }

                        if (_setFocusStatus.Equals(CallBackProgressStatus.IsCanceling) || _isCancelRequestAccepted || _userCancelled)
                        {
                            Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_Idle - IF{Environment.NewLine}" +
                                      $"_setFocusStatus.Equals(CallBackProgressStatus.IsCanceling) = {_setFocusStatus.Equals(CallBackProgressStatus.IsCanceling)}{Environment.NewLine}" +
                                      $"_isCancelRequestAccepted = {_isCancelRequestAccepted}" + $"   _userCancelled = {_userCancelled}");
                            _setFocusStatus = CallBackProgressStatus.IsCanceled;
                            _setFocusTimer?.Stop();

                            if (_isCancelRequestAccepted)
                            {
                                Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_Idle - _isCancelRequestAccepted - Close");
                                Close(null);
                            }
                            else if (_userCancelled)
                            {
                                Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_Idle - _userCancelled - Close");
                                Close(null);
                            }
                            else
                            {
                                Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_Idle - OnFaultError - non-user commanded cancel");
                                OnFaultError();
                            }
                            return;
                        }

                        NotifyPropertyChanged(nameof(IsStatusChecked7));
                        _setFocusStatus = CallBackProgressStatus.IsFinish;
                        IsStepOneEnable = false;

                        IsCompleteAvailable = true;
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_Idle - Complete");
                        break;

                    case eAutofocusState.af_Failed:
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_Failed - Start");
                        IsSkipAvailable = false;
                        _setFocusStatus = CallBackProgressStatus.IsError;
                        _enableAutoFocusListener = false;
                        _setFocusTimer?.Stop();
                        OnFaultError();
                        Log.Debug($"SetFocusDVM::AutoFocusState()::case eAutofocusState.af_Failed - Complete");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Debug($"SetFocusDVM::AutoFocusState()::Exception during {status}", ex);
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_AUTO_FOCUS_STATE"));
            }
            finally
            {
                // Update the reagent status
                var msg = new Notification<ReagentContainerStateDomain>(
                    ReagentModel.GetReagentContainerStatusAll().FirstOrDefault(),
                    MessageToken.RefreshReagentStatus, "");
                MessageBus.Default.Publish(msg);
            }
        }

        private string GetStringFromSeconds(int seconds)
        {
            var sec = seconds % 60;
            var min = seconds / 60;
            return $"{min:00}:{sec:00}";
        }

        private void CancelAutoFocus()
        {
            Log.Debug($"CancelAutoFocus() - Start");
            _userCancelled = true;
            
            if (IsLiveImageOn)
            {
                StopLiveImage();
            }

            var setFocusCancel = _setFocusModel.svc_CameraAutoFocus_Cancel();
            if (!setFocusCancel.Equals(HawkeyeError.eSuccess))
            {
                ApiHawkeyeMsgHelper.ErrorCommon(setFocusCancel);
            }
            else
            {
                _isCancelRequestAccepted = true;
                Log.Debug($"CancelAutoFocus()::Setting _isCancelRequestAccepted to TRUE");
            }
            Log.Debug($"CancelAutoFocus() - Complete");
        }

        private void StartLiveImageFeed()
        {
            _enableLiveImageListener = true;
            var liveImageStatus = _setFocusModel.StartLiveImageFeed();
            if (liveImageStatus.Equals(HawkeyeError.eSuccess))
            {
                IsImageAvailable = false;
                IsImageEnable = IsImageActive = IsLiveImageOn = true;
            }
            else
            {
                _enableLiveImageListener = false;
                ApiHawkeyeMsgHelper.ErrorCommon(liveImageStatus);
            }
        }

        private void StopLiveImage()
        {
            try
            {
                if (!IsLiveImageOn) return;

                var stopStatus = _setFocusModel.StopLiveImageFeed();
                ApiHawkeyeMsgHelper.ErrorCommon(stopStatus);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_STOP_LIVE_IMAGE"));
            }
        }

        private void SetAutoFocusResult(AutofocusResultsDto autoFocusResults)
        {
            SetFocusProperties = GetSetFocusDomain(autoFocusResults);
            SetAutoFocusDataPoints(autoFocusResults, autoFocusResults.Dataset);
            IsLiveImageOn = false;
            SampleImageDomain = new SampleImageRecordDomain
            {
                ImageSet = new ImageSetDto { BrightfieldImage = autoFocusResults.BestAutofocusImage }
            };
        }

        private SetFocusDomain GetSetFocusDomain(AutofocusResultsDto autoFocusResults)
        {
            var setFocusProperties = new SetFocusDomain { Position = autoFocusResults.FinalAutofocusPosition };
            if (autoFocusResults.Dataset != null && autoFocusResults.Dataset.Any())
            {
                setFocusProperties.InFocusCount = (int)autoFocusResults.Dataset.Max(x => x.focalvalue);
            }
            return setFocusProperties;
        }

        private void SetAutoFocusDataPoints(AutofocusResultsDto autoFocusResults, List<AutofocusDatapoint> dataPointList)
        {
            _setFocusModel.LogFocusResults(autoFocusResults, dataPointList);
            var barGraph = _setFocusModel.GraphHelper.GetAutoFocusGraphData(dataPointList);
            var sortedList = barGraph.PrimaryGraphDetailList.Where(x => x.Value > (long)0);
            barGraph.PrimaryGraphDetailList = new List<KeyValuePair<dynamic, double>>(sortedList);
            BarGraphList = new ObservableCollection<BarGraphDomain> { barGraph };
            BarGraph = barGraph;
        }

        private void OnFaultError()
        {
            try
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ButtonContent_SetFocusFailed"));
                Close(null);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_FAULT_ERROR"));
            }
        }

        #endregion
    }
}