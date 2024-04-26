using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class InfoDialog : Dialog
    {
        public InfoDialog(InfoDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
