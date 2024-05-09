using AutoMapper;
using Grpc.Core.Logging;
using GrpcClient;
using GrpcClient.Services;
using GrpcServer;
using GrpcService;
using HawkeyeCoreAPI.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using Ninject;
using Ninject.Parameters;
using NUnit.Framework;
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
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ninject.Extensions.Factory;
using CellType = GrpcService.CellType;
using SubstrateType = ScoutUtilities.Enums.SubstrateType;

namespace ScoutServicesTests
{

    [TestFixture]
    public class SampleProcessingServiceTests
    {
        public const string _username = "username";
        public const string _password = "password";

        private OpcUaGrpcServer _opcUaGrpcServer;
        private OpcUaGrpcClient _grpcClient;
        private IKernel _kernel;
        private IMapper _mapper;

        private readonly Mock<ISystemStatus> _sysStatusMock = new Mock<ISystemStatus>();
        private readonly Mock<ISecurityService> _mockSecurityService = new Mock<ISecurityService>();
        private readonly Mock<ILockManager> _mockLockManager = new Mock<ILockManager>();
        private readonly Mock<IAutomationSettingsService> _autoMock = new Mock<IAutomationSettingsService>();
        private InstrumentStatusService _instrumentStatusService;

        [SetUp]
        public void Setup()
        {
			_kernel = new StandardKernel(new OpcUaGrpcModule());

            _kernel.Bind<ILogger>().To<OpcGrpcLogger>().InSingletonScope();
            _kernel.Bind<IOpcUaCfgManager>().To<OpcUaCfgManager>().InSingletonScope();
            _kernel.Bind<IDbSettingsService>().To<DbSettingsModel>().InSingletonScope();
            _kernel.Bind<ISmtpSettingsService>().To<SmtpSettingsModel>().InSingletonScope();
            _kernel.Bind<ISampleResultsManager>().To<SampleResultsManager>().InSingletonScope();
            _kernel.Bind<ISampleProcessingService>().To<SampleProcessingService>().InSingletonScope();
            _kernel.Bind<ICellTypeManager>().To<CellTypeManager>().InSingletonScope();
            _kernel.Bind<IConfigurationManager>().To<ConfigurationManager>().InSingletonScope();

            _kernel.Bind<ISecurityService>().ToConstant(_mockSecurityService.Object);
            _kernel.Bind<ILockManager>().ToConstant(_mockLockManager.Object);
            _kernel.Bind<IAutomationSettingsService>().ToConstant(_autoMock.Object);
            var errorMock = new Mock<IErrorLog>();
            var loggerMock = new Mock<Ninject.Extensions.Logging.ILogger>();
            var applicationStateServiceMock = new Mock<IApplicationStateService>();
            _instrumentStatusService = new InstrumentStatusService(_sysStatusMock.Object, errorMock.Object, loggerMock.Object, applicationStateServiceMock.Object);
            _kernel.Bind<IWorkListModel>().To<WorkListModel>().InSingletonScope();
            _kernel.Bind<ICapacityManager>().To<CapacityManager>().InSingletonScope();
            _kernel.Bind<IDisplayService>().To<DisplayService>().InSingletonScope();
            _kernel.Bind<IRunningWorkListModel>().To<RunningWorkListModel>().InSingletonScope();
            _kernel.Bind<IScoutModelsFactory>().ToFactory();
            _kernel.Bind<IErrorLog>().To<HawkeyeCoreAPI.ErrorLog>().InSingletonScope();
            _kernel.Bind<ISystemStatus>().ToConstant(_sysStatusMock.Object);
            _kernel.Bind<IInstrumentStatusService>().ToConstant(_instrumentStatusService);
            var mockConfiguration = new Mock<IConfiguration>();
            _kernel.Bind<IConfiguration>().ToConstant(mockConfiguration.Object);

            _mockSecurityService.Reset();
            var adminUserRecord = new UserRecord
            {
                AllowFastMode = true
            };

            var fakeUserRecord = new UserRecord
            {
                AllowFastMode = true
            };

            var noFastUserRecord = new UserRecord
            {
                AllowFastMode = false
            };

            _mockSecurityService.Setup(m => m.GetUserRecord(_username)).Returns(adminUserRecord);
            _mockSecurityService.Setup(m => m.GetUserRecord("fake_user")).Returns(fakeUserRecord);
            _mockSecurityService.Setup(m => m.GetUserRecord("no_fast_user")).Returns(noFastUserRecord);

            _mapper = _kernel.Get<IMapper>();
            var auditMock = new Mock<IAuditLog>();
            _kernel.Bind<IAuditLog>().ToConstant(auditMock.Object);
        }

        [Test]
        [Ignore("Requires refactoring")]
        public void CanProcessSamples_TestRequest_InvalidDilution() // PC3549-4423
        {
            try
            {
                SetupInstrumentStatusModel();
                _mockLockManager.Setup(m => m.IsLocked()).Returns(true);
                _mockLockManager.Setup(m => m.OwnsLock(It.IsAny<string>())).Returns(true);
                _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));

                // Create and start server.
                _opcUaGrpcServer = _kernel.Get<OpcUaGrpcServer>();
                _opcUaGrpcServer.StartServer();

                // Create Grpc client.
                _grpcClient = _kernel.Get<OpcUaGrpcClient>();
                _grpcClient.Init(_username, _password);

                var request = new RequestStartSample
                {
                    SampleConfig = new SampleConfig
                    {
                        SampleName = "My Cool Sample",
                        CellType = new CellType
                        {
                            CellTypeName = "Insect"
                        },
                        Dilution = 0, // the failing condition for this test
                        SamplePosition = new SamplePosition { Row = "Y", Column = 1},
                        SaveEveryNthImage = 1,
                        Tag = "My tag for the cool sample",
                        WorkflowType = WorkflowTypeEnum.LowCellDensityWorkflowType
                    }
                };

                var result = _grpcClient.SendRequestStartSample(request);
                Assert.AreEqual(ErrorLevelEnum.Error, result.ErrorLevel);
                Assert.AreEqual(MethodResultEnum.Failure, result.MethodResult);

                // Shutdown server
                _opcUaGrpcServer.ShutdownServer();
            }
            catch (Exception e)
            {
                _opcUaGrpcServer?.ShutdownServer();
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void CanProcessSamples_TestService_Dilution() // PC3549-4423
        {
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));

            var expectedName = "Insect";
            var expectedIndex = (uint) 2;
            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            }, null);

            var sample = new SampleConfig
            {
                Dilution = 0, // invalid Dilution

                SampleName = "My Cool Sample",
                CellType = new CellType { CellTypeName = expectedName},
                SamplePosition = new SamplePosition { Row = "Y", Column = 1 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);
            var map = set.Samples.FirstOrDefault();
            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.AreEqual(SampleProcessingValidationResult.Valid, validationResult);

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, false);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidDilution));
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, true);
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidDilution));

            sample.Dilution = 10000; // another invalid dilution

            samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out validationResult, 1);
            map = set.Samples.FirstOrDefault();
            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.AreEqual(SampleProcessingValidationResult.Valid, validationResult);

            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidDilution));
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidDilution));

            sample.Dilution = 1; // valid dilution

            samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out validationResult, 1);
            map = set.Samples.FirstOrDefault();
            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.AreEqual(SampleProcessingValidationResult.Valid, validationResult);

            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsTrue(vResult.HasValidFlag());
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.HasValidFlag());

            sample.Dilution = 9999; // valid dilution

            samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out validationResult, 1);
            map = set.Samples.FirstOrDefault();
            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.AreEqual(SampleProcessingValidationResult.Valid, validationResult);

            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsTrue(vResult.HasValidFlag());
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.HasValidFlag());
        }

        [Test]
        public void CanProcessSamples_TestService_NthImage() // PC3549-4423
        {
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));

            var expectedName = "Insect";
            var expectedIndex = (uint)2;
            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            }, null);

            var sample = new SampleConfig
            {
                SaveEveryNthImage = 1, // valid nth image
                Dilution = 2,
                SampleName = "My Cool Sample",
                CellType = new CellType {CellTypeName = expectedName},
                SamplePosition = new SamplePosition {Row = "Y", Column = 1},
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain> {_mapper.Map<SampleEswDomain>(sample)};
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);
            var map = set.Samples.FirstOrDefault();
            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.AreEqual(SampleProcessingValidationResult.Valid, validationResult);

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsTrue(vResult.HasValidFlag());
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.HasValidFlag());

            sample.SaveEveryNthImage = 7; // valid nth image value

            samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out validationResult, 1);
            map = set.Samples.FirstOrDefault();
            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.AreEqual(SampleProcessingValidationResult.Valid, validationResult);

            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsTrue(vResult.HasValidFlag());
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.HasValidFlag());

            sample.SaveEveryNthImage = 100; // invalid nth image value

            samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out validationResult, 1);
            map = set.Samples.FirstOrDefault();
            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.AreEqual(SampleProcessingValidationResult.Valid, validationResult);

            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidSaveNthImage));
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidSaveNthImage));

            sample.SaveEveryNthImage = 100; // invalid nth image value

            samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out validationResult, 1);
            map = set.Samples.FirstOrDefault();
            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.AreEqual(SampleProcessingValidationResult.Valid, validationResult);

            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidSaveNthImage));
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidSaveNthImage));
        }

        [Test]
        public void CanProcessSamples_CannotProcess_CellTypeAndQcIsNull() // PC3549-4423
        {

            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));

            var service = GetSampleProcessingService(null, null);
            
            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                // Don't specify cell type or quality control
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "Y", Column = 1 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);
            var map = set.Samples.FirstOrDefault();
            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.AreEqual(SampleProcessingValidationResult.InvalidAutomationCupSample, validationResult);
            Assert.AreEqual(1, set.Samples.Count);
            Assert.IsTrue(string.IsNullOrEmpty(map.CellTypeQcName));
            Assert.IsTrue(map.SamplePosition.IsValid());
            Assert.AreEqual(0, map.CellTypeIndex); // the default
            Assert.IsFalse(map.IsQualityControl);

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidCellTypeOrQcName));
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidCellTypeOrQcName));
        }

        [Test]
        public void CanProcessSamples_CanProcess_QcMatchesCellType() // PC3549-4423
        {
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));
            var expectedName = "QC";
            var expectedIndex = (uint) 2;
            var service = GetSampleProcessingService(null, new QualityControlDomain
            {
                QcName = expectedName,
                CellTypeIndex = expectedIndex
            });

            var ct = new CellType {CellTypeName = "Insect"};
            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = ct,
                QualityControl = new QualityControl {QualityControlName = expectedName},
                Dilution = 1,
                SamplePosition = new SamplePosition {Row = "Y", Column = 1},
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);
            var map = set.Samples.FirstOrDefault();
            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.AreEqual(SampleProcessingValidationResult.Valid, validationResult);
            Assert.AreEqual(1, set.Samples.Count);
            Assert.AreEqual(expectedName, map.CellTypeQcName);
            Assert.AreEqual(expectedIndex, map.CellTypeIndex);
            Assert.IsTrue(map.IsQualityControl);
            Assert.IsTrue(map.SamplePosition.IsValid());

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsTrue(vResult.HasValidFlag());
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.HasValidFlag());
        }

        [Test]
        public void CanProcessSamples_CannotProcess_QcDoesNotMatchesCellType() // PC3549-4423
        {
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));
            var service = GetSampleProcessingService(null, null);

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                // Don't specify cell type or quality control
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "Y", Column = 1 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);
            var map = set.Samples.FirstOrDefault();
            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.IsFalse(validationResult.HasValidFlag());
            Assert.AreEqual(1, set.Samples.Count);
            Assert.IsTrue(string.IsNullOrEmpty(map.CellTypeQcName));
            Assert.AreEqual(0, map.CellTypeIndex); // the default
            Assert.IsFalse(map.IsQualityControl);
            Assert.IsTrue(map.SamplePosition.IsValid());

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidAutomationCupSample));
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidAutomationCupSample));
        }

        [Test]
        public void CanProcessSamples_CannotProcess_CellTypeNotFound() // PC3549-4423
        {
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));
            var expectedName = "ThisCellTypeDoesNotExist";
            //var expectedIndex = (uint)0;
            var service = GetSampleProcessingService(null, null);

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType {CellTypeName = expectedName},
                Dilution = 1,
                SamplePosition = new SamplePosition {Row = "Y", Column = 1},
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);
            var map = set.Samples.FirstOrDefault();
            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.IsFalse(validationResult.HasValidFlag());
            Assert.AreEqual(1, set.Samples.Count);
            Assert.AreEqual(expectedName, map.CellTypeQcName);
            Assert.AreEqual(0, map.CellTypeIndex); // the default
            Assert.IsFalse(map.IsQualityControl);
            Assert.IsTrue(map.SamplePosition.IsValid());

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidAutomationCupSample));
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidAutomationCupSample));
        }

        [Test]
        public void CanProcessSamples_CannotProcess_QualityControlNotFound() // PC3549-4423
        {
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));
            var expectedName = "ThisQCDoesNotExist";
            //var expectedIndex = (uint) 123;
            var service = GetSampleProcessingService(null, null);

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                QualityControl = new QualityControl {QualityControlName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition {Row = "Y", Column = 1},
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);
            var map = set.Samples.FirstOrDefault();
            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.IsFalse(validationResult.IsValid());
            Assert.AreEqual(1, set.Samples.Count);
            Assert.AreEqual(expectedName, map.CellTypeQcName);
            Assert.AreEqual(default(uint), map.CellTypeIndex);
            Assert.IsTrue(map.IsQualityControl);
            Assert.IsTrue(map.SamplePosition.IsValid());

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidAutomationCupSample));
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidAutomationCupSample));
        }

        [Test]
        public void CanProcessSamples_CanProcess_CellTypeNameIsValid() // PC3549-4423
        {
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));
            var expectedName = "Yeast";
            var expectedIndex = (uint) 3;

            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            }, null);

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType {CellTypeName = expectedName},
                Dilution = 1,
                SamplePosition = new SamplePosition {Row = "Y", Column = 1},
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var map = _mapper.Map<SampleEswDomain>(sample);
            Assert.IsNotNull(map);
            var samples = new List<SampleEswDomain> { map };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);
            Assert.IsNotNull(set);
            Assert.IsNotNull(set.Samples);
            Assert.IsTrue(validationResult.IsValid());
            map = set.Samples.FirstOrDefault();
            Assert.IsNotNull(map);
            Assert.AreEqual(1, set.Samples.Count);
            Assert.AreEqual(expectedName, map.CellTypeQcName);
            Assert.AreEqual(expectedIndex, map.CellTypeIndex);
            Assert.IsTrue(map.SamplePosition.IsValid());
            Assert.IsFalse(map.IsQualityControl);
            Assert.IsTrue(map.SamplePosition.IsValid());

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsTrue(vResult.IsValid());
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.IsValid());
        }

        [Test]
        public void CanProcessSamples_CanProcess_QualityControlIsValid() // PC3549-4423
        {
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));
            var expectedName = "QC";
            var expectedIndex = (uint) 123;
            var service = GetSampleProcessingService(null, new QualityControlDomain()
            {
                CellTypeIndex = expectedIndex,
                QcName = expectedName
            });

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                QualityControl = new QualityControl {QualityControlName = expectedName},
                Dilution = 1,
                SamplePosition = new SamplePosition {Row = "Y", Column = 1},
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);
            var map = set.Samples.FirstOrDefault();
            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.IsTrue(validationResult.IsValid());
            Assert.AreEqual(1, set.Samples.Count);
            Assert.AreEqual(expectedName, map.CellTypeQcName);
            Assert.AreEqual(expectedIndex, map.CellTypeIndex);
            Assert.IsTrue(map.IsQualityControl);
            Assert.IsTrue(map.SamplePosition.IsValid());

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsTrue(vResult.IsValid());
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.IsValid());
        }

        [Test]
        [Ignore("Obsoleted due to need to run alternate sample workflows for a-cup on CHM")]
        public void AcupCannotBeFastWashForSample()
        {
		//TODO: look at this for Vi-Cell variant...
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));
            var expectedName = "Yeast";
            var expectedIndex = (uint)3;
            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            }, null);

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "y", Column = 1 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.LowCellDensityWorkflowType
            };

            var sampleEswDomain = _mapper.Map<SampleEswDomain>(sample);
            var samples = new List<SampleEswDomain> {sampleEswDomain};
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);
            Assert.AreEqual(1, set.Samples.Count);
            Assert.AreEqual(SubstrateType.AutomationCup, sampleEswDomain.SubstrateType);
            Assert.IsNotNull(set);
            Assert.IsFalse(validationResult.IsValid());
            Assert.IsTrue(validationResult.HasFlag(SampleProcessingValidationResult.InvalidAutomationCupSample));
        }

        [Test]
        public void CanProcessSamples_CannotProcess_IncorrectSamplePositionGiven() // PC3549-4423
        {
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));

            var expectedName = "Yeast";
            var expectedIndex = (uint) 3;
            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            }, null);

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType { CellTypeName = expectedName},
                Dilution = 1,
                SamplePosition = new SamplePosition {Row = "", Column = 0}, // bad sample position
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.LowCellDensityWorkflowType
            };

            var samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);
            Assert.IsNotNull(set);
            Assert.IsNull(set.Samples);
            Assert.IsFalse(validationResult.IsValid());
            Assert.IsTrue(validationResult.HasFlag(SampleProcessingValidationResult.InvalidSamplePosition));

            sample.SamplePosition = new SamplePosition {Row = "Y", Column = 2};
            set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out validationResult, 1);
            Assert.IsNotNull(set);
            Assert.IsNull(set.Samples);
            Assert.AreEqual(SubstrateType.NoType, set.Carrier);
            Assert.IsFalse(validationResult.IsValid());
            Assert.IsTrue(validationResult.HasFlag(SampleProcessingValidationResult.InvalidSamplePosition));

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, false);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.CarrierNotDefined));
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidStageAndCarrier));
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.NoSamplesDefined));
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, true);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.CarrierNotDefined));
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.NoSamplesDefined));

            sample.SamplePosition = new SamplePosition {Row = "X", Column = 21};
            set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", true, out validationResult, 1);
            Assert.IsNotNull(set);
            Assert.IsNull(set.Samples);
            Assert.IsFalse(validationResult.IsValid());
            Assert.IsTrue(validationResult.HasFlag(SampleProcessingValidationResult.InvalidSamplePosition));

            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.CarrierNotDefined));
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidStageAndCarrier));
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.NoSamplesDefined));
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.CarrierNotDefined));
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.NoSamplesDefined));
        }

        [Test]
        public void CanProcessSamples_CanProcess_SamplePositionLowercaseGiven() // PC3549-4423
        {
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));
            var expectedName = "Yeast";
            var expectedIndex = (uint)3;
            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            }, null);

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "y", Column = 1 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);
            var map = set.Samples.FirstOrDefault();

            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.AreEqual(1, set.Samples.Count);
            Assert.IsTrue(validationResult.IsValid());
            Assert.IsTrue(map.SamplePosition.IsValid());
            Assert.AreEqual(sample.SamplePosition.Row.ToUpper(), map.SamplePosition.Row.ToString());
            Assert.AreEqual(sample.SamplePosition.Column, map.SamplePosition.Column);
            Assert.AreEqual(SubstrateType.AutomationCup, map.SubstrateType);

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsTrue(vResult.IsValid());
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.IsValid());
        }

        [Test]
        public void CanProcessSamples_CannotProcess_InvalidSampleName()
        {
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));
            var expectedName = "Yeast";
            var expectedIndex = (uint)3;
            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            }, null);

            var sample = new SampleConfig
            {
                SampleName = "<>?*|/",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "y", Column = 1 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);
            var map = set.Samples.FirstOrDefault();

            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.AreEqual(1, set.Samples.Count);
            Assert.IsTrue(validationResult.IsValid());
            Assert.IsTrue(map.SamplePosition.IsValid());
            Assert.AreEqual(sample.SamplePosition.Row.ToUpper(), map.SamplePosition.Row.ToString());
            Assert.AreEqual(sample.SamplePosition.Column, map.SamplePosition.Column);
            Assert.AreEqual(SubstrateType.AutomationCup, map.SubstrateType);

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsFalse(vResult.IsValid());
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidSampleOrSampleSetName));
        }

        [Test]
        public void CanProcessSamples_CannotProcess_SampleNameIsBlank()
        {
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));
            var expectedName = "Yeast";
            var expectedIndex = (uint)3;
            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            }, null);

            var sample = new SampleConfig
            {
                SampleName = "",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "y", Column = 1 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain> { _mapper.Map<SampleEswDomain>(sample) };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);
            var map = set.Samples.FirstOrDefault();

            Assert.IsNotNull(set);
            Assert.IsNotNull(map);
            Assert.AreEqual(1, set.Samples.Count);
            Assert.IsTrue(validationResult.IsValid());
            Assert.IsTrue(map.SamplePosition.IsValid());
            Assert.AreEqual(sample.SamplePosition.Row.ToUpper(), map.SamplePosition.Row.ToString());
            Assert.AreEqual(sample.SamplePosition.Column, map.SamplePosition.Column);
            Assert.AreEqual(SubstrateType.AutomationCup, map.SubstrateType);

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsFalse(vResult.IsValid());
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidSampleOrSampleSetName));
        }

        [Test]
        public void CanProcessSamples_CanProcess_SampleSet() // PC3549-4423
        {
            var expectedName = "Yeast";
            var expectedIndex = (uint)3;
            var expectedName2 = "Insect";
            var expectedIndex2 = (uint)2;
            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            },
                null,
                new CellTypeDomain
                {
                    CellTypeIndex = expectedIndex2,
                    CellTypeName = expectedName2
                });

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "h", Column = 1 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.LowCellDensityWorkflowType
            };
            var sample2 = new SampleConfig
            {
                SampleName = "My Cool Sample 2",
                CellType = new CellType { CellTypeName = expectedName2 },
                Dilution = 10,
                SamplePosition = new SamplePosition { Row = "b", Column = 4 },
                SaveEveryNthImage = 5,
                Tag = "My tag for the cool sample 2",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain>
            {
                _mapper.Map<SampleEswDomain>(sample),
                _mapper.Map<SampleEswDomain>(sample2),
            };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", true, out var validationResult, 1);

            Assert.IsNotNull(set);
            Assert.AreEqual(2, set.Samples.Count);
            Assert.IsTrue(validationResult.IsValid());

            Assert.IsNotNull(set.Samples[0]);
            Assert.IsTrue(set.Samples[0].SamplePosition.IsValid());
            Assert.AreEqual(sample.SamplePosition.Row.ToUpper(), set.Samples[0].SamplePosition.Row.ToString());
            Assert.AreEqual(sample.SamplePosition.Column, set.Samples[0].SamplePosition.Column);
            Assert.AreEqual(SubstrateType.Plate96, set.Samples[0].SubstrateType);
            Assert.AreEqual(expectedName, set.Samples[0].CellTypeQcName);
            Assert.AreEqual(expectedIndex, set.Samples[0].CellTypeIndex);

            Assert.IsNotNull(set.Samples[1]);
            Assert.IsTrue(set.Samples[1].SamplePosition.IsValid());
            Assert.AreEqual(sample2.SamplePosition.Row.ToUpper(), set.Samples[1].SamplePosition.Row.ToString());
            Assert.AreEqual(sample2.SamplePosition.Column, set.Samples[1].SamplePosition.Column);
            Assert.AreEqual(SubstrateType.Plate96, set.Samples[1].SubstrateType);
            Assert.AreEqual(expectedName2, set.Samples[1].CellTypeQcName);
            Assert.AreEqual(expectedIndex2, set.Samples[1].CellTypeIndex);

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, false);
            Assert.IsTrue(vResult.IsValid());
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, true);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.CarouselInstalledAndSetIs96WellPlate));
        }

        [Test]
        public void CanProcessSamples_CannotProcess_SampleSetNameInvalid()
        {
            var expectedName = "Yeast";
            var expectedIndex = (uint)3;
            var expectedName2 = "Insect";
            var expectedIndex2 = (uint)2;
            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            },
                null,
                new CellTypeDomain
                {
                    CellTypeIndex = expectedIndex2,
                    CellTypeName = expectedName2
                });

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "h", Column = 1 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.LowCellDensityWorkflowType
            };
            var sample2 = new SampleConfig
            {
                SampleName = "My Cool Sample 2",
                CellType = new CellType { CellTypeName = expectedName2 },
                Dilution = 10,
                SamplePosition = new SamplePosition { Row = "b", Column = 4 },
                SaveEveryNthImage = 5,
                Tag = "My tag for the cool sample 2",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain>
            {
                _mapper.Map<SampleEswDomain>(sample),
                _mapper.Map<SampleEswDomain>(sample2),
            };

            var badSetName = "?/*|<>:";
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", badSetName, true, out var validationResult, 1);
            
            Assert.IsNotNull(set);
            Assert.AreEqual(2, set.Samples.Count);
           
            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsFalse(vResult.IsValid());
            
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidSampleOrSampleSetName));
        }

        [Test]
        public void CanProcessSamples_CannotProcess_SampleSetNameIsBlank()
        {
            var expectedName = "Yeast";
            var expectedIndex = (uint)3;
            var expectedName2 = "Insect";
            var expectedIndex2 = (uint)2;
            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            },
                null,
                new CellTypeDomain
                {
                    CellTypeIndex = expectedIndex2,
                    CellTypeName = expectedName2
                });

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "h", Column = 1 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.LowCellDensityWorkflowType
            };
            var sample2 = new SampleConfig
            {
                SampleName = "My Cool Sample 2",
                CellType = new CellType { CellTypeName = expectedName2 },
                Dilution = 10,
                SamplePosition = new SamplePosition { Row = "b", Column = 4 },
                SaveEveryNthImage = 5,
                Tag = "My tag for the cool sample 2",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain>
            {
                _mapper.Map<SampleEswDomain>(sample),
                _mapper.Map<SampleEswDomain>(sample2),
            };

            var badSetName = "";
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", badSetName, true, out var validationResult, 1);

            Assert.IsNotNull(set);
            Assert.AreEqual(2, set.Samples.Count);

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.InvalidSampleOrSampleSetName));
        }

        [Test]
        public async Task CanProcessSamples_CannotProcess_StartMultiSample_WorkListNotIdle() // PC3549-4423
        {
            var expectedName = "Yeast";
            var expectedIndex = (uint)3;
            var expectedName2 = "Insect";
            var expectedIndex2 = (uint)2;

            // make a mock that can change the protected WorkListStatus property
            var runWorkListModelMock = new Mock<IRunningWorkListModel>();
            runWorkListModelMock.Setup(m => m.PauseProcessing(_username, _password)).Returns(true);

            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            },
                null,
                new CellTypeDomain
                {
                    CellTypeIndex = expectedIndex2,
                    CellTypeName = expectedName2
                },
                runWorkListModelMock);

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "h", Column = 1 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.LowCellDensityWorkflowType
            };
            var sample2 = new SampleConfig
            {
                SampleName = "My Cool Sample 2",
                CellType = new CellType { CellTypeName = expectedName2 },
                Dilution = 10,
                SamplePosition = new SamplePosition { Row = "b", Column = 4 },
                SaveEveryNthImage = 5,
                Tag = "My tag for the cool sample 2",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain>
            {
                _mapper.Map<SampleEswDomain>(sample),
                _mapper.Map<SampleEswDomain>(sample2),
            };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", true, out var validationResult, 1);
            Assert.IsTrue(validationResult.IsValid());

            // Act: change the WorkListStatus to something other than IDLE:
            var pauseResult = await Task.Run(() => service.PauseProcessing(_username, _password));

            // Assert
            Assert.IsTrue(pauseResult);
            SimulateBackendChangingSystemStatus(SystemStatus.Paused);
            Assert.AreEqual(WorkListStatus.Paused, service.GetWorkListStatus());
            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, false);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.WorkListStatusNotIdle));
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, true);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.WorkListStatusNotIdle));
        }

        [Test]
        public async Task CanProcessSamples_CannotProcess_StartSingleSample_WorkListNotIdle() // PC3549-4423
        {
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));

            var expectedName = "Yeast";
            var expectedIndex = (uint)3;

            // make a mock that can change the protected WorkListStatus property
            var runWorkListModelMock = new Mock<IRunningWorkListModel>();
            runWorkListModelMock.Setup(m => m.PauseProcessing(_username, _password)).Returns(true);

            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            },
                null,
                null,
                runWorkListModelMock);

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "y", Column = 1 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain>
            {
                _mapper.Map<SampleEswDomain>(sample)
            };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);
            Assert.IsTrue(validationResult.IsValid());

            // Act: change the WorkListStatus to something other than IDLE
            //var startResult = await Task.Run(() => _sampleProcessingService.ProcessSamples(sampleSets, username, template, runOptionsDataAccess));
            var pauseResult = await Task.Run(() => service.PauseProcessing(_username, _password));

            // Assert
            Assert.IsTrue(pauseResult);
            SimulateBackendChangingSystemStatus(SystemStatus.Paused);
            Assert.AreEqual(WorkListStatus.Paused, service.GetWorkListStatus());
            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, false);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.WorkListStatusNotIdle));
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, true);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.WorkListStatusNotIdle));
        }

        [Test]
        public void CanProcessSamples_CanProcess_StartSingleSampleUsing96WellPlate() // PC3549-4423
        {
            var expectedName = "Yeast";
            var expectedIndex = (uint)3;
            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            },
                null);

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "c", Column = 8 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.LowCellDensityWorkflowType
            };

            var samples = new List<SampleEswDomain>
            {
                _mapper.Map<SampleEswDomain>(sample),
            };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", true, out var validationResult, 1);

            Assert.IsNotNull(set);
            Assert.AreEqual(1, set.Samples.Count);

            Assert.IsNotNull(set.Samples[0]);
            Assert.IsTrue(set.Samples[0].SamplePosition.IsValid());
            Assert.AreEqual(sample.SamplePosition.Row.ToUpper(), set.Samples[0].SamplePosition.Row.ToString());
            Assert.AreEqual(sample.SamplePosition.Column, set.Samples[0].SamplePosition.Column);
            Assert.AreEqual(SubstrateType.Plate96, set.Carrier);
            Assert.AreEqual(SubstrateType.Plate96, set.Samples[0].SubstrateType);
            Assert.AreEqual(expectedName, set.Samples[0].CellTypeQcName);
            Assert.AreEqual(expectedIndex, set.Samples[0].CellTypeIndex);

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, false);
            Assert.IsTrue(vResult.IsValid());
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, true);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.CarouselInstalledAndSetIs96WellPlate));
        }

        [Test]
        public void CanProcessSamples_CannotProcess_StartMultiSampleUsingMixedCarrierPositions() // PC3549-4423
        {
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));

            var expectedName = "Yeast";
            var expectedIndex = (uint)3;
            var expectedName2 = "Insect";
            var expectedIndex2 = (uint)2;
            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            },
                null,
                new CellTypeDomain
                {
                    CellTypeIndex = expectedIndex2,
                    CellTypeName = expectedName2
                });

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "h", Column = 1 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.LowCellDensityWorkflowType
            };
            var sample2 = new SampleConfig
            {
                SampleName = "My Cool Sample 2",
                CellType = new CellType { CellTypeName = expectedName2 },
                Dilution = 10,
                SamplePosition = new SamplePosition { Row = "y", Column = 1 },
                SaveEveryNthImage = 5,
                Tag = "My tag for the cool sample 2",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain>
            {
                _mapper.Map<SampleEswDomain>(sample),
                _mapper.Map<SampleEswDomain>(sample2),
            };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", true, out var validationResult, 1);

            Assert.IsNotNull(set);
            Assert.IsNull(set.Samples);
            Assert.IsFalse(validationResult.IsValid());
            Assert.IsTrue(validationResult.HasFlag(SampleProcessingValidationResult.InvalidSamplePosition));

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, false);
            Assert.IsFalse(vResult.IsValid());
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, true);
            Assert.IsFalse(vResult.IsValid());
        }

        [Test]
        public void CanProcessSamples_CannotProcess_StartMultiSampleUsingAutomationCup() // PC3549-4423
        {
            var expectedName = "Yeast";
            var expectedIndex = (uint)3;
            var expectedName2 = "Insect";
            var expectedIndex2 = (uint)2;
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, true, 1));

            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            },
                null,
                new CellTypeDomain
                {
                    CellTypeIndex = expectedIndex2,
                    CellTypeName = expectedName2
                });

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "y", Column = 1 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.LowCellDensityWorkflowType
            };
            var sample2 = new SampleConfig
            {
                SampleName = "My Cool Sample 2",
                CellType = new CellType { CellTypeName = expectedName2 },
                Dilution = 10,
                SamplePosition = new SamplePosition { Row = "y", Column = 1 },
                SaveEveryNthImage = 5,
                Tag = "My tag for the cool sample 2",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain>
            {
                _mapper.Map<SampleEswDomain>(sample),
                _mapper.Map<SampleEswDomain>(sample2),
            };

            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", true, out var validationResult, 1);

            Assert.IsNotNull(set);
            Assert.IsNull(set.Samples);
            Assert.IsFalse(validationResult.IsValid());
            Assert.IsTrue(validationResult.HasFlag(SampleProcessingValidationResult.InvalidSamplePosition));

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, false);
            Assert.IsFalse(vResult.IsValid());
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, true);
            Assert.IsFalse(vResult.IsValid());
        }

        //This sample is invalid because usingStartMultipleSamplesMethod = false
        [Test]
        public void CanProcessSamples_CannotProcess_StartSingleSampleUsing96WellPlate() // PC3549-4423
        {
            var expectedName = "Yeast";
            var expectedIndex = (uint)3;
            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            }, null);

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "d", Column = 5 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.LowCellDensityWorkflowType
            };
            var samples = new List<SampleEswDomain>
            {
                _mapper.Map<SampleEswDomain>(sample)
            };

            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);

            Assert.IsNotNull(set);
            Assert.IsNull(set.Samples);
            Assert.IsFalse(validationResult.IsValid());
            Assert.IsTrue(validationResult.HasFlag(SampleProcessingValidationResult.InvalidSamplePosition));

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, false);
            Assert.IsFalse(vResult.IsValid());
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> {set}, true);
            Assert.IsFalse(vResult.IsValid());
        }

        [Test] // PC3549-4912
        public void CanProcessSamples_CannotProcess_CarouselInstalledAndStarting96WellPlateSet()
        {
            var expectedName = "Yeast";
            var expectedIndex = (uint)3;
            var expectedName2 = "Insect";
            var expectedIndex2 = (uint)2;
            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            },
                null,
                new CellTypeDomain
                {
                    CellTypeIndex = expectedIndex2,
                    CellTypeName = expectedName2
                });

            var sample1 = new SampleConfig
            {
                SampleName = "My Cool Sample 1",
                CellType = new CellType { CellTypeName = expectedName2 },
                Dilution = 10,
                SamplePosition = new SamplePosition { Row = "a", Column = 2 },
                SaveEveryNthImage = 5,
                Tag = "My tag for the cool sample 1",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };
            var sample2 = new SampleConfig
            {
                SampleName = "My Cool Sample 2",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "c", Column = 9 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample 2",
                WorkflowType = WorkflowTypeEnum.LowCellDensityWorkflowType
            };

            var samples = new List<SampleEswDomain>
            {
                _mapper.Map<SampleEswDomain>(sample1),
                _mapper.Map<SampleEswDomain>(sample2),
            };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", true, out var validationResult, 1);

            Assert.IsNotNull(set);
            Assert.IsNotNull(set.Samples);
            Assert.AreEqual(2, set.Samples.Count);
            Assert.IsTrue(validationResult.IsValid());

            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.CarouselInstalledAndSetIs96WellPlate));

            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsTrue(vResult.IsValid());
        }

        [Test] // PC3549-5631
        public void CanProcessSamples_CannotProcess_UserNoFastModePermission()
        {
            var expectedName = "Yeast";
            var expectedIndex = (uint)3;
            var service = GetSampleProcessingService(new CellTypeDomain
                {
                    CellTypeIndex = expectedIndex,
                    CellTypeName = expectedName
                },
                null);

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "c", Column = 8 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.LowCellDensityWorkflowType
            };

            var samples = new List<SampleEswDomain>
            {
                _mapper.Map<SampleEswDomain>(sample),
            };
            var set = service.CreateSampleSetFromAutomation(samples, "no_fast_user", "setname", true, out var validationResult, 1);

            Assert.IsNotNull(set);
            Assert.AreEqual(1, set.Samples.Count);

            var vResult = service.CanProcessSamples("no_fast_user", new List<SampleSetDomain> { set }, false);

            Assert.IsFalse(vResult.IsValid());
            Assert.AreEqual(SampleProcessingValidationResult.InvalidPermissions, vResult, "Was supposed to reject sample due to not having permission to run FastMode.");
        }

        [Test] // PC3549-4537
        public void CanProcessSamples_CannotProcess_MultipleSamplesWithSame96WellPosition()
        {
            var expectedName = "Yeast";
            var expectedIndex = (uint)3;
            var expectedName2 = "Insect";
            var expectedIndex2 = (uint)2;
            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            },
                null,
                new CellTypeDomain
                {
                    CellTypeIndex = expectedIndex2,
                    CellTypeName = expectedName2
                });

            var sample1 = new SampleConfig
            {
                SampleName = "My Cool Sample 2",
                CellType = new CellType { CellTypeName = expectedName2 },
                Dilution = 10,
                SamplePosition = new SamplePosition { Row = "a", Column = 2 },
                SaveEveryNthImage = 5,
                Tag = "My tag for the cool sample 2",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };
            var sample2 = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "c", Column = 9 }, // use the same position as below
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.LowCellDensityWorkflowType
            };
            var sample3 = new SampleConfig
            {
                SampleName = "My Cool Sample 2",
                CellType = new CellType { CellTypeName = expectedName2 },
                Dilution = 10,
                SamplePosition = new SamplePosition { Row = "d", Column = 9 },
                SaveEveryNthImage = 5,
                Tag = "My tag for the cool sample 2",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };
            var sample4 = new SampleConfig
            {
                SampleName = "My Cool Sample 2",
                CellType = new CellType { CellTypeName = expectedName2 },
                Dilution = 10,
                SamplePosition = new SamplePosition { Row = "C", Column = 9 }, // use the same position as above
                SaveEveryNthImage = 5,
                Tag = "My tag for the cool sample 2",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };
            var sample5 = new SampleConfig
            {
                SampleName = "My Cool Sample 2",
                CellType = new CellType { CellTypeName = expectedName2 },
                Dilution = 10,
                SamplePosition = new SamplePosition { Row = "e", Column = 9 },
                SaveEveryNthImage = 5,
                Tag = "My tag for the cool sample 2",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };

            var samples = new List<SampleEswDomain>
            {
                _mapper.Map<SampleEswDomain>(sample1),
                _mapper.Map<SampleEswDomain>(sample2),
                _mapper.Map<SampleEswDomain>(sample3),
                _mapper.Map<SampleEswDomain>(sample4),
                _mapper.Map<SampleEswDomain>(sample5),
            };
            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", true, out var validationResult, 1);

            Assert.IsNotNull(set);
            Assert.IsNotNull(set.Samples);
            Assert.AreEqual(5, set.Samples.Count);
            Assert.IsTrue(validationResult.IsValid());

            var vResult = service.CanProcessSamples(_username,new List<SampleSetDomain> { set }, true);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.CarouselInstalledAndSetIs96WellPlate));

            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.MultipleSamplesSharePosition));
        }

        [Test]
        public void CanProcessSamples_CannotProcess_ACupDisabled() 
        {
            _autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, false, 1));

            var expectedName = "Yeast";
            var expectedIndex = (uint)3;
            var service = GetSampleProcessingService(new CellTypeDomain
            {
                CellTypeIndex = expectedIndex,
                CellTypeName = expectedName
            }, null);

            var sample = new SampleConfig
            {
                SampleName = "My Cool Sample",
                CellType = new CellType { CellTypeName = expectedName },
                Dilution = 1,
                SamplePosition = new SamplePosition { Row = "Y", Column = 1 },
                SaveEveryNthImage = 1,
                Tag = "My tag for the cool sample",
                WorkflowType = WorkflowTypeEnum.NormalWorkflowType
            };
            var samples = new List<SampleEswDomain>
            {
                _mapper.Map<SampleEswDomain>(sample)
            };

            var set = service.CreateSampleSetFromAutomation(samples, "fake_user", "setname", false, out var validationResult, 1);
            Assert.IsNotNull(set);
           
            var vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, false);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.ACupNotInstalled));
            vResult = service.CanProcessSamples(_username, new List<SampleSetDomain> { set }, true);
            Assert.IsFalse(vResult.IsValid());
            Assert.IsTrue(vResult.HasFlag(SampleProcessingValidationResult.ACupNotInstalled));

        }

        #region Helper Methods

        private ISampleProcessingService GetSampleProcessingService(CellTypeDomain cellTypeDomain, 
            QualityControlDomain qcDomain, CellTypeDomain cellType2 = null, 
            Mock<IRunningWorkListModel> runningWorkListModelMock = null)
        {
            var cellTypeManager = new Mock<ICellTypeManager>();
            if (cellType2 == null)
            {
                cellTypeManager.Setup(m => m.GetCellTypeDomain(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                               .Returns(cellTypeDomain);
            }
            else
            {
                cellTypeManager.Setup(m => m.GetCellTypeDomain(It.IsAny<string>(), It.IsAny<string>(), cellTypeDomain.CellTypeName))
                               .Returns(cellTypeDomain);
                cellTypeManager.Setup(m => m.GetCellTypeDomain(It.IsAny<string>(), It.IsAny<string>(), cellType2.CellTypeName))
                               .Returns(cellType2);
            }
            
            cellTypeManager.Setup(m => m.GetQualityControlDomain(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                           .Returns(qcDomain);

            var workListModel = new Mock<IWorkListModel>();
            workListModel.Setup(m => m.CheckReagentsAndWasteTray(It.IsAny<int>(),
                             It.IsAny<SubstrateType>(), out It.Ref<bool>.IsAny,
                             out It.Ref<bool>.IsAny, It.IsAny<bool>()))
                         .Returns(false);

            if (runningWorkListModelMock != null)
            {
                var service = _kernel.Get<ISampleProcessingService>(
                    new ConstructorArgument("runningWorkListModel", runningWorkListModelMock.Object),
                    new ConstructorArgument("cellTypeManager", cellTypeManager.Object),
                    new ConstructorArgument("workListModel", workListModel.Object));
                return service;
            }
            else
            {
                var service = _kernel.Get<ISampleProcessingService>(
                    new ConstructorArgument("cellTypeManager", cellTypeManager.Object),
                    new ConstructorArgument("workListModel", workListModel.Object));
                return service;
            }
        }

        delegate void SystemStatusCallback(ref SystemStatusData systemStatus);

        private void SetupInstrumentStatusModel()
        {
            // Setup the SystemStatusService with the mocks it needs
            var systemStatusData = new SystemStatusData
            {
                remainingReagentPackUses = 123,
                sample_tube_disposal_remaining_capacity = 69,
                sensor_reagent_pack_in_place = eSensorStatus.ssStateActive,
                sensor_reagent_pack_door_closed = eSensorStatus.ssStateActive,
                sensor_carousel_detect = eSensorStatus.ssStateActive,
                sampleStageLocation = new ScoutUtilities.Common.SamplePosition('Y', 1)
            };

            _sysStatusMock.Reset();

            _sysStatusMock.Setup(m => m.GetSystemStatusAPI(ref It.Ref<SystemStatusData>.IsAny))
                               .Callback(new SystemStatusCallback((ref SystemStatusData systemStatus) => { systemStatus = systemStatusData; }))
                               .Returns(IntPtr.Zero);
            _sysStatusMock.Setup(m => m.FreeSystemStatusAPI(It.IsAny<IntPtr>()));
            _instrumentStatusService.GetAndPublishSystemStatus();
        }

        private void SimulateBackendChangingSystemStatus(SystemStatus systemStatus)
        {
            var systemStatusDomain = _instrumentStatusService.SystemStatusDom ?? new SystemStatusDomain();
            systemStatusDomain.SystemStatus = systemStatus;
            _instrumentStatusService.PublishSystemStatusCallback(systemStatusDomain);
        }

        #endregion
    }
}
