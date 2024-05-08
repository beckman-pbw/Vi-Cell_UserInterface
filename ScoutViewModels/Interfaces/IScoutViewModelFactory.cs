using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.Common;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutModels.Common;
using ScoutModels.Interfaces;
using ScoutModels.Settings;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Interfaces;
using ScoutViewModels.ViewModels;
using ScoutViewModels.ViewModels.CellTypes;
using ScoutViewModels.ViewModels.Dialogs;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;
using ScoutViewModels.ViewModels.Home;
using ScoutViewModels.ViewModels.Home.ExpandedSampleWorkflow;
using ScoutViewModels.ViewModels.QualityControl;
using ScoutViewModels.ViewModels.Reports;
using ScoutViewModels.ViewModels.Service;
using ScoutViewModels.ViewModels.Tabs;
using ScoutViewModels.ViewModels.Tabs.SettingsPanel;
using ScoutViewModels.Common;
using ScoutViewModels.ViewModels.Common;
using ScoutDomains.Reports.Common;
using ScoutUtilities.Enums;
using System.Collections.Generic;
using System.Windows;
using ScoutDomains.Reports.ScheduledExports;
using ScoutServices.Interfaces;
using ScoutViewModels.ViewModels.Reports.ReportViewModels;
using ScoutViewModels.ViewModels.Service.ConcentrationSlope;
using ScoutViewModels.ViewModels.Service.ConcentrationSlope.DataTabs;

namespace ScoutViewModels.Interfaces
{
    public interface IScoutViewModelFactory
    {
        CellTypeViewModel CreateCellTypeViewModel(UserDomain currentUser);
        CreateCellViewModel CreateCreateCellTypeViewModel(UserDomain currentUser);
        OpenCellTypeViewModel CreateOpenCellTypeViewModel(UserDomain currentUser);
        QualityControlViewModel CreateQualityControlViewModel();

        QualityControlsReportViewModel CreateQualityControlsReportViewModel(string printTitle, string comments,
            QualityControlDomain selectedQualityControl, List<LineGraphDomain> graphListForReport);
        AddQualityControlDialogViewModel CreateAddQualityControlDialogViewModel(AddCellTypeEventArgs args, System.Windows.Window parentWindow);

        ResultsQualityControlViewModel CreateResultsQualityControlViewModel();
        ResultViewModel CreateResultViewModel();
        LogsViewModel CreateLogsViewModel();
        LogPanelViewModel CreateLogPanelViewModel(int reportId);
        ReportsPanelViewModel CreateReportsPanelViewModel();
        TitleBarViewModel CreateTitleBarViewModel();
        SampleSetViewModel CreateSampleSetViewModel(ResultRecordHelper resultRecordHelper, SampleTemplateViewModel template,
            string createdByUser, string runByUser, string sampleSetName, bool isOrphanSet = false, bool isExpanded = false);
        SampleSetViewModel CreateSampleSetViewModel(SampleSetDomain setDomain, bool isOrphanSet, bool isExpanded = false, bool createForAutomationSampleSet = false);
        InstrumentStatusDialogViewModel CreateInstrumentStatusDialogViewModel(InstrumentStatusEventArgs args, Window parentWindow);
        UserProfileDialogViewModel CreateUserProfileDialogViewModel(UserProfileDialogEventArgs args, Window parentWindow);
        DeleteSampleResultsViewModel CreateDeleteSampleResultsViewModel(DeleteSampleResultsEventArgs args, Window parentWindow);
        InstrumentSettingsViewModel CreateInstrumentSettingsViewModel();
        SignatureSettingsViewModel CreateSignatureSettingsViewModel();
        RunOptionSettingsViewModel CreateRunOptionSettingsViewModel();
        LanguageSettingsViewModel CreateLanguageSettingsViewModel();
        SettingsViewModel CreateSettingsViewModel();
        SettingsTabViewModel CreateSettingsTabViewModel();
        ReagentStatusDialogViewModel CreateReagentStatusDialogViewModel(ReagentStatusEventArgs args, Window parentWindow);
        HomeViewModel CreateHomeViewModel();
        SampleResultDialogViewModel CreateSampleResultDialogViewModel(SampleResultDialogEventArgs<SampleViewModel> args, Window parentWindow);
        
        ServiceViewModel CreateServiceViewModel();
        ConcentrationViewModel CreateConcentrationViewModel();
        AcupConcentrationSlopeViewModel CreateAcupConcentrationSlopeViewModel();
        AcupSamplesPanelViewModel CreateAcupSamplesPanelViewModel();
        AcupDataPanelViewModel CreateAcupDataPanelViewModel();
        AcupSummaryTabViewModel CreateAcupSummaryTabViewModel();
        AcupImagesTabViewModel CreateAcupImagesTabViewModel();
        AcupGraphsTabViewModel CreateAcupGraphsTabViewModel();
        AcupHistoricalTabViewModel CreateAcupHistoricalTabViewModel();
        AcupConcentrationResultsViewModel CreateAcupConcentrationResultsViewModel();

        InstrumentStatusResultViewModel CreateInstrumentStatusResultViewModel();
        LowLevelViewModel CreateLowLevelViewModel();
        MotorRegistrationViewModel CreateMotorRegistrationViewModel();

        CreateSampleSetDialogViewModel CreateCreateSampleSetDialogViewModel(CreateSampleSetEventArgs<SampleSetViewModel> args,
            Window parentWindow,
            ISolidColorBrushService colorBrushService, RunOptionSettingsModel runOptionSettings,
            IAutomationSettingsService automationSettingsService);

        ReplaceReagentPackDialogViewModel CreateReplaceReagentPackDialogViewModel(ReplaceReagentPackEventArgs args, System.Windows.Window parentWindow);
        SetFocusDialogViewModel CreateSetFocusDialogViewModel(SetFocusEventArgs args, Window parentWindow);
        ManualControlsOpticsViewModel CreateManualControlsOpticsViewModel();
        EmptySampleTubesDialogViewModel CreateEmptySampleTubesDialogViewModel(EmptySampleTubesEventArgs args, System.Windows.Window parentWindow);
        SystemStatusViewModel CreateSystemStatusViewModel();
        StorageTabViewModel CreateStorageTabViewModel();
        ReviewViewModel CreateReviewViewModel();
        ResultsRunResultsViewModel CreateResultsRunResultsViewModel();
        ExportHelper CreateExportHelper();

        SelectCellTypeViewModel CreateSelectCellTypeViewModel(SelectCellTypeEventArgs args,
            System.Windows.Window parentWindow);

        InstrumentStatusReportViewModel CreateInstrumentStatusReportViewModel(string printTitle, string comments,
            SystemStatusDomain systemStatus,
            List<ReportPrintOptions> reportPrintOptions, List<CalibrationActivityLogDomain> calErrorLogs,
            List<CalibrationActivityLogDomain> aCupCalErrorLogs, List<UserDomain> userList,
            IList<ReagentContainerStateDomain> reagentContainers, string analysisType);

        SignInViewModel CreateSignInViewModel(bool usernameIsLocked);

        ScheduledExportsViewModel CreateScheduledExportsViewModel();
        AuditLogScheduledExportsViewModel CreateAuditLogScheduledExportsViewModel();
        ScheduledExportAddEditViewModel CreateScheduledExportAddEditViewModel(ScheduledExportDialogEventArgs<SampleResultsScheduledExportDomain> args, Window parentWindow);
        AdvanceSampleSettingsDialogViewModel CreateAdvancedSampleSettingsDialogViewModel(AdvanceSampleSettingsDialogEventArgs<AdvancedSampleSettingsViewModel> args, Window parentWindow);
        ScheduledExportAuditLogsAddEditDialogViewModel CreateScheduledExportAuditLogsAddEditViewModel(ScheduledExportAuditLogsDialogEventArgs<AuditLogScheduledExportDomain> args, Window parentWindow);
        OrphanSampleTemplateViewModel CreateOrphanSampleTemplateViewModel();
        UserSampleTemplateViewModel CreateUserSampleTemplateViewModel();
        CreateSampleTemplateViewModel CreateCreateSampleTemplateViewModel();
        OpenSampleDialogViewModel CreateOpenSampleDialogViewModel(OpenSampleEventArgs args, System.Windows.Window parentWindow);
    }
}
