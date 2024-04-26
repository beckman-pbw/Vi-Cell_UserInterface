using ScoutUI.Views.Dialogs;
using ScoutViewModels.ViewModels.CellTypes;

namespace ScoutUI.Views.Review
{
    public partial class SelectCellTypeView : Dialog
    {
        public SelectCellTypeView(SelectCellTypeViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}