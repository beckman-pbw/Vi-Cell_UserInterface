using System;
using System.Diagnostics.CodeAnalysis;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;

namespace ApiProxies.Callbacks
{
    public class ReagentLoadStatus : ApiCallbackEvent<ReagentLoadSequence>, IApiCallback<reagent_load_status_callback>
    {
        public ReagentLoadStatus() : base(typeof(ReagentLoadStatus).Name)
        {
            EventType = ApiEventType.Reagent_Load_Status;
            Callback = DoCallback;
        }

        /// <summary>
        /// Gets a delegate to the callback handling method of this IApiCallback.
        /// </summary>
        public reagent_load_status_callback Callback { get; }

        private ReagentLoadSequence CallbackArg1 { get; set; }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<ReagentLoadSequence>(CallbackArg1);
        }

        private void DoCallback(ReagentLoadSequence loadStatus)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = loadStatus;

                OnCallback();
            }
        }
    }
}