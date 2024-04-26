using System;
using System.Diagnostics.CodeAnalysis;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;

namespace ApiProxies.Callbacks
{
    public class ReagentUnloadComplete : ApiCallbackEvent<ReagentUnloadSequence>, IApiCallback<reagent_unload_complete_callback>
    {
        public ReagentUnloadComplete() : base(typeof(ReagentUnloadComplete).Name)
        {
            EventType = ApiEventType.Reagent_Unload_Complete;
            Callback = DoCallback;
        }

        /// <summary>
        /// Gets a delegate to the callback handling method of this IApiCallback.
        /// </summary>
        public reagent_unload_complete_callback Callback { get; }

        private ReagentUnloadSequence CallbackArg1 { get; set; }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<ReagentUnloadSequence>(CallbackArg1);
        }

        private void DoCallback(ReagentUnloadSequence unloadStatus)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = unloadStatus;

                OnCallback();
            }
        }
    }
}