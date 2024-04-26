using System;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;

namespace ApiProxies.Commands.Reagent
{
    public class StartPrimeReagentLines : ApiCommand<IApiCallback<prime_reagentlines_callback>>
    {
        public StartPrimeReagentLines()
        {
            ManagesMemory = false;

            // Get api callback instance to be supplied as argument to the API call.
            Arguments = new Tuple<IApiCallback<prime_reagentlines_callback>>(
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Prime_Reagent_Lines) as
                    IApiCallback<prime_reagentlines_callback>);
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Reagent.StartPrimeReagentLinesAPI(Arguments.Item1.Callback);
        }
    }
}
