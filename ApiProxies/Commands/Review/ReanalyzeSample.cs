using System;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Structs;

namespace ApiProxies.Commands.Review
{
    public class ReanalyzeSample : ApiCommand<uuidDLL, UInt32, UInt32, bool, IApiCallback<sample_analysis_callback>>
    {
        public ReanalyzeSample(uuidDLL sampleId, UInt32 cellTypeIndex, UInt32 analysisIndex,
            bool fromImages)
        {
            ManagesMemory = false;
            // Get a api callback instance to be supplied to the single argument to the API call.
            Arguments = new Tuple<uuidDLL, UInt32, UInt32, bool, IApiCallback<sample_analysis_callback>>(
                sampleId,
                cellTypeIndex,
                analysisIndex,
                fromImages,
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Sample_Analysis) as
                    IApiCallback<sample_analysis_callback>);
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Review.ReanalyzeSampleAPI(Arguments.Item1, Arguments.Item2, Arguments.Item3, Arguments.Item4, Arguments.Item5.Callback);
        }
    }
}
