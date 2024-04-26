using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class DustReferenceDialog : Dialog
    {
        public DustReferenceDialog(DustReferenceDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
