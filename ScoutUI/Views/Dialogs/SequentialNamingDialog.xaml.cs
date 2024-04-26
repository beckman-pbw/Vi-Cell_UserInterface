using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class SequentialNamingDialog : Dialog
    {
        public SequentialNamingDialog(SequentialNamingDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
