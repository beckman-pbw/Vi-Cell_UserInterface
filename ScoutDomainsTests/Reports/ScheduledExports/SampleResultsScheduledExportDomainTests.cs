using NUnit.Framework;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScoutDomains.Reports.ScheduledExports.Tests
{
    [TestFixture()]
    public class SampleResultsScheduledExportDomainTests
    {
        [Test()]
        public void CloneTest()
        {
            var export = GetScheduledExportDomain();
            var clone = (SampleResultsScheduledExportDomain) export.Clone();

            clone.Uuid = new uuidDLL(Guid.NewGuid());
            clone.Name = "Clone Name";
            clone.Comments = "Clone Comments";
            clone.FilenameTemplate = "Clone Filename";
            clone.DestinationFolder = "C:\\CloneFilename";
            clone.IsEnabled = false;
            clone.IsEncrypted = false;
            clone.NotificationEmail = "clone@email.com";
            clone.LastRunStatus = ScheduledExportLastRunStatus.Error;

            clone.DataFilterCriteria.FilterType = eFilterItem.eSampleSet;
            clone.DataFilterCriteria.FromDate = DateTime.Now.AddDays(-30);
            clone.DataFilterCriteria.ToDate = DateTime.Now.AddDays(-10);
            clone.DataFilterCriteria.Usernames = new ObservableCollection<string>
            {
                "User3",
                "User4"
            };
            clone.DataFilterCriteria.SelectedUsername = clone.DataFilterCriteria.Usernames.FirstOrDefault();
            clone.DataFilterCriteria.Tag = "Clone Tag";
            clone.DataFilterCriteria.SampleSearchString = "Clone Search String";
            clone.DataFilterCriteria.CellTypesAndQualityControls.FirstOrDefault().Name = "Clone QC Name";

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
            Assert.AreNotEqual(clone.IsEncrypted, export.IsEncrypted);
            Assert.AreNotEqual(clone.NotificationEmail, export.NotificationEmail);
            Assert.AreNotEqual(clone.LastRunStatus, export.LastRunStatus);
            
            Assert.AreNotEqual(clone.DataFilterCriteria.FilterType, export.DataFilterCriteria.FilterType);
            Assert.AreNotEqual(clone.DataFilterCriteria.FromDate, export.DataFilterCriteria.FromDate);
            Assert.AreNotEqual(clone.DataFilterCriteria.ToDate, export.DataFilterCriteria.ToDate);
            Assert.AreNotEqual(clone.DataFilterCriteria.SelectedUsername, export.DataFilterCriteria.SelectedUsername);
            Assert.AreNotEqual(clone.DataFilterCriteria.Tag, export.DataFilterCriteria.Tag);
            Assert.AreNotEqual(clone.DataFilterCriteria.SampleSearchString, export.DataFilterCriteria.SampleSearchString);
            Assert.AreNotEqual(clone.DataFilterCriteria.CellTypesAndQualityControls.FirstOrDefault().Name, export.DataFilterCriteria.CellTypesAndQualityControls.FirstOrDefault().Name);
            
            Assert.AreNotEqual(clone.RecurrenceRule.RecurrenceFrequency, export.RecurrenceRule.RecurrenceFrequency);
            Assert.AreNotEqual(clone.RecurrenceRule.Hour, export.RecurrenceRule.Hour);
            Assert.AreNotEqual(clone.RecurrenceRule.Minutes, export.RecurrenceRule.Minutes);
            Assert.AreNotEqual(clone.RecurrenceRule.SelectedClockFormat, export.RecurrenceRule.SelectedClockFormat);
            Assert.AreNotEqual(clone.RecurrenceRule.Weekday, export.RecurrenceRule.Weekday);
        }

        private SampleResultsScheduledExportDomain GetScheduledExportDomain()
        {
            var export = new SampleResultsScheduledExportDomain();
            export.Uuid = new uuidDLL(Guid.NewGuid());
            export.Name = "OG name";
            export.Comments = "OG comments";
            export.FilenameTemplate = "OG Filename";
            export.DestinationFolder = "C:\\OGFilename";
            export.IsEnabled = true;
            export.IsEncrypted = true;
            export.NotificationEmail = "my@email.com";
            export.LastRunStatus = ScheduledExportLastRunStatus.Success;

            export.DataFilterCriteria = new DataFilterCriteriaDomain();
            export.DataFilterCriteria.FilterType = eFilterItem.eSample;
            export.DataFilterCriteria.FromDate = DateTime.Now.AddDays(-10);
            export.DataFilterCriteria.ToDate = DateTime.Now;
            export.DataFilterCriteria.Usernames = new ObservableCollection<string>
            {
                "User1", 
                "User2"
            };
            export.DataFilterCriteria.SelectedUsername = export.DataFilterCriteria.Usernames.FirstOrDefault();
            export.DataFilterCriteria.Tag = "OG Tag";
            export.DataFilterCriteria.SampleSearchString = "OG Search String";
            export.DataFilterCriteria.CellTypesAndQualityControls =
                new ObservableCollection<CellTypeQualityControlGroupDomain>
                {
                    new CellTypeQualityControlGroupDomain
                    {
                        AppTypeIndex = 1,
                        CellTypeIndex = 1,
                        Name = "QC Name"
                    }
                };
            export.DataFilterCriteria.SelectedCellTypeOrQualityControlGroup = export.DataFilterCriteria.CellTypesAndQualityControls.FirstOrDefault();

            export.RecurrenceRule = new RecurrenceRuleDomain();
            export.RecurrenceRule.RecurrenceFrequency = RecurrenceFrequency.Monthly;
            export.RecurrenceRule.Hour = 1;
            export.RecurrenceRule.Minutes = 23;
            export.RecurrenceRule.SelectedClockFormat = ClockFormat.PM;
            export.RecurrenceRule.DayOfTheMonth = 20;

            return export;
        }
    }
}