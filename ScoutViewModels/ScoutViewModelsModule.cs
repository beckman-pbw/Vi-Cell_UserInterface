using Ninject.Extensions.Factory;
using Ninject.Modules;
using ScoutViewModels.Common;
using ScoutViewModels.Interfaces;
using ScoutViewModels.ViewModels;
using ScoutViewModels.ViewModels.CellTypes;
using ScoutViewModels.ViewModels.Dialogs;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;
using ScoutViewModels.ViewModels.Home;
using ScoutViewModels.ViewModels.QualityControl;
using ScoutViewModels.ViewModels.Reports;
using ScoutViewModels.ViewModels.Service;
using ScoutViewModels.ViewModels.Tabs;
using ScoutViewModels.ViewModels.Tabs.SettingsPanel;
using ScoutViewModels.ViewModels.Common;
using ScoutViewModels.ViewModels.Home.ExpandedSampleWorkflow;
using ScoutViewModels.ViewModels.Reports.ReportViewModels;
using ScoutViewModels.ViewModels.Service.ConcentrationSlope;
using ScoutViewModels.ViewModels.Service.ConcentrationSlope.DataTabs;

namespace ScoutViewModels
{
    public class ScoutViewModelsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<CellTypeViewModel>().ToSelf().InTransientScope();
            Bind<CreateCellViewModel>().ToSelf().InTransientScope();
            Bind<IScoutViewModelFactory>().ToFactory();
            Bind<QualityControlViewModel>().ToSelf().InTransientScope();
            Bind<QualityControlsReportViewModel>().ToSelf().InTransientScope();
            Bind<AddQualityControlDialogViewModel>().ToSelf().InTransientScope();
            Bind<ResultsQualityControlViewModel>().ToSelf().InTransientScope();
            Bind<ResultViewModel>().ToSelf().InTransientScope();
            Bind<LogsViewModel>().ToSelf().InTransientScope();
            Bind<LogPanelViewModel>().ToSelf().InTransientScope();
            Bind<ReportsPanelViewModel>().ToSelf().InTransientScope();
            Bind<TitleBarViewModel>().ToSelf().InSingletonScope();
            Bind<SampleSetViewModel>().ToSelf().InTransientScope();
            Bind<InstrumentStatusDialogViewModel>().ToSelf().InTransientScope();
            Bind<UserProfileDialogViewModel>().ToSelf().InTransientScope();
            Bind<InstrumentSettingsViewModel>().ToSelf().InSingletonScope();
            Bind<LanguageSettingsViewModel>().ToSelf().InSingletonScope();
            Bind<RunOptionSettingsViewModel>().ToSelf().InSingletonScope();
            Bind<SignatureSettingsViewModel>().ToSelf().InSingletonScope();
            Bind<SettingsViewModel>().ToSelf().InSingletonScope();
            Bind<SettingsTabViewModel>().ToSelf().InSingletonScope();
            Bind<HomeViewModel>().ToSelf().InSingletonScope();
            Bind<SampleResultDialogViewModel>().ToSelf().InTransientScope();
            Bind<ServiceViewModel>().ToSelf().InTransientScope();
            Bind<ConcentrationViewModel>().ToSelf().InTransientScope();
            
            Bind<AcupConcentrationSlopeViewModel>().ToSelf().InTransientScope();
            Bind<AcupSamplesPanelViewModel>().ToSelf().InSingletonScope();
            Bind<AcupDataPanelViewModel>().ToSelf().InSingletonScope();
            Bind<AcupSummaryTabViewModel>().ToSelf().InSingletonScope();
            Bind<AcupImagesTabViewModel>().ToSelf().InSingletonScope();
            Bind<AcupGraphsTabViewModel>().ToSelf().InSingletonScope();
            Bind<AcupHistoricalTabViewModel>().ToSelf().InSingletonScope();
            Bind<AcupConcentrationResultsViewModel>().ToSelf().InSingletonScope();

            Bind<ReagentStatusDialogViewModel>().ToSelf().InTransientScope();
            Bind<InstrumentStatusResultViewModel>().ToSelf().InTransientScope();
            Bind<LowLevelViewModel>().ToSelf().InTransientScope();
            Bind<MotorRegistrationViewModel>().ToSelf().InTransientScope();
            Bind<CreateSampleSetDialogViewModel>().ToSelf().InTransientScope();
            Bind<ReplaceReagentPackDialogViewModel>().ToSelf().InTransientScope();
            Bind<SetFocusDialogViewModel>().ToSelf().InTransientScope();
            Bind<ManualControlsOpticsViewModel>().ToSelf().InTransientScope();
            Bind<EmptySampleTubesDialogViewModel>().ToSelf().InTransientScope();
            Bind<SystemStatusViewModel>().ToSelf().InTransientScope();
            Bind<StorageTabViewModel>().ToSelf().InTransientScope();
            Bind<ReviewViewModel>().ToSelf().InTransientScope();
            Bind<ResultsRunResultsViewModel>().ToSelf().InTransientScope();
            Bind<ExportHelper>().ToSelf().InSingletonScope();
            Bind<SelectCellTypeViewModel>().ToSelf().InTransientScope();
            Bind<InstrumentStatusReportViewModel>().ToSelf().InTransientScope();
            Bind<SignInViewModel>().ToSelf().InTransientScope();
            Bind<ScheduledExportsViewModel>().ToSelf().InTransientScope();
            Bind<AuditLogScheduledExportsViewModel>().ToSelf().InTransientScope();
            Bind<ScheduledExportAddEditViewModel>().ToSelf().InTransientScope();

            Bind<IRunSampleHelper>().To<RunSampleHelper>().InTransientScope();
            Bind<OrphanSampleTemplateViewModel>().ToSelf().InTransientScope();
            Bind<UserSampleTemplateViewModel>().ToSelf().InTransientScope();
            Bind<CreateSampleTemplateViewModel>().ToSelf().InTransientScope();
        }
    }
}