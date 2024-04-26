using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class ChangePasswordDialog : Dialog
    {
        public ChangePasswordDialog(ChangePasswordViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
