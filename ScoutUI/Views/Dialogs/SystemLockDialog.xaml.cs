using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class SystemLockDialog : Dialog
    {
        public SystemLockDialog(SystemLockDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
