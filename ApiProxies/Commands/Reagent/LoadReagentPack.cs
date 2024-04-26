using System;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;

namespace ApiProxies.Commands.Reagent
{
    public class LoadReagentPack : ApiCommand<IApiCallback<reagent_load_status_callback>,
        IApiCallback<reagent_load_complete_callback>>
    {
        public LoadReagentPack()
        {
            ManagesMemory = false;

            // Get api callback instances to be supplied as arguments to the API call.
            Arguments =
                new Tuple<IApiCallback<reagent_load_status_callback>, IApiCallback<reagent_load_complete_callback>>(
                    ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Reagent_Load_Status) as
                        IApiCallback<reagent_load_status_callback>,
                    ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Reagent_Load_Complete) as
                        IApiCallback<reagent_load_complete_callback>);
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Reagent.LoadReagentPackAPI(Arguments.Item1.Callback, Arguments.Item2.Callback);
        }
    }
}