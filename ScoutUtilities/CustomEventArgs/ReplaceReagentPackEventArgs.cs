namespace ScoutUtilities.CustomEventArgs
{
    public class ReplaceReagentPackEventArgs : BaseDialogEventArgs
    {
        public string ReagentPartNumber { get; set; }

        public ReplaceReagentPackEventArgs(string reagentPartNumber)
        {
            ReagentPartNumber = reagentPartNumber;
            SizeToContent = true;
        }
    }
}