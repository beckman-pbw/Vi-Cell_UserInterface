using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class InstrumentStatusDialog : Dialog
    {
        public InstrumentStatusDialog(InstrumentStatusDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
