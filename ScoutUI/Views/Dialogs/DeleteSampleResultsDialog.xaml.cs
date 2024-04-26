using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class DeleteSampleResultsDialog : Dialog
    {
        public DeleteSampleResultsDialog(DeleteSampleResultsViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
