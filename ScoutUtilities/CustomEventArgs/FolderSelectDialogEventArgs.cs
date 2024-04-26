using System;

namespace ScoutUtilities.CustomEventArgs
{
    public class FolderSelectDialogEventArgs : BaseDialogEventArgs
    {
        public string SelectedFolderPath { get; set; }
        public string DialogDescription { get; set; }
        public bool ShowNewFolderButton { get; set; }

        /// <summary>
        /// Used in case the caller wants to know what went wrong.
        /// </summary>
        public Exception ThrownException { get; set; }

        public FolderSelectDialogEventArgs(string initialSelectedFolderPath, bool showNewFolderButton = true, string dialogDescription = null)
        {
            SelectedFolderPath = initialSelectedFolderPath;
            ShowNewFolderButton = showNewFolderButton;
            DialogDescription = dialogDescription ?? string.Empty;
            ThrownException = null;
        }
    }
}