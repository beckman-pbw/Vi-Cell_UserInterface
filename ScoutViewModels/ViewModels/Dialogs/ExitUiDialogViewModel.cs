using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class ExitUiDialogViewModel : BaseDialogViewModel
    {
        public ExitUiDialogViewModel(ExitUiDialogEventArgs args, System.Windows.Window parentWindow) : base(args, parentWindow)
        {
            ShowDialogTitleBar = false;
            ExitAndNightCleanMessage = args.ExitAndNightCleanMessage;
            DialogLocation = DialogLocation.CenterScreen;
        }

        public string ExitAndNightCleanMessage
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
    }
}