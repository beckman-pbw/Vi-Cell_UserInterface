using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class SampleSetSettingsDialog : Dialog
    {
        public SampleSetSettingsDialog(SampleSetSettingsDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
