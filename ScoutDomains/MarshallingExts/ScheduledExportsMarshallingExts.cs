using log4net;
using ScoutDomains.Reports.ScheduledExports;
using ScoutLanguageResources;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ScoutDomains
{
    public static class ScheduledExportsMarshallingExts
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Returns a SampleResultsScheduledExportDomain

        public static List<SampleResultsScheduledExportDomain> MarshalToScheduledExportDomainList(
            this IntPtr scheduledExportsPtr, uint count,
            List<CellTypeQualityControlGroupDomain> ctQcList)
        {
            var list = new List<SampleResultsScheduledExportDomain>();

            try
            {
                var size = Marshal.SizeOf(typeof(ScheduledExport));
                var iterator = scheduledExportsPtr;
                for (int i = 0; i < count; i++)
                {
                    list.Add(iterator.MarshalToScheduledExportDomain(ctQcList));
                    iterator += size;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to marshal '{nameof(IntPtr)}' to 'List<{nameof(SampleResultsScheduledExportDomain)}>'", e);
            }

            return list;
        }

        public static SampleResultsScheduledExportDomain MarshalToScheduledExportDomain(this IntPtr iterator,
            List<CellTypeQualityControlGroupDomain> ctQcList)
        {
            try
            {
                var scheduledExport = (ScheduledExport)Marshal.PtrToStructure(iterator, typeof(ScheduledExport));
                return scheduledExport.MarshalToScheduledExportDomain(ctQcList);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to marshal '{nameof(IntPtr)}' to '{nameof(SampleResultsScheduledExportDomain)}'", e);
                return new SampleResultsScheduledExportDomain();
            }
        }

        public static SampleResultsScheduledExportDomain MarshalToScheduledExportDomain(this ScheduledExport scheduledExport,
            List<CellTypeQualityControlGroupDomain> ctQcList)
        {
            var domain = new SampleResultsScheduledExportDomain();

            try
            {
                domain.Uuid = scheduledExport.Uuid;
                domain.Name = scheduledExport.Name.ToSystemString();
                domain.Comments = scheduledExport.Comments.ToSystemString();
                domain.FilenameTemplate = scheduledExport.FilenameTemplate.ToSystemString();
                domain.DestinationFolder = scheduledExport.DestinationFolder.ToSystemString();
                domain.IsEnabled = Misc.ByteToBool(scheduledExport.IsEnabled);
                domain.IsEncrypted = Misc.ByteToBool(scheduledExport.IsEncrypted);
                domain.NotificationEmail = scheduledExport.NotificationEmail.ToSystemString();
                domain.LastRunStatus = scheduledExport.LastRunStatus;
                domain.LastRunTime = DateTimeConversionHelper.FromSecondUnixToDateTime(scheduledExport.LastTimeRun);
                domain.LastSuccessRunTime = DateTimeConversionHelper.FromSecondUnixToDateTime(scheduledExport.LastSuccessTimeRun);

                domain.DataFilterCriteria = new DataFilterCriteriaDomain();
                var user = scheduledExport.DataFilter.UsernameForData.ToSystemString();
                domain.DataFilterCriteria.SelectedUsername = string.IsNullOrEmpty(user)
                    ? LanguageResourceHelper.Get("LID_Label_All")
                    : user;
                domain.DataFilterCriteria.Tag = scheduledExport.DataFilter.SampleTagSearchString.ToSystemString();
                domain.DataFilterCriteria.IsAllCellTypeSelected = Misc.ByteToBool(scheduledExport.DataFilter.AllCellTypesSelected);

                var sampleNameSearch = scheduledExport.DataFilter.SampleNameSearchString.ToSystemString();
                var sampleSetNameSearch = scheduledExport.DataFilter.SampleSetNameSearchString.ToSystemString();

                if (string.IsNullOrEmpty(sampleSetNameSearch))
                {
                    domain.DataFilterCriteria.SampleSearchString = sampleNameSearch;
                    domain.DataFilterCriteria.FilterType = eFilterItem.eSample;
                }
                else
                {
                    domain.DataFilterCriteria.SampleSearchString = sampleSetNameSearch;
                    domain.DataFilterCriteria.FilterType = eFilterItem.eSampleSet;
                }

                domain.DataFilterCriteria.FromDate = DateTimeConversionHelper.FromSecondUnixToDateTime(scheduledExport.DataFilter.FromDate);
                domain.DataFilterCriteria.ToDate = DateTimeConversionHelper.FromSecondUnixToDateTime(scheduledExport.DataFilter.ToDate);

                var qcName = scheduledExport.DataFilter.QualityControlName.ToSystemString();
                if (!string.IsNullOrEmpty(qcName))
                {
                    var item = ctQcList.GetCellTypeQualityControlByName(qcName);
                    // For quality control, don't want to include cell type name
                    if (item != null && item.KeyName == qcName)
                        item.Name = qcName;
                    domain.DataFilterCriteria.SelectedCellTypeOrQualityControlGroup = item;
                }
                else
                {
                    var item = ctQcList.GetCellTypeQualityControlByIndex(scheduledExport.DataFilter.CellTypeIndex);
                    domain.DataFilterCriteria.SelectedCellTypeOrQualityControlGroup = item;
                }

                domain.RecurrenceRule = new RecurrenceRuleDomain();
                domain.RecurrenceRule.RecurrenceFrequency = scheduledExport.RecurrenceFrequency.RepeatEvery;
                domain.RecurrenceRule.SelectedClockFormat = ClockFormat.Hour24;
                domain.RecurrenceRule.Hour = scheduledExport.RecurrenceFrequency.Hour;
                domain.RecurrenceRule.Minutes = scheduledExport.RecurrenceFrequency.Minute;
                domain.RecurrenceRule.Weekday = scheduledExport.RecurrenceFrequency.Weekday;
                domain.RecurrenceRule.DayOfTheMonth = scheduledExport.RecurrenceFrequency.DayOfTheMonth;
                domain.RecurrenceRule.ExportOnDate = DateTimeConversionHelper.FromSecondUnixToDateTime(scheduledExport.RecurrenceFrequency.ExportOnDate);


            }
            catch (Exception e)
            {
                Log.Error($"Failed to marshal '{nameof(ScheduledExport)}' to '{nameof(SampleResultsScheduledExportDomain)}'", e);
            }

            return domain;
        }

        #endregion

        #region Returns a AuditLogScheduledExportDomain

        public static List<AuditLogScheduledExportDomain> MarshalToAuditLogScheduledExportDomainList(
            this IntPtr scheduledExportsPtr, uint count)
        {
            var list = new List<AuditLogScheduledExportDomain>();

            try
            {
                var size = Marshal.SizeOf(typeof(ScheduledExport));
                var iterator = scheduledExportsPtr;
                for (int i = 0; i < count; i++)
                {
                    list.Add(iterator.MarshalToAuditLogScheduledExportDomain());
                    iterator += size;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to marshal '{nameof(IntPtr)}' to 'List<{nameof(AuditLogScheduledExportDomain)}>'", e);
            }

            return list;
        }

        public static AuditLogScheduledExportDomain MarshalToAuditLogScheduledExportDomain(this IntPtr iterator)
        {
            try
            {
                var scheduledExport = (ScheduledExport)Marshal.PtrToStructure(iterator, typeof(ScheduledExport));
                return scheduledExport.MarshalToAuditLogScheduledExportDomain();
            }
            catch (Exception e)
            {
                Log.Error($"Failed to marshal '{nameof(IntPtr)}' to '{nameof(AuditLogScheduledExportDomain)}'", e);
                return new AuditLogScheduledExportDomain();
            }
        }

        public static AuditLogScheduledExportDomain MarshalToAuditLogScheduledExportDomain(
            this ScheduledExport scheduledExport)
        {
            var domain = new AuditLogScheduledExportDomain();

            try
            {
                domain.Uuid = scheduledExport.Uuid;
                domain.Name = scheduledExport.Name.ToSystemString();
                domain.Comments = scheduledExport.Comments.ToSystemString();
                domain.FilenameTemplate = scheduledExport.FilenameTemplate.ToSystemString();
                domain.DestinationFolder = scheduledExport.DestinationFolder.ToSystemString();
                domain.IsEnabled = Misc.ByteToBool(scheduledExport.IsEnabled);
                domain.NotificationEmail = scheduledExport.NotificationEmail.ToSystemString();

                domain.LastRunStatus = scheduledExport.LastRunStatus;
                domain.LastRunTime = DateTimeConversionHelper.FromSecondUnixToDateTime(scheduledExport.LastTimeRun);
                domain.LastSuccessRunTime = DateTimeConversionHelper.FromSecondUnixToDateTime(scheduledExport.LastSuccessTimeRun);

                domain.IncludeAuditLog = Misc.ByteToBool(scheduledExport.IncludeAuditLog);
                domain.IncludeErrorLog = Misc.ByteToBool(scheduledExport.IncludeErrorLog);
                
                domain.RecurrenceRule = new RecurrenceRuleDomain();
                domain.RecurrenceRule.RecurrenceFrequency = scheduledExport.RecurrenceFrequency.RepeatEvery;
                domain.RecurrenceRule.SelectedClockFormat = ClockFormat.Hour24;
                domain.RecurrenceRule.Hour = scheduledExport.RecurrenceFrequency.Hour;
                domain.RecurrenceRule.Minutes = scheduledExport.RecurrenceFrequency.Minute;
                domain.RecurrenceRule.Weekday = scheduledExport.RecurrenceFrequency.Weekday;
                domain.RecurrenceRule.DayOfTheMonth = scheduledExport.RecurrenceFrequency.DayOfTheMonth;
                domain.RecurrenceRule.ExportOnDate = DateTimeConversionHelper.FromSecondUnixToDateTime(scheduledExport.RecurrenceFrequency.ExportOnDate);
                domain.DataFilterCriteria.FromDate = DateTimeConversionHelper.FromSecondUnixToDateTime(scheduledExport.DataFilter.FromDate);
                domain.DataFilterCriteria.ToDate = DateTimeConversionHelper.FromSecondUnixToDateTime(scheduledExport.DataFilter.ToDate);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to marshal '{nameof(ScheduledExport)}' to '{nameof(AuditLogScheduledExportDomain)}'", e);
            }

            return domain;
        }

        #endregion

        #region Returns a ScheduledExport

        private static ScheduledExport GetBaseScheduledExport(BaseScheduledExportDomain domain)
        {
            var export = new ScheduledExport();
            export.Uuid = domain.Uuid;
            export.Name = domain.Name.ToIntPtr();
            export.Comments = domain.Comments.ToIntPtr();
            export.FilenameTemplate = domain.FilenameTemplate.ToIntPtr();
            export.DestinationFolder = domain.DestinationFolder.ToIntPtr();
            export.IsEnabled = Misc.BoolToByte(domain.IsEnabled);

            export.NotificationEmail = domain.NotificationEmail.ToIntPtr();

            export.LastRunStatus = domain.LastRunStatus;
            export.LastTimeRun = DateTimeConversionHelper.DateTimeToUnixSecondRounded(domain.LastRunTime);
            export.LastSuccessTimeRun = DateTimeConversionHelper.DateTimeToUnixSecondRounded(domain.LastSuccessRunTime);

            export.RecurrenceFrequency = new RecurrenceFrequencyStruct();
            export.RecurrenceFrequency.Hour = domain.RecurrenceRule.Get24Hour();
            export.RecurrenceFrequency.Minute = domain.RecurrenceRule.Minutes;
            export.RecurrenceFrequency.DayOfTheMonth = domain.RecurrenceRule.DayOfTheMonth;
            export.RecurrenceFrequency.RepeatEvery = domain.RecurrenceRule.RecurrenceFrequency;
            export.RecurrenceFrequency.Weekday = domain.RecurrenceRule.Weekday;
            export.RecurrenceFrequency.ExportOnDate = DateTimeConversionHelper.DateTimeToUnixSecondRounded(domain.RecurrenceRule.ExportOnDate);

            return export;
        }

        public static ScheduledExport GetScheduledExport(this AuditLogScheduledExportDomain domain)
        {
            var export = GetBaseScheduledExport(domain);
            export.ExportType = ScheduledExportType.AuditLogs;
            export.IncludeAuditLog = Misc.BoolToByte(domain.IncludeAuditLog);
            export.IncludeErrorLog = Misc.BoolToByte(domain.IncludeErrorLog);
            export.DataFilter.FromDate = DateTimeConversionHelper.DateTimeToUnixSecondRounded(domain.DataFilterCriteria.FromDate);
            export.DataFilter.ToDate = DateTimeConversionHelper.DateTimeToUnixSecondRounded(domain.DataFilterCriteria.ToDate);
            return export;
        }

        public static ScheduledExport GetScheduledExport(this SampleResultsScheduledExportDomain domain)
        {
            var export = GetBaseScheduledExport(domain);
            export.ExportType = ScheduledExportType.SampleResults;
            export.IsEncrypted = Misc.BoolToByte(domain.IsEncrypted);

            export.DataFilter = new DataFilterCriteria();
            export.DataFilter.SampleTagSearchString = domain.DataFilterCriteria.Tag.ToIntPtr();
            export.DataFilter.FromDate = DateTimeConversionHelper.DateTimeToUnixSecondRounded(domain.DataFilterCriteria.FromDate);
            export.DataFilter.ToDate = DateTimeConversionHelper.DateTimeToUnixSecondRounded(domain.DataFilterCriteria.ToDate);
            export.DataFilter.AllCellTypesSelected = Misc.BoolToByte(domain.DataFilterCriteria.IsAllCellTypeSelected);

            export.LastRunStatus = domain.LastRunStatus;
            export.LastTimeRun = DateTimeConversionHelper.DateTimeToUnixSecondRounded(domain.LastRunTime);
            export.LastSuccessTimeRun = DateTimeConversionHelper.DateTimeToUnixSecondRounded(domain.LastSuccessRunTime);

            export.IsEnabled = Misc.BoolToByte(domain.IsEnabled);

            if (!domain.DataFilterCriteria.IsAllCellTypeSelected &&
                domain.DataFilterCriteria.SelectedCellTypeOrQualityControlGroup != null)
            {
                if (domain.DataFilterCriteria.SelectedCellTypeOrQualityControlGroup.SelectedCtBpQcType == CtBpQcType.QualityControl)
                {
                    export.DataFilter.QualityControlName = domain.DataFilterCriteria.SelectedCellTypeOrQualityControlGroup.KeyName.ToIntPtr();
                }
                else
                {
                    export.DataFilter.CellTypeIndex = domain.DataFilterCriteria.SelectedCellTypeOrQualityControlGroup.CellTypeIndex;
                }
            }

            if (domain.DataFilterCriteria.SelectedUsername.Equals(LanguageResourceHelper.Get("LID_Label_All")))
            {
                export.DataFilter.UsernameForData = string.Empty.ToIntPtr();
            }
            else
            {
                export.DataFilter.UsernameForData = domain.DataFilterCriteria.SelectedUsername.ToIntPtr();
            }

            if (domain.DataFilterCriteria.FilterType == eFilterItem.eSampleSet)
            {
                export.DataFilter.SampleSetNameSearchString = domain.DataFilterCriteria.SampleSearchString.ToIntPtr();
                export.DataFilter.SampleNameSearchString = string.Empty.ToIntPtr();
            }
            else
            {
                export.DataFilter.SampleNameSearchString = domain.DataFilterCriteria.SampleSearchString.ToIntPtr();
                export.DataFilter.SampleSetNameSearchString = string.Empty.ToIntPtr();
            }

            return export;
        }

        #endregion
    }
}