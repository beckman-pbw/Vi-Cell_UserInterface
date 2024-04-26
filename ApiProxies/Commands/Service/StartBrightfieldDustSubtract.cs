using ApiProxies.Callbacks;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using System;
using System.Diagnostics;

namespace ApiProxies.Commands.Service
{
    public class StartBrightfieldDustSubtract : ApiCommand<IApiCallback<brightfield_dustsubtraction_callback>>
    {
        public StartBrightfieldDustSubtract() : this(null, null)
        {
        }

        public StartBrightfieldDustSubtract(string imageNamePrefix, string imageDirPath)
        {
            ManagesMemory = false;
            // Get a api callback instance to be supplied to the single argument to the API call.
            Arguments = new Tuple<IApiCallback<brightfield_dustsubtraction_callback>>(
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Brightfield_Dust_Subtraction) as
                    IApiCallback<brightfield_dustsubtraction_callback>);

            var dustCallback = Arguments.Item1 as BrightfieldDustSubtraction;
            Debug.Assert(dustCallback != null);
            dustCallback.DustImagesFilePrefix = imageNamePrefix;
            dustCallback.DustImagesDirPath = imageDirPath;
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Service.StartBrightfieldDustSubtractAPI(Arguments.Item1.Callback);
        }
    }
}