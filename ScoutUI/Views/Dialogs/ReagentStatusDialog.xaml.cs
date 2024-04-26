using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class ReagentStatusDialog : Dialog
    {
        public ReagentStatusDialog(ReagentStatusDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
