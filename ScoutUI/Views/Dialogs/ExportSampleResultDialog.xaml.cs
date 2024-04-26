using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class ExportSampleResultDialog : Dialog
    {
        public ExportSampleResultDialog(ExportSampleResultViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
