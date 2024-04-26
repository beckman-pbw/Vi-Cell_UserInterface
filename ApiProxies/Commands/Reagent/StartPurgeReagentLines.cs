using System;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;

namespace ApiProxies.Commands.Reagent
{
    public class StartPurgeReagentLines : ApiCommand<IApiCallback<purge_reagentlines_callback>>
    {
        public StartPurgeReagentLines()
        {
            ManagesMemory = false;

            // Get api callback instance to be supplied as argument to the API call.
            Arguments = new Tuple<IApiCallback<purge_reagentlines_callback>>(
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Purge_Reagent_Lines) as
                    IApiCallback<purge_reagentlines_callback>);
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Reagent.StartPurgeReagentLinesAPI(Arguments.Item1.Callback);
        }
    }
}
