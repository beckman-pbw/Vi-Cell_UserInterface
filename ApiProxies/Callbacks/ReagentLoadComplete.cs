using System;
using System.Diagnostics.CodeAnalysis;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;

namespace ApiProxies.Callbacks
{
    public class ReagentLoadComplete : ApiCallbackEvent<ReagentLoadSequence>, IApiCallback<reagent_load_complete_callback>
    {
        public ReagentLoadComplete() : base(typeof(ReagentLoadComplete).Name)
        {
            EventType = ApiEventType.Reagent_Load_Complete;
            Callback = DoCallback;
        }

        /// <summary>
        /// Gets a delegate to the callback handling method of this IApiCallback.
        /// </summary>
        public reagent_load_complete_callback Callback { get; }

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