using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HawkeyeCoreAPI;
using Moq;
using Ninject.Extensions.Logging;
using NUnit.Framework;
using ScoutModels;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;

namespace TestSupport
{
    /// <summary>
    /// Tests that use the backend (ApplicationSource) need to call initialize and wait until the backend is ready.
    /// This base class performs that common functionality. Other helper methods could also be added.
    /// </summary>
    public class BackendTestBase
    {
        protected readonly AutoResetEvent _mainThreadCoordinator = new AutoResetEvent(false);
        protected readonly Mock<ILogger> _mockLogger = new Mock<ILogger>();
        protected readonly Mock<IDialogCaller> _mockDialogCaller = new Mock<IDialogCaller>();

        protected void ProcessInitializationMessages(InitializationState initializationState)
        {
            if (InitializationState.eInitializationComplete == initializationState || InitializationState.eInitializationFailed == initializationState)
            {
                _mainThreadCoordinator.Set();
            }
        }

        [SetUp]
        public virtual void Setup()
        {
            _mockDialogCaller
                .Setup(m => m.DialogBoxOk(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            _mockDialogCaller
                .Setup(m => m.DialogBoxOkCancel(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var hardwareManager = new HardwareManager(_mockLogger.Object, _mockDialogCaller.Object)
                {IsFromHardware = false};
            var initializationObserver = hardwareManager.SubscribeStateChanges();
            var initializationSubscription = initializationObserver.Subscribe(ProcessInitializationMessages);
            Task.Run(() => hardwareManager.StartHardwareInitialize());

            // Wait until initialization completed
            _mainThreadCoordinator.WaitOne();
            Assert.IsTrue(HardwareManager.Initialized);
        }

        [TearDown]
        public virtual void Cleanup()
        {
            InitializeShutdown.ShutdownAPI();
        }

    }
}
