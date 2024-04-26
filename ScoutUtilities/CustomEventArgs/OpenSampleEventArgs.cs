using ScoutUtilities.Enums;

namespace ScoutUtilities.CustomEventArgs
{
    public class OpenSampleEventArgs : BaseDialogEventArgs
    {
        public object SelectedSampleRecord { get; set; }

        public OpenSampleEventArgs()
        {
            SizeToContent = true;
        }
    }
}