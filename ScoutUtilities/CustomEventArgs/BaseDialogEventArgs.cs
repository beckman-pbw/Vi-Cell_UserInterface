using System;
using System.Threading;
using ScoutUtilities.Enums;

namespace ScoutUtilities.CustomEventArgs
{
    public class BaseDialogEventArgs : EventArgs
    {
        public bool? DialogResult { get; set; }

        public bool IsModal { get; set; }
        public DialogLocation DialogLocation { get; set; }
        public string DialogIconPath { get; set; }
        public int CloseButtonSize { get; set; }
        public bool DialogIconIsSquare { get; set; }
        public bool SizeToContent { get; set; }
        public bool FadeBackground { get; set; }

        public CancellationToken? CancellationToken { get; }
        public Action<bool?> OnCloseCallbackAction { get; }
        
        public BaseDialogEventArgs(bool isModal = true, DialogLocation dialogLocation = DialogLocation.CenterApp, bool sizeToContent = false, int closeButtonSize = 40, 
            bool fadeBg = true, string dialogIconPath = null, bool dialogIconIsSquare = false, Action<bool?> onCloseCallbackAction = null, CancellationToken? cancellationToken = null)
        {
            IsModal = isModal;
            DialogLocation = dialogLocation;
            SizeToContent = sizeToContent;
            CloseButtonSize = closeButtonSize;
            FadeBackground = fadeBg;
            DialogIconPath = dialogIconPath;
            DialogIconIsSquare = dialogIconIsSquare;

            OnCloseCallbackAction = onCloseCallbackAction;
            CancellationToken = cancellationToken;
        }
    }
}
