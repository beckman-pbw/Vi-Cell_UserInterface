using System.IO;
using HawkeyeCoreAPI.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using ScoutModels;
using ScoutModels.Interfaces;
using ScoutModels.Settings;
using ScoutServices;
using ScoutServices.Interfaces;
using ScoutServices.Watchdog;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using ScoutViewModels.ViewModels.Tabs.SettingsPanel;

namespace ScoutVmTests.Settings.Tabs
{
    [TestFixture]
    public class InstrumentSettingsViewModelTests
    {
        [Test]
        public void TestCanSetAutomationConfigCommandIfSettingsAreValid()
        {
            var autoMock = new Mock<IAutomationSettingsService>();
            
            autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(false, false, 1));

            autoMock.Setup(m => m.CanSaveAutomationConfig(It.IsAny<AutomationConfig>(),It.IsAny<bool>(), It.IsAny<bool>())).Returns(true);
            
            autoMock.Setup(m => m.CanSaveAutomationConfig(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<uint>(),It.IsAny<bool>(), It.IsAny<bool>())).Returns(true);

            var vm = GetVm(autoMock);
            
            Assert.IsFalse(vm.SetAutomationConfigCommand.CanExecute(null)); // settings haven't changed

            vm.IsAutomationOn = true; // change a setting
            Assert.IsTrue(vm.SetAutomationConfigCommand.CanExecute(null));

            vm.IsACupEnabled = true; //change a setting
            Assert.IsTrue(vm.SetAutomationConfigCommand.CanExecute(null));

            vm.IsAutomationOn = false; // reset 
            vm.IsACupEnabled = false;
            Assert.IsFalse(vm.SetAutomationConfigCommand.CanExecute(null)); // settings haven't changed

            vm.AutomationPort = 25; // change a setting. Can't change port if automation isn't on
            Assert.IsFalse(vm.SetAutomationConfigCommand.CanExecute(null));

        }

        [Test]
        public void TestCanNotSetAutomationConfigCommandIfSettingsAreInvalid()
        {
            var autoMock = new Mock<IAutomationSettingsService>();
            
            autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(false, false, 1));

            autoMock.Setup(m => m.CanSaveAutomationConfig(It.IsAny<AutomationConfig>(),It.IsAny<bool>(), It.IsAny<bool>())).Returns(false);
            
            autoMock.Setup(m => m.CanSaveAutomationConfig(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<uint>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(false);

            var vm = GetVm(autoMock);
            
            Assert.IsFalse(vm.SetAutomationConfigCommand.CanExecute(null)); // settings haven't changed

            vm.IsAutomationOn = true;

            Assert.IsFalse(vm.SetAutomationConfigCommand.CanExecute(null));

            vm.IsACupEnabled = true; //change a setting. Can't enable A-Cup if automation isn't on
            Assert.IsFalse(vm.SetAutomationConfigCommand.CanExecute(null)); // settings haven't changed

        }

        [Test]
        public void TestCanCancelAutomationConfig()
        {
            var autoMock = new Mock<IAutomationSettingsService>();
            
            autoMock.Setup(m => m.GetAutomationConfig()).Returns(new AutomationConfig(true, false, 12345));

            var vm = GetVm(autoMock);
            
            Assert.IsFalse(vm.CancelAutomationConfigCommand.CanExecute(null)); // settings haven't changed
            
            vm.IsAutomationOn = false;
            Assert.IsTrue(vm.CancelAutomationConfigCommand.CanExecute(null));

            vm.CancelAutomationConfigCommand.Execute(null);
            Assert.IsFalse(vm.CancelAutomationConfigCommand.CanExecute(null)); // settings haven't changed

            vm.AutomationPort = 54321;
            Assert.IsTrue(vm.CancelAutomationConfigCommand.CanExecute(null));

            vm.CancelAutomationConfigCommand.Execute(null);
            Assert.IsFalse(vm.CancelAutomationConfigCommand.CanExecute(null)); // settings haven't changed

            vm.IsAutomationOn = false;
            vm.AutomationPort = 54321;
            Assert.IsTrue(vm.CancelAutomationConfigCommand.CanExecute(null));

            vm.IsAutomationOn = true;
            vm.AutomationPort = 62641;
            Assert.IsFalse(vm.CancelAutomationConfigCommand.CanExecute(null)); // settings actually haven't changed

            vm.IsAutomationOn = false;
            vm.AutomationPort = 54321;
            Assert.IsTrue(vm.CancelAutomationConfigCommand.CanExecute(null));
            vm.CancelAutomationConfigCommand.Execute(null);

            Assert.IsFalse(vm.CancelAutomationConfigCommand.CanExecute(null));
            Assert.AreEqual(true, vm.IsAutomationOn);
            Assert.AreEqual(62641, vm.AutomationPort);

            vm.IsAutomationOn = true;
            vm.IsACupEnabled = true;
            Assert.IsTrue(vm.CancelAutomationConfigCommand.CanExecute(null));

        }

        private InstrumentSettingsViewModel GetVm(Mock<IAutomationSettingsService> autoMock)
        {
            var watchdog = new Mock<IWatchdog>();
            var lockManager = new Mock<ILockManager>();
            var testConfigLocation = Path.Combine(TestContext.CurrentContext.TestDirectory,
                "TestResources\\ViCellBLU.Server.Config.xml");
            var configurationRootMock = new Mock<IConfigurationRoot>();
            configurationRootMock.SetupGet(x => x["opcua:server:configlocation"]).Returns(testConfigLocation);
            var opcUaCfgManager = new OpcUaCfgManager(configurationRootMock.Object);
            var dbMock = new Mock<IDbSettingsService>();
            var smtpMock = new Mock<ISmtpSettingsService>();
            var instrStatusMock = new Mock<ISystemStatus>();
            var errorMock = new Mock<IErrorLog>();
            var iloggerMock = new Mock<Ninject.Extensions.Logging.ILogger>();
            var sysStatusMock = new Mock<ISystemStatus>();
            var hardwareSettingsMock = new Mock<IHardwareSettingsModel>();
            string serialNumber = "12345";
            hardwareSettingsMock.Setup(m => m.GetSystemSerialNumber(ref serialNumber)).Returns(HawkeyeError.eSuccess);
            var applicationStateServiceMock = new Mock<IApplicationStateService>();
            var instrumentStatusService = new InstrumentStatusService(instrStatusMock.Object, errorMock.Object, iloggerMock.Object, hardwareSettingsMock.Object, applicationStateServiceMock.Object);
            var vm = new InstrumentSettingsViewModel(watchdog.Object, lockManager.Object, opcUaCfgManager, dbMock.Object, smtpMock.Object, autoMock.Object, instrumentStatusService);
            
            Assert.IsNotNull(vm);
            return vm;
        }
    }
}