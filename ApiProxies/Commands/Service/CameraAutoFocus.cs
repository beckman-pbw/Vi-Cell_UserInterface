using System;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Common;
using ScoutUtilities.Delegate;
using ScoutUtilities.Structs;

namespace ApiProxies.Commands.Service
{
    public class CameraAutoFocus : ApiCommand<SamplePosition, IApiCallback<autofocus_state_callback_t>,
        IApiCallback<countdown_timer_callback_t>>
    {
        public CameraAutoFocus(SamplePosition samplePosition)
        {
            ManagesMemory = false;
            // Get a api callback instance to be supplied to the single argument to the API call.
            Arguments =
                new Tuple<SamplePosition, IApiCallback<autofocus_state_callback_t>,
                    IApiCallback<countdown_timer_callback_t>>(
                    samplePosition,
                    ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Autofocus_State) as
                        IApiCallback<autofocus_state_callback_t>,
                    ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Autofocus_Countdown_Timer) as
                        IApiCallback<countdown_timer_callback_t>);
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Service.CameraAutoFocusAPI(Arguments.Item1, Arguments.Item2.Callback, Arguments.Item3.Callback);
        }
    }
}