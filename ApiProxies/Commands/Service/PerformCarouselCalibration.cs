using System;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;

namespace ApiProxies.Commands.Service
{
    public class PerformCarouselCalibration : ApiCommand<IApiCallback<carousel_motor_calibration_state_callback>>
    {
        public PerformCarouselCalibration()
        {
            ManagesMemory = false;
            // Get a api callback instance to be supplied to the single argument to the API call.
            Arguments = new Tuple<IApiCallback<carousel_motor_calibration_state_callback>>(
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Carousel_Motor_Calibration_State) as
                    IApiCallback<carousel_motor_calibration_state_callback>);
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Service.PerformCarouselCalibrationAPI(Arguments.Item1.Callback);
        }
    }
}