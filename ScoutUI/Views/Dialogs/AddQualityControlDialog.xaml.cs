using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class AddCellTypeDialog : Dialog
    {
        public AddCellTypeDialog(AddQualityControlDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
