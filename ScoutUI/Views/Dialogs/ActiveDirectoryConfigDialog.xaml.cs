using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class ActiveDirectoryConfigDialog : Dialog
    {
        public ActiveDirectoryConfigDialog(ActiveDirectoryConfigDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
