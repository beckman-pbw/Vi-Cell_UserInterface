using System.Windows;
using NUnit.Framework;
using ScoutLanguageResources;
using ScoutUI.Common.Converters;
using ScoutUtilities.Enums;

namespace ScoutUiTest.Converters
{
    [TestFixture]
    public class NightlyCleanConverterTests
    {
        [Test]
        public void NightlyCleanStatusVisibilityConverterTests()
        {
            var conv = new NightlyCleanStatusVisibilityConverter();
            
            Assert.AreEqual(Visibility.Collapsed, 
                conv.Convert(eNightlyCleanStatus.ncsAutomationUnableToPerform, null, null, null));
            Assert.AreEqual(Visibility.Visible,
                conv.Convert(eNightlyCleanStatus.ncsAutomationInProgress, null, null, null));
            Assert.AreEqual(Visibility.Collapsed,
                conv.Convert(eNightlyCleanStatus.ncsIdle, null, null, null));
            Assert.AreEqual(Visibility.Visible,
                conv.Convert(eNightlyCleanStatus.ncsInProgress, null, null, null));
            Assert.AreEqual(Visibility.Collapsed,
                conv.Convert(eNightlyCleanStatus.ncsUnableToPerform, null, null, null));
        }

        [Test]
        public void NightCleanInProgressMessageConverterTests()
        {
            var conv = new NightCleanInProgressMessage();

            Assert.AreEqual(string.Empty,
                conv.Convert(eNightlyCleanStatus.ncsAutomationUnableToPerform, null, null, null));
            Assert.AreEqual(LanguageResourceHelper.Get("LID_MSGBOX_AutomationNightCleaning"),
                conv.Convert(eNightlyCleanStatus.ncsAutomationInProgress, null, null, null));
            Assert.AreEqual(string.Empty,
                conv.Convert(eNightlyCleanStatus.ncsIdle, null, null, null));
            Assert.AreEqual(LanguageResourceHelper.Get("LID_MSGBOX_NightCleaning"),
                conv.Convert(eNightlyCleanStatus.ncsInProgress, null, null, null));
            Assert.AreEqual(string.Empty,
                conv.Convert(eNightlyCleanStatus.ncsUnableToPerform, null, null, null));
        }
    }
}