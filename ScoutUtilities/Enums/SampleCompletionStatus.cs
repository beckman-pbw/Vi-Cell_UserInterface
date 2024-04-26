using System.ComponentModel;

namespace ScoutUtilities.Enums
{
    // This is only used for logging purposes. It shouldn't be used in any sort of calculation or logic.
    public enum sample_completion_status : ushort
    {
        [Description("LID_Status_Completed")] sample_completed = 0,
        [Description("LID_Status_Skipped")] sample_skipped,
        [Description("LID_Label_Error")] sample_errored,
        [Description("LID_Status_NotRun")] sample_not_run
    }
}
