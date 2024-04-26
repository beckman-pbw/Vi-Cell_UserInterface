using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutDomains;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ScoutModels.Interfaces;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class ReplaceReagentPackDialogViewModel : BaseDialogViewModel
    {
        #region Constructor

        public ReplaceReagentPackDialogViewModel(IInstrumentStatusService instrumentStatusService, ReplaceReagentPackEventArgs args, System.Windows.Window parentWindow) : base(args, parentWindow)
        {
            ShowDialogTitleBar = true;
            DialogTitle = LanguageResourceHelper.Get("LID_POPUPHeader_ReplaceReagentPack");

            IsStepOneEnable = true;
            IsProgressActivate = true;
            IsServiceEnable = LoggedInUser.CurrentUserRoleId.Equals(UserPermissionLevel.eService);
            _progressStatus = CallBackProgressStatus.IsFinish;
            _instrumentStatusService = instrumentStatusService;
            ReagentContainers = new ObservableCollection<ReagentContainerStateDomain>();

            _reagentModel = new ReagentModel();
            _reagentModel.PartNumber = args.ReagentPartNumber;
            _reagentModel.PropertyChanged += HandleReagentModelPropertyChanges;
            _reagentModel.LoadStatusChanged += HandleLoadStatusChanged;
            _reagentModel.LoadCompleted += HandleLoadCompleted;
            _reagentModel.UnloadStatusChanged += HandleUnloadStatusChanged;
            _reagentModel.UnloadCompleted += HandleUnloadCompleted;
        }

        protected override void DisposeUnmanaged()
        {
            if (_reagentModel != null)
            {
                _reagentModel.PropertyChanged -= HandleReagentModelPropertyChanges;
                _reagentModel.LoadStatusChanged -= HandleLoadStatusChanged;
                _reagentModel.LoadCompleted -= HandleLoadCompleted;
                _reagentModel.UnloadStatusChanged -= HandleUnloadStatusChanged;
                _reagentModel.UnloadCompleted -= HandleUnloadCompleted;
                _reagentModel.Dispose();
            }
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties & Fields

        private ReagentModel _reagentModel;
        private CallBackProgressStatus _progressStatus;
        private readonly IInstrumentStatusService _instrumentStatusService;
        private bool _enableReagentLoadListener;
        private bool _enableReagentUnloadListener;

        public bool IsServiceEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsProgressActivate
        {
            get { return GetProperty<bool>(); }
            set { DispatcherHelper.ApplicationExecute(() => SetProperty(value)); }
        }

        public ReagentReplaceOption ReagentReplaceOption
        {
            get { return GetProperty<ReagentReplaceOption>(); }
            set { SetProperty(value); }
        }

        public bool IsStepOneEnable
        {
            get { return GetProperty<bool>(); }
            set { DispatcherHelper.ApplicationExecute(() => SetProperty(value)); }
        }

        public bool IsStepTwoEnable
        {
            get { return GetProperty<bool>(); }
            set { DispatcherHelper.ApplicationExecute(() => SetProperty(value)); }
        }

        public bool IsStepThreeEnable
        {
            get { return GetProperty<bool>(); }
            set { DispatcherHelper.ApplicationExecute(() => SetProperty(value)); }
        }

        public bool IsStepFourEnable
        {
            get { return GetProperty<bool>(); }
            set { DispatcherHelper.ApplicationExecute(() => SetProperty(value)); }
        }

        public bool IsStepFiveEnable
        {
            get { return GetProperty<bool>(); }
            set { DispatcherHelper.ApplicationExecute(() => SetProperty(value)); }
        }

        public string Status
        {
            get { return GetProperty<string>(); }
            set { DispatcherHelper.ApplicationExecute(() => SetProperty(value)); }
        }

        public string EventsRemaining => _reagentModel.EventsRemaining;
        public string ProgressIndicator => _reagentModel.ProgressIndicator;
        public string PartNumber => _reagentModel.PartNumber;

        public ObservableCollection<ReagentContainerStateDomain> ReagentContainers
        {
            get { return GetProperty<ObservableCollection<ReagentContainerStateDomain>>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Event Handlers

        private void HandleReagentModelPropertyChanges(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName))
                return;
            
            if (e.PropertyName.Equals(nameof(ReagentModel.EventsRemaining))) NotifyPropertyChanged(nameof(EventsRemaining));
            if (e.PropertyName.Equals(nameof(ReagentModel.ProgressIndicator))) NotifyPropertyChanged(nameof(ProgressIndicator));
            if (e.PropertyName.Equals(nameof(ReagentModel.PartNumber))) NotifyPropertyChanged(nameof(PartNumber));
            if (e.PropertyName.Equals(nameof(ReagentModel.ReagentContainers))) ReagentContainers = new ObservableCollection<ReagentContainerStateDomain>(_reagentModel.ReagentContainers);
        }

        private void HandleLoadStatusChanged(object sender, ApiEventArgs<ReagentLoadSequence> args)
        {
            if (!_enableReagentLoadListener)
            {
                return;
            }

            Log.Info($"Replace Reagent Load Status: '{args.Arg1}'");
            switch (args.Arg1)
            {
                case ReagentLoadSequence.eLWaitingForDoorLatch:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_LDoorLatch");
                    break;
                case ReagentLoadSequence.eLWaitingForReagentSensor:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_LReagentSensor");
                    break;
                case ReagentLoadSequence.eLIdentifyingReagentContainers:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_LReagentContainers");
                    break;
                case ReagentLoadSequence.eLWaitingOnContainerLocation:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_LContainerLocation");
                    break;
                case ReagentLoadSequence.eLInsertingProbes:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_LInsertingProbe");
                    break;
                case ReagentLoadSequence.eLSynchronizingReagentData:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_LSyncReagentData");
                    break;
                case ReagentLoadSequence.eLPrimingFluidLines:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_LPrimingFLuid");
                    break;
            }
        }


        private void HandleLoadCompleted(object sender, ApiEventArgs<ReagentLoadSequence> args)
        {
            if (!_enableReagentLoadListener)
            {
                return;
            }

            _enableReagentLoadListener = false;
            try
            {
                Log.Info($"Replace Reagent Load Complete status: '{args.Arg1}'");
                switch (args.Arg1)
                {
                    case ReagentLoadSequence.eLComplete:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_Lcomplete");

                        // Update the reagent status.
                        _reagentModel.OnRefreshReagent();
                        DispatcherHelper.ApplicationExecute(() =>
                        {
                            var msg = new Notification<ReagentContainerStateDomain>(
                                ReagentModel.GetReagentContainerStatusAll().FirstOrDefault(),
                                MessageToken.RefreshReagentStatus, "");
                            MessageBus.Default.Publish(msg);
                        });

                        IsProgressActivate = true;
                        IsStepOneEnable = IsStepTwoEnable = IsStepThreeEnable = IsStepFiveEnable = false;
                        IsStepFourEnable = true;
                        break;
                    case ReagentLoadSequence.eLFailure_DoorLatchTimeout:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_LDoorLatchFail");
                        ShowStatusAndClose();
                        break;
                    case ReagentLoadSequence.eLFailure_ReagentSensorDetect:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_LReagentSensorFail");
                        ShowStatusAndClose();
                        break;
                    case ReagentLoadSequence.eLFailure_NoWasteDetected:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_LNoWasteFail");
                        ShowStatusAndClose();
                        break;
                    case ReagentLoadSequence.eLFailure_NoReagentsDetected:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_LNoReagentFail");
                        ShowStatusAndClose();
                        break;
                    case ReagentLoadSequence.eLFailure_ReagentInvalid:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_LReagentInvalidFail");
                        ShowStatusAndClose();
                        break;
                    case ReagentLoadSequence.eLFailure_ReagentEmpty:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_LReagentEmptyFail");
                        ShowStatusAndClose();
                        break;
                    case ReagentLoadSequence.eLFailure_ReagentExpired:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_LReagetnExpired");
                        ShowStatusAndClose();
                        break;
                    case ReagentLoadSequence.eLFailure_InvalidContainerLocations:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_LInvalidContainerFail");
                        ShowStatusAndClose();
                        break;
                    case ReagentLoadSequence.eLFailure_ProbeInsert:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_LProbeInsertFail");
                        ShowStatusAndClose();
                        break;
                    case ReagentLoadSequence.eLFailure_Fluidics:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_LFludicFail");
                        ShowStatusAndClose();
                        break;
                    case ReagentLoadSequence.eLFailure_StateMachineTimeout:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_LTimeOut");
                        ShowStatusAndClose();
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_HANDLE_LOAD_COMPLETED"));
            }
        }

        private void HandleUnloadStatusChanged(object sender, ApiEventArgs<ReagentUnloadSequence> args)
        {
            if (!_enableReagentUnloadListener)
            {
                return;
            }

            Log.Info($"Replace Reagent Unload Status: '{args.Arg1}'");
            switch (args.Arg1)
            {
                case ReagentUnloadSequence.eULDraining1:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_ULDraining"), ApplicationConstants.NumericOne);
                    break;
                case ReagentUnloadSequence.eULPurging1:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_ULPurging"), ApplicationConstants.NumericOne);
                    break;
                case ReagentUnloadSequence.eULDraining2:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_ULDraining"), ApplicationConstants.NumericTwo);
                    break;
                case ReagentUnloadSequence.eULPurging2:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_ULPurging"), ApplicationConstants.NumericTwo);
                    break;
                case ReagentUnloadSequence.eULDraining3:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_ULDraining"), ApplicationConstants.NumericThree);
                    break;
                case ReagentUnloadSequence.eULDraining4:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_ULDraining"), ApplicationConstants.NumericFour);
                    break;
                case ReagentUnloadSequence.eULDraining5:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_ULDraining"), ApplicationConstants.NumericFive);
                    break;
                case ReagentUnloadSequence.eULPurging3:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_ULPurging"), ApplicationConstants.NumericThree);
                    break;
                case ReagentUnloadSequence.eULPurgingReagent1:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_ULPurging_Reagent"), ApplicationConstants.NumericOne);
                    break;
                case ReagentUnloadSequence.eULPurgingReagent2:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_ULPurging_Reagent"), ApplicationConstants.NumericTwo);
                    break;
                case ReagentUnloadSequence.eULRetractingProbes:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_ULRetractingProbe");
                    break;
                case ReagentUnloadSequence.eULUnlatchingDoor:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_ULUnlatchingDoor");
                    break;
            }
        }

        private void HandleUnloadCompleted(object sender, ApiEventArgs<ReagentUnloadSequence> args)
        {
            if (!_enableReagentUnloadListener)
            {
                return;
            }

            _enableReagentUnloadListener = false;
            try
            {
                Log.Info($"Replace Reagent Unload Complete: '{args.Arg1}'");
                switch (args.Arg1)
                {
                    case ReagentUnloadSequence.eULComplete:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_ULComplete");
                        IsProgressActivate = true;
                        IsStepOneEnable = IsStepTwoEnable = IsStepFourEnable = IsStepFiveEnable = false;
                        IsStepThreeEnable = true;
                        break;
                    case ReagentUnloadSequence.eULFailed_DrainPurge:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_ULDrainFailed");
                        ShowStatusAndClose();
                        break;
                    case ReagentUnloadSequence.eULFailed_ProbeRetract:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_ULProbeRetractFailed");
                        ShowStatusAndClose();
                        break;
                    case ReagentUnloadSequence.eULFailed_DoorUnlatch:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_ULDoorUnlatchFailed");
                        ShowStatusAndClose();
                        break;
                    case ReagentUnloadSequence.eULFailure_StateMachineTimeout:
                        Status = LanguageResourceHelper.Get("LID_ReagentStatus_ULTimeOut");
                        ShowStatusAndClose();
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_HANDLE_UNLOAD_COMPLETED"));
            }
        }

        #endregion

        #region Commands

        protected override void OnCancel()
        {
            try
            {
                if (_progressStatus.Equals(CallBackProgressStatus.IsRunning))
                {
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_ReplaceReagentProgress"));
                    return;
                }

                IsStepOneEnable = IsStepTwoEnable = IsStepThreeEnable = IsStepFourEnable = IsStepFiveEnable = false;
                base.OnCancel();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_UNABLE_TO_CLOSE_WINDOW"));
            }
        }

        #region Step One Proceed

        private RelayCommand _stepOneForwardCommand;
        public RelayCommand StepOneForwardCommand => _stepOneForwardCommand ?? (_stepOneForwardCommand = new RelayCommand(StepOneProceed, CanStepOneProceed));

        private bool CanStepOneProceed()
        {
            return IsProgressActivate;
        }

        private void StepOneProceed()
        {
            IsStepOneEnable = false;
            IsStepTwoEnable = true;
        }

        #endregion

        #region Step Two Proceed

        private RelayCommand _stepTwoForwardCommand;
        public RelayCommand StepTwoForwardCommand => _stepTwoForwardCommand ?? (_stepTwoForwardCommand = new RelayCommand(StepTwoProceed, CanStepTwoProceed));

        private bool CanStepTwoProceed()
        {
            return IsProgressActivate;
        }

        private void StepTwoProceed()
        {
            try
            {
                _progressStatus = CallBackProgressStatus.IsStart;
                switch (ReagentReplaceOption)
                {
                    case ReagentReplaceOption.IsDrain:
                        RequestReagentPackUnload(ReagentUnloadOption.eULDrainToWaste);
                        break;
                    case ReagentReplaceOption.IsPurge:
                        RequestReagentPackUnload(ReagentUnloadOption.eULPurgeLinesToContainer);
                        break;
                    case ReagentReplaceOption.IsNone:
                        RequestReagentPackUnload(ReagentUnloadOption.eULNone);
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_UNABLE_TO_REPLACE_REAGENT"));
            }
        }

        private void RequestReagentPackUnload(ReagentUnloadOption option)
        {
            _enableReagentUnloadListener = true;
            var emptyStatus = _reagentModel.UnloadReagentPack(option);

            if (emptyStatus.Equals(HawkeyeError.eSuccess))
            {
                _progressStatus = CallBackProgressStatus.IsRunning;
                IsProgressActivate = false;
            }
            else
            {
                _enableReagentUnloadListener = false;
                _progressStatus = CallBackProgressStatus.IsError;
                ApiHawkeyeMsgHelper.ErrorCommon(emptyStatus);
            }
        }

        #endregion

        #region Step Three Proceed

        private RelayCommand _stepThreeForwardCommand;
        public RelayCommand StepThreeForwardCommand => _stepThreeForwardCommand ?? (_stepThreeForwardCommand = new RelayCommand(StepThreeProceed, CanStepThreeProceed));

        private bool CanStepThreeProceed()
        {
            return IsProgressActivate;
        }

        private void StepThreeProceed()
        {
            try
            {
                Status = string.Empty;
                _enableReagentLoadListener = true;
                var status = _reagentModel.LoadReagentPack();
                if (status.Equals(HawkeyeError.eSuccess))
                {
                    IsProgressActivate = false;
                }
                else
                {
                    _enableReagentLoadListener = false;
                    ApiHawkeyeMsgHelper.ErrorCommon(status);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_UNABLE_TO_REPLACE_REAGENT"));
            }
        }

        #endregion

        #region Step Four Proceed

        private RelayCommand _stepFourForwardCommand;
        public RelayCommand StepFourForwardCommand => _stepFourForwardCommand ?? (_stepFourForwardCommand = new RelayCommand(StepFourProceed, CanStepFourProceed));

        private bool CanStepFourProceed()
        {
            return IsProgressActivate;
        }

        private void StepFourProceed()
        {
            try
            {
                IsProgressActivate = true;
                _progressStatus = CallBackProgressStatus.IsFinish;
                IsStepOneEnable = IsStepTwoEnable = IsStepThreeEnable = IsStepFourEnable = false;
                LocalUpdateSystemStatus();
                IsStepFiveEnable = true;
                DialogEventBus.EmptySampleTubesDialog(this, new EmptySampleTubesEventArgs((int)_instrumentStatusService.SystemStatusDom.SampleTubeDisposalRemainingCapacity));
                LocalUpdateSystemStatus(); // update the RemainingSpentTubeTrayCapacity (the dialog box might have changed this)
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_UNABLE_TO_REPLACE_REAGENT"));
            }
        }
        
        private void LocalUpdateSystemStatus()
        {
            MainWindowViewModel.Instance.UpdateSystemStatus();
        }

        #endregion

        #endregion

        #region Private Methods

        private void ShowStatusAndClose()
        {
            if (DialogEventBus.DialogBoxOk(this, Status) != true)
            {
                return;
            }
            
            Close(false);
        }

        #endregion
    }
}