using Grpc.Core;
using Grpc.Core.Logging;
using GrpcClient;
using GrpcClient.Services;
using GrpcServer;
using GrpcService;
using HawkeyeCoreAPI;
using HawkeyeCoreAPI.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using Ninject;
using Ninject.Parameters;
using NUnit.Framework;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutModels;
using ScoutModels.ExpandedSampleWorkflow;
using ScoutModels.Interfaces;
using ScoutModels.Service;
using ScoutModels.Settings;
using ScoutServices;
using ScoutServices.Interfaces;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using AutoMapper;
using Ninject.Extensions.Factory;
using ScoutModels.Security;
using SubstrateType = ScoutUtilities.Enums.SubstrateType;

namespace ScoutOpcUaTests
{
    public class BaseInterceptorTest
    {
        public string Username = "username";
        public string Password = "password";

        protected OpcUaGrpcServer OpcUaGrpcServer;
        protected OpcUaGrpcClient GrpcClient;

        protected IKernel Kernel;
        protected IMapper Mapper;

        protected readonly Mock<ISystemStatus> SysStatusMock = new Mock<ISystemStatus>();
        protected readonly Mock<ISecurityService> MockSecurityService = new Mock<ISecurityService>();
        protected readonly Mock<ILockManager> MockLockManager = new Mock<ILockManager>();
        protected readonly Mock<IConfiguration> MockConfiguration = new Mock<IConfiguration>();
        protected readonly Mock<IHardwareSettingsModel> MockHardwareSettings = new Mock<IHardwareSettingsModel>();
        protected readonly Mock<ICellTypeManager> MockCellTypeManager = new Mock<ICellTypeManager>();
        protected readonly Mock<IConfigurationManager> MockConfigurationManager = new Mock<IConfigurationManager>();
        protected readonly Mock<ISampleProcessingService> MockSampleProcessingService = new Mock<ISampleProcessingService>();
        protected readonly Mock<ISampleResultsManager> MockSampleResultsManager = new Mock<ISampleResultsManager>();
        protected InstrumentStatusService InstrumentStatusService;

        #region Setup & Teardown

        [SetUp]
        public void Setup()
        {
            Kernel = new StandardKernel(new OpcUaGrpcModule());

            Kernel.Bind<ILogger>().To<OpcGrpcLogger>().InSingletonScope();
            Kernel.Bind<IOpcUaCfgManager>().To<OpcUaCfgManager>().InSingletonScope();
            Kernel.Bind<IDbSettingsService>().To<DbSettingsModel>().InSingletonScope();
            Kernel.Bind<ISmtpSettingsService>().To<SmtpSettingsModel>().InSingletonScope();
            Kernel.Bind<IAutomationSettingsService>().To<AutomationSettingsService>().InSingletonScope();
            Kernel.Bind<IWorkListModel>().To<WorkListModel>().InSingletonScope();
            Kernel.Bind<ICapacityManager>().To<CapacityManager>().InSingletonScope();
            Kernel.Bind<IDisplayService>().To<DisplayService>().InSingletonScope();
            Kernel.Bind<IRunningWorkListModel>().To<RunningWorkListModel>().InSingletonScope();
            Kernel.Bind<IErrorLog>().To<ErrorLog>().InSingletonScope();

			Kernel.Bind<IConfigurationManager>().ToConstant(MockConfigurationManager.Object);
            Kernel.Bind<ISecurityService>().ToConstant(MockSecurityService.Object);
            Kernel.Bind<SecuredTask>().ToSelf().InTransientScope();
            Kernel.Bind<IScoutModelsFactory>().ToFactory();
            Kernel.Bind<ILockManager>().ToConstant(MockLockManager.Object);
            Kernel.Bind<ISampleProcessingService>().ToConstant(MockSampleProcessingService.Object);
            Kernel.Bind<ISampleResultsManager>().ToConstant(MockSampleResultsManager.Object);

            Kernel.Bind<IMaintenanceService>().To<MaintenanceService>().InSingletonScope();

            var errorMock = new Mock<IErrorLog>();
            var loggerMock = new Mock<Ninject.Extensions.Logging.ILogger>();

			var auditMock = new Mock<IAuditLog>();
			Kernel.Bind<IAuditLog>().ToConstant(auditMock.Object);

            var serialNumber = "12345";

            MockSampleResultsManager.Reset();

            MockHardwareSettings.Reset();
            MockHardwareSettings.Setup(m => m.GetSystemSerialNumber(ref serialNumber)).Returns(HawkeyeError.eSuccess);
            var applicationStateServiceMock = new Mock<IApplicationStateService>();
            InstrumentStatusService = new InstrumentStatusService(SysStatusMock.Object, errorMock.Object, loggerMock.Object, MockHardwareSettings.Object, applicationStateServiceMock.Object);

            MockConfigurationManager.Reset();
            MockConfigurationManager.Setup(m =>
                m.ImportConfig(Username, Password, It.IsAny<RequestImportConfig>())).Returns(HawkeyeError.eSuccess);

            MockSampleProcessingService.Reset();
            MockSampleProcessingService.Setup(m =>
                m.EjectStage(Username, Password, It.IsAny<bool>())).Returns(true);
            MockSampleProcessingService.Setup(m =>
                m.PauseProcessing(Username, Password)).Returns(true);
            MockSampleProcessingService.Setup(m =>
                m.StopProcessing(Username, Password)).Returns(true);

            Kernel.Bind<ISystemStatus>().ToConstant(SysStatusMock.Object);
            Kernel.Bind<IInstrumentStatusService>().ToConstant(InstrumentStatusService);
            Kernel.Bind<IConfiguration>().ToConstant(MockConfiguration.Object);
            Kernel.Bind<ICellTypeManager>().ToConstant(MockCellTypeManager.Object);

            MockSecurityService.Reset();
            var dummyUser = new UserDomain(Username);
            MockSecurityService.Setup(m => m.LoginRemoteUser(Username, Password)).Returns(true);
            Mapper = Kernel.Get<IMapper>();
        }

        [TearDown]
        public void TearDown()
        {
            CloseGrpc();
        }

        #endregion

        #region Helper Methods

        protected void Init(LockStateEnum currentLockState, UserPermissionLevel userPermission = UserPermissionLevel.eAdministrator, ScoutUtilities.Enums.SystemStatus systemStatus = ScoutUtilities.Enums.SystemStatus.Idle, bool ownsLockReturnsTrue = true)
        {
            SetupInstrumentStatusModel(systemStatus);

            var isLocked = currentLockState == LockStateEnum.Locked;
            MockLockManager.Reset();
            MockLockManager.Setup(m => m.IsLocked()).Returns(isLocked);
            MockLockManager.Setup(m => m.OwnsLock(It.IsAny<string>())).Returns(ownsLockReturnsTrue);
           
            MockSecurityService.Setup(m => m.GetUserRole(Username)).Returns(userPermission);
            MockSecurityService.Setup(m => m.LoginRemoteUser(Username, Password)).Returns(true);
            InitGrpc();
        }

        protected virtual void AssertRpcExceptionDetailText(RpcException rpcException)
        {
            // assert that the rpcException.Status.Detail string is what is expected
            throw new NotImplementedException();
        }

        protected void HandleShouldFailException(Exception e)
        {
            if (e is TestClientException tce)
            {
                Assert.AreEqual(TestClientException.ERROR_CODE_BAD_LOCK_STATE, tce.ErrorCode,
                    "TestClientException does not match expected");
                return;
            }

            if (e is RpcException rpcException)
            {
                AssertRpcExceptionDetailText(rpcException);
                return;
            }

            if (e is Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        protected void InitGrpc()
        {
            // Create and start server.
            OpcUaGrpcServer = Kernel.Get<OpcUaGrpcServer>();
            OpcUaGrpcServer.StartServer();

            // Create Grpc client.
            GrpcClient = Kernel.Get<OpcUaGrpcClient>();
            GrpcClient.Init(Username, Password);
        }

        protected void CloseGrpc()
        {
            // Shutdown server
            OpcUaGrpcServer?.ShutdownServer();
        }

        protected ISampleProcessingService GetSampleProcessingService(CellTypeDomain cellTypeDomain,
            QualityControlDomain qcDomain, CellTypeDomain cellType2 = null,
            Mock<IRunningWorkListModel> runningWorkListModelMock = null)
        {
            SetupCellTypeManager(cellTypeDomain, qcDomain, cellType2);
            var workListModel = SetupWorkListModel();

            if (runningWorkListModelMock != null)
            {
                var service = Kernel.Get<ISampleProcessingService>(
                    new ConstructorArgument("runningWorkListModel", runningWorkListModelMock.Object),
                    new ConstructorArgument("cellTypeManager", MockCellTypeManager.Object),
                    new ConstructorArgument("workListModel", workListModel.Object));
                return service;
            }
            else
            {
                var service = Kernel.Get<ISampleProcessingService>(
                    new ConstructorArgument("cellTypeManager", MockCellTypeManager.Object),
                    new ConstructorArgument("workListModel", workListModel.Object));
                return service;
            }
        }

        protected static Mock<IWorkListModel> SetupWorkListModel()
        {
            var workListModel = new Mock<IWorkListModel>();
            workListModel.Setup(m => m.CheckReagentsAndWasteTray(It.IsAny<int>(),
                             It.IsAny<SubstrateType>(), out It.Ref<bool>.IsAny,
                             out It.Ref<bool>.IsAny, It.IsAny<bool>()))
                         .Returns(false);
            return workListModel;
        }

        protected void SetupCellTypeManager(CellTypeDomain cellTypeDomain, QualityControlDomain qcDomain, CellTypeDomain cellType2)
        {
            MockCellTypeManager.Reset();
            MockCellTypeManager.Setup(m =>
                m.CreateCellType(Username, Password, It.IsAny<CellTypeDomain>(), "", It.IsAny<bool>())).Returns(true);
            MockCellTypeManager.Setup(m =>
                m.DeleteCellType(Username, Password, It.IsAny<CellTypeDomain>(), It.IsAny<bool>())).Returns(true);
            MockCellTypeManager.Setup(m =>
                m.CreateQualityControl(Username, Password, It.IsAny<QualityControlDomain>(), It.IsAny<bool>())).Returns(true);

            if (cellType2 == null)
            {
                MockCellTypeManager.Setup(m => m.GetCellTypeDomain(Username, Password, It.IsAny<string>()))
                                    .Returns(cellTypeDomain);
            }
            else
            {
                MockCellTypeManager.Setup(m => m.GetCellTypeDomain(Username, Password, cellTypeDomain.CellTypeName))
                                    .Returns(cellTypeDomain);
                MockCellTypeManager.Setup(m => m.GetCellTypeDomain(Username, Password, cellType2.CellTypeName))
                                    .Returns(cellType2);
            }

            MockCellTypeManager.Setup(m => m.GetQualityControlDomain(Username, Password, It.IsAny<string>()))
                                .Returns(qcDomain);
        }

        public delegate void SystemStatusCallback(ref SystemStatusData systemStatus);

        protected void SetupInstrumentStatusModel(ScoutUtilities.Enums.SystemStatus inputSystemStatus)
        {
            // Setup the SystemStatusService with the mocks it needs
            var systemStatusData = new SystemStatusData
            {
                status = inputSystemStatus,
                remainingReagentPackUses = 123,
                sample_tube_disposal_remaining_capacity = 69,
                sensor_reagent_pack_in_place = eSensorStatus.ssStateActive,
                sensor_reagent_pack_door_closed = eSensorStatus.ssStateActive,
                sensor_carousel_detect = eSensorStatus.ssStateActive,
                sampleStageLocation = new ScoutUtilities.Common.SamplePosition('Y', 1)
            };

            SysStatusMock.Reset();

            SysStatusMock.Setup(m => m.GetSystemStatusAPI(ref It.Ref<SystemStatusData>.IsAny))
                          .Callback(new SystemStatusCallback((ref SystemStatusData systemStatus) => { systemStatus = systemStatusData; }))
                          .Returns(IntPtr.Zero);
            SysStatusMock.Setup(m => m.FreeSystemStatusAPI(It.IsAny<IntPtr>()));
            InstrumentStatusService.GetAndPublishSystemStatus();
        }

        #endregion
    }
}