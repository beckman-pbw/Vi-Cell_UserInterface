using NUnit.Framework;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;

namespace ScoutDomains.Reports.ScheduledExports.Tests
{
    [TestFixture]
    public class AuditLogScheduledExportDomainTests
    {
        [Test]
        public void CloneTest()
        {
            var export = GetScheduledExportDomainMonthly();
            var clone = (AuditLogScheduledExportDomain)export.Clone();

            clone.Uuid = new uuidDLL(Guid.NewGuid());
            clone.Name = "Clone Name";
            clone.Comments = "Clone Comments";
            clone.FilenameTemplate = "Clone Filename";
            clone.DestinationFolder = "C:\\CloneFilename";
            clone.IsEnabled = false;
            clone.IncludeAuditLog = false;
            clone.IncludeErrorLog = false;
            clone.NotificationEmail = "clone@email.com";
            clone.LastRunStatus = ScheduledExportLastRunStatus.Error;
            
            clone.RecurrenceRule.RecurrenceFrequency = RecurrenceFrequency.Weekly;
            clone.RecurrenceRule.Hour = 2;
            clone.RecurrenceRule.Minutes = 34;
            clone.RecurrenceRule.SelectedClockFormat = ClockFormat.Hour24;
            clone.RecurrenceRule.Weekday = Weekday.Saturday;

            Assert.AreNotEqual(clone.Uuid, export.Uuid);
            Assert.AreNotEqual(clone.Name, export.Name);
            Assert.AreNotEqual(clone.Comments, export.Comments);
            Assert.AreNotEqual(clone.FilenameTemplate, export.FilenameTemplate);
            Assert.AreNotEqual(clone.DestinationFolder, export.DestinationFolder);
            Assert.AreNotEqual(clone.IsEnabled, export.IsEnabled);
            Assert.AreNotEqual(clone.IncludeAuditLog, export.IncludeAuditLog);
            Assert.AreNotEqual(clone.IncludeErrorLog, export.IncludeErrorLog);
            Assert.AreNotEqual(clone.NotificationEmail, export.NotificationEmail);
            Assert.AreNotEqual(clone.LastRunStatus, export.LastRunStatus);
            
            Assert.AreNotEqual(clone.RecurrenceRule.RecurrenceFrequency, export.RecurrenceRule.RecurrenceFrequency);
            Assert.AreNotEqual(clone.RecurrenceRule.Hour, export.RecurrenceRule.Hour);
            Assert.AreNotEqual(clone.RecurrenceRule.Minutes, export.RecurrenceRule.Minutes);
            Assert.AreNotEqual(clone.RecurrenceRule.SelectedClockFormat, export.RecurrenceRule.SelectedClockFormat);
            Assert.AreNotEqual(clone.RecurrenceRule.Weekday, export.RecurrenceRule.Weekday);
        }

        [Test]
        public void CloneTest2()
        {
            var export = GetScheduledExportDomainOnce();
            var clone = (AuditLogScheduledExportDomain) export.Clone();

            clone.Uuid = new uuidDLL(Guid.NewGuid());
            clone.Name = "Clone Name";
            clone.Comments = "Clone Comments";
            clone.FilenameTemplate = "Clone Filename";
            clone.DestinationFolder = "C:\\CloneFilename";
            clone.IsEnabled = false;
            clone.IncludeAuditLog = false;
            clone.IncludeErrorLog = false;
            clone.NotificationEmail = "clone@email.com";
            clone.LastRunStatus = ScheduledExportLastRunStatus.Error;

            clone.RecurrenceRule.RecurrenceFrequency = RecurrenceFrequency.Weekly;
            clone.RecurrenceRule.Hour = 2;
            clone.RecurrenceRule.Minutes = 34;
            clone.RecurrenceRule.SelectedClockFormat = ClockFormat.Hour24;
            clone.RecurrenceRule.Weekday = Weekday.Saturday;
            clone.RecurrenceRule.ExportOnDate = DateTime.Now.AddDays(10);

            clone.DataFilterCriteria = new DataFilterCriteriaDomain();
            clone.DataFilterCriteria.FromDate = DateTime.Now.AddDays(-100);
            clone.DataFilterCriteria.ToDate = DateTime.Now.AddDays(-90);

            Assert.AreNotEqual(clone.Uuid, export.Uuid);
            Assert.AreNotEqual(clone.Name, export.Name);
            Assert.AreNotEqual(clone.Comments, export.Comments);
            Assert.AreNotEqual(clone.FilenameTemplate, export.FilenameTemplate);
            Assert.AreNotEqual(clone.DestinationFolder, export.DestinationFolder);
            Assert.AreNotEqual(clone.IsEnabled, export.IsEnabled);
            Assert.AreNotEqual(clone.IncludeAuditLog, export.IncludeAuditLog);
            Assert.AreNotEqual(clone.IncludeErrorLog, export.IncludeErrorLog);
            Assert.AreNotEqual(clone.NotificationEmail, export.NotificationEmail);
            Assert.AreNotEqual(clone.LastRunStatus, export.LastRunStatus);

            Assert.AreNotEqual(clone.RecurrenceRule.RecurrenceFrequency, export.RecurrenceRule.RecurrenceFrequency);
            Assert.AreNotEqual(clone.RecurrenceRule.Hour, export.RecurrenceRule.Hour);
            Assert.AreNotEqual(clone.RecurrenceRule.Minutes, export.RecurrenceRule.Minutes);
            Assert.AreNotEqual(clone.RecurrenceRule.SelectedClockFormat, export.RecurrenceRule.SelectedClockFormat);
            Assert.AreNotEqual(clone.RecurrenceRule.Weekday, export.RecurrenceRule.Weekday);
            Assert.AreNotEqual(clone.RecurrenceRule.ExportOnDate, export.RecurrenceRule.ExportOnDate);

            Assert.AreNotEqual(clone.DataFilterCriteria.FromDate, export.DataFilterCriteria.FromDate);
            Assert.AreNotEqual(clone.DataFilterCriteria.ToDate, export.DataFilterCriteria.ToDate);
        }

        [TestCase(true, true, ExpectedResult = true)]
        [TestCase(false, true, ExpectedResult = true)]
        [TestCase(true, false, ExpectedResult = true)]
        [TestCase(false, false, ExpectedResult = false)]
        public bool TestIsValid(bool includeAuditLog, bool includeErrorLog)
        {
            var domain = GetScheduledExportDomainMonthly();
            domain.IncludeAuditLog = includeAuditLog;
            domain.IncludeErrorLog = includeErrorLog;
            return domain.IsValid();
        }

        private AuditLogScheduledExportDomain GetScheduledExportDomainMonthly()
        {
            var export = new AuditLogScheduledExportDomain();
            export.Uuid = new uuidDLL(Guid.NewGuid());
            export.Name = "OG name";
            export.Comments = "OG comments";
            export.FilenameTemplate = "OG Filename";
            export.DestinationFolder = "C:\\OGFilename";
            export.IsEnabled = true;
            export.NotificationEmail = "my@email.com";
            export.LastRunStatus = ScheduledExportLastRunStatus.Success;
            export.IncludeAuditLog = true;
            export.IncludeErrorLog = true;
            
            export.RecurrenceRule = new RecurrenceRuleDomain();
            export.RecurrenceRule.RecurrenceFrequency = RecurrenceFrequency.Monthly;
            export.RecurrenceRule.Hour = 1;
            export.RecurrenceRule.Minutes = 23;
            export.RecurrenceRule.SelectedClockFormat = ClockFormat.PM;
            export.RecurrenceRule.DayOfTheMonth = 20;

            export.DataFilterCriteria = new DataFilterCriteriaDomain(); // not used for this case
            export.DataFilterCriteria.FromDate = DateTime.Now;
            export.DataFilterCriteria.ToDate = DateTime.Now;

            return export;
        }

        private AuditLogScheduledExportDomain GetScheduledExportDomainOnce()
        {
            var export = new AuditLogScheduledExportDomain();
            export.Uuid = new uuidDLL(Guid.NewGuid());
            export.Name = "OG name";
            export.Comments = "OG comments";
            export.FilenameTemplate = "OG Filename";
            export.DestinationFolder = "C:\\OGFilename";
            export.IsEnabled = true;
            export.NotificationEmail = "my@email.com";
            export.LastRunStatus = ScheduledExportLastRunStatus.Success;
            export.IncludeAuditLog = true;
            export.IncludeErrorLog = true;
            
            export.RecurrenceRule = new RecurrenceRuleDomain();
            export.RecurrenceRule.RecurrenceFrequency = RecurrenceFrequency.Once;
            export.RecurrenceRule.Hour = 1;
            export.RecurrenceRule.Minutes = 23;
            export.RecurrenceRule.SelectedClockFormat = ClockFormat.PM;
            export.RecurrenceRule.ExportOnDate = DateTime.Now.AddDays(3);

            export.DataFilterCriteria = new DataFilterCriteriaDomain();
            export.DataFilterCriteria.FromDate = DateTime.Now.AddDays(-7);
            export.DataFilterCriteria.ToDate = DateTime.Now;

            return export;
        }
    }
}