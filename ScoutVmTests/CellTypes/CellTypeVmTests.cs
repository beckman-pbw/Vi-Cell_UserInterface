using HawkeyeCoreAPI.Facade;
using HawkeyeCoreAPI.Interfaces;
using Moq;
using Ninject;
using NUnit.Framework;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutServices.Interfaces;
using ScoutServices.Ninject;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutViewModels;
using ScoutViewModels.Interfaces;
using ScoutViewModels.ViewModels.CellTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using ScoutModels;
using ScoutModels.ExpandedSampleWorkflow;
using ScoutModels.Interfaces;
using ScoutModels.Service;
using ScoutModels.Settings;
using TestSupport;

namespace ScoutVmTests.CellTypes
{
    [TestFixture]
    public class CellTypeVmTests : BaseTest
    {
        private static Mock<IErrorLog> _errorMock;
        private static Mock<ISystemStatus> _instrStatusMock;
        private static InstrumentStatusService _instrumentStatusService;
        
        [OneTimeSetUp]
        public void ClassInit()
        {
            _instrStatusMock = new Mock<ISystemStatus>();
            _errorMock = new Mock<IErrorLog>();
            var iloggerMock = new Mock<Ninject.Extensions.Logging.ILogger>();
            var sysStatusMock = new Mock<ISystemStatus>();
            var hardwareSettingsMock = new Mock<IHardwareSettingsModel>();
            string serialNumber = "12345";
            hardwareSettingsMock.Setup(m => m.GetSystemSerialNumber(ref serialNumber)).Returns(HawkeyeError.eSuccess);
            var applicationStateServiceMock = new Mock<IApplicationStateService>();
            _instrumentStatusService = new InstrumentStatusService(_instrStatusMock.Object, _errorMock.Object, iloggerMock.Object, hardwareSettingsMock.Object, applicationStateServiceMock.Object);

            SetUser(Username, UserPermissionLevel.eAdministrator);

            Kernel = new StandardKernel(new ScoutServiceModule(), new ScoutViewModelsModule());
            Kernel.Bind<IWorkListModel>().To<WorkListModel>().InSingletonScope();
            Kernel.Bind<ICapacityManager>().To<CapacityManager>().InSingletonScope();
            Kernel.Bind<IDisplayService>().To<DisplayService>().InSingletonScope();
            Kernel.Bind<IErrorLog>().To<HawkeyeCoreAPI.ErrorLog>().InSingletonScope();
            Kernel.Bind<ISystemStatus>().ToConstant(sysStatusMock.Object);
            Kernel.Bind<IInstrumentStatusService>().ToConstant(_instrumentStatusService);
        }

        [SetUp]
        public void SetUp()
        {
            _errorMock.Reset();
            _instrStatusMock.Reset();
			Kernel.Get<ICellTypeManager>();
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestAdvancedUsersCannotEditFactoryCellTypes()
        {
            SetCellTypesAndQualityControls();

            var viewModelFactory = Kernel.Get<IScoutViewModelFactory>();
            
            SetSystemStatusDomain(SystemStatus.Idle);
            var vm = viewModelFactory.CreateCellTypeViewModel(GetUser(UserPermissionLevel.eElevated));
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == (uint)CellTypeIndex.Insect);
            vm.SelectedCellTypeView = CellTypeViewSelection.ViewMode;

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsFalse(vm.AreFieldsEditable);
            Assert.IsFalse(vm.EditCellTypeCommand.CanExecute(null));
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestAdvancedUsersCannotEditAdjFactorOnCustomCellTypes()
        {
            SetCellTypesAndQualityControls();

            var viewModelFactory = Kernel.Get<IScoutViewModelFactory>();
            SetSystemStatusDomain(SystemStatus.Idle);
            var vm = viewModelFactory.CreateCellTypeViewModel(GetUser(UserPermissionLevel.eElevated));
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == 9001);
            vm.SelectedCellTypeView = CellTypeViewSelection.ViewMode;

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsFalse(vm.AreFieldsEditable);
            Assert.IsTrue(vm.EditCellTypeCommand.CanExecute(null));
            
            vm.EditCellTypeCommand.Execute(null);
            // these 2 values are used to determine if the Adj Factor field is enabled:
            Assert.IsTrue(vm.AreFieldsEditable);
            Assert.IsFalse(vm.IsAdminUser);
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestServiceUsersCannotEditFactoryCellTypes()
        {
            SetCellTypesAndQualityControls();

            var viewModelFactory = Kernel.Get<IScoutViewModelFactory>();
            SetSystemStatusDomain(SystemStatus.Idle);
            var vm = viewModelFactory.CreateCellTypeViewModel(GetUser(UserPermissionLevel.eService));
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == (uint)CellTypeIndex.Insect);
            vm.SelectedCellTypeView = CellTypeViewSelection.ViewMode;

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsFalse(vm.AreFieldsEditable);
            Assert.IsFalse(vm.EditCellTypeCommand.CanExecute(null));
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestAdminUsersCannotEditFactoryCellTypes()
        {
            SetCellTypesAndQualityControls();
            var viewModelFactory = Kernel.Get<IScoutViewModelFactory>();
            SetSystemStatusDomain(SystemStatus.Idle);
            var user = GetUser(UserPermissionLevel.eAdministrator);
            LoggedInUser.SetCurrentUserForUnitTestsOnly("Th1$Is@5ecr3tK3y___T311N0one!", user);
            var vm = viewModelFactory.CreateCellTypeViewModel(user);
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == (uint)CellTypeIndex.Insect);
            vm.SelectedCellTypeView = CellTypeViewSelection.ViewMode;

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsFalse(vm.AreFieldsEditable);
            Assert.IsFalse(vm.EditCellTypeCommand.CanExecute(null));
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestNormalUsersCannotEditFactoryCellTypes()
        {
            SetCellTypesAndQualityControls();
            var viewModelFactory = Kernel.Get<IScoutViewModelFactory>();
            SetSystemStatusDomain(SystemStatus.Idle);
            var user = GetUser(UserPermissionLevel.eNormal);
            LoggedInUser.SetCurrentUserForUnitTestsOnly("Th1$Is@5ecr3tK3y___T311N0one!", user);
            var vm = viewModelFactory.CreateCellTypeViewModel(user);
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == (uint)CellTypeIndex.Insect);
            vm.SelectedCellTypeView = CellTypeViewSelection.ViewMode;

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsFalse(vm.AreFieldsEditable);
            Assert.IsFalse(vm.EditCellTypeCommand.CanExecute(null));
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestFactoryCellTypesCannotBeEdited()
        {
            SetCellTypesAndQualityControls();
            var viewModelFactory = Kernel.Get<IScoutViewModelFactory>();
            SetSystemStatusDomain(SystemStatus.Idle);
            var user = GetUser(UserPermissionLevel.eAdministrator);
            LoggedInUser.SetCurrentUserForUnitTestsOnly("Th1$Is@5ecr3tK3y___T311N0one!", user);
            var vm = viewModelFactory.CreateCellTypeViewModel(user);
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == (uint)CellTypeIndex.BciDefault);
            vm.SelectedCellTypeView = CellTypeViewSelection.ViewMode;

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsFalse(vm.AreFieldsEditable);
            Assert.IsFalse(vm.AreSaveDeleteCancelButtonsVisible);
            Assert.IsFalse(vm.SaveCellTypeCommand.CanExecute(null));
            Assert.IsTrue(vm.ReanalyzeCommand.CanExecute(null));
            Assert.IsFalse(vm.EditCellTypeCommand.CanExecute(null));

            SetSystemStatusDomain(SystemStatus.Idle);
            vm = viewModelFactory.CreateCellTypeViewModel(GetUser(UserPermissionLevel.eAdministrator));
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == (uint)CellTypeIndex.Insect);
            vm.SelectedCellTypeView = CellTypeViewSelection.ViewMode;

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsFalse(vm.AreFieldsEditable);
            Assert.IsFalse(vm.AreSaveDeleteCancelButtonsVisible);
            Assert.IsFalse(vm.SaveCellTypeCommand.CanExecute(null));
            Assert.IsTrue(vm.ReanalyzeCommand.CanExecute(null));
            Assert.IsFalse(vm.EditCellTypeCommand.CanExecute(null));
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestBciDefaultCanBeCopied()
        {
            SetCellTypesAndQualityControls();

            var viewModelFactory = Kernel.Get<IScoutViewModelFactory>();
            SetSystemStatusDomain(SystemStatus.Idle);
            var user = GetUser(UserPermissionLevel.eAdministrator);
            LoggedInUser.SetCurrentUserForUnitTestsOnly("Th1$Is@5ecr3tK3y___T311N0one!", user);
            var vm = viewModelFactory.CreateCellTypeViewModel(user);
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == (uint)CellTypeIndex.BciDefault);
            vm.SelectedCellTypeView = CellTypeViewSelection.ViewMode;

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsFalse(vm.AreFieldsEditable);
            Assert.IsFalse(vm.AreSaveDeleteCancelButtonsVisible);
            Assert.IsFalse(vm.DeleteCommand.CanExecute(null));
            Assert.IsTrue(vm.CopyCellTypeCommand.CanExecute(null));
            Assert.IsFalse(vm.SaveCellTypeCommand.CanExecute(null));
            Assert.IsTrue(vm.ReanalyzeCommand.CanExecute(null));

            vm.CopyCellTypeCommand.Execute(null);

            Assert.IsTrue(vm.AreSaveDeleteCancelButtonsVisible);
            Assert.IsTrue(vm.AreFieldsEditable);
            Assert.IsTrue(vm.SaveCellTypeCommand.CanExecute(null));
            Assert.IsFalse(vm.ReanalyzeCommand.CanExecute(null));
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestInsectCannotBeEdited()
        {
            SetCellTypesAndQualityControls();

            var viewModelFactory = Kernel.Get<IScoutViewModelFactory>();
            SetSystemStatusDomain(SystemStatus.Idle);
            var user = GetUser(UserPermissionLevel.eAdministrator);
            LoggedInUser.SetCurrentUserForUnitTestsOnly("Th1$Is@5ecr3tK3y___T311N0one!", user);
            var vm = viewModelFactory.CreateCellTypeViewModel(user);
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == (uint)CellTypeIndex.Insect);
            vm.SelectedCellTypeView = CellTypeViewSelection.ViewMode;

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsFalse(vm.AreFieldsEditable);
            Assert.IsFalse(vm.EditCellTypeCommand.CanExecute(null));
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestInsectCanBeCopied()
        {
            SetCellTypesAndQualityControls();
            var vm = GetVm(UserPermissionLevel.eAdministrator, (uint) CellTypeIndex.Insect);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsFalse(vm.AreFieldsEditable);
            Assert.IsFalse(vm.AreSaveDeleteCancelButtonsVisible);
            Assert.IsFalse(vm.DeleteCommand.CanExecute(null));
            Assert.IsTrue(vm.CopyCellTypeCommand.CanExecute(null));
            Assert.IsFalse(vm.SaveCellTypeCommand.CanExecute(null));
            Assert.IsTrue(vm.ReanalyzeCommand.CanExecute(null));

            vm.CopyCellTypeCommand.Execute(null);

            Assert.IsTrue(vm.AreSaveDeleteCancelButtonsVisible);
            Assert.IsTrue(vm.AreFieldsEditable);
            Assert.IsTrue(vm.SaveCellTypeCommand.CanExecute(null));
            Assert.IsFalse(vm.ReanalyzeCommand.CanExecute(null));
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestCustomCt1CanBeEdited()
        {
            SetCellTypesAndQualityControls();

            var viewModelFactory = Kernel.Get<IScoutViewModelFactory>();
            SetSystemStatusDomain(SystemStatus.Idle);
            var user = GetUser(UserPermissionLevel.eAdministrator);
            LoggedInUser.SetCurrentUserForUnitTestsOnly("Th1$Is@5ecr3tK3y___T311N0one!", user);
            var vm = viewModelFactory.CreateCellTypeViewModel(user);
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == 9001);
            vm.SelectedCellTypeView = CellTypeViewSelection.ViewMode;

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsFalse(vm.AreFieldsEditable);
            Assert.IsTrue(vm.EditCellTypeCommand.CanExecute(null));

            vm.EditCellTypeCommand.Execute(null);

            Assert.IsTrue(vm.AreFieldsEditable);
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestCustomCt1CanBeCopied()
        {
            SetCellTypesAndQualityControls();
            var vm = GetVm(UserPermissionLevel.eAdministrator, 9001);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsFalse(vm.AreFieldsEditable);
            Assert.IsFalse(vm.AreSaveDeleteCancelButtonsVisible);
            Assert.IsTrue(vm.DeleteCommand.CanExecute(null));
            Assert.IsTrue(vm.CopyCellTypeCommand.CanExecute(null));
            Assert.IsFalse(vm.SaveCellTypeCommand.CanExecute(null));
            Assert.IsTrue(vm.ReanalyzeCommand.CanExecute(null));

            vm.CopyCellTypeCommand.Execute(null);

            Assert.IsTrue(vm.AreSaveDeleteCancelButtonsVisible);
            Assert.IsTrue(vm.AreFieldsEditable);
            Assert.IsTrue(vm.SaveCellTypeCommand.CanExecute(null));
            Assert.IsFalse(vm.ReanalyzeCommand.CanExecute(null));
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestCustomCt2UsedInQualityControlCannotBeEdited()
        {
            SetCellTypesAndQualityControls();

            var viewModelFactory = Kernel.Get<IScoutViewModelFactory>();
            SetSystemStatusDomain(SystemStatus.Idle);
            var user = GetUser(UserPermissionLevel.eAdministrator);
            LoggedInUser.SetCurrentUserForUnitTestsOnly("Th1$Is@5ecr3tK3y___T311N0one!", user);
            var vm = viewModelFactory.CreateCellTypeViewModel(user);
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == 9002);
            vm.SelectedCellTypeView = CellTypeViewSelection.ViewMode;

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsFalse(vm.AreFieldsEditable);
            Assert.IsFalse(vm.AreSaveDeleteCancelButtonsVisible);
            Assert.IsFalse(vm.EditCellTypeCommand.CanExecute(null));
            Assert.IsFalse(vm.SaveCellTypeCommand.CanExecute(null));
            Assert.IsTrue(vm.ReanalyzeCommand.CanExecute(null));
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestCustomCt2UsedInQualityControlCanBeCopied()
        {
            SetCellTypesAndQualityControls();
            var vm = GetVm(UserPermissionLevel.eAdministrator, 9002);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsFalse(vm.AreFieldsEditable);
            Assert.IsFalse(vm.AreSaveDeleteCancelButtonsVisible);
            Assert.IsFalse(vm.DeleteCommand.CanExecute(null));
            Assert.IsTrue(vm.CopyCellTypeCommand.CanExecute(null));
            Assert.IsFalse(vm.SaveCellTypeCommand.CanExecute(null));
            Assert.IsTrue(vm.ReanalyzeCommand.CanExecute(null));

            vm.CopyCellTypeCommand.Execute(null);

            Assert.IsTrue(vm.AreSaveDeleteCancelButtonsVisible);
            Assert.IsTrue(vm.AreFieldsEditable);
            Assert.IsTrue(vm.SaveCellTypeCommand.CanExecute(null));
            Assert.IsFalse(vm.ReanalyzeCommand.CanExecute(null));
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestCannotEnterInvalidNumbersInAdjustmentFactorTests()
        {
            var cellTypeManager = Kernel.Get<ICellTypeManager>();

            SetCellTypesAndQualityControls();
            var vm = GetVm(UserPermissionLevel.eAdministrator, 9001);
            
            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsTrue(vm.EditCellTypeCommand.CanExecute(null));

            vm.EditCellTypeCommand.Execute(null);

            Assert.IsTrue(vm.AreFieldsEditable);
            Assert.AreEqual(0f, vm.SelectedCellTypeClone.CalculationAdjustmentFactor);
            vm.SelectedCellTypeClone.CalculationAdjustmentFactor = -20.1f;
            Assert.IsFalse(cellTypeManager.SaveCellTypeValidation(vm.SelectedCellTypeClone, false));
            
            vm.CancelCommand.Execute(null);
            vm.EditCellTypeCommand.Execute(null);

            vm.SaveCellTypeCommand.Execute(null);
            Assert.AreEqual(0.0f, vm.SelectedCellTypeClone.CalculationAdjustmentFactor);

            vm.SelectedCellTypeClone.CalculationAdjustmentFactor = 20.1f;
            Assert.IsFalse(cellTypeManager.SaveCellTypeValidation(vm.SelectedCellTypeClone, false));

            vm.CancelCommand.Execute(null);
            vm.EditCellTypeCommand.Execute(null);

            vm.SaveCellTypeCommand.Execute(null);
            Assert.AreEqual(0.0f, vm.SelectedCellTypeClone.CalculationAdjustmentFactor);
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestCanEnterValidNumbersInAdjustmentFactorTests()
        {
            var cellTypeManager = Kernel.Get<ICellTypeManager>();

            SetCellTypesAndQualityControls();
            var vm = GetVm(UserPermissionLevel.eAdministrator, 9001);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsTrue(vm.EditCellTypeCommand.CanExecute(null));

            vm.EditCellTypeCommand.Execute(null);

            vm.SelectedCellTypeClone.CalculationAdjustmentFactor = -20.0f;
            Assert.IsTrue(cellTypeManager.SaveCellTypeValidation(vm.SelectedCellTypeClone, false));
            vm.SaveCellTypeCommand.Execute(null);
            Assert.AreEqual(-20.0f, vm.SelectedCellTypeClone.CalculationAdjustmentFactor);

            vm.EditCellTypeCommand.Execute(null);

            vm.SelectedCellTypeClone.CalculationAdjustmentFactor = 20.0f;
            Assert.IsTrue(cellTypeManager.SaveCellTypeValidation(vm.SelectedCellTypeClone, false));
            vm.SaveCellTypeCommand.Execute(null);
            Assert.AreEqual(20.0f, vm.SelectedCellTypeClone.CalculationAdjustmentFactor);
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestCannotEditWithBadSystemStatusStates()
        {
            SetCellTypesAndQualityControls();

            var vm = GetVm(UserPermissionLevel.eAdministrator, 9001, SystemStatus.Paused);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsFalse(vm.EditCellTypeCommand.CanExecute(null));
            Assert.IsFalse(vm.DeleteCommand.CanExecute(null));
            Assert.IsFalse(vm.ReanalyzeCommand.CanExecute(null));

            vm = GetVm(UserPermissionLevel.eAdministrator, (uint)CellTypeIndex.Insect, SystemStatus.Paused);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.IsFalse(vm.EditCellTypeCommand.CanExecute(null));
            Assert.IsFalse(vm.DeleteCommand.CanExecute(null));
            Assert.IsFalse(vm.ReanalyzeCommand.CanExecute(null));
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestCanAdminEditWithGoodSystemStatusStates()
        {
            SetCellTypesAndQualityControls();

            var vm = GetVm(UserPermissionLevel.eAdministrator, 9001, 
                SystemStatus.Idle);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.AreEqual(9001, vm.SelectedCellTypeClone.CellTypeIndex);
            Assert.IsTrue(vm.EditCellTypeCommand.CanExecute(null));
            Assert.IsTrue(vm.DeleteCommand.CanExecute(null));
            Assert.IsTrue(vm.ReanalyzeCommand.CanExecute(null));

            vm = GetVm(UserPermissionLevel.eAdministrator, 9001, 
                SystemStatus.ProcessingSample);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.AreEqual(9001, vm.SelectedCellTypeClone.CellTypeIndex);
            Assert.IsFalse(vm.EditCellTypeCommand.CanExecute(null));
            Assert.IsFalse(vm.DeleteCommand.CanExecute(null));
            Assert.IsFalse(vm.ReanalyzeCommand.CanExecute(null));
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestCanAdvancedUserEditWithGoodSystemStatusStates()
        {
            SetCellTypesAndQualityControls();

            var vm = GetVm(UserPermissionLevel.eElevated, 9001, 
                SystemStatus.Idle);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.AreEqual(9001, vm.SelectedCellTypeClone.CellTypeIndex);
            Assert.IsTrue(vm.EditCellTypeCommand.CanExecute(null));
            Assert.IsTrue(vm.DeleteCommand.CanExecute(null));
            Assert.IsTrue(vm.ReanalyzeCommand.CanExecute(null));

            vm = GetVm(UserPermissionLevel.eElevated, 9001, 
                SystemStatus.ProcessingSample);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellTypeClone);
            Assert.AreEqual(9001, vm.SelectedCellTypeClone.CellTypeIndex);
            Assert.IsFalse(vm.EditCellTypeCommand.CanExecute(null));
            Assert.IsFalse(vm.DeleteCommand.CanExecute(null));
            Assert.IsFalse(vm.ReanalyzeCommand.CanExecute(null));
        }

        #region Setup methods

        private CellTypeViewModel GetVm(UserPermissionLevel level, uint cellTypeIndexToSelect,
            SystemStatus status = SystemStatus.Idle)
        {
            var viewModelFactory = Kernel.Get<IScoutViewModelFactory>();
            SetSystemStatusDomain(status);
            var vm = viewModelFactory.CreateCellTypeViewModel(GetUser(level));
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == cellTypeIndexToSelect);
            vm.SelectedCellTypeView = CellTypeViewSelection.ViewMode;
            SetSystemStatusDomain(status);

            return vm;
        }

        private void SetSystemStatusDomain(SystemStatus status)
        {
            var systemStatus = new SystemStatusDomain {SystemStatus = status};
            _instrumentStatusService.PublishSystemStatusCallback(systemStatus);
        }

        private UserDomain GetUser(UserPermissionLevel level)
        {
            return new UserDomain
            {
                UserID = "User1",
                RoleID = level,
                AssignedCellTypes = CellTypeFacade.Instance.GetAllowedCellTypes_BECall("User1")
            };
        }

        delegate void CtCallback(ref uint count, ref List<CellTypeDomain> cellTypes);
        delegate void QcCallback(string user, string pass, ref List<QualityControlDomain> qcs, ref uint count);

        private void SetCellTypesAndQualityControls()
        {
            uint numberOfFactoryCellTypes = 2;
            var factoryCellTypes = GetFactoryCellTypes();

            uint numberOfAllCellTypes = 4;
            var allCellTypes = GetAllCellTypes();

            uint numberOfQuality = 2;
            var qualDomainList = GetQualityControls();

            var cellTypeAccessMock = new Mock<ICellType>();
            cellTypeAccessMock.Setup(m =>  m.GetFactoryCellTypesAPI(ref It.Ref<List<CellTypeDomain>>.IsAny))
                .Callback(new CtCallback((ref uint c, ref List<CellTypeDomain> cts) =>
                {
                    c = numberOfFactoryCellTypes;
                    cts = factoryCellTypes;
                }))
                .Returns(HawkeyeError.eSuccess);

            cellTypeAccessMock.Setup(m => m.GetAllCellTypesAPI(ref It.Ref<uint>.IsAny, ref It.Ref<List<CellTypeDomain>>.IsAny))
                .Callback(new CtCallback((ref uint c, ref List<CellTypeDomain> cts) =>
                {
                    c = numberOfAllCellTypes;
                    cts = allCellTypes;
                }))
                .Returns(HawkeyeError.eSuccess);

            var qualTypeAccessMock = new Mock<IQualityControl>();
            qualTypeAccessMock.Setup(m => m.GetQualityControlListAPI(It.IsAny<string>(), It.IsAny<string>(), true, ref It.Ref<List<QualityControlDomain>>.IsAny, out It.Ref<uint>.IsAny))
                .Callback(new QcCallback((string user, string pass, ref List<QualityControlDomain> qcs, ref uint c) =>
                {
                    qcs = qualDomainList;
                    c = numberOfQuality;
                }))
                .Returns(HawkeyeError.eSuccess);

            qualTypeAccessMock.Setup(m => m.GetQualityControlListAPI(It.IsAny<string>(), It.IsAny<string>(), true, ref It.Ref<List<QualityControlDomain>>.IsAny, out It.Ref<uint>.IsAny))
                .Callback(new QcCallback((string user, string pass, ref List<QualityControlDomain> qcs, ref uint c) =>
                {
                    qcs = qualDomainList;
                    c = numberOfQuality;
                }))
                .Returns(HawkeyeError.eSuccess);

            CellTypeFacade.Instance.UnitTest_SetCellTypeAccess(cellTypeAccessMock.Object, qualTypeAccessMock.Object);
            //CellTypeFacade.Instance.SetCellTypesAndQualityControls(allCellTypes, qualDomainList);

            //var ct = CellTypeFacade.Instance.GetAllCellTypesCopy();
            //var ct2 = CellTypeFacade.Instance.GetFactoryCellTypes();
            //var qc = CellTypeFacade.Instance.GetAllQualityControlsCopy();

            //Assert.AreEqual(4, ct.Count);
            //Assert.AreEqual(2, ct2.Count);
            //Assert.AreEqual(2, qc.Count);

            SetSystemStatusDomain(SystemStatus.Idle);
        }

        private List<CellTypeDomain> GetAllCellTypes()
        {
            var cellTypeDomainList = new List<CellTypeDomain>
            {
                new CellTypeDomain
                {
                    AnalysisDomain = new AnalysisDomain
                    {
                        AnalysisParameter = new List<AnalysisParameterDomain>
                        {
                            new AnalysisParameterDomain {ThresholdValue = 40},
                            new AnalysisParameterDomain {ThresholdValue = 50}
                        }
                    },
                    AspirationCycles = 1,
                    CalculationAdjustmentFactor = 0.0f,
                    CellSharpness = 1,
                    CellTypeIndex = (uint) CellTypeIndex.BciDefault,
                    CellTypeName = CellTypeIndexHelper.GetCellTypeName(CellTypeIndex.BciDefault),
                    DeclusterDegree = eCellDeclusterSetting.eDCMedium,
                    Images = 100,
                    IsQualityControlCellType = false,
                    IsCellEnable = true,
                    IsUserDefineCellType = false,
                    IsCellSelected = false,
                    MinimumCircularity = 0.5,
                    MinimumDiameter = 10.0,
                    MaximumDiameter = 12.0
                },
                new CellTypeDomain
                {
                    AnalysisDomain = new AnalysisDomain
                    {
                        AnalysisParameter = new List<AnalysisParameterDomain>
                        {
                            new AnalysisParameterDomain {ThresholdValue = 40},
                            new AnalysisParameterDomain {ThresholdValue = 50}
                        }
                    },
                    AspirationCycles = 1,
                    CalculationAdjustmentFactor = 0.0f,
                    CellSharpness = 1,
                    CellTypeIndex = (uint) CellTypeIndex.Insect,
                    CellTypeName = CellTypeIndexHelper.GetCellTypeName(CellTypeIndex.Insect),
                    DeclusterDegree = eCellDeclusterSetting.eDCMedium,
                    Images = 100,
                    IsQualityControlCellType = false,
                    IsCellEnable = true,
                    IsUserDefineCellType = false,
                    IsCellSelected = false,
                    MinimumCircularity = 0.5,
                    MinimumDiameter = 10.0,
                    MaximumDiameter = 12.0
                },
                new CellTypeDomain
                {
                    AnalysisDomain = new AnalysisDomain
                    {
                        AnalysisParameter = new List<AnalysisParameterDomain>
                        {
                            new AnalysisParameterDomain {ThresholdValue = 40},
                            new AnalysisParameterDomain {ThresholdValue = 50}
                        },
                        MixingCycle = 3,
                    },
                    AspirationCycles = 1,
                    CalculationAdjustmentFactor = 0.0f,
                    CellSharpness = 1,
                    CellTypeIndex = 9001,
                    CellTypeName = "CustomCellType",
                    DeclusterDegree = eCellDeclusterSetting.eDCMedium,
                    Images = 100,
                    IsQualityControlCellType = false,
                    IsCellEnable = true,
                    IsUserDefineCellType = true,
                    IsCellSelected = false,
                    MinimumCircularity = 0.5,
                    MinimumDiameter = 10.0,
                    MaximumDiameter = 12.0
                },
                new CellTypeDomain
                {
                    AnalysisDomain = new AnalysisDomain
                    {
                        AnalysisParameter = new List<AnalysisParameterDomain>
                        {
                            new AnalysisParameterDomain {ThresholdValue = 40},
                            new AnalysisParameterDomain {ThresholdValue = 50}
                        }
                    },
                    AspirationCycles = 1,
                    CalculationAdjustmentFactor = 0.0f,
                    CellSharpness = 1,
                    CellTypeIndex = 9002,
                    CellTypeName = "CustomCellType2",
                    DeclusterDegree = eCellDeclusterSetting.eDCMedium,
                    Images = 100,
                    IsQualityControlCellType = true,
                    IsCellEnable = true,
                    IsUserDefineCellType = true,
                    IsCellSelected = false,
                    MinimumCircularity = 0.5,
                    MinimumDiameter = 10.0,
                    MaximumDiameter = 12.0
                }
            };

            return cellTypeDomainList;
        }

        private List<CellTypeDomain> GetFactoryCellTypes()
        {
            var cellTypeDomainList = new List<CellTypeDomain>
            {
                new CellTypeDomain
                {
                    AnalysisDomain = new AnalysisDomain
                    {
                        AnalysisParameter = new List<AnalysisParameterDomain>
                        {
                            new AnalysisParameterDomain {ThresholdValue = 40},
                            new AnalysisParameterDomain {ThresholdValue = 50}
                        }
                    },
                    AspirationCycles = 1,
                    CalculationAdjustmentFactor = 0.0f,
                    CellSharpness = 1,
                    CellTypeIndex = (uint) CellTypeIndex.BciDefault,
                    CellTypeName = CellTypeIndexHelper.GetCellTypeName(CellTypeIndex.BciDefault),
                    DeclusterDegree = eCellDeclusterSetting.eDCMedium,
                    Images = 100,
                    IsQualityControlCellType = false,
                    IsCellEnable = true,
                    IsUserDefineCellType = false,
                    IsCellSelected = false,
                    MinimumCircularity = 0.5,
                    MinimumDiameter = 10.0,
                    MaximumDiameter = 12.0
                },
                new CellTypeDomain
                {
                    AnalysisDomain = new AnalysisDomain
                    {
                        AnalysisParameter = new List<AnalysisParameterDomain>
                        {
                            new AnalysisParameterDomain {ThresholdValue = 40},
                            new AnalysisParameterDomain {ThresholdValue = 50}
                        }
                    },
                    AspirationCycles = 1,
                    CalculationAdjustmentFactor = 0.0f,
                    CellSharpness = 1,
                    CellTypeIndex = (uint) CellTypeIndex.Insect,
                    CellTypeName = CellTypeIndexHelper.GetCellTypeName(CellTypeIndex.Insect),
                    DeclusterDegree = eCellDeclusterSetting.eDCMedium,
                    Images = 100,
                    IsQualityControlCellType = false,
                    IsCellEnable = true,
                    IsUserDefineCellType = false,
                    IsCellSelected = false,
                    MinimumCircularity = 0.5,
                    MinimumDiameter = 10.0,
                    MaximumDiameter = 12.0
                }
            };

            return cellTypeDomainList;
        }

        private List<QualityControlDomain> GetQualityControls()
        {
            var qualDomainList = new List<QualityControlDomain>
            {
                new QualityControlDomain
                {
                    AcceptanceLimit = 1,
                    AssayParameter = assay_parameter.ap_Concentration,
                    AssayValue = 10,
                    CellTypeIndex = (uint)CellTypeIndex.Mammalian,
                    CellTypeName = CellTypeIndexHelper.GetCellTypeName(CellTypeIndex.Mammalian),
                    CommentText = "comment",
                    ExpirationDate = DateTime.Now.AddYears(1),
                    LotInformation = "lot info",
                    QcName = "My QC"
                },
                new QualityControlDomain
                {
                    AcceptanceLimit = 1,
                    AssayParameter = assay_parameter.ap_Concentration,
                    AssayValue = 10,
                    CellTypeIndex = 9002,
                    CellTypeName = "CustomCellType2",
                    CommentText = "comment",
                    ExpirationDate = DateTime.Now.AddYears(1),
                    LotInformation = "lot info",
                    QcName = "My QC 2"
                }
            };

            return qualDomainList;
        }

        #endregion
    }
}