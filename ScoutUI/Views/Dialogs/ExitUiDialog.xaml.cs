using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class ExitUiDialog : Dialog
    {
        public ExitUiDialog(ExitUiDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
