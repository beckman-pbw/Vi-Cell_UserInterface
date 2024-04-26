using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class InactivityDialog : Dialog
    {
        public InactivityDialog(InactivityDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
