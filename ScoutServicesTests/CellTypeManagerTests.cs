using Google.Protobuf.WellKnownTypes;
using Grpc.Core.Logging;
using GrpcClient;
using GrpcClient.Services;
using GrpcServer;
using GrpcService;
using HawkeyeCoreAPI.Facade;
using HawkeyeCoreAPI.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using Ninject;
using NUnit.Framework;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutModels;
using ScoutModels.ExpandedSampleWorkflow;
using ScoutModels.Interfaces;
using ScoutModels.Service;
using ScoutModels.Settings;
using ScoutServices.Interfaces;
using ScoutServices.Ninject;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Ninject.Extensions.Factory;
using ScoutServices;
using ScoutUtilities.Structs;
using TestSupport;
using CellType = GrpcService.CellType;

namespace ScoutServicesTests
{
    [TestFixture]
    public class CellTypeManagerTests : BaseTest
    {
        private Grpc.Core.Logging.ILogger _grpcLogger;
        private ICellTypeManager _cellTypeManager;
        private OpcUaGrpcServer _opcUaGrpcServer;
        private OpcUaGrpcClient _grpcClient;
        private static Mock<IErrorLog> _errorMock;
        private static Mock<ISystemStatus> _instrStatusMock;
        private static InstrumentStatusService _instrumentStatusService;
        private readonly Mock<ISecurityService> _mockSecurityService = new Mock<ISecurityService>();
        private readonly Mock<ILockManager> _mockLockManager = new Mock<ILockManager>();

        [OneTimeSetUp]
        public void ClassInit()
        {
            _instrStatusMock = new Mock<ISystemStatus>();
            _errorMock = new Mock<IErrorLog>();
            var iloggerMock = new Mock<Ninject.Extensions.Logging.ILogger>();
            var sysStatusMock = new Mock<ISystemStatus>();
            var applicationStateServiceMock = new Mock<IApplicationStateService>();
            _instrumentStatusService = new InstrumentStatusService(_instrStatusMock.Object, _errorMock.Object, iloggerMock.Object, applicationStateServiceMock.Object);
            var mockConfiguration = new Mock<IConfiguration>();
            
            Kernel = new StandardKernel(new OpcUaGrpcModule());
            Kernel.Bind<ILogger>().To<OpcGrpcLogger>().InSingletonScope();
            Kernel.Bind<IOpcUaCfgManager>().To<OpcUaCfgManager>().InSingletonScope();
            Kernel.Bind<IDbSettingsService>().To<DbSettingsModel>().InSingletonScope();
            Kernel.Bind<ISmtpSettingsService>().To<SmtpSettingsModel>().InSingletonScope();
            Kernel.Bind<IAutomationSettingsService>().To<AutomationSettingsService>().InSingletonScope();
            Kernel.Bind<ISampleResultsManager>().To<SampleResultsManager>().InSingletonScope();
            Kernel.Bind<ISampleProcessingService>().To<SampleProcessingService>().InSingletonScope();
            Kernel.Bind<ICellTypeManager>().To<CellTypeManager>().InSingletonScope();
            Kernel.Bind<IConfigurationManager>().To<ConfigurationManager>().InSingletonScope();

            Kernel.Bind<ISecurityService>().ToConstant(_mockSecurityService.Object);
            Kernel.Bind<ILockManager>().ToConstant(_mockLockManager.Object);

            Kernel.Bind<IConfiguration>().ToConstant(mockConfiguration.Object);
            Kernel.Bind<IWorkListModel>().To<WorkListModel>().InSingletonScope();
            Kernel.Bind<ICapacityManager>().To<CapacityManager>().InSingletonScope();
            Kernel.Bind<IDisplayService>().To<DisplayService>().InSingletonScope();
            Kernel.Bind<IAuditLog>().To<HawkeyeCoreAPI.AuditLog>().InSingletonScope();
            Kernel.Bind<IErrorLog>().To<HawkeyeCoreAPI.ErrorLog>().InSingletonScope();
            Kernel.Bind<ISystemStatus>().ToConstant(sysStatusMock.Object);
            Kernel.Bind<IInstrumentStatusService>().ToConstant(_instrumentStatusService);
            Kernel.Bind<IRunningWorkListModel>().To<RunningWorkListModel>().InSingletonScope();
            Kernel.Bind<IScoutModelsFactory>().ToFactory();
            Kernel.Bind<IMaintenanceService>().ToFactory();

            _grpcLogger = Kernel.Get<ILogger>();
            _cellTypeManager = Kernel.Get<ICellTypeManager>();
        }
        [SetUp]
        public void SetUp()
        {
            _errorMock.Reset();
            _instrStatusMock.Reset();
            _mockSecurityService.Reset();
            _mockSecurityService.Setup(m => m.LoginRemoteUser(Username, Password)).Returns(true);
            SetUser(Username, UserPermissionLevel.eElevated);
            _mockSecurityService.Setup(m => m.GetUserRole(It.IsAny<string>())).Returns(UserPermissionLevel.eElevated);
        }

        #region Create Cell Tests

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellTypeGrpc()
        {
            try
            {
                _mockLockManager.Setup(m => m.IsLocked()).Returns(true);
                _mockLockManager.Setup(m => m.OwnsLock(It.IsAny<string>())).Returns(true);
                // Create and start server.
                _opcUaGrpcServer = Kernel.Get<OpcUaGrpcServer>();
                _opcUaGrpcServer.StartServer();

                // Create Grpc client.
                _grpcClient = Kernel.Get<OpcUaGrpcClient>();
                _grpcClient.Init(Username, Password);

                // Formulate request
                var request = new RequestCreateCellType
                {
                    Cell = new CellType
                    {
                        CellTypeName = "Test Cell!",
                        MinDiameter = 2.05f,
                        MaxDiameter = 10.01f,
                        NumImages = 10,
                        CellSharpness = 99,
                        MinCircularity = .9f,
                        DeclusterDegree = DeclusterDegreeEnum.Medium,
                        NumAspirationCycles = 2,
                        ViableSpotBrightness = 50.3f,
                        ViableSpotArea = 20.0f,
                        NumMixingCycles = 2,
                        ConcentrationAdjustmentFactor = 10.5f,
                    },
                };

                // Invoke SendRequestCreateCellType
                var getCreateCellResult = _grpcClient.SendRequestCreateCellType(request);

                // Verify Success
                Assert.IsNotNull(getCreateCellResult);
                Assert.AreEqual(MethodResultEnum.Success, getCreateCellResult.MethodResult);

                // Shutdown server
                _opcUaGrpcServer.ShutdownServer();
            }
            catch (Exception ex)
            {
                _opcUaGrpcServer?.ShutdownServer();
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellType_GoodValues()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();

            Assert.IsTrue(_cellTypeManager.SaveCellTypeValidation(cell, false));
            Assert.IsTrue(_cellTypeManager.CreateCellType(Username, Password, cell, "", false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_NullName()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.CellTypeName = null;
            cell.TempCellName = null;

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_EmptyName()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.CellTypeName = "";
            cell.TempCellName = "";

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_InvalidCharactersInName()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.CellTypeName = @"?<>/\|:";
            cell.TempCellName = @"?<>/\|:";

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_LowMinDiameter()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.MinimumDiameter = ApplicationConstants.LowerDiameterLimit - 0.01;

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_HighMinDiameter()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.MinimumDiameter = ApplicationConstants.UpperDiameterLimit + 0.01;

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_MinGreaterThanMaxDiameter()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.MinimumDiameter = 50;
            cell.MaximumDiameter = 10;

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_LowMaxDiameter()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.MinimumDiameter = ApplicationConstants.LowerDiameterLimit - 0.01;

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_HighMaxDiameter()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.MinimumDiameter = ApplicationConstants.UpperDiameterLimit + 0.01;

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_LowNumImages()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.Images = ApplicationConstants.MinimumCelltypeImageCount - 1;

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_HighNumImages()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.Images = ApplicationConstants.MaximumCelltypeImageCount + 1;

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_LowCellSharpness()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.CellSharpness = (float)(ApplicationConstants.LowerSharpLimit - 0.01);

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_HighCellSharpness()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.CellSharpness = (float)(ApplicationConstants.UpperSharpLimit + 0.01);

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_LowCircularity()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.MinimumCircularity = ApplicationConstants.LowerCircularityLimit - 0.01;

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_HighCircularity()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.MinimumCircularity = ApplicationConstants.UpperCircularityLimit + 0.01;

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_LowAspirationCycles()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.AspirationCycles = ApplicationConstants.LowerAspirationCyclesLimit - 1;

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_HighAspirationCycles()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.AspirationCycles = ApplicationConstants.UpperAspirationCyclesLimit + 1;

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_LowSpotBrightness()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.AnalysisDomain.AnalysisParameter.Last().ThresholdValue = (float)(ApplicationConstants.LowerCellTypeSpotBrightnessLimit - 0.01);

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_HighSpotBrightness()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.AnalysisDomain.AnalysisParameter.Last().ThresholdValue = (float)(ApplicationConstants.UpperCellTypeSpotBrightnessLimit + 0.01);

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_LowSpotArea()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.AnalysisDomain.AnalysisParameter.First().ThresholdValue = (float)(ApplicationConstants.LowerCellTypeSpotAreaLimit - 0.01);

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_HighSpotArea()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.AnalysisDomain.AnalysisParameter.First().ThresholdValue = (float)(ApplicationConstants.UpperCellTypeSpotAreaLimit + 0.01);

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_LowMixingCycles()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.AnalysisDomain.MixingCycle = ApplicationConstants.LowerMixingCyclesLimit - 1;

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_HighMixingCycles()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.AnalysisDomain.MixingCycle = ApplicationConstants.UpperMixingCyclesLimit + 1;

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_LowAdjustmentFactor()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.CalculationAdjustmentFactor = (float)(ApplicationConstants.LowerCellTypeAdjustmentFactorValue - 0.01);

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_HighAdjustmentFactor()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.CalculationAdjustmentFactor = (float)(ApplicationConstants.UpperCellTypeAdjustmentFactorValue + 0.01);

            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateCellValidation_OutOfRangeDeclusterDegree()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.DeclusterDegree = (eCellDeclusterSetting)1000;
            Assert.IsFalse(_cellTypeManager.SaveCellTypeValidation(cell, false));
        }

        #endregion

        #region Delete Cell Types

        [Test]
        [Ignore("Requires refactoring")]
        public void TestDeleteCellTypeGrpc()
        {
            SetCellTypesAndQualityControls();
            _mockLockManager.Setup(m => m.IsLocked()).Returns(true);
            _mockLockManager.Setup(m => m.OwnsLock(It.IsAny<string>())).Returns(true);
            try
            {
                // Create and start server.
                _opcUaGrpcServer = Kernel.Get<OpcUaGrpcServer>();
                _opcUaGrpcServer.StartServer();

                // Create Grpc client.
                _grpcClient = Kernel.Get<OpcUaGrpcClient>();
                _grpcClient.Init(Username, Password);

                var cell = GetCellType_GoodValues();
                Assert.IsTrue(_cellTypeManager.SaveCellTypeValidation(cell, true));
                Assert.IsTrue(_cellTypeManager.CreateCellType(Username, Password, cell, "", false));

                var request = new RequestDeleteCellType
                {
                    CellTypeName = cell.CellTypeName
                };
                // Invoke SendRequestDeleteCellType
                var getDeleteCellResult = _grpcClient.SendRequestDeleteCellType(request);

                // Verify Success
                Assert.IsNotNull(getDeleteCellResult);
                Assert.AreEqual(MethodResultEnum.Success, getDeleteCellResult.MethodResult);

                // Shutdown server
                _opcUaGrpcServer.ShutdownServer();
            }
            catch (Exception ex)
            {
                _opcUaGrpcServer?.ShutdownServer();
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCantDeleteFactoryCellType()
        {
            SetCellTypesAndQualityControls();
            var factoryCellType = CellTypeFacade.Instance.GetFactoryCellTypes_BECall().First();

            Assert.IsFalse(_cellTypeManager.CanPerformDelete(factoryCellType));
            Assert.IsFalse(_cellTypeManager.DeleteCellType(Username, Password, factoryCellType.CellTypeName, false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCantDeleteWhenInBadStates()
        {
            var cell = GetCellType_GoodValues();
            Assert.IsTrue(_cellTypeManager.SaveCellTypeValidation(cell, false));
            Assert.IsTrue(_cellTypeManager.CreateCellType(Username, Password, cell, "", false));

            MessageBus.Default.Publish(new Notification<SystemStatusDomain>(GetSystemStatusDomain(SystemStatus.ProcessingSample), MessageToken.NewSystemStatus));
            Assert.IsFalse(_cellTypeManager.CanPerformDelete(cell));
            Assert.IsFalse(_cellTypeManager.DeleteCellType(Username, Password, cell.CellTypeName, false));

            MessageBus.Default.Publish(new Notification<SystemStatusDomain>(GetSystemStatusDomain(SystemStatus.Pausing), MessageToken.NewSystemStatus));
            Assert.IsFalse(_cellTypeManager.CanPerformDelete(cell));
            Assert.IsFalse(_cellTypeManager.DeleteCellType(Username, Password, cell.CellTypeName, false));

            MessageBus.Default.Publish(new Notification<SystemStatusDomain>(GetSystemStatusDomain(SystemStatus.Paused), MessageToken.NewSystemStatus));
            Assert.IsFalse(_cellTypeManager.CanPerformDelete(cell));
            Assert.IsFalse(_cellTypeManager.DeleteCellType(Username, Password, cell.CellTypeName, false));

            MessageBus.Default.Publish(new Notification<SystemStatusDomain>(GetSystemStatusDomain(SystemStatus.SearchingTube), MessageToken.NewSystemStatus));
            Assert.IsFalse(_cellTypeManager.CanPerformDelete(cell));
            Assert.IsFalse(_cellTypeManager.DeleteCellType(Username, Password, cell.CellTypeName, false));

            MessageBus.Default.Publish(new Notification<SystemStatusDomain>(GetSystemStatusDomain(SystemStatus.Stopping), MessageToken.NewSystemStatus));
            Assert.IsFalse(_cellTypeManager.CanPerformDelete(cell));
            Assert.IsFalse(_cellTypeManager.DeleteCellType(Username, Password, cell.CellTypeName, false));
        }

        delegate void SystemStatusCallback(ref SystemStatusData systemStatusData);

        [Test]
        [Ignore("This needs to be updated to use new Reactive system status changes/updates")]
        public void TestCanDeleteInGoodStates()
        {
            var cell = GetCellType_GoodValues();
            Assert.IsTrue(_cellTypeManager.SaveCellTypeValidation(cell, false));
            Assert.IsTrue(_cellTypeManager.CreateCellType(Username, Password, cell, "", false));
            
            MessageBus.Default.Publish(new Notification<SystemStatusDomain>(GetSystemStatusDomain(SystemStatus.Idle), MessageToken.NewSystemStatus));
            Assert.IsTrue(_cellTypeManager.CanPerformDelete(cell));
            
            MessageBus.Default.Publish(new Notification<SystemStatusDomain>(GetSystemStatusDomain(SystemStatus.Stopped), MessageToken.NewSystemStatus));
            Assert.IsTrue(_cellTypeManager.CanPerformDelete(cell));
            
            MessageBus.Default.Publish(new Notification<SystemStatusDomain>(GetSystemStatusDomain(SystemStatus.Faulted), MessageToken.NewSystemStatus));
            Assert.IsTrue(_cellTypeManager.CanPerformDelete(cell));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCantDeleteNonExistentCellType()
        {
            SetCellTypesAndQualityControls();

            Assert.IsFalse(_cellTypeManager.DeleteCellType(Username, Password, "Doesn't exist", false));
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCantDeleteQualityControlCell()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            cell.IsQualityControlCellType = true;
            cell.IsUserDefineCellType = true;

            Assert.IsFalse(_cellTypeManager.CanPerformDelete(cell));
            Assert.IsFalse(_cellTypeManager.DeleteCellType(Username, Password, cell.CellTypeName, false));
        }


        [Test]
        [Ignore("Requires refactoring ")]
        public void TestDeleteExistingCellType()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            //var startCellCount = CellTypeFacade.Instance.GetAllCellTypesCopy().Count;

            //Assert.IsTrue(_cellTypeManager.SaveCellTypeValidation(cell, false));
            //Assert.IsTrue(_cellTypeManager.CreateCellType(Username, Password, cell, false));
            //Assert.IsTrue(CellTypeFacade.Instance.GetAllCellTypesCopy().Count > startCellCount);
            //Assert.IsTrue(_cellTypeManager.DeleteCellType(Username, Password, cell, false));
            //Assert.IsTrue(CellTypeFacade.Instance.GetAllCellTypesCopy().Count == startCellCount);
        }

        [Test]
        [Ignore("Requires refactoring ")]
        public void TestDeleteExistingCellTypeByName()
        {
            SetCellTypesAndQualityControls();

            var cell = GetCellType_GoodValues();
            //var startCellCount = CellTypeFacade.Instance.GetAllCellTypesCopy().Count;

            //Assert.IsTrue(_cellTypeManager.SaveCellTypeValidation(cell, false));
            //Assert.IsTrue(_cellTypeManager.CreateCellType(Username, Password, cell, false));
            //Assert.IsTrue(CellTypeFacade.Instance.GetAllCellTypesCopy().Count > startCellCount);
            //Assert.IsTrue(_cellTypeManager.DeleteCellType(Username, Password, cell.CellTypeName, false));
            //Assert.IsTrue(CellTypeFacade.Instance.GetAllCellTypesCopy().Count == startCellCount);
        }

        #endregion

        #region Quality Control Tests

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateQualityControlGrpc()
        {
            try
            {
                _mockLockManager.Setup(m => m.IsLocked()).Returns(true);
                _mockLockManager.Setup(m => m.OwnsLock(It.IsAny<string>())).Returns(true);

                // Create and start server.
                _opcUaGrpcServer = Kernel.Get<OpcUaGrpcServer>();
                _opcUaGrpcServer.StartServer();

                // Create Grpc client.
                _grpcClient = Kernel.Get<OpcUaGrpcClient>();
                _grpcClient.Init(Username, Password);

                SetCellTypesAndQualityControls();
                var cellTypeName = GetFactoryCellTypes().First().CellTypeName;
                var date = DateTime.Now.ToUniversalTime();
                date.AddDays(30);

                // Formulate request
                var request = new RequestCreateQualityControl
                {
                    QualityControl = new QualityControl
                    {
                        CellTypeName = cellTypeName,
                        AcceptanceLimits = 10,
                        AssayParameter = AssayParameterEnum.Concentration,
                        AssayValue = 2.0d,
                        Comments = "This is a test!",
                        ExpirationDate = Timestamp.FromDateTime(date),
                        LotNumber = "121",
                        QualityControlName = "QCName"
                    }
                };

                // Invoke SendRequestCreateCellType
                var getQCResult = _grpcClient.SendRequestCreateQualityControl(request);

                // Verify Success
                Assert.IsNotNull(getQCResult);
                Assert.AreEqual(MethodResultEnum.Success, getQCResult.MethodResult);

                // Shutdown server
                _opcUaGrpcServer.ShutdownServer();
            }
            finally
            {
                try
                {
                    _opcUaGrpcServer?.ShutdownServer();
                }
                catch (Exception)
                {
                    // Ignore exceptions that occur during shutdown
                }
            }
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void TestCreateQualityControlGrpc_BadName()
        {
            try
            {
                _mockLockManager.Setup(m => m.IsLocked()).Returns(true);
                _mockLockManager.Setup(m => m.OwnsLock(Username)).Returns(true);

                // Create and start server.
                _opcUaGrpcServer = Kernel.Get<OpcUaGrpcServer>();
                _opcUaGrpcServer.StartServer();

                // Create Grpc client.
                _grpcClient = Kernel.Get<OpcUaGrpcClient>();
                _grpcClient.Init(Username, Password);

                SetCellTypesAndQualityControls();
                var date = DateTime.Now.ToUniversalTime();
                date.AddDays(30);

                // Formulate request
                var request = new RequestCreateQualityControl
                {
                    QualityControl = new QualityControl
                    {
                        CellTypeName = "Doesn't exist!!",
                        AcceptanceLimits = 10,
                        AssayParameter = AssayParameterEnum.Concentration,
                        AssayValue = 2.0d,
                        Comments = "This is a test!",
                        ExpirationDate = Timestamp.FromDateTime(date),
                        LotNumber = "121",
                        QualityControlName = "QCName"
                    }
                };

                // Invoke SendRequestCreateCellType
                var getQCResult = _grpcClient.SendRequestCreateQualityControl(request);

                // Verify Success
                Assert.IsNotNull(getQCResult);
                Assert.AreNotEqual(MethodResultEnum.Success, getQCResult.MethodResult);

                // Shutdown server
                _opcUaGrpcServer.ShutdownServer();
            }
            catch (Exception ex)
            {
                _opcUaGrpcServer?.ShutdownServer();
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void TestCreateQualityControl_GoodValues()
        {
            var cellTypeName = GetFactoryCellTypes().First().CellTypeName;
            var date = DateTime.Now.ToUniversalTime();
            date.AddDays(30);

            var qc = GetGoodQualityControlDomain();
            Assert.IsTrue(_cellTypeManager.QualityControlValidation(qc, false));
            Assert.IsTrue(_cellTypeManager.CreateQualityControl(Username, Password, qc, false));
        }

        [Test]
        public void TestCreateQualityControl_BadQCName()
        {
            var qc = GetGoodQualityControlDomain();
            qc.QcName = "";
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));
            qc.QcName = null;
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));
        }

        [Test]
        public void TestCreateQualityControl_BadQCName_InvalidCharacters()
        {
            var qc = GetGoodQualityControlDomain();
            qc.QcName = @"?<>/\|:";
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));
            qc.QcName = null;
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));
        }

        [Test]
        public void TestCreateQualityControl_QcNameTooLong()
        {
            var qc = GetGoodQualityControlDomain();
            qc.QcName = $"This Quality Control name is too long. It can't be more than {ApplicationConstants.CellTypeNameLimit} characters";
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));
        }

        [Test]
        public void TestCreateQualityControl_BadLotInformation()
        {
            var qc = GetGoodQualityControlDomain();
            qc.LotInformation = "";
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));
            qc.LotInformation = null;
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));
        }

        [Test]
        public void TestCreateQualityControl_LotInformationTooLong()
        {
            var qc = GetGoodQualityControlDomain();
            qc.LotInformation = $"Lot information can't be more than {ApplicationConstants.QualityControlLotLimit}";
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));
        }

        [Test]
        public void TestCreateQualityControl_CommentTooLong()
        {
            var qc = GetGoodQualityControlDomain();
            qc.CommentText = $"This comment has too many characters. Quality Control comments are not allowed to be more than " +
                             $"{ApplicationConstants.QualityControlCommentLimit} characters long";
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));
        }

        [Test]
        public void TestCreateQualityControl_BadAssayParameter()
        {
            var qc = GetGoodQualityControlDomain();
            qc.AssayParameter = (assay_parameter)999;
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));
        }

        [Test]
        public void TestCreateQualityControl_BadAssayValues()
        {
            var qc = GetGoodQualityControlDomain();
            qc.AssayParameter = assay_parameter.ap_Size;
            qc.AssayValue = ApplicationConstants.LowerConcentrationAssayLimit - 0.01;
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));
            qc.AssayValue = ApplicationConstants.UpperConcentrationAssayLimit + 0.01;
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));

            qc.AssayParameter = assay_parameter.ap_PopulationPercentage;
            qc.AssayValue = ApplicationConstants.LowerViabilityAssayLimit - 0.01;
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));
            qc.AssayValue = ApplicationConstants.UpperViabilityAssayLimit + 0.01;
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));
        }

        [Test]
        public void TestCreateQualityControl_BadAcceptanceLimit()
        {
            var qc = GetGoodQualityControlDomain();
            qc.AcceptanceLimit = null;
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));

            qc.AcceptanceLimit = ApplicationConstants.MinimumQualityControlAcceptanceLimit - 1;
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));
            qc.AcceptanceLimit = ApplicationConstants.MaximumQualityControlAcceptanceLimit + 1;
            Assert.IsFalse(_cellTypeManager.QualityControlValidation(qc, false));
        }

        private QualityControlDomain GetGoodQualityControlDomain()
        {
            return new QualityControlDomain
            {
                AcceptanceLimit = 20,
                AssayParameter = assay_parameter.ap_Concentration,
                AssayValue = 5.5,
                CellTypeIndex = GetFactoryCellTypes().First().CellTypeIndex,
                CellTypeName = GetFactoryCellTypes().First().CellTypeName,
                CommentText = "Woohoo!",
                ExpirationDate = DateTime.Now.AddDays(20),
                LotInformation = "121",
                QcName = "Woohoo QC!"
            };
        }

        #endregion

        #region GetCellTypes

        [Test]
        [Ignore("Requires refactoring")]
        public void TestGetCellTypes()
        {
            try
            {
                _grpcLogger = Kernel.Get<ILogger>();
                _cellTypeManager = Kernel.Get<ICellTypeManager>();

                // Create and start server.
                _opcUaGrpcServer = Kernel.Get<OpcUaGrpcServer>();
                _opcUaGrpcServer.StartServer();

                // Create Grpc client.
                _grpcClient = Kernel.Get<OpcUaGrpcClient>();
                _grpcClient.Init(Username, Password);

                #region Subscribe

                // Acquire the IObservable<SampleResult[]>
                var observeCellTypeRetrieval = _cellTypeManager.SubscribeCellTypeRetrieval();
                Assert.IsNotNull(observeCellTypeRetrieval);
                // Subscribe to cell type retrieval...
                observeCellTypeRetrieval.Subscribe(CellTypeRetrievalCallback);

                #endregion

                #region GetCellTypes

                // Login to the backend so it knows which user to retrieve the cell types.
                var res = HawkeyeCoreAPI.User.LoginConsoleUserAPI(Username, Password);

                // Send Request
                var request = new RequestGetCellTypes();
                var response = _grpcClient.SendRequestGetCellTypes(request);

                // Verify Success
                Assert.IsNotNull(response);
                Assert.AreEqual(response.MethodResult, MethodResultEnum.Success);

                #endregion

                // Shutdown server
                _opcUaGrpcServer.ShutdownServer();
            }
            catch (Exception ex)
            {
                _opcUaGrpcServer?.ShutdownServer();
                Assert.Fail(ex.Message);
            }
        }

        private void CellTypeRetrievalCallback(List<CellTypeDomain> cellTypes)
        {
            _grpcLogger.Info($"Found a total of {cellTypes.Count} cell types.");
        }

        #endregion

        #region GetQualityControls

        [Test]
        public void TestGetQualityControls()
        {
            try
            {
                _grpcLogger = Kernel.Get<ILogger>();
                _cellTypeManager = Kernel.Get<ICellTypeManager>();

                // Create and start server.
                _opcUaGrpcServer = Kernel.Get<OpcUaGrpcServer>();
                _opcUaGrpcServer.StartServer();

                // Create Grpc client.
                _grpcClient = Kernel.Get<OpcUaGrpcClient>();
                _grpcClient.Init(Username, Password);

                #region Subscribe

                // Acquire the IObservable<SampleResult[]>
                var observeQualityControlRetrieval = _cellTypeManager.SubscribeQualityControlRetrieval();
                Assert.IsNotNull(observeQualityControlRetrieval);
                // Subscribe to quality control retrieval...
                observeQualityControlRetrieval.Subscribe(QualityControlRetrievalCallback);

                #endregion

                #region GetQualityControls

                // Login to the backend so it knows which user to retrieve the cell types.
                var res = HawkeyeCoreAPI.User.LoginConsoleUserAPI(Username, Password);

                // Send Request
                var request = new RequestGetQualityControls();
                var response = _grpcClient.SendRequestGetQualityControls(request);

                // Verify Success
                Assert.IsNotNull(response);
                Assert.AreEqual(response.MethodResult, MethodResultEnum.Success);

                #endregion

                // Shutdown server
                _opcUaGrpcServer.ShutdownServer();
            }
            catch (Exception ex)
            {
                _opcUaGrpcServer?.ShutdownServer();
                Assert.Fail(ex.Message);
            }
        }

        private void QualityControlRetrievalCallback(List<QualityControlDomain> qualityControls)
        {
            _grpcLogger.Info($"Found a total of {qualityControls.Count} quality controls.");
        }

        #endregion

        #region Setup

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
                .Callback(new QcCallback((string user, string pass, ref List<QualityControlDomain> qcs, ref uint c) =>
                {
                    qcs = qualDomainList;
                    c = numberOfQuality;
                }))
                .Returns(HawkeyeError.eSuccess);

            qualTypeAccessMock.Setup(m => m.GetQualityControlListAPI(Username, Password, true, ref It.Ref<List<QualityControlDomain>>.IsAny, out It.Ref<uint>.IsAny))
                .Callback(new QcCallback((string user, string pass, ref List<QualityControlDomain> qcs, ref uint c) =>
                {
                    qcs = qualDomainList;
                    c = numberOfQuality;
                }))
                .Returns(HawkeyeError.eSuccess);

            CellTypeFacade.Instance.UnitTest_SetCellTypeAccess(cellTypeAccessMock.Object, qualTypeAccessMock.Object);
            //CellTypeFacade.Instance.PopulateCellTypesAndQualityControls(Username, Password);

            //var ct = CellTypeFacade.Instance.GetAllCellTypesCopy();
            //var ct2 = CellTypeFacade.Instance.GetFactoryCellTypes();
            //var qc = CellTypeFacade.Instance.GetAllQualityControlsCopy();

            //Assert.AreEqual(4, ct.Count);
            //Assert.AreEqual(2, ct2.Count);
            //Assert.AreEqual(2, qc.Count);
            MessageBus.Default.Publish(new Notification<SystemStatusDomain>(GetSystemStatusDomain(SystemStatus.Idle), MessageToken.NewSystemStatus));
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

        private CellTypeDomain GetCellType_GoodValues()
        {
            return new CellTypeDomain
            {
                AnalysisDomain = new AnalysisDomain
                {
                    AnalysisParameter = new List<AnalysisParameterDomain>
                    {
                        new AnalysisParameterDomain { ThresholdValue = 40 },
                        new AnalysisParameterDomain { ThresholdValue = 50 }
                    },
                    MixingCycle = 3
                },
                AspirationCycles = 1,
                CalculationAdjustmentFactor = 0.0f,
                CellSharpness = 1,
                CellTypeIndex = 1234,
                CellTypeName = "Test Cell",
                TempCellName = "Test Cell",
                DeclusterDegree = eCellDeclusterSetting.eDCMedium,
                Images = 100,
                IsQualityControlCellType = false,
                IsCellEnable = true,
                IsUserDefineCellType = false,
                IsCellSelected = false,
                MinimumCircularity = 0.5,
                MinimumDiameter = 10.0,
                MaximumDiameter = 12.0
            };
        }

        private SystemStatusDomain GetSystemStatusDomain(SystemStatus status)
        {
            var systemStatus = new SystemStatusDomain();
            systemStatus.SystemStatus = status;
            return systemStatus;
        }

        #endregion
    }

}
