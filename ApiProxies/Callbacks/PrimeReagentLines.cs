using System;
using System.Diagnostics.CodeAnalysis;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;

namespace ApiProxies.Callbacks
{
    public class PrimeReagentLines : ApiCallbackEvent<ePrimeReagentLinesState>, IApiCallback<prime_reagentlines_callback>
    {
        public PrimeReagentLines() : base(typeof(PrimeReagentLines).Name)
        {
            EventType = ApiEventType.Prime_Reagent_Lines;
            Callback = DoCallback;
        }

        /// <summary>
        /// Gets a delegate to the callback handling method of this IApiCallback.
        /// </summary>
        public prime_reagentlines_callback Callback { get; }

        private ePrimeReagentLinesState CallbackArg1 { get; set; }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<ePrimeReagentLinesState>(CallbackArg1);
        }

        private void DoCallback(ePrimeReagentLinesState reagentLineStatus)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = reagentLineStatus;

                OnCallback();
            }
        }
    }
}