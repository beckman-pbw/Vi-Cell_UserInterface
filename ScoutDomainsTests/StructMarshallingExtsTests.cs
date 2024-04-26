using Newtonsoft.Json;
using NUnit.Framework;
using ScoutDomains;
using ScoutDomains.Reports.ScheduledExports;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TestSupport;

namespace ScoutDomainsTests
{
    [TestFixture]
    public class StructMarshallingExtsTests
    {
        private string _cellTypeName = "Insect";

        [Test]
        public void MappingScheduledExportTests()
        {
            var domain = GetScheduledExportDomain();
            var ctQcList = domain.DataFilterCriteria.CellTypesAndQualityControls.ToList();
            var export = domain.GetScheduledExport();
            var mappedDomain = export.MarshalToScheduledExportDomain(ctQcList);

            var dateThreshold = new TimeSpan(0, 0, 2);

            Assert.AreEqual(domain.Uuid, mappedDomain.Uuid);
            Assert.AreEqual(domain.Name, mappedDomain.Name);
            Assert.AreEqual(domain.Comments, mappedDomain.Comments);
            Assert.AreEqual(domain.FilenameTemplate, mappedDomain.FilenameTemplate);
            Assert.AreEqual(domain.DestinationFolder, mappedDomain.DestinationFolder);
            Assert.AreEqual(domain.IsEnabled, mappedDomain.IsEnabled);
            Assert.AreEqual(domain.IsEncrypted, mappedDomain.IsEncrypted);
            Assert.AreEqual(domain.NotificationEmail, mappedDomain.NotificationEmail);
            Assert.AreEqual(domain.LastRunStatus, mappedDomain.LastRunStatus);

            Assert.AreEqual(domain.LastRunTime.Hour, mappedDomain.LastRunTime.Hour);
            Assert.AreEqual(domain.LastSuccessRunTime.Minute, mappedDomain.LastSuccessRunTime.Minute);

            Assert.AreEqual(domain.DataFilterCriteria.FilterType, mappedDomain.DataFilterCriteria.FilterType);

            Assert.IsTrue(domain.DataFilterCriteria.FromDate - mappedDomain.DataFilterCriteria.FromDate < dateThreshold);
            Assert.IsTrue(domain.DataFilterCriteria.ToDate - mappedDomain.DataFilterCriteria.ToDate < dateThreshold);

            Assert.AreEqual(domain.DataFilterCriteria.SelectedUsername, mappedDomain.DataFilterCriteria.SelectedUsername);

            Assert.AreEqual(domain.DataFilterCriteria.Tag, mappedDomain.DataFilterCriteria.Tag);
            Assert.AreEqual(domain.DataFilterCriteria.SampleSearchString, mappedDomain.DataFilterCriteria.SampleSearchString);

            Assert.AreEqual(2, domain.DataFilterCriteria.CellTypesAndQualityControls.Count);
            Assert.IsNull(mappedDomain.DataFilterCriteria.CellTypesAndQualityControls);
            
            Assert.AreEqual(_cellTypeName, ctQcList.GetCellTypeQualityControlByName(mappedDomain.DataFilterCriteria.SelectedCellTypeOrQualityControlGroup.Name).Name);
            Assert.AreEqual(2, ctQcList.GetCellTypeQualityControlByIndex(mappedDomain.DataFilterCriteria.SelectedCellTypeOrQualityControlGroup.CellTypeIndex).CellTypeIndex);
            
            Assert.AreEqual(domain.RecurrenceRule.RecurrenceFrequency, mappedDomain.RecurrenceRule.RecurrenceFrequency);
            Assert.AreEqual(domain.RecurrenceRule.Get24Hour(), mappedDomain.RecurrenceRule.Get24Hour());
            Assert.AreEqual(domain.RecurrenceRule.Minutes, mappedDomain.RecurrenceRule.Minutes);
            Assert.AreEqual(domain.RecurrenceRule.Weekday, mappedDomain.RecurrenceRule.Weekday);
        }

        [Test]
        public void MappingAuditLogScheduledExportTests()
        {
            var domain = GetAuditLogScheduledExportDomain();
            var export = domain.GetScheduledExport();
            var mappedDomain = export.MarshalToAuditLogScheduledExportDomain();
            
            Assert.AreEqual(domain.Uuid, mappedDomain.Uuid);
            Assert.AreEqual(domain.Name, mappedDomain.Name);
            Assert.AreEqual(domain.Comments, mappedDomain.Comments);
            Assert.AreEqual(domain.FilenameTemplate, mappedDomain.FilenameTemplate);
            Assert.AreEqual(domain.DestinationFolder, mappedDomain.DestinationFolder);

            Assert.AreEqual(domain.IsEnabled, (export.IsEnabled != 0));
            Assert.AreEqual(domain.IsEnabled, mappedDomain.IsEnabled);

            Assert.AreEqual(domain.NotificationEmail, mappedDomain.NotificationEmail);
            Assert.AreEqual(domain.LastRunStatus, mappedDomain.LastRunStatus);
            Assert.AreEqual(domain.LastRunTime.Hour, mappedDomain.LastRunTime.Hour);
            Assert.AreEqual(domain.LastSuccessRunTime.Minute, mappedDomain.LastSuccessRunTime.Minute);

            Assert.AreEqual(domain.IncludeAuditLog, mappedDomain.IncludeAuditLog);
            Assert.AreEqual(domain.IncludeErrorLog, mappedDomain.IncludeErrorLog);
            
            Assert.AreEqual(domain.RecurrenceRule.RecurrenceFrequency, mappedDomain.RecurrenceRule.RecurrenceFrequency);
            Assert.AreEqual(domain.RecurrenceRule.Get24Hour(), mappedDomain.RecurrenceRule.Get24Hour());
            Assert.AreEqual(domain.RecurrenceRule.Minutes, mappedDomain.RecurrenceRule.Minutes);
            Assert.AreEqual(domain.RecurrenceRule.Weekday, mappedDomain.RecurrenceRule.Weekday);
        }

        private SampleResultsScheduledExportDomain GetScheduledExportDomain()
        {
            var now = DateTime.Now;

            //var export = new SampleResultsScheduledExportDomain(); //GetScheduledExportDomainFromJson();
            var export = ObjectsForTesting.GetScheduledExportDomainFromJson();
            export.Uuid = new uuidDLL(Guid.NewGuid());
            export.Name = "OG name";
            export.Comments = "OG comments";
            export.FilenameTemplate = "OG Filename";
            export.DestinationFolder = "C:\\OGFilename";
            export.IsEnabled = true;
            export.IsEncrypted = true;
            export.NotificationEmail = "my@email.com";
            export.LastRunStatus = ScheduledExportLastRunStatus.Success;

            export.DataFilterCriteria.FilterType = eFilterItem.eSampleSet;
            export.DataFilterCriteria.FromDate = now.AddDays(-10);
            export.DataFilterCriteria.ToDate = now;
            export.DataFilterCriteria.Usernames = new ObservableCollection<string>
            {
                "User1",
                "User2"
            };            
            export.DataFilterCriteria.SelectedUsername = export.DataFilterCriteria.Usernames.FirstOrDefault();
            export.DataFilterCriteria.Tag = "OG Tag";
            export.DataFilterCriteria.SampleSearchString = "OG Set Search String";
            export.DataFilterCriteria.IsAllCellTypeSelected = false;
            export.DataFilterCriteria.SelectedCellTypeOrQualityControlGroup = export.DataFilterCriteria.CellTypesAndQualityControls.GetCellTypeQualityControlByName(_cellTypeName);
            
            export.RecurrenceRule.RecurrenceFrequency = RecurrenceFrequency.Monthly;
            export.RecurrenceRule.Hour = 1;
            export.RecurrenceRule.Minutes = 23;
            export.RecurrenceRule.SelectedClockFormat = ClockFormat.PM;
            export.RecurrenceRule.DayOfTheMonth = 20;

            return export;
        }

        private AuditLogScheduledExportDomain GetAuditLogScheduledExportDomain()
        {
            //var export = new AuditLogScheduledExportDomain(); //GetAuditLogScheduledExportDomainFromJson();
            var export = ObjectsForTesting.GetAuditLogScheduledExportDomainFromJson();
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
            export.LastRunTime = DateTime.Now.AddDays(-3);
            export.LastSuccessRunTime = export.LastRunTime;
            
            export.RecurrenceRule.RecurrenceFrequency = RecurrenceFrequency.Monthly;
            export.RecurrenceRule.Hour = 1;
            export.RecurrenceRule.Minutes = 23;
            export.RecurrenceRule.SelectedClockFormat = ClockFormat.PM;
            export.RecurrenceRule.DayOfTheMonth = 20;

            return export;
        }
    }
}