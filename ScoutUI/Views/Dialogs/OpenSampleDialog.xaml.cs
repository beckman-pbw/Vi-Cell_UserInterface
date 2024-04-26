using ScoutViewModels.ViewModels.Dialogs;
using System.Windows.Input;

namespace ScoutUI.Views.Dialogs
{
    public partial class OpenSampleDialog : Dialog
    {
        public OpenSampleDialog(OpenSampleDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }

        protected override void OnDialogKeyDown(object sender, KeyEventArgs e)
        {
            base.OnDialogKeyDown(sender, e);

            if (DataContext is OpenSampleDialogViewModel vm)
            {
                if (e.Key.Equals(Key.Enter) && vm.AcceptCommand?.CanExecute(null) == true)
                {
                    vm.AcceptCommand?.Execute(null);
                    e.Handled = true;
                }
            }
        }
    }
}
