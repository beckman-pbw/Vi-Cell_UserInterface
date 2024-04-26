using System;
using System.Diagnostics.CodeAnalysis;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;

namespace ApiProxies.Callbacks
{
    public class ReagentUnloadStatus : ApiCallbackEvent<ReagentUnloadSequence>, IApiCallback<reagent_unload_status_callback>
    {
        public ReagentUnloadStatus() : base(typeof(ReagentUnloadStatus).Name)
        {
            EventType = ApiEventType.Reagent_Unload_Status;
            Callback = DoCallback;
        }

        /// <summary>
        /// Gets a delegate to the callback handling method of this IApiCallback.
        /// </summary>
        public reagent_unload_status_callback Callback { get; }

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