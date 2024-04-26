namespace ScoutUtilities.CustomEventArgs
{
    public class ExitUiDialogEventArgs : BaseDialogEventArgs
    {
        public string ExitAndNightCleanMessage { get; set; }

        public ExitUiDialogEventArgs(string message) : base(false)
        {
            ExitAndNightCleanMessage = message;
            CloseButtonSize = 0;
        }
    }
}