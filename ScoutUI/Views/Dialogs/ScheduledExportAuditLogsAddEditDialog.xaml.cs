using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for ScheduledExportAuditLogsAddEditDialog.xaml
    /// </summary>
    public partial class ScheduledExportAuditLogsAddEditDialog : Dialog
    {
        public ScheduledExportAuditLogsAddEditDialog(ScheduledExportAuditLogsAddEditDialogViewModel vm) : base (vm)
        {
            InitializeComponent();
        }
    }
}
