using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class CreateSampleSetDialog : Dialog
    {
        public CreateSampleSetDialog(CreateSampleSetDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
