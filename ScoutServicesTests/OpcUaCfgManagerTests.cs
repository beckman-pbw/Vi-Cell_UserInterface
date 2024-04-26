using NUnit.Framework;
using ScoutServices;
using Moq;
using System.IO;
using Microsoft.Extensions.Configuration;
using ScoutModels.Interfaces;

namespace ScoutServicesTests
{
    [TestFixture]
    public class OpcUaCfgManagerTests
    {
        private OpcUaCfgManager _opcUaCfgManager;

        [Test]
        public void TestOpcUaCfgManager()
        {
            // Create the OpcUaCfgManager
            var testConfigLocation = Path.Combine(TestContext.CurrentContext.TestDirectory,
                    "TestResources\\ViCellBLU.Server.Config.xml");
            var configurationRootMock = new Mock<IConfigurationRoot>();
            configurationRootMock.SetupGet(x => x["opcua:server:configlocation"]).Returns(testConfigLocation);
            _opcUaCfgManager = new OpcUaCfgManager(configurationRootMock.Object);

            // Get the configured port number
            var portNumber = _opcUaCfgManager.GetOrSetOpcUaPort();
            Assert.AreEqual(62641, portNumber);

            // Change the port number 
            _opcUaCfgManager.GetOrSetOpcUaPort(62642);
            portNumber = _opcUaCfgManager.GetOrSetOpcUaPort();
            Assert.AreEqual(62642, portNumber);

            // Check for backup copy of config
            var backupConfigLocation = Path.Combine(TestContext.CurrentContext.TestDirectory,
                    "TestResources\\ViCellBLU.Server.Config.xml.bak");
            Assert.IsTrue(File.Exists(backupConfigLocation));

            // Restore original config file from backup
            File.Copy(backupConfigLocation, testConfigLocation, true);

            // Get the original configured port number
            _opcUaCfgManager = new OpcUaCfgManager(configurationRootMock.Object);
            portNumber = _opcUaCfgManager.GetOrSetOpcUaPort();
            Assert.AreEqual(62641, portNumber);
        }
    }
}
