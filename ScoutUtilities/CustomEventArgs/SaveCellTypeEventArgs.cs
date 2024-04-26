namespace ScoutUtilities.CustomEventArgs
{
    public class SaveCellTypeEventArgs : BaseDialogEventArgs
    {
        public string CellTypeName { get; set; }
        public bool EditingExistingCellType { get; set; }

        public SaveCellTypeEventArgs(string cellTypeName, bool editingExistingCellType)
        {
            CellTypeName = cellTypeName ?? string.Empty;
            EditingExistingCellType = editingExistingCellType;
            SizeToContent = true;
        }
    }
}