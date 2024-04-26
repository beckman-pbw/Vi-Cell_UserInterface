using System;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ScoutUtilitiesTest
{
    [TestFixture]
    public class LoggerTests
    {
        [Test]
        public void TestGetAllExceptionMessages()
        {
            var msg1 = "Main Exception Message";
            var msg2 = "Inner Exception Message";
            var msg3 = "Inner Inner Exception Message";

            var exception = new Exception(msg1);
            var str = ScoutUtilities.Logger.GetAllExceptionMessages(exception);
            var expected = $"{msg1}";
            Assert.AreEqual(expected, str);

            exception = new Exception(msg1, new Exception(msg2));
            str = ScoutUtilities.Logger.GetAllExceptionMessages(exception);
            expected = $"{msg1}{Environment.NewLine}\t{msg2}";
            Assert.AreEqual(expected, str);

            exception = new Exception(msg1, new Exception(msg2, new Exception(msg3)));
            str = ScoutUtilities.Logger.GetAllExceptionMessages(exception);
            expected = $"{msg1}{Environment.NewLine}\t{msg2}{Environment.NewLine}\t\t{msg3}";
            Assert.AreEqual(expected, str);

            str = ScoutUtilities.Logger.GetAllExceptionMessages(null);
            expected = string.Empty;
            Assert.AreEqual(expected, str);

            exception = new Exception(msg1, null);
            str = ScoutUtilities.Logger.GetAllExceptionMessages(exception);
            expected = msg1;
            Assert.AreEqual(expected, str);
        }
    }
}