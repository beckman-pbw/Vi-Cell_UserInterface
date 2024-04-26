using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class SampleResultsDialog : Dialog
    {
        public SampleResultsDialog(SampleResultDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
