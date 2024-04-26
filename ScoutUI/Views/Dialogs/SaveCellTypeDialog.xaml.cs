using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class SaveCellTypeDialog : Dialog
    {
        public SaveCellTypeDialog(SaveCellTypeViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
