using System.Windows;
using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class EditSecuritySettingsDialog : Dialog
    {
        private EditSecuritySettingsDialogViewModel _viewModel => (EditSecuritySettingsDialogViewModel) Context;
        public EditSecuritySettingsDialog(EditSecuritySettingsDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.RadioButton radioButton)
            {
                _viewModel.SetSelectedSecurityType(radioButton.Content.ToString());
            }
        }
    }
}
