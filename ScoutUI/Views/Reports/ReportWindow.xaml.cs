using ScoutUI.Views.Dialogs;
using ScoutViewModels.ViewModels;

namespace ScoutUI.Views._6___Reports
{
    public partial class ReportWindow : Dialog
    {
        public ReportWindow(ReportWindowViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
