using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;

namespace ApiProxies.Callbacks
{
    public class StartProcessing : ApiCallbackEvent, IApiCallback<start_Processing>
    {
        public StartProcessing() : base(typeof(StartWorkQueue).Name)
        {
            EventType = ApiEventType.Start_Processing;
            Callback = DoCallback;
        }

        public start_Processing Callback { get; }

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