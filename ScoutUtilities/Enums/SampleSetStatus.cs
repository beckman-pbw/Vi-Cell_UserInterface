namespace ScoutUtilities.Enums
{
    public enum SampleSetStatus
    {
        NoSampleSetStatus,
        SampleSetTemplate,
        Pending, // SampleSetNotRun
        SampleSetActive, // sample set is part of the actively running worklist, but none of the contained items may have been processed yet.
        SampleSetInProgress, // ???
        Running,
        Cancelled, // for sample sets 'removed' or 'skipped' from the processing list
        Complete,

        Paused, // defined/used only in GUI
    }
}