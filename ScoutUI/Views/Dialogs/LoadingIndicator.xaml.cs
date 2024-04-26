using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class LoadingIndicator : Dialog
    {
        public LoadingIndicator(LoadingIndicatorViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
