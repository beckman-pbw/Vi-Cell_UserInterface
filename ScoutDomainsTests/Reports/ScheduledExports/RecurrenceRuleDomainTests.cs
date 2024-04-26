using ScoutDomains.Reports.ScheduledExports;
using NUnit.Framework;
using ScoutUtilities.Enums;
using System;

namespace ScoutDomains.Reports.ScheduledExports.Tests
{
    [TestFixture]
    public class RecurrenceRuleDomainTests
    {
        [TestCase(0, ExpectedResult = true)]
        [TestCase(1, ExpectedResult = true)]
        [TestCase(11, ExpectedResult = true)]
        [TestCase(12, ExpectedResult = true)]
        [TestCase(13, ExpectedResult = true)]
        [TestCase(23, ExpectedResult = true)]
        [TestCase(24, ExpectedResult = true)] // property only allows up to 23
        public bool RecurrenceRulesAreValid_24HourClockFormatAndHour(int hour)
        {
            var rec = new RecurrenceRuleDomain();
            rec.ExportOnDate = DateTime.Now.AddDays(1);
            rec.SelectedClockFormat = ClockFormat.Hour24;
            rec.RecurrenceFrequency = RecurrenceFrequency.Daily;
            rec.Hour = (ushort)hour;
            return rec.RecurrenceRulesAreValid();
        }

        [TestCase(0, ExpectedResult = true)]
        [TestCase(1, ExpectedResult = true)]
        [TestCase(11, ExpectedResult = true)]
        [TestCase(12, ExpectedResult = true)]
        [TestCase(13, ExpectedResult = false)]
        [TestCase(23, ExpectedResult = false)]
        [TestCase(24, ExpectedResult = false)] // property only allows up to 23
        public bool RecurrenceRulesAreValid_AMClockFormatAndHour(int hour)
        {
            var rec = new RecurrenceRuleDomain();
            rec.ExportOnDate = DateTime.Now.AddDays(1);
            rec.SelectedClockFormat = ClockFormat.AM;
            rec.RecurrenceFrequency = RecurrenceFrequency.Daily;
            rec.Hour = (ushort)hour;
            return rec.RecurrenceRulesAreValid();
        }

        [Test]
        public void RecurrenceRulesAreValid_ExportOnDate_Success()
        {
            var rec = new RecurrenceRuleDomain();
            DateTime schedTime = DateTime.Now.AddMinutes(2);
            rec.Hour = (ushort)schedTime.Hour;
            rec.Minutes = (ushort)schedTime.Minute;
            rec.RecurrenceFrequency = RecurrenceFrequency.Once;
            Assert.IsTrue(rec.RecurrenceRulesAreValid());
        }

        [TestCase(0, ExpectedResult = 0)]
        [TestCase(1, ExpectedResult = 1)]
        [TestCase(23, ExpectedResult = 23)]
        [TestCase(24, ExpectedResult = 23)]
        [TestCase(25, ExpectedResult = 23)]
        public int RecurrenceRulesAreValid_HourSetProperty(int hourInput)
        {
            var rec = new RecurrenceRuleDomain();
            rec.ExportOnDate = DateTime.Now.AddDays(1);
            rec.SelectedClockFormat = ClockFormat.AM;
            rec.RecurrenceFrequency = RecurrenceFrequency.Daily;

            rec.Hour = (ushort)hourInput;
            return rec.Hour;
        }

        [TestCase(0, ExpectedResult = 0)]
        [TestCase(1, ExpectedResult = 1)]
        [TestCase(59, ExpectedResult = 59)]
        [TestCase(60, ExpectedResult = 59)]
        [TestCase(61, ExpectedResult = 59)]
        public int RecurrenceRulesAreValid_MinuteSetProperty(int minInput)
        {
            var rec = new RecurrenceRuleDomain();
            rec.ExportOnDate = DateTime.Now.AddDays(1);
            rec.SelectedClockFormat = ClockFormat.AM;
            rec.RecurrenceFrequency = RecurrenceFrequency.Daily;

            rec.Minutes = (ushort)minInput;
            return rec.Minutes;
        }

        [TestCase(0, ExpectedResult = 1)]
        [TestCase(1, ExpectedResult = 1)]
        [TestCase(12, ExpectedResult = 12)]
        [TestCase(31, ExpectedResult = 31)]
        [TestCase(32, ExpectedResult = 31)]
        [TestCase(33, ExpectedResult = 31)]
        public int RecurrenceRulesAreValid_DayOfMonthSetProperty(int dayOfMonthInput)
        {
            var rec = new RecurrenceRuleDomain();
            rec.ExportOnDate = DateTime.Now.AddDays(1);
            rec.SelectedClockFormat = ClockFormat.AM;
            rec.RecurrenceFrequency = RecurrenceFrequency.Daily;

            rec.DayOfTheMonth = (ushort)dayOfMonthInput;
            return rec.DayOfTheMonth;
        }

        [TestCase((ushort) 0, ClockFormat.AM, ExpectedResult = (ushort) 0)]
        [TestCase((ushort) 1, ClockFormat.AM, ExpectedResult = (ushort) 1)]
        [TestCase((ushort) 2, ClockFormat.AM, ExpectedResult = (ushort) 2)]
        [TestCase((ushort) 3, ClockFormat.AM, ExpectedResult = (ushort) 3)]
        [TestCase((ushort) 4, ClockFormat.AM, ExpectedResult = (ushort) 4)]
        [TestCase((ushort) 5, ClockFormat.AM, ExpectedResult = (ushort) 5)]
        [TestCase((ushort) 6, ClockFormat.AM, ExpectedResult = (ushort) 6)]
        [TestCase((ushort) 7, ClockFormat.AM, ExpectedResult = (ushort) 7)]
        [TestCase((ushort) 8, ClockFormat.AM, ExpectedResult = (ushort) 8)]
        [TestCase((ushort) 9, ClockFormat.AM, ExpectedResult = (ushort) 9)]
        [TestCase((ushort) 10, ClockFormat.AM, ExpectedResult = (ushort) 10)]
        [TestCase((ushort) 11, ClockFormat.AM, ExpectedResult = (ushort) 11)]
        [TestCase((ushort) 12, ClockFormat.AM, ExpectedResult = (ushort) 0)]
        
        [TestCase((ushort) 0, ClockFormat.PM, ExpectedResult = (ushort) 12)]
        [TestCase((ushort) 1, ClockFormat.PM, ExpectedResult = (ushort) 13)]
        [TestCase((ushort) 2, ClockFormat.PM, ExpectedResult = (ushort) 14)]
        [TestCase((ushort) 3, ClockFormat.PM, ExpectedResult = (ushort) 15)]
        [TestCase((ushort) 4, ClockFormat.PM, ExpectedResult = (ushort) 16)]
        [TestCase((ushort) 5, ClockFormat.PM, ExpectedResult = (ushort) 17)]
        [TestCase((ushort) 6, ClockFormat.PM, ExpectedResult = (ushort) 18)]
        [TestCase((ushort) 7, ClockFormat.PM, ExpectedResult = (ushort) 19)]
        [TestCase((ushort) 8, ClockFormat.PM, ExpectedResult = (ushort) 20)]
        [TestCase((ushort) 9, ClockFormat.PM, ExpectedResult = (ushort) 21)]
        [TestCase((ushort) 10, ClockFormat.PM, ExpectedResult = (ushort) 22)]
        [TestCase((ushort) 11, ClockFormat.PM, ExpectedResult = (ushort) 23)]
        [TestCase((ushort) 12, ClockFormat.PM, ExpectedResult = (ushort) 12)]
        
        [TestCase((ushort) 0, ClockFormat.Hour24, ExpectedResult = (ushort) 0)]
        [TestCase((ushort) 12, ClockFormat.Hour24, ExpectedResult = (ushort) 12)]
        [TestCase((ushort) 23, ClockFormat.Hour24, ExpectedResult = (ushort) 23)]
        public ushort Get24HourTest(ushort hour, ClockFormat clockFormat)
        {
            var rrd = new RecurrenceRuleDomain();
            rrd.Hour = hour;
            rrd.SelectedClockFormat = clockFormat;
            return rrd.Get24Hour();
        }
    }
}
