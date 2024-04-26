using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class DialogBox : Dialog
    {
        public DialogBox(DialogBoxViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
