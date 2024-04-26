using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ScoutUtilities.Events
{
    public class MessageBus
    {
        #region Singleton Stuff

        private MessageBus()
        {
            _subscriber = new Dictionary<Type, IList>();
        }

        private static MessageBus _default;
        public static MessageBus Default
        {
            get
            {
                if (_default == null) _default = new MessageBus();
                return _default;
            }
        }

        #endregion

        #region Properties

        private readonly object _subLock = new object();

        private Dictionary<Type, IList> _subscriber;

        #endregion

        #region Methods

        public void Publish<TMessageType>(TMessageType message)
        {
            var t = typeof(TMessageType);
            if (!_subscriber.ContainsKey(t)) return;

            IList<Subscription<TMessageType>> subList = null;

            lock (_subLock)
            {
                if (!_subscriber.ContainsKey(t)) return; // just in case it was removed after lock was established
                subList = new List<Subscription<TMessageType>>(_subscriber[t].Cast<Subscription<TMessageType>>());
            }

            foreach (var sub in subList)
            {
                sub.CreateAction()?.Invoke(message);
            }
        }

        public Subscription<TMessageType> Subscribe<TMessageType>(Action<TMessageType> action)
        {
            var t = typeof(TMessageType);
            var actionDetail = new Subscription<TMessageType>(action, this);
            lock (_subLock)
            {
                if (!_subscriber.TryGetValue(t, out var actionList))
                {
                    actionList = new List<Subscription<TMessageType>>();
                    actionList.Add(actionDetail);
                    _subscriber.Add(t, actionList);
                }
                else
                {
                    actionList.Add(actionDetail);
                }
            }

            return actionDetail;
        }

        public void UnSubscribe<TMessageType>(ref Subscription<TMessageType> subscription)
        {
            if (subscription == null)
                return;
            var t = typeof(TMessageType);

            if (_subscriber.ContainsKey(t))
            {
                lock (_subLock)
                {
                    if (!_subscriber.ContainsKey(t)) return; // just in case it was removed after lock was established
                    _subscriber[t].Remove(subscription);
                }

                subscription = null;
            }
        }

        #endregion
    }
}