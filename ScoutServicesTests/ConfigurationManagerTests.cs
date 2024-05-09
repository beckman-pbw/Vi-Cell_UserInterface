using Grpc.Core.Logging;
using GrpcClient;
using GrpcClient.Services;
using GrpcServer;
using GrpcService;
using Moq;
using Ninject;
using NUnit.Framework;
using ScoutModels;
using ScoutModels.Interfaces;
using ScoutServices.DTOs;
using ScoutServices.Interfaces;
using ScoutServices.Ninject;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ScoutModels.ExpandedSampleWorkflow;
using ScoutModels.Ninject;
using Microsoft.Extensions.Configuration;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutModels.Settings;
using ScoutUtilities;
using QualityControl = HawkeyeCoreAPI.QualityControl;

namespace ScoutServicesTests
{
    [TestFixture]
    public class ConfigurationManagerTests
    {
        public const string _username = "username";
        public const string _password = "password";

        private ILogger _grpcLogger;
        private IConfigurationManager _configurationServiceManager;

        private OpcUaGrpcServer _opcUaGrpcServer;
        private OpcUaGrpcClient _grpcClient;
        private IKernel _kernel;

        private readonly Mock<ISecurityService> _mockSecurityService = new Mock<ISecurityService>();

        private string _exportConfigFullFilePath;
        private string _importConfigFullFilePath;

        [SetUp]
        [Ignore("Ignoring due to backend hanging on initialization on jenkins")]
        public void Setup()
        {
            var mockConfiguration = new Mock<IConfiguration>();

            _kernel = new StandardKernel(new ScoutServiceModule(), new ScoutModelsModule(_mockSecurityService.Object), new OpcUaGrpcModule());
            _kernel.Bind<ILogger>().To<OpcGrpcLogger>().InSingletonScope();
            _kernel.Bind<IConfiguration>().ToConstant(mockConfiguration.Object);
            _kernel.Bind<IInstrumentStatusService>().To<InstrumentStatusService>().InSingletonScope();
            _kernel.Bind<HawkeyeCoreAPI.Interfaces.ISystemStatus>().To<HawkeyeCoreAPI.SystemStatus>().InSingletonScope();
            _kernel.Bind<HawkeyeCoreAPI.Interfaces.IErrorLog>().To<HawkeyeCoreAPI.ErrorLog>().InSingletonScope();

            _mockSecurityService.Reset();

            if (HawkeyeCoreAPI.InitializeShutdown.IsInitializationCompleteAPI() !=
                InitializationState.eInitializationComplete)
            {
                HawkeyeCoreAPI.InitializeShutdown.InitializeAPI(out ushort instrumentType, false);
                Thread.Sleep(10000);
            }

            Assert.True(HawkeyeCoreAPI.InitializeShutdown.IsInitializationCompleteAPI() ==
                        InitializationState.eInitializationComplete);
        }

        #region Import/Export Config

        [Test]
        [Ignore("Ignoring due to backend hanging on initialization on jenkins")]
        public void TestExportImportConfiguration()
        {
            try
            {
                _grpcLogger = _kernel.Get<ILogger>();
                // Create the sample results manager
                _configurationServiceManager = _kernel.Get<IConfigurationManager>();

                // Create and start server.
                _opcUaGrpcServer = _kernel.Get<OpcUaGrpcServer>();
                _opcUaGrpcServer.StartServer();

                // Create Grpc client.
                _grpcClient = _kernel.Get<OpcUaGrpcClient>();
                _grpcClient.Init(_username, _password);

                #region Subscribe

                // Acquire the IObservable<SampleResult[]>
                var observeConfigChanges = _configurationServiceManager.SubscribeStateChanges();
                Assert.IsNotNull(observeConfigChanges);
                // Subscribe to config changes...
                observeConfigChanges.Subscribe(ConfigCallback);

                #endregion

                #region ExportConfigFile

                var dir = "\\Instrument\\Export\\unit_tests";
                var file = "example.cfg";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                Assert.AreEqual(Directory.Exists(dir), true);

                _exportConfigFullFilePath = Path.Combine(dir, file);

                // Formulate request
                var requestExportConfig = new RequestExportConfig();

                // Invoke SendRequestExportConfig
                var exportResponse = _grpcClient.SendRequestExportConfig(requestExportConfig);

                // Verify Success
                Assert.IsNotNull(exportResponse);
                Assert.AreEqual(exportResponse.MethodResult, MethodResultEnum.Success);

                #endregion

                #region ImportConfigFile

                var dir1 = "\\Instrument\\Export\\unit_tests";
                var file1 = "example.cfg";

                if (!Directory.Exists(dir1))
                    Directory.CreateDirectory(dir1);

                Assert.AreEqual(Directory.Exists(dir1), true);

                _importConfigFullFilePath = Path.Combine(dir1, file1);
                byte[] data = new byte[256];

                // Formulate request
                var requestImportConfig = new RequestImportConfig
                {
                    FileData = Google.Protobuf.ByteString.CopyFrom(data)
                };

                // Invoke SendRequestImportConfig
                var importResponse = _grpcClient.SendRequestImportConfig(requestImportConfig);

                // Verify Success
                Assert.IsNotNull(importResponse);
                Assert.AreEqual(importResponse.MethodResult, MethodResultEnum.Success);

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

        private void ConfigCallback(ConfigSubjectDto config)
        {
            _grpcLogger.Info(config?.Result == HawkeyeError.eSuccess
                ? $"ConfigCallback :: {config.State} Success."
                : $"ConfigCallback :: Failure.");
        }

        #endregion

        #region GetAvailableDiskSpace

        [Test]
        [Ignore("Ignoring due to backend hanging on initialization on jenkins")]
        public void TestGetAvailableDiskSpace()
        {
            try
            {
                _grpcLogger = _kernel.Get<ILogger>();
                _configurationServiceManager = _kernel.Get<IConfigurationManager>();

                // Create and start server.
                _opcUaGrpcServer = _kernel.Get<OpcUaGrpcServer>();
                _opcUaGrpcServer.StartServer();

                // Create Grpc client.
                _grpcClient = _kernel.Get<OpcUaGrpcClient>();
                _grpcClient.Init(_username, _password);

                #region Subscribe

                // Acquire the IObservable<SampleResult[]>
                var observeAvailableDiskSpace = _configurationServiceManager.SubscribeAvailableDiskSpace();
                Assert.IsNotNull(observeAvailableDiskSpace);
                // Subscribe to quality control retrieval...
                observeAvailableDiskSpace.Subscribe(AvailableDiskSpaceCallback);

                #endregion

                #region GetAvailableDiskSpace

                // Login
                var res = HawkeyeCoreAPI.User.LoginConsoleUserAPI(_username, _password);

                // Send Request
                var request = new RequestGetAvailableDiskSpace();
                var response = _grpcClient.SendRequestGetAvailableDiskSpace(request);

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

        private void AvailableDiskSpaceCallback(ulong availableDiskSpaceBytes)
        {
            _grpcLogger.Info($"Available Disk Space : {Misc.ConvertBytesToSize(availableDiskSpaceBytes)} or {availableDiskSpaceBytes} bytes");
        }

        #endregion
    }
}
