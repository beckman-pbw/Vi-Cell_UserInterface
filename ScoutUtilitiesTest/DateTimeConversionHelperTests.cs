using System;
using NUnit.Framework;
using ScoutUtilities;

namespace ScoutUtilitiesTest
{
    [TestFixture]
    public class DateTimeConversionHelperTests
    {
        private readonly ulong _unixLongTime = 12345;
        private readonly double _unixDoubleTime = 123456789;
        private readonly DateTime _dateTime = DateTime.Parse("1/16/2019 12:15:12 PM -07:00");

        [Test]
        public void TestFromLongSecondUnixToDateTime()
        {
            var retVal = DateTimeConversionHelper.FromSecondUnixToDateTime(_unixLongTime);
            Assert.AreEqual(DateTime.Parse("1969-12-31 20:25:45 -07:00"), retVal);
        }

        [Test]
        public void TestFromDoubleSecondUnixToDateTime()
        {
            var retVal = DateTimeConversionHelper.FromSecondUnixToDateTime(_unixDoubleTime);
            Assert.AreEqual(DateTime.Parse("11/29/1973 2:33:09 PM -07:00"), retVal);
        }

        [Test]
        public void TestFromMinUnixToDateTime()
        {
            var retVal = DateTimeConversionHelper.FromMinUnixToDateTime(_unixLongTime);
            Assert.AreEqual(DateTime.Parse("1970-01-09 06:45:00 -07:00"), retVal);
        }

        [Test]
        public void TestFromDaysUnixToDateTime()
        {
            var retVal = DateTimeConversionHelper.FromDaysUnixToDateTime(_unixLongTime);
            Assert.AreEqual(DateTime.Parse("10/20/2003 12:00:00 AM"), retVal);
        }

        [Test]
        public void TestDateTimeToUnixSecond()
        {
            var retVal = DateTimeConversionHelper.DateTimeToUnixSecond(_dateTime);
            Assert.AreEqual(1547666112, retVal);
        }

        [Test]
        public void TestDateTimeToUnixDays()
        {
            var retVal = DateTimeConversionHelper.DateTimeToUnixDays(_dateTime);
            Assert.AreEqual(17912.802222222221, retVal);
        }

        [Test]
        public void TestDateTimeToUnixMin()
        {
            var retVal = DateTimeConversionHelper.DateTimeToUnixMin(_dateTime);
            Assert.AreEqual(25794435.2, retVal);
        }
    }
}
