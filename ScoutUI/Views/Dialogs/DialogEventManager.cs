using ScoutDataAccessLayer.DAL;
using ScoutDomains.Analysis;
using ScoutDomains.Reports.ScheduledExports;
using ScoutModels.Settings;
using ScoutServices.Interfaces;
using ScoutUI.Views.Review;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Events;
using ScoutUtilities.Services;
using ScoutViewModels.Interfaces;
using ScoutViewModels.ViewModels.Common;
using ScoutViewModels.ViewModels.Dialogs;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;
using System.Collections.Generic;
using System.Windows;

namespace ScoutUI.Views.Dialogs
{
    public class DialogEventManager : BaseEventManager
    {
        #region Constructor

        public DialogEventManager(Window parent, IScoutViewModelFactory viewModelFactory) : base(parent)
        {
            _viewModelFactory = viewModelFactory;
            DialogEventBus.GetUiDispatcher = () => ParentWindow.Dispatcher;

            DialogEventBus.LoginRequested += HandleLoginRequest;
            DialogEventBus.DialogBoxRequested += HandleDialogBoxRequest;
            DialogEventBus.MessageHubRequested += HandleMessageHubRequest;
            DialogEventBus.UserProfileRequested += HandleUserProfileRequest;
            DialogEventBus.ChangePasswordRequested += HandleChangePasswordRequest;
            DialogEventBus.LargeLoadingScreenRequested += HandleLargeLoadingScreenRequest;
            DialogEventBus.AddSignatureRequested += HandleAddSignatureRequest;
            DialogEventBus.SaveCellTypeDialogRequested += HandleSaveCellTypeDialogRequest;
            DialogEventBus.SelectCellTypeDialogRequested += HandleSelectCellTypeDialogRequest;
            DialogEventBus.ExpandedImageGraphRequested += HandleExpandedImageGraphRequest;
            DialogEventBus.DeleteSampleResultsRequested += HandleDeleteSampleResultsRequest;
            DialogEventBus.ExportSampleResultsRequested += HandleExportSampleResultsRequest;
            DialogEventBus.EmptySampleTubesRequested += HandleEmptySampleTubesRequest;
            DialogEventBus.InstrumentStatusRequested += HandleInstrumentStatusRequest;
            DialogEventBus.SystemLockDialogRequested += HandleSystemLockDialogRequest;
            DialogEventBus.ReagentStatusRequested += HandleReagentStatusRequest;
            DialogEventBus.DustReferenceRequested += HandleDustReferenceRequest;
            DialogEventBus.SetFocusRequested += HandleSetFocusRequest;
            DialogEventBus.DecontaminateRequested += HandleDecontaminateRequest;
            DialogEventBus.ReplaceReagentPackRequested += HandleReplaceReagentPackRequest;
            DialogEventBus.AddCellTypeRequested += HandleAddCellTypeRequest;
            DialogEventBus.ExitUiDialogRequested += HandleExitUiDialogRequest;
            DialogEventBus.OpenSampleDialogRequested += HandleOpenSampleDialogRequest;
            DialogEventBus.InactivityDialogRequested += HandleInactivityDialogRequest;
            DialogEventBus<SampleSetViewModel>.CreateSampleSetDialogRequested += HandleCreateSampleSetDialogRequest;
            DialogEventBus<SampleViewModel>.SampleResultsDialogRequested += HandleSampleResultsDialogRequest;
            DialogEventBus.SequentialNamingDialogRequested += HandleSequentialNamingDialogRequest;
            DialogEventBus.FilterSampleSetsDialogRequested += HandleFilterSampleSetsDialogRequested;
            DialogEventBus<AdvancedSampleSettingsViewModel>.AdvanceSampleSettingsDialogRequested += HandleAdvanceSampleSettingsDialogRequested;
            DialogEventBus.InfoDialogRequested += HandleInfoDialogRequest;
            DialogEventBus.ActiveDirectoryConfigDialogRequested += HandleActiveDirectoryConfigDialogRequest;
            DialogEventBus<UserDomain>.AddEditUserDialogRequested += HandleAddEditUserDialogRequest;
            DialogEventBus.EditSecuritySettingsDialogRequested += HandleEditSecuritySettingsDialogRequest;
            DialogEventBus<Dictionary<string, ColumnSettingViewModel>>.SampleSetSettingsDialogRequested += HandleSampleSetSettingsDialogRequest;
            DialogEventBus<SampleResultsScheduledExportDomain>.ScheduledExportDialogRequested += HandleScheduledExportDialogRequested;
            DialogEventBus<AuditLogScheduledExportDomain>.ScheduledExportAuditLogsDialogRequested += HandleScheduledExportAuditLogsDialogRequested;
        }

        #region Properties & Fields

        private IScoutViewModelFactory _viewModelFactory;

        #endregion

        protected override void DisposeUnmanaged()
        {
            DialogEventBus.GetUiDispatcher = null;

            DialogEventBus.LoginRequested -= HandleLoginRequest;
            DialogEventBus.DialogBoxRequested -= HandleDialogBoxRequest;
            DialogEventBus.MessageHubRequested -= HandleMessageHubRequest;
            DialogEventBus.UserProfileRequested -= HandleUserProfileRequest;
            DialogEventBus.ChangePasswordRequested -= HandleChangePasswordRequest;
            DialogEventBus.LargeLoadingScreenRequested -= HandleLargeLoadingScreenRequest;
            DialogEventBus.AddSignatureRequested -= HandleAddSignatureRequest;
            DialogEventBus.SaveCellTypeDialogRequested -= HandleSaveCellTypeDialogRequest;
            DialogEventBus.SelectCellTypeDialogRequested -= HandleSelectCellTypeDialogRequest;
            DialogEventBus.ExpandedImageGraphRequested -= HandleExpandedImageGraphRequest;
            DialogEventBus.DeleteSampleResultsRequested -= HandleDeleteSampleResultsRequest;
            DialogEventBus.ExportSampleResultsRequested -= HandleExportSampleResultsRequest;
            DialogEventBus.EmptySampleTubesRequested -= HandleEmptySampleTubesRequest;
            DialogEventBus.InstrumentStatusRequested -= HandleInstrumentStatusRequest;
            DialogEventBus.SystemLockDialogRequested -= HandleSystemLockDialogRequest;
            DialogEventBus.ReagentStatusRequested -= HandleReagentStatusRequest;
            DialogEventBus.DustReferenceRequested -= HandleDustReferenceRequest;
            DialogEventBus.SetFocusRequested -= HandleSetFocusRequest;
            DialogEventBus.DecontaminateRequested -= HandleDecontaminateRequest;
            DialogEventBus.ReplaceReagentPackRequested -= HandleReplaceReagentPackRequest;
            DialogEventBus.AddCellTypeRequested -= HandleAddCellTypeRequest;
            DialogEventBus.ExitUiDialogRequested -= HandleExitUiDialogRequest;
            DialogEventBus.OpenSampleDialogRequested -= HandleOpenSampleDialogRequest;
            DialogEventBus.InactivityDialogRequested -= HandleInactivityDialogRequest;
            DialogEventBus<SampleSetViewModel>.CreateSampleSetDialogRequested -= HandleCreateSampleSetDialogRequest;
            DialogEventBus<SampleViewModel>.SampleResultsDialogRequested -= HandleSampleResultsDialogRequest;
            DialogEventBus.SequentialNamingDialogRequested -= HandleSequentialNamingDialogRequest;
            DialogEventBus.FilterSampleSetsDialogRequested -= HandleFilterSampleSetsDialogRequested;
            DialogEventBus<AdvancedSampleSettingsViewModel>.AdvanceSampleSettingsDialogRequested -= HandleAdvanceSampleSettingsDialogRequested;
            DialogEventBus.InfoDialogRequested -= HandleInfoDialogRequest;
            DialogEventBus.ActiveDirectoryConfigDialogRequested -= HandleActiveDirectoryConfigDialogRequest;
            DialogEventBus<UserDomain>.AddEditUserDialogRequested -= HandleAddEditUserDialogRequest;
            DialogEventBus.EditSecuritySettingsDialogRequested -= HandleEditSecuritySettingsDialogRequest;
            DialogEventBus<Dictionary<string, ColumnSettingViewModel>>.SampleSetSettingsDialogRequested -= HandleSampleSetSettingsDialogRequest;
            DialogEventBus<SampleResultsScheduledExportDomain>.ScheduledExportDialogRequested -= HandleScheduledExportDialogRequested;
            DialogEventBus<AuditLogScheduledExportDomain>.ScheduledExportAuditLogsDialogRequested -= HandleScheduledExportAuditLogsDialogRequested;
            base.DisposeUnmanaged();
        }

        #endregion

        #region Bus Handlers

        private void HandleLoginRequest(object sender, LoginEventArgs args)
        {
            var vm = new LoginViewModel(args, ParentWindow);
            ShowDialog(() => new Login(vm), vm, args);
            
            // set the results to return (by updating the event args)
            args.DisplayedUsername = vm.DisplayedUsername; // username may have changed
            args.LoginResult = vm.LoginResult;

            if (args.ReturnPassword) args.Password = vm.Password;
            else args.Password = string.Empty;
        }

        private void HandleDialogBoxRequest(object sender, DialogBoxEventArgs args)
        {
            var vm = new DialogBoxViewModel(args, ParentWindow);
            ShowDialog(() => new DialogBox(vm), vm, args);

            if (args.ShowTextEntry) args.TextEntered = vm.TextEntry;
            else args.TextEntered = string.Empty;

            if (args.ShowCheckBox) args.DeleteAllACupData = vm.DeleteAllACupData;
            else args.DeleteAllACupData = false;
        }

        private void HandleMessageHubRequest(object sender, MessageHubEventArgs args)
        {
            var vm = new MessageHubViewModel(args, ParentWindow);
            ShowDialog(() => new MessageHubView(vm), vm, args);
        }

        private void HandleUserProfileRequest(object sender, UserProfileDialogEventArgs args)
        {
            var vm = _viewModelFactory.CreateUserProfileDialogViewModel(args, ParentWindow);
            ShowDialog(() => new UserProfileDialog(vm), vm, args);
        }

        private void HandleChangePasswordRequest(object sender, ChangePasswordEventArgs args)
        {
            var vm = new ChangePasswordViewModel(args, ParentWindow);
            ShowDialog(() => new ChangePasswordDialog(vm), vm, args);
        }

        private void HandleLargeLoadingScreenRequest(object sender, LargeLoadingScreenEventArgs args)
        {
            var vm = new LoadingIndicatorViewModel(args, ParentWindow);
            ShowDialog(() => new LoadingIndicator(vm), vm, args);
        }

        private void HandleAddSignatureRequest(object sender, AddSignatureEventArgs args)
        {
            var vm = new AddSignatureViewModel(args, ParentWindow);
            ShowDialog(() => new AddSignatureDialog(vm), vm, args);

            // set the results to return (by updating the event args)
            args.SignatureSelected = vm.SelectedSignatureType;
            args.SignaturePassword = args.ReturnPassword ? vm.SignaturePassword : string.Empty;
        }

        private void HandleSaveCellTypeDialogRequest(object sender, SaveCellTypeEventArgs args)
        {
            var vm = new SaveCellTypeViewModel(args, ParentWindow);
            ShowDialog(() => new SaveCellTypeDialog(vm), vm, args);
        }

        private void HandleSelectCellTypeDialogRequest(object sender, SelectCellTypeEventArgs args)
        {
            var vm = _viewModelFactory.CreateSelectCellTypeViewModel(args, ParentWindow);
            ShowDialog(() => new SelectCellTypeView(vm), vm, args);

            // set the results to return (by updating the event args)
            args.SelectedCellTypeDomain = vm.SelectedCellType;
        }

        private void HandleExpandedImageGraphRequest(object sender, ExpandedImageGraphEventArgs args)
        {
            var vm = new ExpandedImageGraphViewModel(args, ParentWindow);
            ShowDialog(() => new ExpandedImageGraphDialog(vm), vm, args);

            // set the results to return (by updating the event args)
            args.SelectedImageIndex = vm.CurrentImageIndex;
            args.SelectedGraphIndex = vm.CurrentGraphIndex;
        }

        private void HandleDeleteSampleResultsRequest(object sender, DeleteSampleResultsEventArgs args)
        {
            var vm = _viewModelFactory.CreateDeleteSampleResultsViewModel(args, ParentWindow);
            ShowDialog(() => new DeleteSampleResultsDialog(vm), vm, args);
        }

        private void HandleExportSampleResultsRequest(object sender, ExportSampleResultEventArgs args)
        {
            var vm = new ExportSampleResultViewModel(args, ParentWindow);
            ShowDialog(() => new ExportSampleResultDialog(vm), vm, args);
        }

        private void HandleEmptySampleTubesRequest(object sender, EmptySampleTubesEventArgs args)
        {
            var vm = _viewModelFactory.CreateEmptySampleTubesDialogViewModel(args, ParentWindow);
            ShowDialog(() => new EmptySampleTubesDialog(vm), vm, args);
        }

        private void HandleInstrumentStatusRequest(object sender, InstrumentStatusEventArgs args)
        {
            var vm = _viewModelFactory.CreateInstrumentStatusDialogViewModel(args, ParentWindow);
            ShowDialog(() => new InstrumentStatusDialog(vm), vm, args);
        }

        private void HandleSystemLockDialogRequest(object sender, BaseDialogEventArgs args)
        {
            var vm = new SystemLockDialogViewModel(args, ParentWindow);
            ShowDialog(() => new SystemLockDialog(vm), vm, args);
        }

        private void HandleReagentStatusRequest(object sender, ReagentStatusEventArgs args)
        {
            var vm = _viewModelFactory.CreateReagentStatusDialogViewModel(args, ParentWindow);
            ShowDialog(() => new ReagentStatusDialog(vm), vm, args);
        }

        private void HandleDustReferenceRequest(object sender, DustReferenceEventArgs args)
        {
            var vm = new DustReferenceDialogViewModel(args, ParentWindow);
            ShowDialog(() => new DustReferenceDialog(vm), vm, args);
        }

        private void HandleSetFocusRequest(object sender, SetFocusEventArgs args)
        {
            var vm = _viewModelFactory.CreateSetFocusDialogViewModel(args, ParentWindow);
            ShowDialog(() => new SetFocusDialog(vm), vm, args);
        }

        private void HandleDecontaminateRequest(object sender, DecontaminateEventArgs args)
        {
            var vm = new DecontaminateDialogViewModel(args, ParentWindow);
            ShowDialog(() => new DecontaminateDialog(vm), vm, args);
        }

        private void HandleReplaceReagentPackRequest(object sender, ReplaceReagentPackEventArgs args)
        {
            var vm = _viewModelFactory.CreateReplaceReagentPackDialogViewModel(args, ParentWindow);
            ShowDialog(() => new ReplaceReagentPackDialog(vm), vm, args);
        }

        private void HandleAddCellTypeRequest(object sender, AddCellTypeEventArgs args)
        {
            var vm = _viewModelFactory.CreateAddQualityControlDialogViewModel(args, ParentWindow);
            ShowDialog(() => new AddCellTypeDialog(vm), vm, args);

            // update args for the dialog caller
            if (args.DialogResult == true)
                args.NewQualityControlName = vm.QualityControl.QcName;
        }

        private void HandleExitUiDialogRequest(object sender, ExitUiDialogEventArgs args)
        {
            var vm = new ExitUiDialogViewModel(args, ParentWindow);
            ShowDialog(() => new ExitUiDialog(vm), vm, args);
        }

        private void HandleOpenSampleDialogRequest(object sender, OpenSampleEventArgs args)
        {
            var vm = _viewModelFactory.CreateOpenSampleDialogViewModel(args, ParentWindow);
            ShowDialog(() => new OpenSampleDialog(vm), vm, args);

            // update args for the dialog caller
            if (args.DialogResult == true)
                args.SelectedSampleRecord = vm.SelectedSampleRecord;
        }

        private void HandleInactivityDialogRequest(object sender, InactivityEventArgs args)
        {
            var vm = new InactivityDialogViewModel(args, ParentWindow);
            vm.SetParentWindowPositions(ParentWindow);
            ShowDialog(() => new InactivityDialog(vm), vm, args);
        }

        private void HandleCreateSampleSetDialogRequest(object sender, CreateSampleSetEventArgs<SampleSetViewModel> args)
        {
            var vm = _viewModelFactory.CreateCreateSampleSetDialogViewModel(args, ParentWindow, new SolidColorBrushService(), 
                new RunOptionSettingsModel(XMLDataAccess.Instance, args.CreatedByUser),
                new AutomationSettingsService());
            ShowDialog(() => new CreateSampleSetDialog(vm), vm, args);

            args.NewSampleSet = vm.SampleSet;
        }

        private void HandleSampleResultsDialogRequest(object sender, SampleResultDialogEventArgs<SampleViewModel> args)
        {
            var vm = _viewModelFactory.CreateSampleResultDialogViewModel(args, ParentWindow);
            ShowDialog(() => new SampleResultsDialog(vm), vm, args);
        }

        private void HandleSequentialNamingDialogRequest(object sender, SequentialNamingEventArgs args)
        {
            var vm = new SequentialNamingDialogViewModel(args, ParentWindow);
            ShowDialog(() => new SequentialNamingDialog(vm), vm, args);

            args.SeqNamingItems = vm.SequentialNamingSet.GetSequentialNamingItems();
            args.UseSequencing = vm.UseSequencing;
            args.TextItemIsFirst = vm.SequentialNamingSet.TextFirst;
        }

        private void HandleFilterSampleSetsDialogRequested(object sender, FilterSampleSetsEventArgs args)
        {
            var vm = new FilterSampleSetsViewModel(args, ParentWindow);
            ShowDialog(() => new FilterSampleSetsDialog(vm), vm, args);
        }

        private void HandleAdvanceSampleSettingsDialogRequested(object sender, 
            AdvanceSampleSettingsDialogEventArgs<AdvancedSampleSettingsViewModel> args)
        {
            var vm = _viewModelFactory.CreateAdvancedSampleSettingsDialogViewModel(args, ParentWindow);
            ShowDialog(() => new AdvanceSampleSettingsDialog(vm), vm, args);

            args.AdvancedSampleSettingsViewModel = vm.AdvancedSampleSettingsVm;
        }

        private void HandleInfoDialogRequest(object sender, InfoDialogEventArgs args)
        {
            var vm = new InfoDialogViewModel(args, ParentWindow);
            ShowDialog(() => new InfoDialog(vm), vm, args);
        }

        private void HandleActiveDirectoryConfigDialogRequest(object sender, ActiveDirectoryConfigDialogEventArgs args)
        {
            var vm = new ActiveDirectoryConfigDialogViewModel(args, ParentWindow);
            ShowDialog(() => new ActiveDirectoryConfigDialog(vm), vm, args);
        }

        private void HandleAddEditUserDialogRequest(object sender, AddEditUserDialogEventArgs<UserDomain> args)
        {
            var vm = new AddEditUserDialogViewModel(args, ParentWindow);
            ShowDialog(() => new AddEditUserDialog(vm), vm, args);
        }

        private void HandleEditSecuritySettingsDialogRequest(object sender, EditSecuritySettingsDialogEventArgs args)
        {
            var vm = new EditSecuritySettingsDialogViewModel(args, ParentWindow);
            ShowDialog(() => new EditSecuritySettingsDialog(vm), vm, args);

            if (args.DialogResult == true)
            {
                args.SelectedSecurityType = vm.SelectedSecurityType;
                args.InactivityTimeoutMinutes = vm.InactivityTimeout;
                args.PasswordChangeDays = vm.PasswordExpiry;
            }
        }

        private void HandleSampleSetSettingsDialogRequest(object sender, 
            SampleSetSettingsDialogEventArgs<Dictionary<string, ColumnSettingViewModel>> args)
        {
            var vm = new SampleSetSettingsDialogViewModel(args, ParentWindow);
            ShowDialog(() => new SampleSetSettingsDialog(vm), vm, args);
            
            if (args.DialogResult == true)
            {
                args.ColumnSettingViewModelDictionary = vm.Columns;
            }
        }

        private void HandleScheduledExportDialogRequested(object sender,
            ScheduledExportDialogEventArgs<SampleResultsScheduledExportDomain> args)
        {
            var vm = _viewModelFactory.CreateScheduledExportAddEditViewModel(
                args, ParentWindow);
            ShowDialog(() => new ScheduledExportAddEditDialog(vm), vm, args);
        }

        private void HandleScheduledExportAuditLogsDialogRequested(object sender,
            ScheduledExportAuditLogsDialogEventArgs<AuditLogScheduledExportDomain> args)
        {
            var vm = _viewModelFactory.CreateScheduledExportAuditLogsAddEditViewModel(
                args, ParentWindow);
            ShowDialog(() => new ScheduledExportAuditLogsAddEditDialog(vm), vm, args);
        }

        #endregion
    }
}
