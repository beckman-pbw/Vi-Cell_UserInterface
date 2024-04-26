using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for ScheduledExportAddEditDialog.xaml
    /// </summary>
    public partial class ScheduledExportAddEditDialog : Dialog
    {
        public ScheduledExportAddEditDialog(ScheduledExportAddEditViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
