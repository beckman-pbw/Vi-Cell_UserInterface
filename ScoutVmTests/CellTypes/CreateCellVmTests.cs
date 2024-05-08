using HawkeyeCoreAPI.Facade;
using HawkeyeCoreAPI.Interfaces;
using Moq;
using Ninject;
using NUnit.Framework;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutServices.Ninject;
using ScoutUtilities.Enums;
using ScoutViewModels;
using ScoutViewModels.Interfaces;
using ScoutViewModels.ViewModels.CellTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using HawkeyeCoreAPI;
using ScoutModels;
using ScoutModels.ExpandedSampleWorkflow;
using ScoutModels.Interfaces;
using ScoutModels.Service;
using ScoutModels.Settings;
using TestSupport;
using SystemStatus = ScoutUtilities.Enums.SystemStatus;

namespace ScoutVmTests.CellTypes
{
    [TestFixture]
    public class CreateCellVmTests : BaseTest
    {
        private IScoutViewModelFactory _viewModelFactory;
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
            var applicationStateServiceMock = new Mock<IApplicationStateService>();
            _instrumentStatusService = new InstrumentStatusService(_instrStatusMock.Object, _errorMock.Object, iloggerMock.Object, applicationStateServiceMock.Object);
            Kernel = new StandardKernel(new ScoutServiceModule(), new ScoutViewModelsModule());
            Kernel.Bind<IWorkListModel>().To<WorkListModel>().InSingletonScope();
            Kernel.Bind<ICapacityManager>().To<CapacityManager>().InSingletonScope();
            Kernel.Bind<IDisplayService>().To<DisplayService>().InSingletonScope();
            Kernel.Bind<IErrorLog>().To<HawkeyeCoreAPI.ErrorLog>().InSingletonScope();
            Kernel.Bind<ISystemStatus>().ToConstant(sysStatusMock.Object);
            Kernel.Bind<IInstrumentStatusService>().ToConstant(_instrumentStatusService);
            Kernel.Bind<ICellType>().To<CellType>().InSingletonScope();
        }

        [SetUp]
        public void SetUp()
        {
            _errorMock.Reset();
            _instrStatusMock.Reset();
            _viewModelFactory = Kernel.Get<IScoutViewModelFactory>();
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestAdvancedUsersCannotEditFactoryCellTypes()
        {
            SetCellTypesAndQualityControls();

            SetSystemStatusDomain(SystemStatus.Idle);
            var vm = _viewModelFactory.CreateCreateCellTypeViewModel(GetUser(UserPermissionLevel.eElevated));
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == (uint)CellTypeIndex.Insect);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellType);
            Assert.IsFalse(vm.IsEditActive);
            Assert.IsFalse(vm.IsEditEnable);
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestAdminUsersCannotEditFactoryCellTypes()
        {
            SetCellTypesAndQualityControls();

            SetSystemStatusDomain(SystemStatus.Idle);
            var vm = _viewModelFactory.CreateCreateCellTypeViewModel(GetUser(UserPermissionLevel.eAdministrator));
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == (uint)CellTypeIndex.Insect);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellType);
            Assert.IsFalse(vm.IsEditActive);
            Assert.IsFalse(vm.IsEditEnable);
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestAdvancedUsersCanEditUserCellTypes()
        {
            SetCellTypesAndQualityControls();

            SetSystemStatusDomain(SystemStatus.Idle);
            var vm = _viewModelFactory.CreateCreateCellTypeViewModel(GetUser(UserPermissionLevel.eElevated));
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == 9001);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellType);
            Assert.IsFalse(vm.IsEditActive);
            Assert.IsTrue(vm.IsEditEnable);
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestAdminUsersCanEditUserCellTypes()
        {
            SetCellTypesAndQualityControls();

            SetSystemStatusDomain(SystemStatus.Idle);
            var vm = _viewModelFactory.CreateCreateCellTypeViewModel(GetUser(UserPermissionLevel.eAdministrator));
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == 9001);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellType);
            Assert.IsFalse(vm.IsEditActive);
            Assert.IsTrue(vm.IsEditEnable);
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestAdminUsersCanEditUserCellTypesAdjustmentFactor()
        {
            SetCellTypesAndQualityControls();

            SetSystemStatusDomain(SystemStatus.Idle);
            var vm = _viewModelFactory.CreateCreateCellTypeViewModel(GetUser(UserPermissionLevel.eAdministrator));
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == 9001);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellType);
            Assert.IsFalse(vm.IsEditActive);
            Assert.IsTrue(vm.IsEditEnable);

            vm.EditCommand.Execute(null);

            // these are the values in the xaml for the MultipleBooleanAndConverter
            Assert.IsTrue(vm.IsAdminUser);
            Assert.IsTrue(vm.IsEditActive);
            Assert.IsTrue(vm.SelectedCellFromList.IsUserDefineCellType);
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestAdvancedUsersCannotEditUserCellTypesAdjustmentFactor()
        {
            SetCellTypesAndQualityControls();

            SetSystemStatusDomain(SystemStatus.Idle);
            var vm = _viewModelFactory.CreateCreateCellTypeViewModel(GetUser(UserPermissionLevel.eElevated));
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == 9001);

            Assert.IsNotNull(vm);
            Assert.IsNotNull(vm.SelectedCellType);
            Assert.IsFalse(vm.IsEditActive);
            Assert.IsTrue(vm.IsEditEnable);

            vm.EditCommand.Execute(null);

            // these are the values in the xaml for the MultipleBooleanAndConverter
            Assert.IsFalse(vm.IsAdminUser);
            Assert.IsTrue(vm.IsEditActive);
            Assert.IsTrue(vm.SelectedCellFromList.IsUserDefineCellType);
        }

        #region Setup methods

        private CellTypeViewModel GetVm(UserPermissionLevel level, uint cellTypeIndexToSelect,
            SystemStatus status = SystemStatus.Idle)
        {
            SetSystemStatusDomain(status);
            var vm = _viewModelFactory.CreateCellTypeViewModel(GetUser(level));
            vm.SelectedCellType = vm.AvailableCellTypes.FirstOrDefault(ct => ct.CellTypeIndex == cellTypeIndexToSelect);
            vm.SelectedCellTypeView = CellTypeViewSelection.ViewMode;
            return vm;
        }

        private void SetSystemStatusDomain(SystemStatus status)
        {
            var systemStatus = new SystemStatusDomain();
            systemStatus.SystemStatus = status;
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
        delegate void QcCallback(string username, string password, ref List<QualityControlDomain> qcs, ref uint count);

        private void SetCellTypesAndQualityControls()
        {
            uint numberOfFactoryCellTypes = 2;
            var factoryCellTypes = GetFactoryCellTypes();

            uint numberOfAllCellTypes = 4;
            var allCellTypes = GetAllCellTypes();

            uint numberOfQuality = 2;
            var qualDomainList = GetQualityControls();

            var cellTypeAccessMock = new Mock<ICellType>();
            cellTypeAccessMock.Setup(m => m.GetFactoryCellTypesAPI(ref It.Ref<List<CellTypeDomain>>.IsAny))
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
            qualTypeAccessMock.Setup(m => m.GetQualityControlListAPI(Username, Password, true, ref It.Ref<List<QualityControlDomain>>.IsAny, out It.Ref<uint>.IsAny))
                .Callback(new QcCallback((string username, string password, ref List<QualityControlDomain> qcs, ref uint c) =>
                {
                    qcs = qualDomainList;
                    c = numberOfQuality;
                }))
                .Returns(HawkeyeError.eSuccess);

            qualTypeAccessMock.Setup(m => m.GetQualityControlListAPI(Username, Password, true, ref It.Ref<List<QualityControlDomain>>.IsAny, out It.Ref<uint>.IsAny))
                .Callback(new QcCallback((string username, string password, ref List<QualityControlDomain> qcs, ref uint c) =>
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
                        }
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