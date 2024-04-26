using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class FilterSampleSetsDialog : Dialog
    {
        public FilterSampleSetsDialog(FilterSampleSetsViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
