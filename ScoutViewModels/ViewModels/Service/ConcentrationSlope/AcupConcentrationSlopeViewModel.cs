using ApiProxies.Misc;
using Ninject.Extensions.Logging;
using ScoutDataAccessLayer.DAL;
using ScoutDomains;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Interfaces;
using ScoutModels.Service.ConcentrationSlope;
using ScoutServices.Enums;
using ScoutServices.Interfaces;
using ScoutServices.Service.ConcentrationSlope;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using ScoutUtilities.Structs;
using ScoutViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScoutViewModels.ViewModels.Service.ConcentrationSlope
{
    public class AcupConcentrationSlopeViewModel : BaseViewModel, 
        IHandlesCalibrationState, IHandlesSystemStatus, 
        IHandlesSampleStatus, IHandlesSampleCompleted, 
        IHandlesImageReceived, IHandlesWorkListCompleted, 
        IHandlesConcentrationCalculated
    {
        public AcupConcentrationSlopeViewModel(
            IScoutViewModelFactory viewModelFactory, 
            ILockManager lockManager,
            ISampleProcessingService sampleProcessingService, 
            IInstrumentStatusService instrumentStatusService,
            IAcupConcentrationService acupConcentrationService, 
            IConcentrationSlopeService concentrationSlopeService,
            ILogger logger, 
            ICellTypeManager cellTypeManager,
            IDialogCaller dialogCaller)
        {
            _sampleProcessingService = sampleProcessingService;
            _acupConcentrationService = acupConcentrationService;
            _concentrationService = concentrationSlopeService;
            _logger = logger;
            _cellTypeManager = cellTypeManager;
            _dialogCaller = dialogCaller;

            _acupConcentrationSlopeModel = new AcupConcentrationSlopeModel();

            // build sub-ViewModels
            SamplesPanelViewModel = viewModelFactory.CreateAcupSamplesPanelViewModel();
            DataPanelViewModel = viewModelFactory.CreateAcupDataPanelViewModel();

            // setup subscriptions
            _systemStatusSubscription = instrumentStatusService.SubscribeToSystemStatusCallback()
                                                               .Subscribe(OnSystemStatusChanged);
            _lockStatusSubscription = lockManager.SubscribeStateChanges()
                                                 .Subscribe(OnLockStatusChanged);

            _sampleCompleteSubscription = sampleProcessingService.SubscribeToSampleCompleteCallback()
                                                                 .Subscribe(OnSampleCompleted);
            _sampleStatusSubscription = sampleProcessingService.SubscribeToSampleStatusCallback()
                                                               .Subscribe(OnSampleStatusChanged);
            _imageReceivedSubscription = sampleProcessingService.SubscribeToImageResultCallback()
                                                                .Subscribe(OnReceivedImageResult);
            _workListCompleteSubscription = sampleProcessingService.SubscribeToWorkListCompleteCallback()
                                                                   .Subscribe(OnWorkListCompleted);

            HandleNewCalibrationState(CalibrationGuiState.NotStarted);
        }
        
        protected override void DisposeUnmanaged()
        {
            _systemStatusSubscription?.Dispose();
            _lockStatusSubscription?.Dispose();
            _sampleCompleteSubscription?.Dispose();
            _sampleStatusSubscription?.Dispose();
            _imageReceivedSubscription?.Dispose();
            _workListCompleteSubscription?.Dispose();
            base.DisposeUnmanaged();
        }

        #region Properties & Fields

        private readonly ISampleProcessingService _sampleProcessingService;
        private readonly IAcupConcentrationService _acupConcentrationService;
        private readonly IConcentrationSlopeService _concentrationService;
        private readonly ILogger _logger;
        private readonly ICellTypeManager _cellTypeManager;
        private readonly IDialogCaller _dialogCaller;

        private readonly IDisposable _systemStatusSubscription;
        private readonly IDisposable _lockStatusSubscription;
        private readonly IDisposable _sampleCompleteSubscription;
        private readonly IDisposable _sampleStatusSubscription;
        private readonly IDisposable _imageReceivedSubscription;
        private readonly IDisposable _workListCompleteSubscription;

        private readonly AcupConcentrationSlopeModel _acupConcentrationSlopeModel;

        private uuidDLL _workListUuid;
        
        // Used to keep track of a user-aborted concentration slope assay
        public bool Aborted
        {
            get { return GetProperty<bool>(); }
            private set
            {
                SetProperty(value);
                DispatcherHelper.ApplicationExecute(() => StopConcentrationCommand.RaiseCanExecuteChanged());
            }
        }

        public bool RunningConcentration
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public SystemStatus SystemStatus
        {
            get { return GetProperty<SystemStatus>(); }
            private set
            {
                SetProperty(value);
                StartACupConcentrationSampleCommand.RaiseCanExecuteChanged();
            }
        }
        
        public LockResult LockState
        {
            get { return GetProperty<LockResult>(); }
            private set
            {
                SetProperty(value);
                StartACupConcentrationSampleCommand.RaiseCanExecuteChanged();
            }
        }

        public AcupSamplesPanelViewModel SamplesPanelViewModel
        {
            get { return GetProperty<AcupSamplesPanelViewModel>(); }
            private set { SetProperty(value); }
        }

        public AcupDataPanelViewModel DataPanelViewModel
        {
            get { return GetProperty<AcupDataPanelViewModel>(); }
            private set { SetProperty(value); }
        }

        #endregion

        #region Subscription Event Handlers

        private void OnSystemStatusChanged(SystemStatusDomain systemStatusDomain)
        {
            HandleSystemStatusChanged(systemStatusDomain);
        }

        private void OnLockStatusChanged(LockResult lockState)
        {
            LockState = lockState;
        }

        private void OnSampleStatusChanged(ApiEventArgs<SampleEswDomain> sampleStatusArgs)
        {
            if (!RunningConcentration) return; // don't worry about it, this callback is not for an a cup slope
            if (sampleStatusArgs?.Arg1 == null)
            {
                _logger.Error($"Sample Status Callback [AcupConcentration]::Arg1 is null");
                return;
            }

            _logger.Debug($"Sample Status Callback [AcupConcentration]::New Status {sampleStatusArgs.Arg1.SampleStatus}");

            var concentration = GetAcupCalibrationConcentration(sampleStatusArgs.Arg1);
            HandleSampleStatusChanged(sampleStatusArgs.Arg1, concentration);
        }

        /// <summary>
        /// Keep in mind that SampleComplete can be called *after* WorkListComplete due to the
        /// fact that SampleComplete service calls the backend to get the sample results before
        /// being triggered here.
        /// </summary>
        /// <param name="sampleCompleteArgs"></param>
        private void OnSampleCompleted(ApiEventArgs<SampleEswDomain, SampleRecordDomain> sampleCompleteArgs)
        {
            if (!RunningConcentration) return; // don't worry about it, this callback is not for an a cup slope
            if (sampleCompleteArgs.Arg1 == null)
            {
                _logger.Error($"Sample Complete Callback [AcupConcentration]::Arg1 is null");
                return;
            }

            if (sampleCompleteArgs.Arg2 == null)
            {
                _logger.Error($"Sample Complete Callback [AcupConcentration]::Arg2 is null");
                return;
            }

            _logger.Debug($"Sample Complete Callback [AcupConcentration]::Sample Completed for index {sampleCompleteArgs.Arg1.Index}");

            var concentration = GetAcupCalibrationConcentration(sampleCompleteArgs.Arg1);
            HandleSampleCompleted(sampleCompleteArgs.Arg1, sampleCompleteArgs.Arg2, concentration);
        }

        private void OnReceivedImageResult(
            ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers> imageArgs)
        {
            if (!RunningConcentration) return; // don't worry about it, this callback is not for an a cup slope
            if (imageArgs.Arg1 == null)
            {
                _logger.Error($"Image Received Callback [AcupConcentration]::Arg1 is null");
                return;
            }

            var concentration = GetAcupCalibrationConcentration(imageArgs.Arg1);
            HandleImageReceived(imageArgs.Arg1, imageArgs.Arg2, imageArgs.Arg3,
                imageArgs.Arg4, imageArgs.Arg5, concentration);
        }

        /// <summary>
        /// Keep in mind WorkListCompleted can be triggered *before* SampleCompleted (due to the sample
        /// complete making a 2nd call for the sample results).
        /// </summary>
        /// <param name="workListArgs"></param>
        private void OnWorkListCompleted(ApiEventArgs<List<uuidDLL>> workListArgs)
        {
            if (!RunningConcentration || workListArgs.Arg1 == null || workListArgs.Arg1.Count <= 0) return;

            _logger.Debug($"WorkList Complete Callback [AcupConcentration]::Uuids: '{string.Join(",", workListArgs.Arg1)}'");
            HandleWorkListCompleted(workListArgs.Arg1);
        }

        #endregion

        #region Interface Methods

        public void HandleNewCalibrationState(CalibrationGuiState state)
        {
            _logger.Debug($"{nameof(AcupConcentrationSlopeViewModel)}::{nameof(HandleNewCalibrationState)}::New State: '{state}");
            switch (state)
            {
                case CalibrationGuiState.NotStarted:
                case CalibrationGuiState.Aborted:
                    _acupConcentrationSlopeModel.Reset();
                    Aborted = false;
                    RunningConcentration = false;
                    _workListUuid = new uuidDLL();
                    goto case CalibrationGuiState.Ended;
                case CalibrationGuiState.Started:
                    EnableDisableAppAccess(false);
                    _acupConcentrationSlopeModel.Reset();
                    Aborted = false;
                    RunningConcentration = true;
                    _workListUuid = new uuidDLL();
                    break;
                case CalibrationGuiState.Ended:
                    EnableDisableAppAccess(true);
                    break;
                case CalibrationGuiState.CalibrationApplied:
                case CalibrationGuiState.CalibrationRejected:
                    RunningConcentration = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            SamplesPanelViewModel.HandleNewCalibrationState(state);
            DataPanelViewModel.HandleNewCalibrationState(state);

            // triggers on IsComplete for all samples
            DispatcherHelper.ApplicationExecute(() =>
            {
                StopConcentrationCommand.RaiseCanExecuteChanged();
                StartACupConcentrationSampleCommand.RaiseCanExecuteChanged();
            });
        }

        public void HandleSystemStatusChanged(SystemStatusDomain systemStatus)
        {
            var oldStatus = SystemStatus;
            var newStatus = systemStatus.SystemStatus;
            if (newStatus == oldStatus)
            {
                return; // don't need to change anything
            }

            SystemStatus = newStatus;
            _logger.Debug($"{nameof(AcupConcentrationSlopeViewModel)}::System Status changed from '{oldStatus}' to '{newStatus}'");
            
            SamplesPanelViewModel.HandleSystemStatusChanged(systemStatus);
        }

        public void HandleSampleStatusChanged(SampleEswDomain sample, AcupCalibrationConcentrationListDomain concentration)
        {
            SamplesPanelViewModel.HandleSampleStatusChanged(sample, concentration);
            DataPanelViewModel.HandleSampleStatusChanged(sample, concentration);
        }

        public void HandleSampleCompleted(SampleEswDomain sample, SampleRecordDomain sampleResult, 
            AcupCalibrationConcentrationListDomain concentration)
        {
            SamplesPanelViewModel.HandleSampleCompleted(sample, sampleResult, concentration);
        }

        public void HandleImageReceived(SampleEswDomain sample, ushort imageNum, BasicResultAnswers cumulativeResults,
            ImageSetDto image, BasicResultAnswers imageResults, AcupCalibrationConcentrationListDomain concentration)
        {
            var cellType = _cellTypeManager.GetCellType(sample.CellTypeIndex);
            _acupConcentrationSlopeModel.UpdateSampleRecord(sample, cellType, cumulativeResults);
            
            DataPanelViewModel.HandleImageReceived(sample, imageNum, cumulativeResults, image, imageResults, concentration);
        }

        public void HandleWorkListCompleted(List<uuidDLL> workListUuid)
        {
            if (_acupConcentrationSlopeModel.CalibrationSampleIndex >= SamplesPanelViewModel.ConcentrationSamples.Count)
            {
                _logger.Info($"WorkList Complete Callback [AcupConcentration]:: Initial CalibrationSampleIndex is greater than/equal to number of samples. Was aborted: {Aborted}");
            }
            else
            {
                var completedSample = SamplesPanelViewModel.ConcentrationSamples[_acupConcentrationSlopeModel.CalibrationSampleIndex];
                MarkSampleCompleted(completedSample);
            }
            
            _acupConcentrationSlopeModel.CalibrationSampleIndex++;
            var allSamplesComplete = SamplesPanelViewModel.ConcentrationSamples.All(s => s.IsComplete);

            if (allSamplesComplete)
            {
                _logger.Debug($"WorkList Complete Callback [AcupConcentration]::All samples have completed");
                CalculateSlope();
                HandleNewCalibrationState(CalibrationGuiState.Ended);
            }
            else if (_acupConcentrationSlopeModel.CalibrationSampleIndex >= SamplesPanelViewModel.ConcentrationSamples.Count)
            {
                _logger.Error($"WorkList Complete Callback [AcupConcentration]::CalibrationSampleIndex is greater than/equal to number of samples. Stopping calibration...");
                CalculateSlope();
                HandleNewCalibrationState(CalibrationGuiState.Ended);
            }
            else
            {
                var nextSample = SamplesPanelViewModel.ConcentrationSamples[_acupConcentrationSlopeModel.CalibrationSampleIndex];
                nextSample.IsActiveRow = true;
                _logger.Debug($"WorkList Complete Callback [AcupConcentration]::Sample with index '{_acupConcentrationSlopeModel.CalibrationSampleIndex}' is now active");
            }

            if (Aborted && !allSamplesComplete)
            {
                HandleNewCalibrationState(CalibrationGuiState.Aborted);
            }
        }
        
        public void HandleConcentrationCalculated(CalibrationData totalCells, CalibrationData originalConcentration, CalibrationData adjustedConcentration)
        {
            DataPanelViewModel.HandleConcentrationCalculated(totalCells, originalConcentration, adjustedConcentration);
        }

        #endregion

        #region Commands

        #region Start Acup Concentration Sample

        private RelayCommand<AcupCalibrationConcentrationListDomain> _startACupConcentrationSampleCommand;
        public RelayCommand<AcupCalibrationConcentrationListDomain> StartACupConcentrationSampleCommand =>
            _startACupConcentrationSampleCommand ?? (_startACupConcentrationSampleCommand = 
                new RelayCommand<AcupCalibrationConcentrationListDomain>(PerformStartACupConcSample, CanPerformStartACupConcSample));

        private bool CanPerformStartACupConcSample(AcupCalibrationConcentrationListDomain calSample)
        {
            if (DataPanelViewModel.ConcentrationResultsViewModel.IsCalibrationCompleted)
            {
                // don't allow if we're waiting on the user to accept or reject a calibration
                return false;
            }

            return SystemStatus == SystemStatus.Idle;
        }

        private void PerformStartACupConcSample(AcupCalibrationConcentrationListDomain calSample)
        {
            if (calSample.Index == 0) // indexing starts at 0 (0 -> 17)
            {
                // This is the first sample for a cup calibration.
                // Validate the entire set of acup sample definitions:
                var concentrationTemplates = DataPanelViewModel.SummaryTabViewModel.ConcentrationTemplates;

                var isValid = _concentrationService.ValidateConcentrationValues(concentrationTemplates,
                    out var localizedErrorMessage);
                if (isValid)
                {
                    _acupConcentrationService.UpdateACupConcentrationList(concentrationTemplates, SamplesPanelViewModel.ConcentrationSamples);
                }
                else
                {
                    _logger.Debug($"Acup concentration::values are not valid");
                    DialogEventBus.DialogBoxOk(this, localizedErrorMessage);
                    return;
                }
            }

            if (_dialogCaller.DialogBoxOkCancel(this, 
                LanguageResourceHelper.Get("LID_Label_ACupConcentrationPlay")) == true)
            {
                var comment = DataPanelViewModel.SummaryTabViewModel.ACupConcentrationComment;
                var sampleSet = _acupConcentrationService.GetACupConcentrationSampleSetDomain(comment, calSample);
                var setList = new List<SampleSetDomain> {sampleSet};
                var canResult = _sampleProcessingService.CanProcessSamples(LoggedInUser.CurrentUserId, setList, false);
                if (canResult == SampleProcessingValidationResult.Valid)
                {
                    var template = new SampleSetTemplateDomain(); // this is not used with a cup, so it can be empty
                    if (_sampleProcessingService.ProcessSamples(setList, LoggedInUser.CurrentUserId,
                        template, XMLDataAccess.Instance))
                    {
                        if (calSample.Index == 0)
                        {
                            HandleNewCalibrationState(CalibrationGuiState.Started);
                        }
                    }
                    else
                    {
                        _logger.Debug($"Acup concentration::Unable to ProcessSamples");
                    }
                }
                else
                {
                    _logger.Debug($"Acup concentration::Cannot process samples: {canResult}");
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_UNABLETOSTARTWORKQUEUE"));
                }
            }
        }

        #endregion

        #region Stop Concentration

        private RelayCommand _stopConcentrationCommand;
        public RelayCommand StopConcentrationCommand => _stopConcentrationCommand ?? (_stopConcentrationCommand = new RelayCommand(PerformStopConcentration, CanPerformStopConcentration));

        private bool CanPerformStopConcentration()
        {
            var allSamplesComplete = SamplesPanelViewModel.ConcentrationSamples.All(s => s.IsComplete);
            if (allSamplesComplete)
                return false;
            return !Aborted;
        }

        private void PerformStopConcentration()
        {
            if (DialogEventBus.DialogBoxYesNo(this,
                LanguageResourceHelper.Get("LID_MSGBOX_QueueManagementAbort")) != true)
            {
                return;
            }

            if (SystemStatus == SystemStatus.Idle)
            {
                // we are stopping in-between samples (no more callbacks coming) -- just stop and reset everything
                _logger.Debug($"Acup concentration aborted between samples. Resetting now...");
                HandleNewCalibrationState(CalibrationGuiState.Ended);
                HandleNewCalibrationState(CalibrationGuiState.NotStarted);
            }
            else
            {
                if (_sampleProcessingService.StopProcessing(LoggedInUser.CurrentUserId, string.Empty))
                {
                    _logger.Debug($"Acup concentration aborted. Waiting on WorkListCompleted to reset...");
                    Aborted = true;
                    // SetCalibrationStopped and Reset get called on WorkListCompleted
                }
                else
                {
                    _logger.Debug($"Acup concentration::Unable to stop concentration");
                }
            }
        }

        #endregion

        #region Export Calibration Command

        private RelayCommand _exportConcentrationCommand;
        public RelayCommand ExportConcentrationCommand =>
            _exportConcentrationCommand ?? (_exportConcentrationCommand = new RelayCommand(PerformExport, CanPerformExport));

        private bool CanPerformExport()
        {
            return true;
        }

        private void PerformExport()
        {
            if (ExportModel.OpenCsvSaveFileDialog(string.Empty, out var fullFilePath))
            {
                MessageBus.Default.Publish(new Notification<bool>(true, MessageToken.AdornerVisible));

                var slope = Misc.UpdateDecimalPoint(
                    DataPanelViewModel.ConcentrationResultsViewModel.CalibrationSlope,
                    TrailingPoint.Four);
                var intercept = Misc.UpdateDecimalPoint(
                    DataPanelViewModel.ConcentrationResultsViewModel.CalibrationIntercept,
                    TrailingPoint.Four);
                var r2 = Misc.UpdateDecimalPoint(
                    DataPanelViewModel.ConcentrationResultsViewModel.CalibrationR2,
                    TrailingPoint.Four);

                var slopeValues = new List<KeyValuePair<string, double>>
                {
                    new KeyValuePair<string, double>("Slope", slope),
                    new KeyValuePair<string, double>("Intercept", intercept),
                    new KeyValuePair<string, double>("R-squared", r2)
                };
                
                ExportModel.ExportConcentrationSlopeDetails(
                    DataPanelViewModel.SummaryTabViewModel.ConcentrationTemplates.ToList(),
                    slopeValues, 
                    DataPanelViewModel.ConcentrationResultsViewModel.ColumnHeaders.ToList(), 
                    FileType.Csv,
                    Path.GetDirectoryName(fullFilePath), 
                    Path.GetFileName(fullFilePath));

                MessageBus.Default.Publish(new Notification<bool>(false, MessageToken.AdornerVisible));
            }
        }

        #endregion

        #region Cancel Calibration Command
        
        private RelayCommand _cancelConcentrationCommand;
        public RelayCommand CancelConcentrationCommand =>
            _cancelConcentrationCommand ?? (_cancelConcentrationCommand = new RelayCommand(PerformCancel, CanPerformCancel));

        private bool CanPerformCancel()
        {
            return true;
        }

        private void PerformCancel()
        {
            if (DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_MSGBOX_CancelConcentration")) != true)
            {
                return;
            }
            HandleNewCalibrationState(CalibrationGuiState.CalibrationApplied);
        }

        #endregion

        #region Accept Calibration Command
        
        private RelayCommand _acceptConcentrationCommand;
        public RelayCommand AcceptConcentrationCommand =>
            _acceptConcentrationCommand ?? (_acceptConcentrationCommand = new RelayCommand(PerformAccept, CanPerformAccept));

        private bool CanPerformAccept()
        {
            return true;
        }

        private void PerformAccept()
        {
            // confirm the user wants to set the calibration
            if (DialogEventBus.DialogBoxYesNo(this, 
                LanguageResourceHelper.Get("LID_MSGBOX_SetSlope")) != true)
            {
                return;
            }

            // save the calibration to the backend
            var consumables = DataPanelViewModel.SummaryTabViewModel.ConcentrationTemplates;
            var slope = DataPanelViewModel.ConcentrationResultsViewModel.CalibrationSlope;
            var intercept = DataPanelViewModel.ConcentrationResultsViewModel.CalibrationIntercept;
            var result = _concentrationService.SaveCalibration(consumables, calibration_type.cal_ACupConcentration,
                _workListUuid, slope, intercept);
            
            if (result != HawkeyeError.eSuccess)
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_MSGBOX_SetSlopeFailed"));
                return; // don't reset anything when the backend call fails
            }
            
            PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_NewConcentrationSlopeSet"));
            HandleNewCalibrationState(CalibrationGuiState.CalibrationApplied);
        }

        #endregion

        #endregion

        #region Private Helper Methods

        private void MarkSampleCompleted(AcupCalibrationConcentrationListDomain completedSample)
        {
            if (completedSample == null)
                return;

            completedSample.IsComplete = true;
            completedSample.IsActiveRow = false;
            
            // triggers on IsComplete for all samples
            DispatcherHelper.ApplicationExecute(() => StopConcentrationCommand.RaiseCanExecuteChanged());
        }

        private AcupCalibrationConcentrationListDomain GetAcupCalibrationConcentration(SampleEswDomain sample)
        {
            var number = Regex.Match(sample.SampleName, @"\d+$", RegexOptions.RightToLeft).Value;
            int.TryParse(number, out var sampleNumber);
            var concentration = SamplesPanelViewModel.ConcentrationSamples
                .FirstOrDefault(x => x.StartPosition <= sampleNumber && x.EndPosition >= sampleNumber);
            return concentration;
        }

        private void CalculateSlope()
        {
            _logger.Debug($"Performing a-cup concentration calculations...");
            
            _acupConcentrationService.CalculateAcupConcentration(
                _acupConcentrationSlopeModel.SampleRecordDomains, 
                SamplesPanelViewModel.ConcentrationSamples, 
                out var totalCells,
                out var originalConcentration,
                out var adjustedConcentration);
            
            HandleConcentrationCalculated(totalCells, originalConcentration, adjustedConcentration);
        }

        private void EnableDisableAppAccess(bool enable)
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                // enable/disable the Service page's other tabs
                MessageBus.Default.Publish(new Notification<bool>(enable, nameof(ServiceViewModel), nameof(ServiceViewModel.IsMotorRunning)));

                // enable/disable the apps hamburger menu
                MainWindowViewModel.Instance?.EnableDisableHamburgerMenu(enable);
            });
        }

        #endregion
    }
}