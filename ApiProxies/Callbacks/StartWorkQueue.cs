using System.Diagnostics.CodeAnalysis;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;

namespace ApiProxies.Callbacks
{
    public class StartWorkQueue : ApiCallbackEvent, IApiCallback<start_WorkQueue>
    {
        public StartWorkQueue() : base(typeof(StartWorkQueue).Name)
        {
            EventType = ApiEventType.Start_WorkQueue;
            Callback = DoCallback;
        }

        public start_WorkQueue Callback { get; }

        protected override void MarshalArgsToMembers()
        {
            //nothing to do
        }

        private void DoCallback()
        {
            lock (_callbackLock)
            {
                OnCallback();
            }
        }
    }
}