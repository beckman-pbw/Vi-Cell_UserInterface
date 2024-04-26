using System;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;

namespace ApiProxies.Commands.Reagent
{
    public class StartCleanFluidics : ApiCommand<IApiCallback<flowcell_flush_status_callback>>
    {
        public StartCleanFluidics()
        {
            ManagesMemory = false;

            // Get api callback instance to be supplied as argument to the API call.
            Arguments = new Tuple<IApiCallback<flowcell_flush_status_callback>>(
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Flowcell_Flush_Status) as
                    IApiCallback<flowcell_flush_status_callback>);
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Reagent.StartCleanFluidicsAPI(Arguments.Item1.Callback);
        }
    }
}
