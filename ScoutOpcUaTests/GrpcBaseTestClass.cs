using GrpcClient;
using GrpcClient.Interfaces;
using GrpcClient.Services;
using GrpcServer;
using GrpcServer.EventProcessors;
using GrpcServer.GrpcInterceptor;
using GrpcService;
using Microsoft.Extensions.Configuration;
using Moq;
using Ninject;
using Ninject.Extensions.Factory;
using Ninject.Extensions.Logging;
using Ninject.Extensions.Logging.Log4net;
using NUnit.Framework;
using ScoutServices;
using ScoutServices.Interfaces;
using System;
using System.Collections.Generic;
using HawkeyeCoreAPI;
using HawkeyeCoreAPI.Interfaces;
using ScoutModels;
using ScoutModels.Interfaces;
using ScoutModels.Settings;

// ReSharper disable LocalizableElement

namespace ScoutOpcUaTests
{
    public class GrpcBaseTestClass
    {
        protected IKernel Kernel;
        protected ILogger Logger;

        // gRPC server (pretend Scout)
        private EmbeddedGrpcServer _grpcServer;
        protected readonly Mock<ISecurityService> MockSecurityService = new Mock<ISecurityService>();
        protected readonly Mock<ISampleProcessingService> MockSampleProcessingService = new Mock<ISampleProcessingService>();
        protected readonly Mock<ISampleResultsManager> MockSampleResultsManager = new Mock<ISampleResultsManager>();
        protected Dictionary<Type, object> EventProcessors;
        private object _eventProcessorLock = new object();

        protected IGrpcClient GrpcClient { get; set; }

        /// <summary>
        /// Run the GrpcServer inside this test instance, not as a separate process.
        /// </summary>
        [OneTimeSetUp]
        public virtual void ClassInit()
        {
            EventProcessors = new Dictionary<Type, object>();
            ConfigNinject();

            _grpcServer = Kernel.Get<EmbeddedGrpcServer>();
            _grpcServer.Start();
        }

        protected virtual void ConfigNinject()
        {
            var settings = new NinjectSettings()
            {
                LoadExtensions = false
            };

            Kernel = new StandardKernel(settings, new Log4NetModule(), new FuncModule(), new OpcUaGrpcModule(true));

            // Setup IConfiguration implementation
            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder.Build();

            Kernel.Bind<IConfiguration>().ToConstant(config);

            // Bindings for Embedded gRPC server pretending to be Scout.
            Kernel.Bind<GrpcBaseTestClass>().ToConstant(this);
            Kernel.Bind<GrpcServices.GrpcServicesBase, EmbeddedGrpcServer>().To<EmbeddedGrpcServer>().InSingletonScope();
            Kernel.Bind<OpcUaGrpcServer>().ToSelf().InSingletonScope();
            Kernel.Bind<ScoutInterceptor>().ToSelf().InSingletonScope();

            Kernel.Bind<ISecurityService>().ToConstant(MockSecurityService.Object);
            Kernel.Bind<ILockManager>().To<LockManager>().InSingletonScope();
            Kernel.Bind<IInstrumentStatusService>().To<InstrumentStatusService>().InSingletonScope();
            Kernel.Bind<ISystemStatus>().To<SystemStatus>().InSingletonScope();
            Kernel.Bind<IErrorLog>().To<ErrorLog>().InSingletonScope();
            Kernel.Bind<IHardwareSettingsModel>().To<HardwareSettingsModel>().InSingletonScope();
            Kernel.Bind<LockResultProcessor>().ToMethod(m => CreateEventProcessor<TestLockResultProcessor>()).InTransientScope();
            Kernel.Bind<ISampleProcessingService>().ToConstant(MockSampleProcessingService.Object);
            Kernel.Bind<ISampleResultsManager>().ToConstant(MockSampleResultsManager.Object);
            var applicationStateServiceMock = new Mock<IApplicationStateService>();
            Kernel.Bind<IApplicationStateService>().ToConstant(applicationStateServiceMock.Object);

            // Kernel.Bind<SampleProcessingSampleStatusEventProcessor>().ToSelf().InTransientScope();
            // Kernel.Bind<SampleProcessingSampleCompleteEventProcessor>().ToSelf().InTransientScope();
            // Kernel.Bind<SampleProcessingWorkListCompleteEventProcessor>().ToSelf().InTransientScope();
            Kernel.Bind<IOpcUaGrpcFactory>().ToFactory();

            // For the gRPC client
            Kernel.Bind<Grpc.Core.Logging.ILogger, OpcGrpcLogger>().To<OpcGrpcLogger>().InTransientScope();
            var clientInterceptor = new TestClientInterceptor();
            Kernel.Bind<IGrpcClient>().To<OpcUaGrpcClient>().InTransientScope()
                .WithConstructorArgument("clientInterceptor", clientInterceptor);

            Kernel.Bind<IMaintenanceService>().To<MaintenanceService>().InSingletonScope();

            Logger = Kernel.Get<ILoggerFactory>().GetCurrentClassLogger();
        }

        [OneTimeTearDown]
        public virtual void ClassCleanup()
        {
            _grpcServer?.Stop();
            DisposeAllEventProcessors();
        }

        [TearDown]
        public virtual void Cleanup()
        {
            MockSecurityService.Reset();
            MockSampleProcessingService.Reset();
            MockSampleResultsManager.Reset();
        }

        protected IGrpcClient ClientLogin(string username, string password)
        {
            var client = Kernel.Get<IGrpcClient>();
            client.Init(username, password);
            return client;
        }

        /// <summary>
        /// One EventProcessor of each type is created for each test.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateEventProcessor<T>()
        {
            lock (_eventProcessorLock)
            {
                if (!EventProcessors.TryGetValue(typeof(T), out var eventProcessorObject))
                {
                    eventProcessorObject = Kernel.Get<T>();
                    EventProcessors[typeof(T)] = eventProcessorObject;
                }
                return (T)eventProcessorObject;
            }
        }

        public void WaitOnEventProcessor<T>()
        {
            var eventProcessor = CreateEventProcessor<T>();
            Assert.NotNull(eventProcessor, "EventProcessor not found in Kernel. Did you bind it?");
            if (eventProcessor is ITestRegisterMutex testProcessor)
            {
                testProcessor.RegisterMutex.WaitOne();
            }
            else
            {
                Assert.Fail("Registered EventProcessor of {nameof(typeof(T))} does not have a Bind<> to a TestResultProcessor and does not implement ITestRegisterMutex.");
            }
        }

        protected void DisposeAllEventProcessors()
        {
            foreach (var eventProcessor in EventProcessors.Values)
            {
                if (eventProcessor is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}