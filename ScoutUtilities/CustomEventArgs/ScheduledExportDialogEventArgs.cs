using ScoutUtilities.Enums;

namespace ScoutUtilities.CustomEventArgs
{
    public class ScheduledExportDialogEventArgs<T> : BaseDialogEventArgs
    {
        public T ScheduledExportDomain { get; set; }
        public bool InEditMode { get; set; }

        public ScheduledExportDialogEventArgs(T scheduledExport, bool inEditMode)
        {
            DialogLocation = DialogLocation.TopCenterApp; // don't center because the dialog can change vertical size
            ScheduledExportDomain = scheduledExport;
            InEditMode = inEditMode;
        }
    }
}