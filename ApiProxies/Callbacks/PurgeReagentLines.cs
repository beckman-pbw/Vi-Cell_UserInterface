using System;
using System.Diagnostics.CodeAnalysis;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;

namespace ApiProxies.Callbacks
{
    public class PurgeReagentLines : ApiCallbackEvent<ePurgeReagentLinesState>, IApiCallback<purge_reagentlines_callback>
    {
        public PurgeReagentLines() : base(typeof(PurgeReagentLines).Name)
        {
            EventType = ApiEventType.Purge_Reagent_Lines;
            Callback = DoCallback;
        }

        /// <summary>
        /// Gets a delegate to the callback handling method of this IApiCallback.
        /// </summary>
        public purge_reagentlines_callback Callback { get; }

        private ePurgeReagentLinesState CallbackArg1 { get; set; }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<ePurgeReagentLinesState>(CallbackArg1);
        }

        private void DoCallback(ePurgeReagentLinesState reagentLineStatus)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = reagentLineStatus;

                OnCallback();
            }
        }
    }
}