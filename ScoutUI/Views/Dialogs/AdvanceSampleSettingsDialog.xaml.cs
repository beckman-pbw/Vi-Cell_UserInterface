using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class AdvanceSampleSettingsDialog : Dialog
    {
        public AdvanceSampleSettingsDialog(AdvanceSampleSettingsDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
