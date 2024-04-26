using System;

namespace ScoutUtilities.Events
{
    public class BaseNotification
    {
        /// <summary>
        /// This is the classification of the notification you are sending. Only interested parties will be listening.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// If you need to relay a specific message to the listeners, you can add more info here.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Action to use if you need a callback once the message has been received and processed.
        /// </summary>
        public Action CallbackAction { get; set; }

        public BaseNotification(string tokenStr, string message = null, Action callbackAction = null)
        {
            Token = tokenStr;
            Message = message;
        }
    }

    public class Notification : BaseNotification
    {
        public Notification(string tokenStr, string message = null, Action callbackAction = null)
            : base(tokenStr, message, callbackAction)
        {

        }
    }

    public class Notification<T> : BaseNotification
    {
        /// <summary>
        /// If your Notification needs to have a referenced object, this is it.
        /// </summary>
        public T Target { get; set; }

        /// <summary>
        /// Action to use if you need a callback once the message has been received and processed.
        /// </summary>
        public new Action<T> CallbackAction { get; set; }

        public Notification(string tokenStr, string message = null) : base(tokenStr, message)
        {
            Target = default(T);
        }

        public Notification(string tokenStr, string message, Action<T> callbackAction) : base(tokenStr, message)
        {
            Target = default(T);
            CallbackAction = callbackAction;
        }

        public Notification(T target, string tokenStr, string message = null) : base(tokenStr, message)
        {
            Target = target;
        }

        public Notification(T target, string tokenStr, string message, Action<T> callbackAction) : base(tokenStr, message)
        {
            Target = target;
            CallbackAction = callbackAction;
        }
    }
}
