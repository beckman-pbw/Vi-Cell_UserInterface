using ScoutUtilities.Enums;
using System;
using System.Runtime.InteropServices;

namespace ScoutUtilities.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ScheduledExport
    {
        public uuidDLL Uuid;
        public IntPtr Name;
        public IntPtr Comments;
        public IntPtr FilenameTemplate;
        public IntPtr DestinationFolder;
        public byte IsEnabled;
        public byte IsEncrypted; // Only used with SampleResults Scheduled Exports
        public IntPtr NotificationEmail;
        public RecurrenceFrequencyStruct RecurrenceFrequency;
        public DataFilterCriteria DataFilter;
        
        public ulong LastTimeRun; // not used on GUI
        public ulong LastSuccessTimeRun; // not used on GUI
        public ScheduledExportLastRunStatus LastRunStatus; // not used on GUI

        public byte IncludeAuditLog; // Only used with Audit Log Scheduled Exports
        public byte IncludeErrorLog; // Only used with Audit Log Scheduled Exports

        public ScheduledExportType ExportType;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RecurrenceFrequencyStruct
    {
        public RecurrenceFrequency RepeatEvery;
        public ulong ExportOnDate;
        public ushort Hour;
        public ushort Minute;
        public Weekday Weekday;
        public ushort DayOfTheMonth;

        public ulong StartOn; // not used on GUI
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DataFilterCriteria
    {
        public ulong FromDate;
        public ulong ToDate;
        public IntPtr UsernameForData;
        public IntPtr SampleTagSearchString;
        public IntPtr SampleNameSearchString;
        public IntPtr SampleSetNameSearchString;
        public byte AllCellTypesSelected;
        public uint CellTypeIndex;
        public IntPtr QualityControlName;

        public byte SinceLastExport; // not used on GUI
        public ulong StartDate; // not used on GUI
        public ulong EndDate; // not used on GUI
    }
}