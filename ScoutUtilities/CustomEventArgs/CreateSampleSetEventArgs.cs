using ScoutUtilities.Enums;

namespace ScoutUtilities.CustomEventArgs
{
    public class CreateSampleSetEventArgs<T> : BaseDialogEventArgs
    {
        public T NewSampleSet { get; set; }
        public string CreatedByUser { get; set; }
        public string RunByUser { get; set; }
        public string SampleSetName { get; set; }
        public SampleSetStatus Status { get; set; }
        public bool AllowSubstrateToChange { get; set; }
        public SubstrateType InitialSubstrateType { get; set; }
        public WorkListStatus WorkListStatus { get; set; }

        public CreateSampleSetEventArgs(T newSampleSet, string createdByUser, string runByUser, string sampleSetName,
            SampleSetStatus status, bool allowSubstrateToChange, SubstrateType initialSubstrateType, WorkListStatus wlStatus)
        {
            IsModal = true;
            DialogLocation = DialogLocation.CenterApp;
            CloseButtonSize = 50;
            SizeToContent = false;
            FadeBackground = true;

            NewSampleSet = newSampleSet;
            CreatedByUser = createdByUser;
            RunByUser = runByUser;
            SampleSetName = sampleSetName;
            Status = status;
            AllowSubstrateToChange = allowSubstrateToChange;
            InitialSubstrateType = initialSubstrateType;
            WorkListStatus = wlStatus;
        }
    }
}