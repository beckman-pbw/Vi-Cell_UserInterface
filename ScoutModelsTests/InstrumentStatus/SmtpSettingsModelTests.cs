using NUnit.Framework;
using ScoutModels.Settings;

namespace ScoutModelsTests.InstrumentStatus
{
    [TestFixture]
    public class SmtpSettingsModelTests
    {
        [Test]
        public void ValidEmail_IsValidEmailTest()
        {
            var service = new SmtpSettingsModel();

            Assert.IsTrue(service.IsValidEmail("jteague@beckman.com"));
            Assert.IsTrue(service.IsValidEmail("jteague@b3ckman.com"));
            Assert.IsTrue(service.IsValidEmail("jteague@b3ckm!n.com"));
            Assert.IsTrue(service.IsValidEmail("jteague.1@beckman.1.com"));
            Assert.IsTrue(service.IsValidEmail("jteague.a@beckman.a.com"));
            Assert.IsTrue(service.IsValidEmail("jteague.a@beckman.a#2.com"));
        }

        [Test]
        public void InvalidEmail_IsValidEmailTest()
        {
            var service = new SmtpSettingsModel();

            Assert.IsFalse(service.IsValidEmail(""));
            Assert.IsFalse(service.IsValidEmail(" "));
            Assert.IsFalse(service.IsValidEmail("jteague@b3@ckm!n.com"));
            Assert.IsFalse(service.IsValidEmail("@beckman.1.com"));
            Assert.IsFalse(service.IsValidEmail("jteague@beckman"));
            Assert.IsFalse(service.IsValidEmail("jteague"));
            Assert.IsFalse(service.IsValidEmail("jteague.com"));
        }
    }
}