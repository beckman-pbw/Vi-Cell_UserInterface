using ScoutUtilities.Common;
using System;
using System.Collections.Generic;
using ScoutUtilities.Enums;

namespace ScoutUtilities.CustomEventArgs
{
    public class MessageHubEventArgs : BaseDialogEventArgs
    {
        public List<HubMessage> Messages { get; set; }

        public MessageHubEventArgs(List<HubMessage> messages, Action<bool?> onCloseCallback = null)
            : base(false, onCloseCallbackAction: onCloseCallback, dialogLocation: DialogLocation.MessageHubLocation, sizeToContent: true, fadeBg: false)
        {
            Messages = messages;
        }
    }
}
