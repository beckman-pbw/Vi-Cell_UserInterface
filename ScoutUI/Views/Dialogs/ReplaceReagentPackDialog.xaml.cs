using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class ReplaceReagentPackDialog : Dialog
    {
        public ReplaceReagentPackDialog(ReplaceReagentPackDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
