using System;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;

namespace ApiProxies.Commands.Reagent
{
    public class StartDecontaminateFlowCell : ApiCommand<IApiCallback<flowcell_decontaminate_status_callback>>
    {        
        public StartDecontaminateFlowCell()
        {
            ManagesMemory = false;

            // Get api callback instance to be supplied as argument to the API call.
            Arguments = new Tuple<IApiCallback<flowcell_decontaminate_status_callback>>(
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Flowcell_Decontaminate_Status) as
                    IApiCallback<flowcell_decontaminate_status_callback>);
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Reagent.StartDecontaminateFlowCellAPI(Arguments.Item1.Callback);
        }
    }
}