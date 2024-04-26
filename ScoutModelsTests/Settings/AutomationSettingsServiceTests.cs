using NUnit.Framework;
using ScoutModels.Settings;
using ScoutUtilities.Structs;

namespace ScoutModelsTests.Settings
{
    [TestFixture]
    public class AutomationSettingsServiceTests
    {
        [Test]
        public void TestIsValid_WhenEnablingWithValidPort()
        {
            var service = new AutomationSettingsService();
            Assert.IsNotNull(service);

            Assert.IsTrue(service.IsValid(true, false, 1));
            Assert.IsTrue(service.IsValid(new AutomationConfig(true, false, 1)));
        }

        [Test]
        public void TestIsNotValid_WhenEnablingWithInvalidPort()
        {
            var service = new AutomationSettingsService();
            Assert.IsNotNull(service);

            Assert.IsFalse(service.IsValid(true, false, 0));
            Assert.IsFalse(service.IsValid(new AutomationConfig(true, false, 0)));
        }

        [Test]
        public void TestIsValid_WhenDisablingWithInvalidPort()
        {
            var service = new AutomationSettingsService();
            Assert.IsNotNull(service);

            Assert.IsTrue(service.IsValid(false, false, 0));
            Assert.IsTrue(service.IsValid(new AutomationConfig(false, false, 0)));
        }

        [Test]
        public void TestIsValid_WhenDisablingWithValidPort()
        {
            var service = new AutomationSettingsService();
            Assert.IsNotNull(service);

            Assert.IsTrue(service.IsValid(false, false, 1));
            Assert.IsTrue(service.IsValid(new AutomationConfig(false, false, 1)));
        }

        [Test]
        public void CanSave_WhenEnablingWithValidPort()
        {
            var service = new AutomationSettingsService();
            Assert.IsNotNull(service);

            Assert.IsTrue(service.CanSaveAutomationConfig(true, false, 1, false, true));
            Assert.IsTrue(service.CanSaveAutomationConfig(new AutomationConfig(true, false, 1), false, true));

            Assert.IsTrue(service.CanSaveAutomationConfig(true, false, 1, true, false));
            Assert.IsTrue(service.CanSaveAutomationConfig(new AutomationConfig(true, false, 1), 
                true, false));
        }

        [Test]
        public void CanSave_WhenEnablingWithInvalidPort()
        {
            var service = new AutomationSettingsService();
            Assert.IsNotNull(service);

            Assert.IsFalse(service.CanSaveAutomationConfig(true, false, 0, false, true));
            Assert.IsFalse(service.CanSaveAutomationConfig(new AutomationConfig(true, false, 0), false, true));

            Assert.IsFalse(service.CanSaveAutomationConfig(true, false, 0, true, false));
            Assert.IsFalse(service.CanSaveAutomationConfig(new AutomationConfig(true, false, 0), true, false));
        }

        [Test]
        public void CanSave_WhenDisablingWithInvalidPort()
        {
            var service = new AutomationSettingsService();
            Assert.IsNotNull(service);

            Assert.IsTrue(service.CanSaveAutomationConfig(false, false, 0, false, true));
            Assert.IsTrue(service.CanSaveAutomationConfig(new AutomationConfig(false, false, 0), false, true));

            Assert.IsTrue(service.CanSaveAutomationConfig(false, false, 0, true, false));
            Assert.IsTrue(service.CanSaveAutomationConfig(new AutomationConfig(false, false, 0), 
                true, false));
        }

        [Test]
        public void CanSave_WhenDisablingWithValidPort()
        {
            var service = new AutomationSettingsService();
            Assert.IsNotNull(service);

            Assert.IsTrue(service.CanSaveAutomationConfig(false, false, 1, false, true));
            Assert.IsTrue(service.CanSaveAutomationConfig(new AutomationConfig(false, false, 1), false, true));

            Assert.IsTrue(service.CanSaveAutomationConfig(false, false, 1, true, false));
            Assert.IsTrue(service.CanSaveAutomationConfig(new AutomationConfig(false, false, 1), true, false));
        }

        [Test]
        public void CanSave_ACupEnable()
        {
            var service = new AutomationSettingsService();
            Assert.IsNotNull(service);

            Assert.IsTrue(service.CanSaveAutomationConfig(true, true, 1, false, true));
            Assert.IsTrue(service.CanSaveAutomationConfig(new AutomationConfig(true, true, 1), false, true));

            Assert.IsFalse(service.CanSaveAutomationConfig(true, true, 1, true, false));
            Assert.IsFalse(service.CanSaveAutomationConfig(new AutomationConfig(true, true, 1), true, false));
        }

        [Test]
        public void CanSave_ACupDisable()
        {
            var service = new AutomationSettingsService();
            Assert.IsNotNull(service);

            Assert.IsTrue(service.CanSaveAutomationConfig(true, false, 1, false, true));
            Assert.IsTrue(service.CanSaveAutomationConfig(new AutomationConfig(true, false, 1), false, true));

            Assert.IsTrue(service.CanSaveAutomationConfig(true, false, 1, true, false));
            Assert.IsTrue(service.CanSaveAutomationConfig(new AutomationConfig(true, false, 1), true, false));
        }
    }
}
