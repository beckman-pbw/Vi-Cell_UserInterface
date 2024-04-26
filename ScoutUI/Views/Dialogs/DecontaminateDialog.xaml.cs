using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class DecontaminateDialog : Dialog
    {
        public DecontaminateDialog(DecontaminateDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
