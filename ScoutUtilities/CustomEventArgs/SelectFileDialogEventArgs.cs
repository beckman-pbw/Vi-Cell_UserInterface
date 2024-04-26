namespace ScoutUtilities.CustomEventArgs
{
    public class SelectFileDialogEventArgs : BaseDialogEventArgs
    {
        public string DefaultExt { get; set; }
        public string Filter { get; set; }
        public string InitialDirectory { get; set; }

        public string FullFilePathSelected { get; set; }

        public SelectFileDialogEventArgs(string directory = "", string filter = "", string defaultExt = "")
        {
            InitialDirectory = directory;
            Filter = string.IsNullOrEmpty(filter) ? "All Files (*.*)|*.*" : filter; // todo: localize
            DefaultExt = defaultExt;
            
            FullFilePathSelected = string.Empty;
        }
    }
}