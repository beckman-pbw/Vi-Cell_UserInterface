namespace ScoutUtilities.CustomEventArgs
{
    public class ScheduledExportAuditLogsDialogEventArgs<T> : BaseDialogEventArgs
    {
        public T ScheduledExportDomain { get; set; }
        public bool InEditMode { get; set; }

        public ScheduledExportAuditLogsDialogEventArgs(T scheduledExport, bool inEditMode)
        {
            ScheduledExportDomain = scheduledExport;
            InEditMode = inEditMode;
        }
    }
}