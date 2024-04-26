using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutDomains;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutUtilities.Delegate;
using System;

namespace ApiProxies.Callbacks
{
    public abstract class SampleStatusBase : ApiCallbackEvent<SampleEswDomain>,
        IApiCallback<sample_status_callback>
    {
        protected SampleStatusBase(string threadName) : base(threadName)
        {
            Callback = DoCallback;
        }

        public sample_status_callback Callback { get; }

        private IntPtr CallbackArg1 { get; set; }

        private void DoCallback(IntPtr sampleDefPtr)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = sampleDefPtr;

                OnCallback();
            }
        }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<SampleEswDomain>(CallbackArg1.MarshalToSampleEswDomain());
        }
    }
}