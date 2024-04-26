using ScoutUtilities;

namespace ScoutViewModels.ViewModels.Interfaces
{
    public interface IScheduledExportsViewModel
    {
        string ExportFilenameColumnHeader { get; }
        string ReportTitle { get; }
        RelayCommand AddScheduledExport { get; }
        RelayCommand EditScheduledExport { get; }
        RelayCommand DeleteScheduledExport { get; }
    }
}