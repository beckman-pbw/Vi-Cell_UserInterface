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
using System.Linq;
using System.Windows;
using ScoutModels.Interfaces;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public enum ReagentProcess
    {
        Flush,
        Prime,
        Clean
    }

    public class ReagentStatusDialogViewModel : BaseDialogViewModel
    {
        public ReagentStatusDialogViewModel(ReagentStatusEventArgs args, Window parentWindow, IInstrumentStatusService instrumentStatusService) : base(args, parentWindow)
        {
            ShowDialogTitleBar = true;
            DialogTitle = LanguageResourceHelper.Get("LID_POPUPHeader_ReagentStatus");

            _instrumentStatusService = instrumentStatusService;
            if (!LoggedInUser.NoLoggedInUser() && LoggedInUser.CurrentUserRoleId.Equals(UserPermissionLevel.eService))
            {
                IsServiceEnable = true;
            }

            _progressStatus = CallBackProgressStatus.IsFinish;
            _reagentModel = new ReagentModel();
            
            _reagentModel.PrimeReagentStateChanged += OnPrimeReagentStateChanged;
            _reagentModel.FlowCellFlushStateChanged += OnFlowCellFlushStateChanged;
            _reagentModel.CleanFluidicsStateChanged += OnCleanFluidicsStateChanged;
            _statusSubscriber = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe((OnUpdateReagentView));

            RefreshReagentStatus();
        }

        protected override void DisposeUnmanaged()
        {
            if (_reagentModel != null)
            {
                _reagentModel.PrimeReagentStateChanged -= OnPrimeReagentStateChanged;
                _reagentModel.FlowCellFlushStateChanged -= OnFlowCellFlushStateChanged;
                _reagentModel.CleanFluidicsStateChanged -= OnCleanFluidicsStateChanged;
            }

            _reagentModel?.Dispose();
            _statusSubscriber?.Dispose();
            base.DisposeUnmanaged();
        }

        #region Properties & Fields
        private IDisposable _statusSubscriber;
        private ReagentModel _reagentModel;
        private CallBackProgressStatus _progressStatus;
        private bool _enablePrimeStatusListener;
        private bool _enableFlushStatusListener;
        private bool _enableCleanStatusListener;
        private ReagentProcess _reagentCurrentProcess;
        private readonly IInstrumentStatusService _instrumentStatusService;

        // These all get updated in the ReagentModel.OnRefreshReagent method which is called by RefreshReagentStatus() and is followed by NotifyAllPropertiesChanged()
        public ObservableCollection<ReagentContainerStateDomain> ReagentContainers => _reagentModel.ReagentContainers;
        public string ProgressIndicator => _reagentModel.ProgressIndicator;
        public string EventsRemaining => _reagentModel.EventsRemaining;
        public ReagentContainerStatus ReagentHealth => _reagentModel.ReagentHealth;
        public string ReagentContainerStatusAsStr => _reagentModel.ReagentContainerStatusAsStr;

        public string Status
        {
            get { return GetProperty<string>(); }
            set
            {
                DispatcherHelper.ApplicationExecute(() => SetProperty(value));
            }
        }

        public bool IsProgressActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsServiceEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsHealthOk
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                PrimeCommand.RaiseCanExecuteChanged();
                FlushCommand.RaiseCanExecuteChanged();
                DecontaminateCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsReplaceReagentActive
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                ReplaceReagentCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Event Handlers

        private void OnPrimeReagentStateChanged(object sender, ApiEventArgs<ePrimeReagentLinesState> e)
        {
            if (!_enablePrimeStatusListener)
            {
                return;
            }

            switch (e.Arg1)
            {
                case ePrimeReagentLinesState.prl_Idle:
                    break;
                case ePrimeReagentLinesState.prl_PrimingCleaner1:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_PrimeClean"), ScoutUtilities.Misc.ConvertToString(1));
                    break;
                case ePrimeReagentLinesState.prl_PrimingCleaner2:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_PrimeClean"), ScoutUtilities.Misc.ConvertToString(2));
                    break;
                case ePrimeReagentLinesState.prl_PrimingCleaner3:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_PrimeClean"), ScoutUtilities.Misc.ConvertToString(3));
                    break;
                case ePrimeReagentLinesState.prl_PrimingReagent1:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_PrimeReagent"), ScoutUtilities.Misc.ConvertToString(1));
                    break;
                case ePrimeReagentLinesState.prl_PrimingDiluent:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_PrimeReagent"), ScoutUtilities.Misc.ConvertToString(2));
                    break;
                case ePrimeReagentLinesState.prl_Completed:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_PrimeComplete");
                    DispatcherHelper.ApplicationExecute(CompleteFinish);
                    break;
                case ePrimeReagentLinesState.prl_Failed:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_PrimeFailed");
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        _progressStatus = CallBackProgressStatus.IsError;
                        FaultError();
                    });
                    break;
            }
        }

        private void OnFlowCellFlushStateChanged(object sender, ApiEventArgs<eFlushFlowCellState> e)
        {
            if (!_enableFlushStatusListener)
            {
                return;
            }

            switch (e.Arg1)
            {
                case eFlushFlowCellState.ffc_FlushingCleaner:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_FlushClean");
                    break;
                case eFlushFlowCellState.ffc_FlushingConditioningSolution:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_FlushConditioningSolution");
                    break;
                case eFlushFlowCellState.ffc_FlushingBuffer:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_FlushBuffer");
                    break;
                case eFlushFlowCellState.ffc_FlushingAir:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_FlushAir");
                    break;
                case eFlushFlowCellState.ffc_Completed:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_FlushComplete");
                    DispatcherHelper.ApplicationExecute(CompleteFinish);
                    break;
                case eFlushFlowCellState.ffc_Failed:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_FlushFailed");
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        _progressStatus = CallBackProgressStatus.IsError;
                        FaultError();
                    });
                    break;
            }
        }

        private void OnCleanFluidicsStateChanged(object sender, ApiEventArgs<eFlushFlowCellState> e)
        {
            if (!_enableCleanStatusListener)
            {
                return;
            }

            switch (e.Arg1)
            {
                case eFlushFlowCellState.ffc_FlushingCleaner:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_FlushClean");
                    break;
                case eFlushFlowCellState.ffc_FlushingConditioningSolution:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_FlushConditioningSolution");
                    break;
                case eFlushFlowCellState.ffc_FlushingBuffer:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_FlushBuffer");
                    break;
                case eFlushFlowCellState.ffc_FlushingAir:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_FlushAir");
                    break;
                case eFlushFlowCellState.ffc_Completed:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_FlushComplete");
                    DispatcherHelper.ApplicationExecute(CompleteFinish);
                    break;
                case eFlushFlowCellState.ffc_Failed:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_FlushFailed");
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        _progressStatus = CallBackProgressStatus.IsError;
                        FaultError();
                    });
                    break;
            }
        }

        private void OnUpdateReagentView(SystemStatusDomain systemStatus)
        {
            if (!_instrumentStatusService.IsRunning())
            {
                DispatcherHelper.ApplicationExecute(RefreshReagentStatus);
            }
        }

        #endregion

        #region Commands

        #region Prime Command

        private RelayCommand _primeCommand;
        public RelayCommand PrimeCommand => _primeCommand ?? (_primeCommand = new RelayCommand(PrimeFluidLines, CanPrime));

        private bool CanPrime()
        {
            return IsHealthOk;
        }

        private void PrimeFluidLines()
        {
            try
            {
                _reagentCurrentProcess = ReagentProcess.Prime;
                _progressStatus = CallBackProgressStatus.IsStart;
                Status = LanguageResourceHelper.Get("LID_ProgressIndication_PRIMINGFLUIDLINES");
                _enablePrimeStatusListener = true;
                var primeStatus = _reagentModel.StartPrimeReagentLines();
                if (primeStatus.Equals(HawkeyeError.eSuccess))
                {
                    _progressStatus = CallBackProgressStatus.IsRunning;
                    IsProgressActive = true;
                    IsHealthOk = IsReplaceReagentActive = false;
                    // Showing prime process started message in time bar
                    PostToMessageHub(LanguageResourceHelper.Get("LID_ButtonContent_PrimeProcessStarted"));
                }
                else
                {
                    _enablePrimeStatusListener = false;
                    _progressStatus = CallBackProgressStatus.IsError;
                    ApiHawkeyeMsgHelper.ErrorCommon(primeStatus);
                }
            }
            catch (Exception ex)
            {
                _enablePrimeStatusListener = false;
                _progressStatus = CallBackProgressStatus.IsError;
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_PRIME_REAGENT_LINES"));
            }
        }

        #endregion

        #region Flush Command

        private RelayCommand _flushCommand;
        public RelayCommand FlushCommand => _flushCommand ?? (_flushCommand = new RelayCommand(FlushFluidLines, CanFlush));

        private bool CanFlush()
        {
            return IsHealthOk;
        }

        private void FlushFluidLines()
        {
            try
            {
                _reagentCurrentProcess = ReagentProcess.Flush;
                _progressStatus = CallBackProgressStatus.IsStart;
                Status = LanguageResourceHelper.Get("LID_ProgressIndication_FLUSHINGFLUIDLINES");

                _enableFlushStatusListener = true;
                var flushStatus = _reagentModel.StartFlushFlowCell();
                if (flushStatus.Equals(HawkeyeError.eSuccess))
                {
                    _progressStatus = CallBackProgressStatus.IsRunning;
                    IsProgressActive = true;
                    IsHealthOk = IsReplaceReagentActive = false;
                    // Showing flush process started message in time bar
                    PostToMessageHub(LanguageResourceHelper.Get("LID_ButtonContent_FlushProcessStarted"));
                }
                else
                {
                    _enableFlushStatusListener = false;
                    _progressStatus = CallBackProgressStatus.IsError;
                    ApiHawkeyeMsgHelper.ErrorCommon(flushStatus);
                }
            }
            catch (Exception ex)
            {
                _enableFlushStatusListener = false;
                _progressStatus = CallBackProgressStatus.IsError;
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_FLUSH_FLOW_CELL"));
            }
        }

        #endregion

        #region Clean Sequence Command

        private RelayCommand _cleanCommand;
        public RelayCommand CleanSequenceCommand => _cleanCommand ?? (_cleanCommand = new RelayCommand(StartCleanSequence, CanClean));

        private bool CanClean()
        {
            return IsHealthOk;
        }

        private void StartCleanSequence()
        {
            try
            {
                _reagentCurrentProcess = ReagentProcess.Clean;
                _progressStatus = CallBackProgressStatus.IsStart;
                Status = LanguageResourceHelper.Get("LID_ProgressIndication_CLEANSEQUENCE");

                _enableCleanStatusListener = true;
                var cleanStatus = _reagentModel.StartCleanFluidics();
                if (cleanStatus.Equals(HawkeyeError.eSuccess))
                {
                    _progressStatus = CallBackProgressStatus.IsRunning;
                    IsProgressActive = true;
                    IsHealthOk = IsReplaceReagentActive = false;
                    // Showing flush process started message in time bar
                    PostToMessageHub(LanguageResourceHelper.Get("LID_ButtonContent_CleanSequenceStarted"));
                }
                else
                {
                    _enableCleanStatusListener = false;
                    _progressStatus = CallBackProgressStatus.IsError;
                    ApiHawkeyeMsgHelper.ErrorCommon(cleanStatus);
                }
            }
            catch (Exception ex)
            {
                _enableCleanStatusListener = false;
                _progressStatus = CallBackProgressStatus.IsError;
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CLEAN_SYSTEM"));
            }
        }

        #endregion

        #region Decontaminate Command

        private RelayCommand _decontaminateCommand;
        public RelayCommand DecontaminateCommand => _decontaminateCommand ?? (_decontaminateCommand = new RelayCommand(Decontaminate, CanDecontaminate));

        private bool CanDecontaminate()
        {
            return IsHealthOk;
        }

        private void Decontaminate()
        {
            DialogEventBus.DecontaminateDialog(this, new DecontaminateEventArgs());

            Close(true);
        }

        #endregion

        #region Replace Reagent Command

        private RelayCommand _replaceReagentCommand;
        public RelayCommand ReplaceReagentCommand => _replaceReagentCommand ?? (_replaceReagentCommand = new RelayCommand(ReplaceReagent, CanReplaceReagent));

        private bool CanReplaceReagent()
        {
            return IsReplaceReagentActive;
        }

        private void ReplaceReagent()
        {
            DialogEventBus.ReplaceReagentPackDialog(this, new ReplaceReagentPackEventArgs(_reagentModel.PartNumber));
            Close(true);
        }

        #endregion

        public override bool Close(bool? result)
        {
            if (!_progressStatus.Equals(CallBackProgressStatus.IsFinish))
            {
                switch (_reagentCurrentProcess)
                {
                    case ReagentProcess.Flush:
                        if (DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_MSGBOX_FLushProgress")) != true) return false;
                        CancelFlush();
                        break;
                    case ReagentProcess.Prime:
                        if (DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_MSGBOX_PrimePropgress")) != true) return false;
                        CancelPrime();
                        break;

                    case ReagentProcess.Clean:
                        if (DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_MSGBOX_CleanInProgress")) != true) return false;
                        CancelClean();
                        break;
                }
                return false; // window isn't closing yet
            }
            
            return base.Close(result);
        }

        #endregion

        #region Public Methods

        public void RefreshReagentStatus()
        {
            try
            {
                _reagentModel.OnRefreshReagent();
                UpdateHealth(_reagentModel.ReagentHealth);
                if (_reagentModel.ReagentContainers.Any())
                {
                    var msg = new Notification<ReagentContainerStateDomain>(_reagentModel.ReagentContainers.FirstOrDefault(),
                        MessageToken.RefreshReagentStatus, "");
                    MessageBus.Default.Publish(msg);
                }
                for (int i = 0; i < ReagentContainers?.Count; i++)
                {
                    if (ReagentContainers[i].ReagentNames != null)
                    {
                        foreach (var reagentPack in ReagentContainers[i].ReagentNames)
                        {
                            if (reagentPack.Value != null && reagentPack.Value.FirstOrDefault() != null)
                                reagentPack.Value.FirstOrDefault().NotifyAllPropertiesChanged();
                        }
                    }
                    ReagentContainers[i].NotifyAllPropertiesChanged();
                }
                
                NotifyAllPropertiesChanged();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_REFRESH_REAGENT"));
            }
        }

        #endregion

        #region Private Methods

        private void UpdateHealth(ReagentContainerStatus reagentContainerStatus)
        {
            IsReplaceReagentActive = true;
            if (LoggedInUser.CurrentUser?.RoleName == null || _instrumentStatusService.IsRunning())
            {
                IsHealthOk = false;
                IsReplaceReagentActive = false;
                return;
            }

            switch (reagentContainerStatus)
            {
                case ReagentContainerStatus.eOK:
                    IsHealthOk = true;
                    break;
                case ReagentContainerStatus.eEmpty:
                case ReagentContainerStatus.eFaulted:
                case ReagentContainerStatus.eNotDetected:
                case ReagentContainerStatus.eExpired:
                case ReagentContainerStatus.eInvalid:
                case ReagentContainerStatus.eUnloading:
                case ReagentContainerStatus.eUnloaded:
                case ReagentContainerStatus.eLoading:
                    IsHealthOk = false;
                    break;
            }
        }

        private void CompleteFinish()
        {
            RefreshReagentStatus();
            _progressStatus = CallBackProgressStatus.IsFinish;
            IsProgressActive = false;
            IsHealthOk = IsReplaceReagentActive = true;
        }

        private void FaultError()
        {
            if (DialogEventBus.DialogBoxOk(this, Status) != true)
            {
                return;
            }

            switch (_reagentCurrentProcess)
            {
                case ReagentProcess.Flush:
                    CancelFlush();
                    break;
                case ReagentProcess.Prime:
                    CancelPrime();
                    break;
            }

            RefreshReagentStatus();
        }

        private void CancelFlush()
        {
            try
            {
                var cancelFLush = _reagentModel.CancelFlushFlowCell();
                if (cancelFLush.Equals(HawkeyeError.eSuccess))
                {
                    _progressStatus = CallBackProgressStatus.IsFinish;
                    IsProgressActive = false;
                    IsHealthOk = IsReplaceReagentActive = true;
                    // Showing flush process cancelled message in time bar
                    PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_FlushCancelled"));
                    return;
                }

                ApiHawkeyeMsgHelper.ErrorCommon(cancelFLush);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CANCEL_FLUSH_FLOW_CELL"));
            }
        }

        private void CancelClean()
        {
            try
            {
                var cancelClean = _reagentModel.CancelCleanSequence();
                if (cancelClean.Equals(HawkeyeError.eSuccess))
                {
                    _progressStatus = CallBackProgressStatus.IsFinish;
                    IsProgressActive = false;
                    IsHealthOk = IsReplaceReagentActive = true;
                    // Showing flush process cancelled message in time bar
                    PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_CleanCancelled"));
                    return;
                }

                ApiHawkeyeMsgHelper.ErrorCommon(cancelClean);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CANCELCLEAN"));
            }
        }

            private void CancelPrime()
        {
            try
            {
                var cancelPrime = _reagentModel.CancelPrimeReagentLines();
                if (cancelPrime.Equals(HawkeyeError.eSuccess))
                {
                    _progressStatus = CallBackProgressStatus.IsFinish;
                    IsProgressActive = false;
                    IsHealthOk = IsReplaceReagentActive = true;
                    // Showing prime process cancelled message in time bar
                    PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_PrimeCancelled"));
                    return;
                }

                ApiHawkeyeMsgHelper.ErrorCommon(cancelPrime);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CANCEL_PRIME_REAGENT_LINES"));
            }
        }

        // todo: remove after moving dialogs in the Commands above to the new dialog system
        private void OpenDialog(object objectWindow, object viewModel)
        {
            if (objectWindow == null) return;
            if (!(objectWindow is Window window)) return;

            window.DataContext = viewModel;
            window.ShowDialog();
        }

        #endregion
    }
}