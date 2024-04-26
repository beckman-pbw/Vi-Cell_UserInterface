using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class ExpandedImageGraphDialog : Dialog
    {
        public ExpandedImageGraphDialog(ExpandedImageGraphViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
