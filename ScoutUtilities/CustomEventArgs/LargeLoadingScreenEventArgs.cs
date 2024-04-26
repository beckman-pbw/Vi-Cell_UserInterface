using ScoutLanguageResources;
using System.Threading;

namespace ScoutUtilities.CustomEventArgs
{
    public class LargeLoadingScreenEventArgs : BaseDialogEventArgs
    {
        public bool IsEnabled { get; set; }
        public string WaitMsg { get; set; }
        public string LoadingMessage { get; set; }

        public LargeLoadingScreenEventArgs(string loadingMessage, string waitText = null, bool isEnabled = true, CancellationToken? token = null)
            : base(true, onCloseCallbackAction: null, cancellationToken: token)
        {
            LoadingMessage = loadingMessage;
            WaitMsg = string.IsNullOrEmpty(waitText) ? LanguageResourceHelper.Get("LID_Label_WaitMsg") : waitText;
            IsEnabled = isEnabled;

            SizeToContent = true;
        }
    }
}
