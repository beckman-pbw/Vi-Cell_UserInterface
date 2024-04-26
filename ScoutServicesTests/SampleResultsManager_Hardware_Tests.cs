using Google.Protobuf.WellKnownTypes;
using Grpc.Core.Logging;
using GrpcClient;
using GrpcClient.Services;
using GrpcServer;
using GrpcService;
using Ninject;
using NUnit.Framework;
using ScoutModels.Ninject;
using ScoutServices.Interfaces;
using ScoutServices.Ninject;
using System;
using ScoutModels.ExpandedSampleWorkflow;
using ScoutModels.Service;
using HawkeyeCoreAPI.Interfaces;

using System.Collections.Generic;
using ScoutDomains;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutModels;
using ScoutModels.Interfaces;
using TestSupport;

namespace ScoutServicesTests
{
    [TestFixture]
    public class SampleResultsManager_Hardware_Tests : BackendTestBase
    {
        private Grpc.Core.Logging.ILogger _grpcLogger;
        private ISampleResultsManager _sampleResultsManager;

        private OpcUaGrpcServer _opcUaGrpcServer;
        private OpcUaGrpcClient _grpcClient;
        private IKernel _kernel;
        private List<string> _sampleUuids;
        public const string _username = "username";
        public const string _password = "password";

        [SetUp]
        public override void Setup()
        {
            _sampleUuids = new List<string>();
            _kernel = new StandardKernel(new ScoutServiceModule(), new OpcUaGrpcModule(),
                new ScoutModelsModule());
            _kernel.Bind<ILogger>().To<OpcGrpcLogger>().InSingletonScope();
            _kernel.Bind<IWorkListModel>().To<WorkListModel>().InSingletonScope();
            _kernel.Bind<ICapacityManager>().To<CapacityManager>().InSingletonScope();
            _kernel.Bind<IDisplayService>().To<DisplayService>().InSingletonScope();
            _kernel.Bind<IInstrumentStatusService>().To<InstrumentStatusService>().InSingletonScope();
            _kernel.Bind<ISystemStatus>().To<HawkeyeCoreAPI.SystemStatus>().InSingletonScope();
            _kernel.Bind<IErrorLog>().To<HawkeyeCoreAPI.ErrorLog>().InSingletonScope();
            _kernel.Bind<IRunningWorkListModel>().To<RunningWorkListModel>().InSingletonScope();
             base.Setup();
        }

        [Test]
        [Ignore("Jenkins build hangs on this tests call to backend")]
        public void TestSampleResultsManager()
        {
            try
            {
                _grpcLogger = _kernel.Get<ILogger>();
                // Create the sample results manager
                _sampleResultsManager = _kernel.Get<ISampleResultsManager>();

                // Create and start server.
                _opcUaGrpcServer = _kernel.Get<OpcUaGrpcServer>();
                _opcUaGrpcServer.StartServer();

                // Create Grpc client.
                _grpcClient = _kernel.Get<OpcUaGrpcClient>();
                _grpcClient.Init(_username, _password);

                #region GetSampleResults

                // Formulate request for GetSampleResults
                var requestGetSampleRecords = new RequestGetSampleResults
                {
                    Username = string.Empty,
                    FromDate = Timestamp.FromDateTime(DateTime.Now.AddDays(-100.0).ToUniversalTime()),
                    ToDate = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
                    FilterType = FilterOnEnum.FilterSampleSet,
                    CellTypeQualityControlName = string.Empty,
                    SearchNameString = string.Empty,
                    SearchTagString = string.Empty
                };

                // Invoke SendRequestGetSampleResults
                var getSampleResult = _grpcClient.SendRequestGetSampleResults(requestGetSampleRecords);

                // Verify Success
                Assert.IsNotNull(getSampleResult);
                Assert.AreEqual(getSampleResult.MethodResult, MethodResultEnum.Success);

                #endregion

                #region RetrieveSampleResults

                // Acquire the IObservable<SampleResult[]>
                var observeRetrieveSampleExportChanges = _sampleResultsManager.SubscribeExportStatus();
                Assert.IsNotNull(observeRetrieveSampleExportChanges);
                // Subscribe to getting samples...
                //observeRetrieveSampleExportChanges.Subscribe(RetrievedSampleExportResults);

                // Formulate request for GetSampleResults
                //var requestRetrieveSampleExport = new RequestRetrieveSampleExport
                //{
                //    SampleUuid = { _sampleUuids }
                //};

                // Invoke SendRequestGetSampleResults
                //var retrieveSampleExportResult = _grpcClient.SendRequestRetrieveSampleExport(requestRetrieveSampleExport);

                // Verify Success
                //Assert.IsNotNull(retrieveSampleExportResult);
                //Assert.AreEqual(retrieveSampleExportResult.MethodResult, MethodResultEnum.Success);

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

        private void RetrievedSampleExportResults(byte[] zipBytes)
        {
            _grpcLogger.Info(zipBytes == null
                ? $"Retrieved a Sample Result Export with a byte length that is null"
                : $"Retrieved a Sample Result Export with a byte length of : {zipBytes.Length}");
        }

        private void ReceivedSampleResults(SampleResult[] results)
        {
            if (results == null)
                _grpcLogger.Info("ReceivedSampleResults sample results is null.");
            else
            {
                _grpcLogger.Info($"Received a total of: {results.Length} sample results.");

                foreach (var sample in results)
                {
                    _sampleUuids.Add(sample.SampleDataUuid.ToString());
                    _grpcLogger.Info($"Received Sample Id: {sample.SampleId} by {sample.AnalysisBy}");
                }
            }
        }
    }
}
