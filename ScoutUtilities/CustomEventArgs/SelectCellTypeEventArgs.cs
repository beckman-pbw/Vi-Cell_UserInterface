namespace ScoutUtilities.CustomEventArgs
{
    public class SelectCellTypeEventArgs : BaseDialogEventArgs
    {
        public string Header { get; set; }
        public bool IsDiagnosticAvailable { get; set; }
        public object SelectedCellTypeDomain { get; set; }

        public SelectCellTypeEventArgs(string header, bool isDiagnosticAvailable, object selectedCellTypeDomain = null)
        {
            Header = header;
            IsDiagnosticAvailable = isDiagnosticAvailable;
            SelectedCellTypeDomain = selectedCellTypeDomain;
            SizeToContent = true;
        }
    }
}