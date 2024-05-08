using HawkeyeCoreAPI.Interfaces;
using Moq;
using Ninject;
using Ninject.Extensions.Factory;
using NUnit.Framework;
using ScoutDataAccessLayer.IDAL;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutModels;
using ScoutModels.ExpandedSampleWorkflow;
using ScoutModels.Interfaces;
using ScoutModels.Service;
using ScoutModels.Settings;
using ScoutServices;
using ScoutServices.Interfaces;
using ScoutServices.Service.ConcentrationSlope;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutViewModels.Common;
using ScoutViewModels.Interfaces;
using ScoutViewModels.ViewModels;
using ScoutViewModels.ViewModels.CellTypes;
using ScoutViewModels.ViewModels.Common;
using ScoutViewModels.ViewModels.Dialogs;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;
using ScoutViewModels.ViewModels.Home;
using ScoutViewModels.ViewModels.QualityControl;
using ScoutViewModels.ViewModels.Reports;
using ScoutViewModels.ViewModels.Reports.ReportViewModels;
using ScoutViewModels.ViewModels.Service;
using ScoutViewModels.ViewModels.Service.ConcentrationSlope;
using ScoutViewModels.ViewModels.Service.ConcentrationSlope.DataTabs;
using ScoutViewModels.ViewModels.Tabs;
using ScoutViewModels.ViewModels.Tabs.SettingsPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using TestSupport;

namespace ScoutVmTests.Service
{
    [TestFixture]
    public class ConcentrationVmTests : BaseTest
    {
        private Mock<IErrorLog> _errorMock;
        private Mock<ISystemStatus> _instrStatusMock;
        private InstrumentStatusService _instrumentStatusService;
        private Mock<IConcentrationSlopeService> _concentrationService;
        private Mock<IRunSampleHelper> _runSampleMock;
        private IScoutViewModelFactory _viewModelFactory;
        private Mock<IDialogCaller> _dialogCallerMock;
        private Mock<ISampleProcessingService> _sampleProcessingServiceMock;

        [OneTimeSetUp]
        public void ClassInit()
        {
            Username = ApplicationConstants.ServiceUser;
            Password = "password";

            _instrStatusMock = new Mock<ISystemStatus>();
            _errorMock = new Mock<IErrorLog>();
            var loggerMock = new Mock<Ninject.Extensions.Logging.ILogger>();
            var sysStatusMock = new Mock<ISystemStatus>();
            var applicationStateServiceMock = new Mock<IApplicationStateService>();
            _instrumentStatusService = new InstrumentStatusService(_instrStatusMock.Object, _errorMock.Object, loggerMock.Object, applicationStateServiceMock.Object);

            _dialogCallerMock = new Mock<IDialogCaller>();
            _sampleProcessingServiceMock = new Mock<ISampleProcessingService>();

            SetUser(ApplicationConstants.ServiceUser, UserPermissionLevel.eService);

            _concentrationService = new Mock<IConcentrationSlopeService>();
            _concentrationService.Setup(m => m.GetAllCalibrations(It.IsAny<calibration_type>()))
                                 .Returns(new List<CalibrationActivityLogDomain>());
            _concentrationService.Setup(m => m.GetStandardConcentrationList())
                                 .Returns(CalibrationModel.GetStandardConcentrationList);
            _runSampleMock = new Mock<IRunSampleHelper>();

            Kernel = new StandardKernel();
            KernelLoadScoutServices();
            KernelLoadScoutVms();
            Kernel.Bind<IWorkListModel>().To<WorkListModel>().InSingletonScope();
            Kernel.Bind<ICapacityManager>().To<CapacityManager>().InSingletonScope();
            Kernel.Bind<IDisplayService>().To<DisplayService>().InSingletonScope();
            Kernel.Bind<IErrorLog>().To<HawkeyeCoreAPI.ErrorLog>().InSingletonScope();
            Kernel.Bind<ISystemStatus>().ToConstant(sysStatusMock.Object);
            Kernel.Bind<IInstrumentStatusService>().ToConstant(_instrumentStatusService);
            Kernel.Bind<IRunningWorkListModel>().To<RunningWorkListModel>().InSingletonScope();
            Kernel.Bind<IScoutModelsFactory>().ToFactory();
            Kernel.Bind<IRunSampleHelper>().ToConstant(_runSampleMock.Object);
            Kernel.Bind<IDialogCaller>().ToConstant(_dialogCallerMock.Object);
        }

        private void KernelLoadScoutServices()
        {
            Kernel.Bind<ILockManager>().To<LockManager>().InSingletonScope();
            Kernel.Bind<IOpcUaCfgManager>().To<OpcUaCfgManager>().InSingletonScope();
            Kernel.Bind<IDbSettingsService>().To<DbSettingsModel>().InSingletonScope();
            Kernel.Bind<ISmtpSettingsService>().To<SmtpSettingsModel>().InSingletonScope();
            Kernel.Bind<IAutomationSettingsService>().To<AutomationSettingsService>().InSingletonScope();
            Kernel.Bind<ISampleResultsManager>().To<SampleResultsManager>().InSingletonScope();
            Kernel.Bind<ICellTypeManager>().To<CellTypeManager>().InSingletonScope();
            Kernel.Bind<IConfigurationManager>().To<ConfigurationManager>().InSingletonScope();
            Kernel.Bind<IScheduledExportsService>().To<ScheduledExportsService>().InSingletonScope();
            Kernel.Bind<IUserService>().To<UserService>().InSingletonScope();
            Kernel.Bind<IAcupConcentrationService>().To<AcupConcentrationService>().InSingletonScope();

            Kernel.Bind<ISampleProcessingService>().ToConstant(_sampleProcessingServiceMock.Object);
            Kernel.Bind<IConcentrationSlopeService>().ToConstant(_concentrationService.Object);
        }

        private void KernelLoadScoutVms()
        {
            Kernel.Bind<CellTypeViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<CreateCellViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<IScoutViewModelFactory>().ToFactory();
            Kernel.Bind<QualityControlViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<QualityControlsReportViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<AddQualityControlDialogViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<ResultsQualityControlViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<ResultViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<LogsViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<LogPanelViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<ReportsPanelViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<TitleBarViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<SampleSetViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<InstrumentStatusDialogViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<UserProfileDialogViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<InstrumentSettingsViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<LanguageSettingsViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<RunOptionSettingsViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<SignatureSettingsViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<SettingsViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<SettingsTabViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<HomeViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<SampleResultDialogViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<ServiceViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<ConcentrationViewModel>().ToSelf().InSingletonScope();

            Kernel.Bind<AcupConcentrationSlopeViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<AcupSamplesPanelViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<AcupDataPanelViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<AcupSummaryTabViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<AcupImagesTabViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<AcupGraphsTabViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<AcupHistoricalTabViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<AcupConcentrationResultsViewModel>().ToSelf().InSingletonScope();

            Kernel.Bind<ReagentStatusDialogViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<InstrumentStatusResultViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<LowLevelViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<MotorRegistrationViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<CreateSampleSetDialogViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<ReplaceReagentPackDialogViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<SetFocusDialogViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<ManualControlsOpticsViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<EmptySampleTubesDialogViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<SystemStatusViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<StorageTabViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<ReviewViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<ResultsRunResultsViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<ExportHelper>().ToSelf().InSingletonScope();
            Kernel.Bind<SelectCellTypeViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<InstrumentStatusReportViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<SignInViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<ScheduledExportsViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<AuditLogScheduledExportsViewModel>().ToSelf().InTransientScope();
            Kernel.Bind<ScheduledExportAddEditViewModel>().ToSelf().InTransientScope();
        }

        [SetUp]
        public void SetUp()
        {
            _errorMock.Reset();
            _instrStatusMock.Reset();
            _dialogCallerMock.Reset();
            _sampleProcessingServiceMock.Reset();
            SetupNullSampleProcessingSubscriptions(_sampleProcessingServiceMock);
            _viewModelFactory = Kernel.Get<IScoutViewModelFactory>();
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        public void TestACupConcentrationListInitialization_2M(int index)
        {
            var vm = _viewModelFactory.CreateAcupConcentrationSlopeViewModel();
            var aCupConcentrationList = vm.SamplesPanelViewModel.ConcentrationSamples;
            Assert.AreEqual(ApplicationConstants.NumberOfTubesInConcentration, vm.SamplesPanelViewModel.ConcentrationSamples.Count);
            Assert.AreEqual(ApplicationConstants.StartPosition2M,aCupConcentrationList[index].StartPosition);
            Assert.AreEqual(ApplicationConstants.EndPosition2M, aCupConcentrationList[index].EndPosition);
            Assert.AreEqual(ApplicationConstants.KnownConcentration2M,aCupConcentrationList[index].KnownConcentration);
        }

        [TestCase(10)]
        [TestCase(11)]
        [TestCase(12)]
        [TestCase(13)]
        [TestCase(14)]
        public void TestACupConcentrationListInitialization_4M(int index)
        {
            var vm = _viewModelFactory.CreateAcupConcentrationSlopeViewModel();
            var aCupConcentrationList = vm.SamplesPanelViewModel.ConcentrationSamples;
            Assert.AreEqual(ApplicationConstants.StartPosition4M, aCupConcentrationList[index].StartPosition);
            Assert.AreEqual(ApplicationConstants.EndPosition4M, aCupConcentrationList[index].EndPosition);
            Assert.AreEqual(ApplicationConstants.KnownConcentration4M, aCupConcentrationList[index].KnownConcentration);
        }

        [TestCase(15)]
        [TestCase(16)]
        [TestCase(17)]
        public void TestACupConcentrationListInitialization_10M(int index)
        {
            var vm = _viewModelFactory.CreateAcupConcentrationSlopeViewModel();
            var aCupConcentrationList = vm.SamplesPanelViewModel.ConcentrationSamples;
            Assert.AreEqual(ApplicationConstants.StartPosition10M, aCupConcentrationList[index].StartPosition);
            Assert.AreEqual(ApplicationConstants.EndPosition10M, aCupConcentrationList[index].EndPosition);
            Assert.AreEqual(ApplicationConstants.KnownConcentration10M, aCupConcentrationList[index].KnownConcentration);
        }

        [Test]
        public void TestIsStartACupConcSlopeButtonEnabled()
        {
            var vm = _viewModelFactory.CreateAcupConcentrationSlopeViewModel();
            Assert.IsNotNull(vm);
            vm.HandleNewCalibrationState(CalibrationGuiState.NotStarted);
            Assert.IsNotNull(vm.SamplesPanelViewModel.ConcentrationSamples);
            Assert.IsTrue(vm.SamplesPanelViewModel.ConcentrationSamples[0].IsActiveRow);
            Assert.IsFalse(vm.RunningConcentration);
        }

        [Test]
        public void TestDoesNotShowACupStopButtonIfNotRunning()
        {
            var vm = _viewModelFactory.CreateAcupConcentrationSlopeViewModel();
            Assert.IsNotNull(vm);
            vm.RunningConcentration = false;
            Assert.IsFalse(vm.RunningConcentration);
        }
        
        [Test]
        public void TestDoesShowACupStopButtonIfRunning()
        {
            // Arrange
            var runWorkListModelMock = new Mock<IRunningWorkListModel>();
            runWorkListModelMock
                .Setup(m => m.StartProcessing(Username, Password))
                .Returns(true);

            _concentrationService
                .Setup(m => m.ValidateConcentrationValues(
                    It.IsAny<IList<ICalibrationConcentrationListDomain>>(),
                    out It.Ref<string>.IsAny))
                .Returns(true);

            _dialogCallerMock
                .Setup(m => m.DialogBoxOkCancel(It.IsAny<object>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            
            _sampleProcessingServiceMock
                .Setup(m => m.CanProcessSamples(It.IsAny<string>(),
                    It.IsAny<IList<SampleSetDomain>>(), It.IsAny<bool>()))
                .Returns(SampleProcessingValidationResult.Valid);
            _sampleProcessingServiceMock
                .Setup(m => m.ProcessSamples(It.IsAny<IList<SampleSetDomain>>(), It.IsAny<string>(), It.IsAny<SampleSetTemplateDomain>(),
                    It.IsAny<IDataAccess>()))
                .Returns(true);

            // verify the initial state is correct
            var vm = _viewModelFactory.CreateAcupConcentrationSlopeViewModel();
            Assert.IsNotNull(vm);
            
            Assert.IsFalse(vm.RunningConcentration);
            Assert.IsFalse(vm.Aborted);
            Assert.IsFalse(vm.SamplesPanelViewModel.ConcentrationSamples.All(s => s.IsComplete));

            // vm.StopConcentrationCommand.CanExecute is TRUE 
            // but vm.SamplesPanelViewModel.ShowCancelCalibrationButton is FALSE.
            // So the button is enabled but not visible (so it cannot be clicked).
            Assert.IsFalse(vm.SamplesPanelViewModel.ShowCancelCalibrationButton);
            Assert.IsTrue(vm.StopConcentrationCommand.CanExecute(null));

            Assert.IsFalse(vm.DataPanelViewModel.ConcentrationResultsViewModel.IsCalibrationCompleted);

            var sample = vm.SamplesPanelViewModel.ConcentrationSamples.FirstOrDefault();
            Assert.IsNotNull(sample);
            Assert.IsTrue(vm.StartACupConcentrationSampleCommand.CanExecute(sample));
            
            // Act
            vm.StartACupConcentrationSampleCommand.Execute(sample);
            
            // Assert
            Assert.IsTrue(vm.RunningConcentration);
            Assert.IsFalse(vm.Aborted);
            Assert.IsTrue(vm.SamplesPanelViewModel.ShowCancelCalibrationButton);
            Assert.IsTrue(vm.StopConcentrationCommand.CanExecute(null));
            Assert.IsFalse(vm.DataPanelViewModel.ConcentrationResultsViewModel.IsCalibrationCompleted);
            Assert.IsFalse(vm.DataPanelViewModel.ConcentrationResultsViewModel.ConcentrationIsCalculated);
        }

        [Test]
        public void TestACupConcSlopeListValidationSuccess()
        {
            var vm = _viewModelFactory.CreateAcupConcentrationSlopeViewModel();
            Assert.IsNotNull(vm);
            var aCupSlopeList = ACupConcentrationList();
            //Assert.IsTrue(vm.ValidateConcentrationValues(aCupSlopeList));
        }

        [Test]
        public void TestACupConcSlopeListValidationFailure_AssayValue()
        {
            var vm = _viewModelFactory.CreateAcupConcentrationSlopeViewModel();
            Assert.IsNotNull(vm);
            var aCupSlopeList = ACupConcentrationList();
            aCupSlopeList[0].AssayValue = 0.00;
            //Assert.IsFalse(vm.ValidateConcentrationValues(aCupSlopeList));
            aCupSlopeList = ACupConcentrationList();
            aCupSlopeList[1].AssayValue = 0.00;
            //Assert.IsFalse(vm.ValidateConcentrationValues(aCupSlopeList));
            aCupSlopeList = ACupConcentrationList();
            aCupSlopeList[2].AssayValue = 0.00;
            //Assert.IsFalse(vm.ValidateConcentrationValues(aCupSlopeList));
        }

        [Test]
        public void TestACupConcSlopeListValidationFailure_LotNumber()
        {
            var vm = _viewModelFactory.CreateAcupConcentrationSlopeViewModel();
            Assert.IsNotNull(vm);
            var aCupSlopeList = ACupConcentrationList();
            aCupSlopeList[0].Lot = "";
            //Assert.IsFalse(vm.ValidateConcentrationValues(aCupSlopeList));
            aCupSlopeList = ACupConcentrationList();
            aCupSlopeList[1].Lot = "";
            //Assert.IsFalse(vm.ValidateConcentrationValues(aCupSlopeList));
            aCupSlopeList = ACupConcentrationList();
            aCupSlopeList[2].Lot = "";
            //Assert.IsFalse(vm.ValidateConcentrationValues(aCupSlopeList));
        }

        [Test]
        public void TestACupConcSlopeListValidationFailure_ExpiryDate()
        {
            var vm = _viewModelFactory.CreateAcupConcentrationSlopeViewModel();
            Assert.IsNotNull(vm);
            var aCupSlopeList = ACupConcentrationList();
            aCupSlopeList[0].ExpiryDate = DateTime.Now.AddDays(-1);
            //Assert.IsFalse(vm.ValidateConcentrationValues(aCupSlopeList));
            aCupSlopeList = ACupConcentrationList();
            aCupSlopeList[1].ExpiryDate = DateTime.Now.AddDays(-1);
            //Assert.IsFalse(vm.ValidateConcentrationValues(aCupSlopeList));
            aCupSlopeList = ACupConcentrationList();
            aCupSlopeList[2].ExpiryDate = DateTime.Now.AddDays(-1);
            //Assert.IsFalse(vm.ValidateConcentrationValues(aCupSlopeList));
        }

        private CalibrationConcentrationListDomain GetCalibrationListDomain_2M()
        {
            var domain = new CalibrationConcentrationListDomain
            {
                AssayValueType = AssayValueEnum.M2,
                EndPosition = ApplicationConstants.EndPosition2M,
                StartPosition = ApplicationConstants.StartPosition2M,
                ExpiryDate = DateTime.Now,
                IsCorrectAssayValue = true,
                AssayValue = ApplicationConstants.AssayValue2M,
                Lot = "123",
                KnownConcentration = ApplicationConstants.KnownConcentration2M,
            };
            return domain;
        }

        private CalibrationConcentrationListDomain GetCalibrationListDomain_4M()
        {
            var domain = new CalibrationConcentrationListDomain
            {
                AssayValueType = AssayValueEnum.M4,
                EndPosition = ApplicationConstants.EndPosition4M,
                StartPosition = ApplicationConstants.StartPosition4M,
                ExpiryDate = DateTime.Now,
                IsCorrectAssayValue = true,
                AssayValue = ApplicationConstants.AssayValue4M,
                Lot = "456",
                KnownConcentration = ApplicationConstants.KnownConcentration4M,
            };
            return domain;
        }

        private CalibrationConcentrationListDomain GetCalibrationListDomain_10M()
        {
            var domain = new CalibrationConcentrationListDomain
            {
                AssayValueType = AssayValueEnum.M10,
                EndPosition = ApplicationConstants.EndPosition10M,
                StartPosition = ApplicationConstants.StartPosition10M,
                ExpiryDate = DateTime.Now,
                IsCorrectAssayValue = true,
                AssayValue = ApplicationConstants.AssayValue10M,
                Lot = "789",
                KnownConcentration = ApplicationConstants.KnownConcentration10M,
            };
            return domain;
        }

        private List<ICalibrationConcentrationListDomain> ACupConcentrationList()
        {
            var list = new List<ICalibrationConcentrationListDomain>();
            
            list.Add(GetCalibrationListDomain_2M());
            list.Add(GetCalibrationListDomain_4M());
            list.Add(GetCalibrationListDomain_10M());

            return list;
        }
    }
}
