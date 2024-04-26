using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class SetFocusDialog : Dialog
    {
        public SetFocusDialog(SetFocusDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
