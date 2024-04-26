using System;
using System.Threading;
using ScoutUtilities.Enums;
using System.Windows;

namespace ScoutUtilities.CustomEventArgs
{
    public enum DialogButtons
    {
        Ok,
        OkCancel,
        YesNo,
        Continue,
        ContinueWorking,
    }

    public class DialogBoxEventArgs : BaseDialogEventArgs
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string AcceptButtonText { get; set; }
        public string DeclineButtonText { get; set; }
        public DialogButtons DialogButtons { get; set; }
        public MessageBoxImage Icon { get; set; }
        public bool ShowTextEntry { get; set; }
        public bool ShowCheckBox { get; set; }
        public string TextEntered { get; set; }

        public string MathFormula { get; set; }
        public bool DeleteAllACupData { get; set; }

        /// <summary>
        /// Arguments used in the creation of a DialogBox
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title">LanguageResourceHelper.Get("LID_Title_ViCellBlu") is used if null</param>
        /// <param name="buttons"></param>
        /// <param name="acceptButtonText">Default text is based on the DialogButtons parameter if kept NULL</param>
        /// <param name="declineButtonText">Default text is based on the DialogButtons parameter if kept NULL</param>
        /// <param name="icon"></param>
        /// <param name="dialogLocation"></param>
        /// <param name="isModel"></param>
        /// <param name="onCloseCallback"></param>
        /// <param name="token"></param>
        /// <param name="showTextEntry">True: Show a text entry textbox in the dialogbox</param>
        /// <param name="initialTextForEntry">The initial text for the entry textbox</param>
        public DialogBoxEventArgs(string message, string title = null, DialogButtons buttons = DialogButtons.Ok,
            string acceptButtonText = null, string declineButtonText = null, MessageBoxImage icon = MessageBoxImage.None,
            DialogLocation dialogLocation = DialogLocation.CenterApp, bool isModel = true, Action<bool?> onCloseCallback = null,
            CancellationToken? token = null, bool showTextEntry = false, string initialTextForEntry = null,
            string mathFormula = null, bool showCheckBox = false)
            : base(isModel, dialogLocation, onCloseCallbackAction: onCloseCallback, cancellationToken: token)
        {
            Title = title;
            Message = message;
            DialogButtons = buttons;
            AcceptButtonText = acceptButtonText;
            DeclineButtonText = declineButtonText;
            Icon = icon;
            DialogLocation = dialogLocation;
            ShowTextEntry = showTextEntry;
            TextEntered = initialTextForEntry;
            MathFormula = mathFormula;
            ShowCheckBox = showCheckBox;

            SizeToContent = true;
        }
    }
}
