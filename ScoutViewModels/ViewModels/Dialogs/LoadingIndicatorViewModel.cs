using ScoutUtilities.CustomEventArgs;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class LoadingIndicatorViewModel : BaseDialogViewModel
    {
        public bool IsEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string WaitMsg
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string LoadingMessage
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public LoadingIndicatorViewModel(LargeLoadingScreenEventArgs args, System.Windows.Window parentWindow) : base(args, parentWindow)
        {
            WaitMsg = args.WaitMsg;
            LoadingMessage = args.LoadingMessage;
            IsEnable = args.IsEnabled;
        }
    }
}
