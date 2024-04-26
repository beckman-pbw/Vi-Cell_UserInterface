namespace ScoutUtilities.CustomEventArgs
{
    public class AddCellTypeEventArgs : BaseDialogEventArgs
    {
        public int CurrentNumberOfQualityControls { get; set; }
        public string NewQualityControlName { get; set; }

        public AddCellTypeEventArgs(int currentNumberOfQualityControls)
        {
            CurrentNumberOfQualityControls = currentNumberOfQualityControls;
            NewQualityControlName = string.Empty;
        }
    }
}