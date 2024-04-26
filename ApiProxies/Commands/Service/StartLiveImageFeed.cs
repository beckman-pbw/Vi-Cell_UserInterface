using System;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;

namespace ApiProxies.Commands.Service
{
    public class StartLiveImageFeed : ApiCommand<IApiCallback<service_live_image_callback>>
    {
        public StartLiveImageFeed()
        {
            ManagesMemory = false;
            // Get a api callback instance to be supplied to the single argument to the API call.
            Arguments = new Tuple<IApiCallback<service_live_image_callback>>(
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Service_Live_Image) as
                    IApiCallback<service_live_image_callback>);
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Service.StartLiveImageFeedAPI(Arguments.Item1.Callback);
        }
    }
}
