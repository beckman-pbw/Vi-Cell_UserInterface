using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class UserProfileDialog : Dialog
    {
        public UserProfileDialog(UserProfileDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
