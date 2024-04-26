using System;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;

namespace ApiProxies.Commands.Service
{
    public class GenerateContinuousAnalysis : ApiCommand<IApiCallback<service_analysis_result_callback>>
    {
        public GenerateContinuousAnalysis()
        {
            ManagesMemory = false;

            // Get a api callback instance to be supplied to the single argument to the API call.
            Arguments = new Tuple<IApiCallback<service_analysis_result_callback>>(
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Service_Analysis_Result) as
                    IApiCallback<service_analysis_result_callback>);
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Service.GenerateContinuousAnalysisAPI(Arguments.Item1.Callback);
        }
    }
}