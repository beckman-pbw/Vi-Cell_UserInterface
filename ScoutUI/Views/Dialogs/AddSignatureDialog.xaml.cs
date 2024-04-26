using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class AddSignatureDialog : Dialog
    {
        public AddSignatureDialog(AddSignatureViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
