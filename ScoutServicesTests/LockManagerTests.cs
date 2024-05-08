using Grpc.Core;
using GrpcClient;
using GrpcClient.Services;
using GrpcServer;
using GrpcServer.GrpcInterceptor;
using GrpcService;
using HawkeyeCoreAPI.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using Ninject;
using NUnit.Framework;
using ScoutDomains.Analysis;
using ScoutModels;
using ScoutModels.ExpandedSampleWorkflow;
using ScoutModels.Interfaces;
using ScoutModels.Service;
using ScoutModels.Settings;
using ScoutServices;
using ScoutServices.Enums;
using ScoutServices.Interfaces;
using ScoutUtilities.Enums;
using System;
using System.Reflection;
using GrpcServer.EventProcessors;
using GrpcServer.GrpcInterceptor.Attributes;
using HawkeyeCoreAPI;
using Ninject.Extensions.Factory;

namespace ScoutServicesTests
{
    [TestFixture]
    public class LockManagerTests
    {
        public const string _username = "username";
        public const string _password = "password";

        private ILockManager _lockManager;
        private Mock<LockManager> _mockLockManager;
        private bool _localLockedStatus;

        private OpcUaGrpcServer _opcUaGrpcServer;
        private OpcUaGrpcClient _grpcClient;
        private IKernel _kernel;
        private readonly Mock<ISecurityService> _mockSecurityService = new Mock<ISecurityService>();

        [SetUp]
        public void Setup()
        {
            _kernel = new StandardKernel(new OpcUaGrpcModule(true));
            _kernel.Bind<GrpcServices.GrpcServicesBase>().To<ScoutOpcUaGrpcService>().InSingletonScope();
            _kernel.Bind<OpcUaGrpcServer>().ToSelf().InSingletonScope();
            _kernel.Bind<LockResultProcessor>().ToSelf().InTransientScope();
            _kernel.Bind<ViCellStatusResultProcessor>().ToSelf().InTransientScope();
            _kernel.Bind<ViCellIdentifierResultProcessor>().ToSelf().InTransientScope();
            _kernel.Bind<SampleProcessingSampleStatusEventProcessor>().ToSelf().InTransientScope();
            _kernel.Bind<SampleProcessingWorkListCompleteEventProcessor>().ToSelf().InTransientScope();
            _kernel.Bind<ReagentUseRemainingResultProcessor>().ToSelf().InTransientScope();
            _kernel.Bind<WasteTubeCapacityResultProcessor>().ToSelf().InTransientScope();
            _kernel.Bind<DeleteSampleResultsEventProcessor>().ToSelf().InTransientScope();
            _kernel.Bind<IOpcUaGrpcFactory>().ToFactory();
            
            _mockLockManager = new Mock<LockManager>();
            _mockLockManager.Setup(m => m.OwnsLock(It.IsAny<string>())).Returns(true);
            _mockLockManager.CallBase = true; // use all other LockManager methods
            _kernel.Bind<ILockManager>().ToConstant(_mockLockManager.Object);

            _kernel.Bind<IOpcUaCfgManager>().To<OpcUaCfgManager>().InSingletonScope();
            _kernel.Bind<IDbSettingsService>().To<DbSettingsModel>().InSingletonScope();
            _kernel.Bind<ISmtpSettingsService>().To<SmtpSettingsModel>().InSingletonScope();
            _kernel.Bind<IAutomationSettingsService>().To<AutomationSettingsService>().InSingletonScope();
            _kernel.Bind<ISampleResultsManager>().To<SampleResultsManager>().InSingletonScope();
            _kernel.Bind<ISampleProcessingService>().To<SampleProcessingService>().InSingletonScope();
            _kernel.Bind<ICellTypeManager>().To<CellTypeManager>().InSingletonScope();
            _kernel.Bind<IConfigurationManager>().To<ConfigurationManager>().InSingletonScope();
            _kernel.Bind<Grpc.Core.Logging.ILogger>().To<OpcGrpcLogger>().InSingletonScope();

            var mockConfiguration = new Mock<IConfiguration>();
            _kernel.Bind<IConfiguration>().ToConstant(mockConfiguration.Object);

            var userRole = UserPermissionLevel.eNormal;
            var user = new UserDomain {UserID = _username, RoleID = userRole};
            _mockSecurityService.Reset();
            _mockSecurityService.Setup(m => m.LoginRemoteUser(_username, _password)).Returns(true);
            _mockSecurityService.Setup(m => m.LoginRemoteUser(
                It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            
            _kernel.Bind<ISecurityService>().ToConstant(_mockSecurityService.Object);
            
            _kernel.Bind<IWorkListModel>().To<WorkListModel>().InSingletonScope();
            _kernel.Bind<ICapacityManager>().To<CapacityManager>().InSingletonScope();
            _kernel.Bind<IDisplayService>().To<DisplayService>().InSingletonScope();
            _kernel.Bind<ISystemStatus>().To<HawkeyeCoreAPI.SystemStatus>().InSingletonScope();
            _kernel.Bind<IErrorLog>().To<HawkeyeCoreAPI.ErrorLog>().InSingletonScope();
            var sysStatusMock = new Mock<ISystemStatus>();
            var errorMock = new Mock<IErrorLog>();
            var iloggerMock = new Mock<Ninject.Extensions.Logging.ILogger>();
            var displaySvcMock = new Mock<IDisplayService>();
            var applicationStateServiceMock = new Mock<IApplicationStateService>();
            var instrumentStatusService = new InstrumentStatusService(sysStatusMock.Object, errorMock.Object, iloggerMock.Object, applicationStateServiceMock.Object);
            _kernel.Bind<IRunningWorkListModel>().To<RunningWorkListModel>().InSingletonScope();
            _kernel.Bind<IScoutModelsFactory>().ToFactory();
            _kernel.Bind<IInstrumentStatusService>().ToConstant(instrumentStatusService);
            _kernel.Bind<ScoutInterceptor>().ToSelf();
            var auditMock = new Mock<IAuditLog>();
            _kernel.Bind<IAuditLog>().ToConstant(auditMock.Object);
            _kernel.Bind<IMaintenanceService>().ToFactory();
        }

        [Test]
        public void TestLockManager()
        {
            try
            {
                // Create the lock manager
                _lockManager = _kernel.Get<ILockManager>();
                var clientManager = _kernel.Get<GrpcClientManager>();

                // Create and start server.
                _opcUaGrpcServer = _kernel.Get<OpcUaGrpcServer>();
                _opcUaGrpcServer.StartServer();

                // Create Grpc client.
                _grpcClient = _kernel.Get<OpcUaGrpcClient>();
                _grpcClient.Init(_username, _password);
                
                // Acquire the IObservable<LockedResult>
                var observeLockStateChanges = _lockManager.SubscribeStateChanges();
                Assert.IsNotNull(observeLockStateChanges);
                // Subscribe to changes to the lock state...
                observeLockStateChanges.Subscribe(SetLockStatus);

                // Verify LockManager indicates the system is not locked
                Assert.AreEqual(false, _lockManager.IsLocked());

                // Request Lock
                var resultLock = _grpcClient.SendRequestRequestLock(new RequestRequestLock());

                // Verify Success
                Assert.IsNotNull(resultLock);
                Assert.AreEqual(MethodResultEnum.Success, resultLock.MethodResult);

                // Verify LockManager indicates the system is now locked
                Assert.AreEqual(true, _lockManager.IsLocked());
                // Verify our local flag is now "locked"
                Assert.AreEqual(true, _localLockedStatus);

                // Request Unlock
                var resultUnlock = _grpcClient.SendRequestReleaseLock(new RequestReleaseLock());

                // Verify Success
                Assert.IsNotNull(resultUnlock);
                Assert.AreEqual(MethodResultEnum.Success, resultUnlock.MethodResult);

                // Verify LockManager indicates the system is now unlocked
                Assert.AreEqual(false, _lockManager.IsLocked());
                // Verify our local flag is now "unlocked"
                Assert.AreEqual(false, _localLockedStatus);

                // Request Lock with Insufficient privileges.
                var resultLockBadPermissions = _grpcClient.SendRequestRequestLock(new RequestRequestLock());

                // Confirm locking back to original state succeeds.
                Assert.IsNotNull(resultLockBadPermissions);
                Assert.AreEqual(MethodResultEnum.Success, resultLockBadPermissions.MethodResult);

                // Shutdown server
                _opcUaGrpcServer.ShutdownServer();
            }
            catch (Exception ex)
            {
                _opcUaGrpcServer?.ShutdownServer();
                Assert.Fail(ex.Message);
            }
        }

        private void SetLockStatus(LockResult result)
        {
            switch (result)
            {
                case LockResult.Locked:
                    _localLockedStatus = true;
                    break;
                case LockResult.Unlocked:
                    _localLockedStatus = false;
                    break;
            }
        }
    }
}
