using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System;
using ScoutUtilities.Common;
using System.Windows;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class DecontaminateDialogViewModel : BaseDialogViewModel
    {
        public DecontaminateDialogViewModel(DecontaminateEventArgs args, System.Windows.Window parentWindow) : base(args, parentWindow)
        {
            ShowDialogTitleBar = true;
            DialogTitle = LanguageResourceHelper.Get("LID_StatusBar_Decontaminate");

            IsProgressActivated = true;

            IsStepOneEnable = true;
            IsStepTwoEnable = false;
            Status = string.Empty;

            _reagentModel = new ReagentModel();
            _reagentModel.FlowCellDecontaminateStateChanged += HandleDecontaminateStateChanged;
        }

        protected override void DisposeUnmanaged()
        {
            if (_reagentModel != null)
                _reagentModel.FlowCellDecontaminateStateChanged -= HandleDecontaminateStateChanged;
            _reagentModel?.Dispose();
            base.DisposeUnmanaged();
        }

        #region Properties & Fields

        private ReagentModel _reagentModel;
        private CallBackProgressStatus _progressStatus;
        private bool _enableDecontaminateStatusListener;

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

        public string Status
        {
            get { return GetProperty<string>(); }
            set { DispatcherHelper.ApplicationExecute(() => SetProperty(value)); }
        }

        public bool IsProgressActivated
        {
            get { return GetProperty<bool>(); }
            set
            {
                DispatcherHelper.ApplicationExecute(() =>
                {
                    SetProperty(value);
                    NextCommand.RaiseCanExecuteChanged();
                    AcceptCommand.RaiseCanExecuteChanged();
                });
            }
        }

        public Visibility HasCarousel
        {
            get
            {
                if (HardwareManager.HardwareSettingsModel.InstrumentType == InstrumentType.CellHealth_ScienceModule)
                {
                    return Visibility.Collapsed;
                }

                return GetProperty<Visibility>();
            }

            set { SetProperty(value); }
        }

        #endregion

        #region Event Handlers

        private void HandleDecontaminateStateChanged(object sender, ApiEventArgs<eDecontaminateFlowCellState> e)
        {
            if (!_enableDecontaminateStatusListener)
            {
                return;
            }

            switch (e.Arg1)
            {
                case eDecontaminateFlowCellState.dfc_AspiratingBleach:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_DAspirating");
                    break;
                case eDecontaminateFlowCellState.dfc_Dispensing1:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_DdispensingStep"), ScoutUtilities.Misc.ConvertToString(1));
                    break;
                case eDecontaminateFlowCellState.dfc_DecontaminateDelay:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_DDEeay");
                    break;
                case eDecontaminateFlowCellState.dfc_Dispensing2:
                    Status = string.Format(LanguageResourceHelper.Get("LID_ReagentStatus_DdispensingStep"), ScoutUtilities.Misc.ConvertToString(2));
                    break;
                case eDecontaminateFlowCellState.dfc_FlushingBuffer:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_FlushBuffer");
                    break;
                case eDecontaminateFlowCellState.dfc_FlushingAir:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_FlushAir");
                    break;
                case eDecontaminateFlowCellState.dfc_Completed:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_Dcompalted");
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        IsProgressActivated = IsStepTwoEnable = true;
                        IsStepOneEnable = false;
                    });
                    break;
                case eDecontaminateFlowCellState.dfc_Failed:
                    Status = LanguageResourceHelper.Get("LID_ReagentStatus_DFailed");
                    DialogEventBus.DialogBoxOk(this, Status);
                    DispatcherHelper.ApplicationExecute(() => Close(false));
                    break;
            }
        }

        #endregion

        #region Commands

        private RelayCommand _nextCommand;
        public RelayCommand NextCommand => _nextCommand ?? (_nextCommand = new RelayCommand(NextDecontaminateStep, CanPerformNext));

        private bool CanPerformNext()
        {
            return IsProgressActivated;
        }

        private void NextDecontaminateStep()
        {
            try
            {
                _progressStatus = CallBackProgressStatus.IsStart;
                Status = LanguageResourceHelper.Get("LID_ProgressIndication_DECONTAMINATE");
                _enableDecontaminateStatusListener = true;
                var status = _reagentModel.StartDecontaminateFlowCell();
                if (status.Equals(HawkeyeError.eSuccess))
                {
                    _progressStatus = CallBackProgressStatus.IsRunning;
                    IsProgressActivated = false;

                    // Showing decontaminate process started message in time bar
                    PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_DecontaminateProcessStarted"));
                }
                else
                {
                    _enableDecontaminateStatusListener = false;
                    _progressStatus = CallBackProgressStatus.IsError;
                    ApiHawkeyeMsgHelper.ErrorCommon(status);
                }
            }
            catch (Exception ex)
            {
                _enableDecontaminateStatusListener = false;
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_UNABLE_TO_DECONTAMINATE"));
            }
        }

        public override bool CanAccept()
        {
            return IsProgressActivated;
        }

        protected override void OnAccept()
        {
            // why are we setting these variables and then closing? Why not just close?
            IsProgressActivated = false;
            IsProgressActivated = true;
            _progressStatus = CallBackProgressStatus.IsFinish;
            Close(true);
        }

        protected override void OnCancel()
        {
            try
            {
                if (_progressStatus.Equals(CallBackProgressStatus.IsRunning))
                {
                    if (DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_MSGBOX_DecontaminationProgress")) != true)
                    {
                        return; // don't close dialog
                    }

                    var status = _reagentModel.CancelDecontaminateFlowCell();
                    if (status.Equals(HawkeyeError.eSuccess))
                    {
                        // Showing decontaminate process cancelled message in time bar
                        PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_DeCancelled"));
                    }
                    else
                    {
                        ApiHawkeyeMsgHelper.ErrorCommon(status);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_UNABLE_TO_CLOSE_WINDOW"));
            }

            IsStepOneEnable = IsStepTwoEnable = false;
            
            base.OnCancel();
        }

        #endregion
    }
}