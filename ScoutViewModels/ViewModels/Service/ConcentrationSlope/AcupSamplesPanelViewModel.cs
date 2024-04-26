using ScoutDomains;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutLanguageResources;
using ScoutServices.Service.ConcentrationSlope;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScoutViewModels.ViewModels.Service.ConcentrationSlope
{
    public class AcupSamplesPanelViewModel : BaseViewModel, IHandlesCalibrationState, IHandlesSystemStatus,
        IHandlesSampleStatus, IHandlesSampleCompleted
    {
        public AcupSamplesPanelViewModel(IAcupConcentrationService acupConcentrationService,
            IConcentrationSlopeService concentrationSlopeService)
        {
            _acupConcentrationService = acupConcentrationService;
            _concentrationSlopeService = concentrationSlopeService;

            IsSingleton = true;
            var latest = _concentrationSlopeService.GetMostRecentCalibration(calibration_type.cal_ACupConcentration);
            MostRecentConcentrationCalibration = latest;

            HandleNewCalibrationState(CalibrationGuiState.NotStarted);
        }

        #region Properties & Fields

        private readonly IAcupConcentrationService _acupConcentrationService;
        private readonly IConcentrationSlopeService _concentrationSlopeService;
        
        public ObservableCollection<AcupCalibrationConcentrationListDomain> ConcentrationSamples
        {
            get { return GetProperty<ObservableCollection<AcupCalibrationConcentrationListDomain>>(); }
            private set { SetProperty(value); }
        }

        public CalibrationActivityLogDomain MostRecentConcentrationCalibration
        {
            get { return GetProperty<CalibrationActivityLogDomain>(); }
            set { SetProperty(value); }
        }

        public bool ShowCancelCalibrationButton
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool RunningConcentration
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool ShowConcentrationProgressBar
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool ShowLoadingIndicator
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string PauseResumeStatusString
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); } // set to string.Empty to change visibility to collapsed
        }

        public string AbortStatusString
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); } // set to string.Empty to change visibility to collapsed
        }

        public SampleStatus CurrentConcentrationSampleStatus
        {
            get { return GetProperty<SampleStatus>(); }
            set { SetProperty(value); }
        }

        // These "brushes" should probably make their way into a control that handles this progress indicator
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

        #endregion

        #region Interface Methods
        
        public void HandleNewCalibrationState(CalibrationGuiState state)
        {
            switch(state)
            {
                case CalibrationGuiState.NotStarted:
                case CalibrationGuiState.Aborted:
                case CalibrationGuiState.CalibrationApplied:
                    var latest = _concentrationSlopeService.GetMostRecentCalibration(calibration_type.cal_ACupConcentration);
                    MostRecentConcentrationCalibration = latest;
                    goto case CalibrationGuiState.CalibrationRejected;
                case CalibrationGuiState.CalibrationRejected:
                    RunningConcentration = false;
                    ShowCancelCalibrationButton = false;
                    goto case CalibrationGuiState.Ended;
                case CalibrationGuiState.Ended:
                    ConcentrationSamples = _acupConcentrationService.GetDefaultACupConcentrationList()
                                                                    .ToObservableCollection();
                    var first = ConcentrationSamples.FirstOrDefault();
                    if (first != null) first.IsActiveRow = true;
                    ShowConcentrationProgressBar = false;
                    ShowLoadingIndicator = false;
                    PauseResumeStatusString = string.Empty;
                    AbortStatusString = string.Empty;
                    CurrentConcentrationSampleStatus = SampleStatus.NotProcessed;
                    AspirationBrush = SampleProgressStatus.IsInActive;
                    MixingDyeBrush = SampleProgressStatus.IsInActive;
                    CleaningBrush = SampleProgressStatus.IsInActive;
                    ImageAnalysisBrush = SampleProgressStatus.IsInActive;
                    ShowCancelCalibrationButton = false;
                    break;
                case CalibrationGuiState.Started:
                    RunningConcentration = true;
                    ShowLoadingIndicator = true;
                    ShowConcentrationProgressBar = true;
                    ShowCancelCalibrationButton = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        
        public void HandleSystemStatusChanged(SystemStatusDomain systemStatus)
        {
            switch (systemStatus.SystemStatus)
            {
                case SystemStatus.Idle:
                    ShowConcentrationProgressBar = false;
                    break;
                case SystemStatus.ProcessingSample:
                    ShowConcentrationProgressBar = true;
                    break;
                case SystemStatus.Pausing:
                    ShowConcentrationProgressBar = true;
                    ShowLoadingIndicator = true;
                    AbortStatusString = string.Empty;
                    PauseResumeStatusString = LanguageResourceHelper.Get("LID_MSGBOX_PausingPleaseWait");
                    break;
                case SystemStatus.Paused:
                    ShowConcentrationProgressBar = true;
                    AbortStatusString = string.Empty;
                    PauseResumeStatusString = LanguageResourceHelper.Get("LID_Label_Resume");
                    PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_WorkQueuePaused"));
                    ShowLoadingIndicator = false;
                    break;
                case SystemStatus.Stopping:
                    ShowConcentrationProgressBar = true;
                    AbortStatusString = LanguageResourceHelper.Get("LID_Label_Aborting");
                    PauseResumeStatusString = string.Empty;
                    break;
                case SystemStatus.Stopped:
                    // todo: we may call this directly in the parent's HandleNewCalibrationState()
                    HandleNewCalibrationState(CalibrationGuiState.Aborted);
                    ShowConcentrationProgressBar = false;
                    break;
                case SystemStatus.Faulted:
                    // todo: we may call this directly in the parent's HandleNewCalibrationState()
                    if (RunningConcentration)
                    {
                        HandleNewCalibrationState(CalibrationGuiState.Ended);
                        DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_ABORTEDSAMPLE"));
                    }
                    break;
                case SystemStatus.SearchingTube:
                    ShowLoadingIndicator = true;
                    ShowConcentrationProgressBar = false;
                    break;
            }
        }

        public void HandleSampleStatusChanged(SampleEswDomain sample, 
            AcupCalibrationConcentrationListDomain concentration)
        {
            var newStatus = sample.SampleStatus;
            CurrentConcentrationSampleStatus = newStatus;

            // This updates the 4 boxes that represent the overall progress on the GUI:
            switch (newStatus)
            {
                case SampleStatus.NotProcessed:
                    SetSampleProgressStatusColor(
                        SampleProgressStatus.IsReady,
                        SampleProgressStatus.IsReady,
                        SampleProgressStatus.IsReady,
                        SampleProgressStatus.IsReady);
                    break;
                case SampleStatus.InProcessAspirating:
                    SetSampleProgressStatusColor(
                        SampleProgressStatus.IsReady,
                        SampleProgressStatus.IsInActive,
                        SampleProgressStatus.IsInActive,
                        SampleProgressStatus.IsInActive);
                    break;
                case SampleStatus.InProcessMixing:
                    SetSampleProgressStatusColor(
                        SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsRunning,
                        SampleProgressStatus.IsInActive,
                        SampleProgressStatus.IsInActive);
                    break;
                case SampleStatus.InProcessImageAcquisition:
                    SetSampleProgressStatusColor(
                        SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsRunning,
                        SampleProgressStatus.IsInActive);
                    break;
                case SampleStatus.InProcessCleaning:
                    SetSampleProgressStatusColor(
                        SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsRunning);
                    break;
                case SampleStatus.AcquisitionComplete:
                    SetSampleProgressStatusColor(
                        SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsRunning);
                    break;
                case SampleStatus.Completed:
                    SetSampleProgressStatusColor(
                        SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsActive,
                        SampleProgressStatus.IsActive);
                    break;
                case SampleStatus.SkipManual:
                    SetSampleProgressStatusColor(
                        SampleProgressStatus.IsInActive,
                        SampleProgressStatus.IsInActive,
                        SampleProgressStatus.IsInActive,
                        SampleProgressStatus.IsInActive);
                    break;
                case SampleStatus.SkipError:
                    SetSampleProgressStatusColor(
                        SampleProgressStatus.IsInActive,
                        SampleProgressStatus.IsInActive,
                        SampleProgressStatus.IsInActive,
                        SampleProgressStatus.IsInActive);
                    break;
            }
        }

        public void HandleSampleCompleted(SampleEswDomain sample, SampleRecordDomain sampleResult, 
            AcupCalibrationConcentrationListDomain concentration)
        {
            // Find the matching sample in SamplesPanelViewModel.ConcentrationSamples and mark as complete.
            // Keep in mind that WorkListComplete can get called before this code is touched and it may 
            // have already set these booleans.
            var concentrationSample = ConcentrationSamples[sample.Index];
            concentrationSample.IsComplete = true;
            concentrationSample.IsActiveRow = false;
        }
        
        #endregion

        #region Private Helper Methods

        private void SetSampleProgressStatusColor(SampleProgressStatus aspirationBrush, SampleProgressStatus mixingDyeBrush,
            SampleProgressStatus imageAnalysisBrush, SampleProgressStatus cleaningBrush)
        {
            AspirationBrush = aspirationBrush;
            MixingDyeBrush = mixingDyeBrush;
            ImageAnalysisBrush = imageAnalysisBrush;
            CleaningBrush = cleaningBrush;
        }

        #endregion
    }
}