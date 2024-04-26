using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Logging;
using GrpcClient;
using GrpcClient.Interfaces;
using GrpcServer;
using GrpcServer.EventProcessors;
using GrpcService;
using Moq;
using Ninject;
using Ninject.Extensions.Factory;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using ScoutDomains.Analysis;
using ScoutServices.Enums;
using ScoutServices.Interfaces;
using ScoutUtilities.Enums;

namespace ScoutOpcUaTests
{
    /// <summary>
    /// The unit test will instantiate the gRPC server on one thread, similar to the way
    /// the event unit test was written, with a client running on another thread.
    /// The client will connect to the server, and using the CallCredentialsHelper and
    /// make a gRPC request. The ISecurityService will be mocked and expectations
    /// asserted to ensure that the methods were called with the expected values.
    /// </summary>
    public class GrpcSecurityTest : GrpcBaseTestClass
    {
        /*
        private const string username = "username1";
        private const string password = "password1";
        */
        private const string username = "factory_admin";
        private const string password = "Vi-CELL#01";
        private const string badPassword = "badPassword";

        protected Mock<ILogger> MockGrpcLogger = new Mock<ILogger>();
        protected Mock<Ninject.Extensions.Logging.ILogger> MockLogger = new Mock<Ninject.Extensions.Logging.ILogger>();

        private void MakeLockRequest(IGrpcClient client)
        {
            var lockRequest = new RequestRequestLock();
            var cancellationTokenSource = new CancellationTokenSource();
            var result = client.ServicesClient.RequestLock(lockRequest, client.ClientCredentialHelper.CreateCallOptions(cancellationTokenSource.Token));
            Assert.AreEqual(GrpcService.MethodResultEnum.Success, result.MethodResult);
        }

        [Test, Ignore("Needs backend to test")]
        public void MakeSuccessfulRequest()
        {
            var client = ClientLogin(username, password);

            MakeLockRequest(client);

            var lockManager = Kernel.Get<ILockManager>();
            Assert.IsTrue(lockManager.IsLocked(), "AutomationLock request should have been processed.");
        }

        [Test, Ignore("Needs backend to test")]
        public void MakeBadPasswordRequest()
        {
            var client = ClientLogin(username, badPassword);
            try
            {
                MakeLockRequest(client);
                Assert.Fail("Should have thrown an RpcException for failing authentication.");
            }
            catch (TestClientException e)
            {
                var lockManager = Kernel.Get<ILockManager>();
                Assert.IsFalse(lockManager.IsLocked(), "AutomationLock request should not have been processed.");
                Assert.AreEqual(TestClientException.ERROR_CODE_PERMISSION_DENIED, e.ErrorCode);
            }
            catch (Exception)
            {
                Assert.Fail("Unexpected Exception.");
            }
        }

        [Test, Ignore("Needs backend to test")]
        public void MakeBadPermissionsRequest()
        {
            var client = ClientLogin(username, password);
            try
            {
                MakeLockRequest(client);
                Assert.Fail("Should have thrown an RpcException for failing authentication.");
            }
            catch (TestClientException e)
            {
                var lockManager = Kernel.Get<ILockManager>();
                Assert.IsFalse(lockManager.IsLocked(), "AutomationLock request should not have been processed.");
                Assert.AreEqual(TestClientException.ERROR_CODE_PERMISSION_DENIED, e.ErrorCode);
            }
            catch (Exception)
            {
                Assert.Fail("Unexpected Exception.");
            }
        }

        [Test, Ignore("Needs backend to test")]
        public void MakeSuccessfulSubscription()
        {
            var client = ClientLogin(username, password);
            var registeredEvent = new TestRegisteredEvent(MockLogger.Object, client);
            registeredEvent.Register();
            Debug.WriteLine("GrpcSecurityTest.MakeSuccessfulSubscription(): About to call RegisterWait.WaitOne().");
            WaitOnEventProcessor<LockResultProcessor>();
            Debug.WriteLine("GrpcSecurityTest.MakeSuccessfulSubscription(): Exited RegisterWait.WaitOne().");
            MakeLockRequest(client);
            var msgHistory = registeredEvent.MessageHistory;
            Assert.AreEqual(1, msgHistory.Count);
            Assert.AreEqual(LockStateEnum.Locked, msgHistory.First().LockState);

            // EventProcessor will not exit loop until Dispose() is called.
            client.Dispose();
        }


    }

    public class TestRegisteredEvent : TestRegisteredEventBase<LockStateChangedEvent>
    {
        public TestRegisteredEvent(Ninject.Extensions.Logging.ILogger logger, IGrpcClient client) : base(logger, client)
        {
        }

        public override void RegisterForTest()
        {
            var request = MakeRequest(TopicTypeEnum.LockStateChangedType);
            var callOptions = MakeCallOptions();
            StreamingCall = Client.ServicesClient.SubscribeLockState(request, callOptions);
        }
    }
}