using HawkeyeCoreAPI.Facade;
using HawkeyeCoreAPI.Interfaces;
using Moq;
using NUnit.Framework;
using ScoutDataAccessLayer.IDAL;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.Common;
using ScoutModels;
using ScoutModels.Settings;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Interfaces;
using ScoutUtilities.Structs;
using ScoutViewModels.ViewModels.Dialogs;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;
using ScoutViewModels.ViewModels.Home.ExpandedSampleWorkflow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Windows.Media;
using ScoutModels.Interfaces;
using Ninject;
using Ninject.Extensions.Factory;
using NUnit.Framework.Internal;
using ScoutModels.ExpandedSampleWorkflow;
using ScoutViewModels;
using ScoutViewModels.Interfaces;
using ScoutServices.Ninject;
using ScoutModels.Service;
using ScoutServices;

//using ILogger = Ninject.Extensions.Logging.ILogger;


namespace ScoutViewModelTests.Dialogs
{
    [TestFixture]
    public class CreateSampleSetDialogViewModelUnitTests : MarshalByRefObject
    {
        private IKernel _kernel;
        private static Mock<IErrorLog> _errorMock;
        private static Mock<ISystemStatus> _instrStatusMock;
        private static InstrumentStatusService _instrumentStatusService;
        private static string _username = "username";
        private static string _password = "password";
        private AppDomainSetup _appDomainSetup;
        private AppDomain _appDomain;
        private RunOptionSettingsModel _runOptions;
        private readonly Mock<ISecurityService> _mockSecurityService = new Mock<ISecurityService>();

        delegate void DataCallback(string userId, string property, out bool userFound, bool remove);

        [OneTimeSetUp]
        public void ClassInit()
        {
            _instrStatusMock = new Mock<ISystemStatus>();
            _errorMock = new Mock<IErrorLog>();
            var iloggerMock = new Mock<Ninject.Extensions.Logging.ILogger>();
            var sysStatusMock = new Mock<ISystemStatus>();
            var displaySvcMock = new Mock<IDisplayService>();
            var hardwareSettingsMock = new Mock<IHardwareSettingsModel>();
            string serialNumber = "12345";
            hardwareSettingsMock.Setup(m => m.GetSystemSerialNumber(ref serialNumber)).Returns(HawkeyeError.eSuccess);
            var applicationStateServiceMock = new Mock<IApplicationStateService>();
            _instrumentStatusService = new InstrumentStatusService(_instrStatusMock.Object, _errorMock.Object, iloggerMock.Object, hardwareSettingsMock.Object, applicationStateServiceMock.Object);
            _kernel = new StandardKernel(new ScoutServiceModule(), new ScoutViewModelsModule());
            _kernel.Bind<IWorkListModel>().To<WorkListModel>().InSingletonScope();
            _kernel.Bind<ICapacityManager>().To<CapacityManager>().InSingletonScope();
            _kernel.Bind<IDisplayService>().ToConstant(displaySvcMock.Object);
            _kernel.Bind<IErrorLog>().ToConstant(_errorMock.Object);
            _kernel.Bind<ISystemStatus>().ToConstant(sysStatusMock.Object);
            _kernel.Bind<IInstrumentStatusService>().ToConstant(_instrumentStatusService);
            _kernel.Bind<IRunningWorkListModel>().To<RunningWorkListModel>().InSingletonScope();
            _kernel.Bind<IScoutModelsFactory>().ToFactory();
            _kernel.Bind<ISecurityService>().ToConstant(_mockSecurityService.Object);

            var dataAccessMock = new Mock<IDataAccess>();

            var userFound = true;
            dataAccessMock.Setup(m => m.ReadConfigurationData<List<GenericParametersDomain>>(
                    It.IsAny<string>(), It.IsAny<string>(), out userFound, It.IsAny<bool>()))
                .Returns(new List<GenericParametersDomain>());

            var runSettings = new RunOptionSettingsDomain();
            _runOptions = new RunOptionSettingsModel(dataAccessMock.Object, _username)
            {
                RunOptionsSettings = runSettings, SavedSettings = runSettings
            };
            
            dataAccessMock.Setup(m => m.ReadConfigurationData<RunOptionSettingsModel>(
                    It.IsAny<string>(), It.IsAny<string>(), out userFound, It.IsAny<bool>()))
                .Returns(_runOptions);

            _kernel.Bind<IDataAccess>().ToConstant(dataAccessMock.Object);
            _kernel.Bind<RunOptionSettingsModel>().ToConstant(_runOptions);
            var settingServiceMock = new Mock<ISettingsService>();
            settingServiceMock.Setup(m => m.GetRunOptions(dataAccessMock.Object, It.IsAny<string>())).Returns(runSettings);
            settingServiceMock.Setup(m => m.GetRunOptionSettingsModel(dataAccessMock.Object, It.IsAny<string>())).Returns(_runOptions);
            _kernel.Bind<ISettingsService>().ToConstant(settingServiceMock.Object);
        }

        [SetUp]
        public void SetUp()
        {
            _errorMock.Reset();
            _instrStatusMock.Reset();
            CarouselModel.Instance.ClearAll();
        }

        [Test]
        public void TestAutoCupVmWellIsClicked() //a-cup sample is auto-populated
        {
            var allowCarrierTypeToChange = true;

            var viewModelFactory = _kernel.Get<IScoutViewModelFactory>();
            var vm = CreateSampleSetDialogViewModel(viewModelFactory, _username, "sample", "set", allowCarrierTypeToChange, new AutomationConfig(true, true, 1));
            Assert.IsNotNull(vm);
            vm.SelectedPlateType = SubstrateType.AutomationCup;
            var well = vm.AutoCupVm.SampleWellButtons.FirstOrDefault();

			// check base conditions
			// CellHealth only supports A-Cup.
//            Assert.AreEqual(2, vm.PlateTypes.Count);
            Assert.AreEqual(1, vm.PlateTypes.Count);
            Assert.IsTrue(vm.PlateTypes.Contains(SubstrateType.AutomationCup));
            Assert.AreEqual(SubstrateType.AutomationCup, vm.SelectedPlateType);
            Assert.IsNotNull(vm.AutoCupVm);
            Assert.IsNotNull(vm.AutoCupVm.SampleWellButtons);
            Assert.AreEqual(1, vm.AutoCupVm.SampleWellButtons.Count);
            Assert.AreEqual(RowPositionHelper.GetRowChar(RowPosition.Y), vm.AutoCupVm.SampleWellButtons.FirstOrDefault()?.SamplePosition.Row);
            Assert.AreEqual((byte) 1, vm.AutoCupVm.SampleWellButtons.FirstOrDefault()?.SamplePosition.Column);
            Assert.AreEqual(0, vm.AutoCupVm.SampleSet.Samples.Count);

            Assert.IsNotNull(vm.SampleTemplate);
            Assert.IsNotNull(vm.SampleTemplate.AdvancedSampleSettings);
            Assert.IsNotNull(vm.AutoCupVm.RunOptionSettings);
            Assert.IsNotNull(vm.AutoCupVm.RunOptionSettings.RunOptionsSettings);

            Assert.IsNotNull(well);
            Assert.IsNotNull(well.Sample);
            Assert.IsNotNull(vm.AutoCupVm.LastSampleWellAdded);

            Assert.AreEqual(SampleWellState.UsedInCurrentSet, well.WellState);
            Assert.IsTrue(vm.CanAccept()); // ensure the user can click the "Add" button

            // perform deselect action to test:
            vm.AutoCupVm.OnClicked.Execute(well); // click to deselect the sample from the well

            Assert.IsNull(well.Sample);
            Assert.AreEqual(SampleWellState.Available, well.WellState);
            Assert.IsFalse(vm.CanAccept()); // ensure the user can NOT click the "Add" button
        }

        [Test]
        public void TestAutoCupVmWellIsAutoPopulatedAndAddedToSampleSet()
        {
            var allowCarrierTypeToChange = true;

            var viewModelFactory = _kernel.Get<IScoutViewModelFactory>();
            var vm = CreateSampleSetDialogViewModel(viewModelFactory, _username, "sample", "set", allowCarrierTypeToChange, new AutomationConfig(true, true, 1));
            Assert.IsNotNull(vm);
            vm.SelectedPlateType = SubstrateType.AutomationCup;
            var well = vm.AutoCupVm.SampleWellButtons.FirstOrDefault();

            Assert.AreEqual(0, vm.AutoCupVm.SampleSet.Samples.Count);

            vm.AcceptCommand.Execute(null);

            // for another test:
            Assert.AreEqual(1, vm.AutoCupVm.SampleSet.Samples.Count);
        }

        [Test]
        [Ignore("CellHealth only supports A-Cup")]
        public void TestCreateSampleSetVm_DoesNotShowAutomationCupInDropdownWhenAutomationIsOff()
        {
            var allowCarrierTypeToChange = true;

            var viewModelFactory = _kernel.Get<IScoutViewModelFactory>();
            var vm = CreateSampleSetDialogViewModel(viewModelFactory, _username, "sample", "set", allowCarrierTypeToChange, new AutomationConfig(false, false, 1));

            Assert.IsNotNull(vm);
            Assert.AreEqual(2, vm.PlateTypes.Count);
            Assert.IsFalse(vm.PlateTypes.Contains(SubstrateType.AutomationCup));
        }

        [Test]
        [Ignore("CellHealth only supports A-Cup")]
        public void TestCreateSampleSetVm_DoesNotShowAutomationCupInDropdownWhenACupIsDisabled()
        {
            var allowCarrierTypeToChange = true;

            var viewModelFactory = _kernel.Get<IScoutViewModelFactory>();
            var vm = CreateSampleSetDialogViewModel(viewModelFactory, _username, "sample", "set", allowCarrierTypeToChange, new AutomationConfig(true, false, 1));

            Assert.IsNotNull(vm);
            Assert.AreEqual(2, vm.PlateTypes.Count);
            Assert.IsFalse(vm.PlateTypes.Contains(SubstrateType.AutomationCup));
        }

        [Test]
        public void TestCreateSampleSetVm_DoesAllowAutomationCupToBeSelectedWhenACupIsEnabled()
        {
            var allowCarrierTypeToChange = true;

            var viewModelFactory = _kernel.Get<IScoutViewModelFactory>();
            var vm = CreateSampleSetDialogViewModel(viewModelFactory, _username, "sample", "set", allowCarrierTypeToChange, new AutomationConfig(true, true, 1));

			// CellHealth only supports A-Cup.
//            Assert.AreEqual(SubstrateType.Carousel, vm.SelectedPlateType);
            vm.SelectedPlateType = SubstrateType.AutomationCup;
            Assert.AreEqual(SubstrateType.AutomationCup, vm.SelectedPlateType);
        }

        // Note: We cannot currently test normal users without some additional refactoring to mock out getting whether a normal user has fastmode enabled.
        [Test]
        [Ignore("Requires refactoring")]
//        [Ignore("Requires refactoring - CHM behavior changed")]		
        public void TestAutoCupVmIsFastModeEnabledAdmin()
        {
            var vm = SetupVmIsFastModeEnabled(SubstrateType.AutomationCup, true, false);
            Assert.IsFalse(vm.SampleTemplate.IsFastModeEnabled);
            var firstSample = vm.AutoCupVm.SampleSet.Samples.First();
            Assert.IsFalse(firstSample.IsFastModeEnabled);
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCarouselVmIsFastModeEnabledAdmin()
        {
            var vm = SetupVmIsFastModeEnabled(SubstrateType.Carousel, true, false);
            Assert.IsTrue(vm.SampleTemplate.IsFastModeEnabled);
            var firstSample = vm.CarouselVm.SampleSet.Samples.First();
            Assert.IsTrue(firstSample.IsFastModeEnabled);
        }

        [Test]
        public void TestWellPlateVmIsFastModeEnabledAdmin()
        {
            var vm = SetupVmIsFastModeEnabled(SubstrateType.Plate96, true, false);
            Assert.IsTrue(vm.SampleTemplate.IsFastModeEnabled);
            var firstSample = vm.PlateVm.SampleSet.Samples.First();
            Assert.IsTrue(firstSample.IsFastModeEnabled);
        }

        [Test]
        [Ignore("Requires refactoring")]
//        [Ignore("Requires refactoring - CHM behavior changed")]		
        public void TestAutoCupVmIsFastModeEnabledServiceUser()
        {
            var vm = SetupVmIsFastModeEnabled(SubstrateType.AutomationCup, false, true);
            Assert.IsFalse(vm.SampleTemplate.IsFastModeEnabled);
            var firstSample = vm.AutoCupVm.SampleSet.Samples.First();
            Assert.IsFalse(firstSample.IsFastModeEnabled);
        }

        [Test]
        public void TestCarouselVmIsFastModeEnabledServiceUser()
        {
            var vm = SetupVmIsFastModeEnabled(SubstrateType.Carousel, false, true);
            Assert.IsTrue(vm.SampleTemplate.IsFastModeEnabled);
            var firstSample = vm.CarouselVm.SampleSet.Samples.First();
            Assert.IsTrue(firstSample.IsFastModeEnabled);
        }

        [Test]
        public void TestWellPlateVmIsFastModeEnabledServiceUser()
        {
            var vm = SetupVmIsFastModeEnabled(SubstrateType.Plate96, false, true);
            Assert.IsTrue(vm.SampleTemplate.IsFastModeEnabled);
            var firstSample = vm.PlateVm.SampleSet.Samples.First();
            Assert.IsTrue(firstSample.IsFastModeEnabled);
        }

        private CreateSampleSetDialogViewModel SetupVmIsFastModeEnabled(SubstrateType substrateType, bool isAdmin, bool isServiceUser)
        {
            var allowCarrierTypeToChange = true;
            var acupEnabled = SubstrateType.AutomationCup == substrateType;
            var viewModelFactory = _kernel.Get<IScoutViewModelFactory>();
            var vm = CreateSampleSetDialogViewModel(viewModelFactory, "user", "sample", "set", allowCarrierTypeToChange, new AutomationConfig(true, acupEnabled, 1));
            Assert.IsNotNull(vm);
            vm.SelectedPlateType = substrateType;
            // Override - still using static LoggedInUser but we can just set the value in the BaseViewModel.
            vm.SampleTemplate.IsAdminUser = isAdmin;
            vm.SampleTemplate.IsServiceUser = isServiceUser;

            // for another test:
            SampleViewModel firstSample = null;
            switch (substrateType)
            {
                case SubstrateType.Carousel:
                    CarouselModel.Instance.SetTopCarouselPosition(1);
                    var carouselButton = vm.CarouselVm.SampleWellButtons.FirstOrDefault();
                    vm.CarouselVm.OnClicked.Execute(carouselButton);
                    vm.AcceptCommand.Execute(null);
                    firstSample = vm.CarouselVm.SampleSet.Samples.First();
                    break;
                case SubstrateType.Plate96:
                    var plateButton = vm.PlateVm.SampleWellButtons.FirstOrDefault();
                    vm.PlateVm.OnClicked.Execute(plateButton);
                    vm.AcceptCommand.Execute(null);
                    firstSample = vm.PlateVm.SampleSet.Samples.First();
                    break;
                case SubstrateType.AutomationCup:
                    vm.AcceptCommand.Execute(null);
                    firstSample = vm.AutoCupVm.SampleSet.Samples.First();
                    break;
            }

            // Override - still using static LoggedInUser but we can just set the value in the BaseViewModel.
            Debug.Assert(firstSample != null, nameof(firstSample) + " != null");
            firstSample.IsAdminUser = isAdmin;
            firstSample.IsServiceUser = isServiceUser;
            return vm;
        }

        private CreateSampleSetDialogViewModel CreateSampleSetDialogViewModel(IScoutViewModelFactory viewModelFactory, string user, string sampleName, string setName, bool allowCarrierTypeToChange, AutomationConfig automationConfig)
        {
            var colorServiceMock = new Mock<ISolidColorBrushService>();
            colorServiceMock.Setup(m => m.GetBrushForColor(It.IsAny<string>())).Returns(new SolidColorBrush(Colors.Blue));

            uint numberOfCellTypes = 0;
            var cellTypeDomainList = new List<CellTypeDomain>();
            var cellTypeAccessMock = new Mock<ICellType>();
            cellTypeAccessMock.Setup(m => m.GetAllCellTypesAPI(ref numberOfCellTypes, ref cellTypeDomainList))
                              .Returns(HawkeyeError.eSuccess);

            uint numberOfQuality = 0;
            var qualDomainList = new List<QualityControlDomain>();
            var qualTypeAccessMock = new Mock<IQualityControl>();
            qualTypeAccessMock.Setup(m => m.GetQualityControlListAPI(_username, _password, true, ref qualDomainList, out numberOfQuality))
                              .Returns(HawkeyeError.eSuccess);

            CellTypeFacade.Instance.UnitTest_SetCellTypeAccess(cellTypeAccessMock.Object, qualTypeAccessMock.Object);

            var sysStatus = new SystemStatusData();
            _instrStatusMock.Setup(m => m.GetSystemStatusAPI(ref sysStatus)).Returns(IntPtr.Zero);
            var str = string.Empty;
            _errorMock.Setup(m => m.SystemErrorCodeToExpandedResourceStringsAPI(It.IsAny<uint>(),
                ref str, 
                ref str, 
                ref str, 
                ref str,
                ref str,
                ref str));
            _instrumentStatusService.GetAndPublishSystemStatus();
            _instrumentStatusService.SystemStatusDom.SamplePosition = SamplePosition.Parse("A1");

            var ct = new CellTypeQualityControlGroupDomain();
            var list = new ObservableCollection<CellTypeQualityControlGroupDomain> { ct };
            var eList = new ObservableCollection<SamplePostWash> { SamplePostWash.NormalWash };
            var seq = new SequentialNamingSetViewModel(sampleName);
            var sampleDomain = new SampleDomain();
            var settings = new AdvancedSampleSettingsViewModel(sampleDomain, _runOptions);
            var sampleProcessingService = _kernel.Get<SampleProcessingService>();
            var cellTypeFacade = _kernel.Get<CellTypeFacade>();
            var settingsService = _kernel.Get<SettingsService>();
            var sampleTemplate = new UserSampleTemplateViewModel(settingsService,list, list.FirstOrDefault(), 1, eList,
                eList.FirstOrDefault(), string.Empty, false, seq, settings, _runOptions, new UserDomain(user), sampleProcessingService, viewModelFactory, cellTypeFacade );
            var set = viewModelFactory.CreateSampleSetViewModel(null, sampleTemplate, user, user, setName);
            var args = new CreateSampleSetEventArgs<SampleSetViewModel>(set, user, user,
                setName, SampleSetStatus.Pending, allowCarrierTypeToChange, SubstrateType.Carousel,
                WorkListStatus.Idle);

            var autoMock = new Mock<IAutomationSettingsService>();
            autoMock.Setup(m => m.GetAutomationConfig()).Returns(automationConfig);

            var vm = viewModelFactory.CreateCreateSampleSetDialogViewModel(args, null, colorServiceMock.Object, _runOptions,
                autoMock.Object);
            return vm;
        }

    }
}
