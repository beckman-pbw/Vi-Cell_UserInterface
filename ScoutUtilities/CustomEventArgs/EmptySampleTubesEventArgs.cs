namespace ScoutUtilities.CustomEventArgs
{
    public class EmptySampleTubesEventArgs : BaseDialogEventArgs
    {
        public int? TubeCapacity { get; set; } // NULL means do not show Tube Capacity in the title bar

        public EmptySampleTubesEventArgs(int? tubeCapacity = null)
        {
            TubeCapacity = tubeCapacity;
        }
    }
}