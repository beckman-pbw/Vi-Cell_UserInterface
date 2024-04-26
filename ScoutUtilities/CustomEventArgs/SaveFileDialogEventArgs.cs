namespace ScoutUtilities.CustomEventArgs
{
    public class SaveFileDialogEventArgs : BaseDialogEventArgs
    {
        public string FullFilePathSelected { get; set; }

        public string DefaultFileName { get; set; }
        public string InitialDirectory { get; set; }
        public string Filter { get; set; }
        public string DefaultExt { get; set; }
        public bool OverwritePrompt { get; set; }

        public SaveFileDialogEventArgs(string fileName, string directory = "", string filter = "", string defaultExt = "", bool overwritePrompt = true)
        {
            DefaultFileName = fileName;
            InitialDirectory = directory;
            Filter = string.IsNullOrEmpty(filter) ? "All Files (*.*)|*.*" : filter; // todo: localize
            DefaultExt = defaultExt;
            OverwritePrompt = overwritePrompt;

            FullFilePathSelected = string.Empty;
        }
    }
}