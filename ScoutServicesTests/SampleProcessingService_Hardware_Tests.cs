using GrpcClient;
using GrpcServer;
using GrpcService;
using Ninject;
using NUnit.Framework;
using ScoutModels.Ninject;
using ScoutServices.Ninject;
using System;
using TestSupport;

namespace ScoutServicesTests
{
    [TestFixture]
    public class SampleProcessingService_Hardware_Tests : BackendTestBase
    {
        private OpcUaGrpcServer _opcUaGrpcServer;
        private OpcUaGrpcClient _grpcClient;
        private IKernel _kernel;
        public const string _username = "username";
        public const string _password = "password";

        [SetUp]
        public override void Setup()
        {
            _kernel = new StandardKernel(new ScoutServiceModule(), new OpcUaGrpcModule(),
                new ScoutModelsModule());
            base.Setup();
        }

        [Test]
        [Ignore("Jenkins build hangs on this tests call to backend")]

        public void TestSampleProcessingService_StopMethod()
        {
            try
            {
                // Create and start server.
                _opcUaGrpcServer = _kernel.Get<OpcUaGrpcServer>();
                _opcUaGrpcServer.StartServer();

                // Create Grpc client.
                _grpcClient = _kernel.Get<OpcUaGrpcClient>();
                _grpcClient.Init(_username, _password);


                // Invoke SendRequestStop
                RequestStop requestStop = new RequestStop();
                var stopResult = _grpcClient.SendRequestStop(requestStop);

                // Verify Failure
                Assert.IsNotNull(stopResult);
                Assert.AreNotEqual(stopResult.MethodResult, MethodResultEnum.Failure);

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
        [Ignore("Jenkins build hangs on this tests call to backend")]
        public void TestSampleProcessingService_EjectMethod()
        {
            try
            {

                // Create and start server.
                _opcUaGrpcServer = _kernel.Get<OpcUaGrpcServer>();
                _opcUaGrpcServer.StartServer();

                // Create Grpc client.
                _grpcClient = _kernel.Get<OpcUaGrpcClient>();
                _grpcClient.Init(_username, _password);

                // Formulate request
                var request = new RequestEjectStage();

                // Invoke SendRequestEjectStage
                var ejectStageResult = _grpcClient.SendRequestEjectStage(request);

                // Verify failure
                Assert.IsNotNull(ejectStageResult);
                Assert.AreNotEqual(ejectStageResult.MethodResult, MethodResultEnum.Failure);

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
        [Ignore("Jenkins build hangs on this tests call to backend")]
        public void TestSampleProcessingService_PauseMethod()
        {
            try
            {
                // Create and start server.
                _opcUaGrpcServer = _kernel.Get<OpcUaGrpcServer>();
                _opcUaGrpcServer.StartServer();

                // Create Grpc client.
                _grpcClient = _kernel.Get<OpcUaGrpcClient>();
                _grpcClient.Init(_username, _password);

                // Invoke SendRequestPause
                RequestPause requestPause = new RequestPause();
                var pauseResult = _grpcClient.SendRequestPause(requestPause);

                // Verify Failure
                Assert.IsNotNull(pauseResult);
                Assert.AreNotEqual(pauseResult.MethodResult, MethodResultEnum.Failure);

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
        [Ignore("Jenkins build hangs on this tests call to backend")]
        public void TestSampleProcessingService_ResumeMethod()
        {
            try
            {
                // Create and start server.
                _opcUaGrpcServer = _kernel.Get<OpcUaGrpcServer>();
                _opcUaGrpcServer.StartServer();

                // Create Grpc client.
                _grpcClient = _kernel.Get<OpcUaGrpcClient>();
                _grpcClient.Init(_username, _password);

                // Invoke SendRequestResume
                RequestResume requestResume = new RequestResume();
                var resumeResult = _grpcClient.SendRequestResume(requestResume);

                // Verify Success
                // TODO: This is not consistent with "Stop" and "Pause", which both return an error
                // if no sample processing has been done
                Assert.IsNotNull(resumeResult);
                Assert.AreEqual(resumeResult.MethodResult, MethodResultEnum.Success);

                // Shutdown server
                _opcUaGrpcServer.ShutdownServer();
            }
            catch (Exception ex)
            {
                _opcUaGrpcServer?.ShutdownServer();
                Assert.Fail(ex.Message);
            }
        }
    }
}