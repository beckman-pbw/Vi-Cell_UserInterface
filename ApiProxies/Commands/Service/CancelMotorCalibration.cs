using System;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;

namespace ApiProxies.Commands.Service
{
    public class CancelMotorCalibration : ApiCommand<IApiCallback<cancel_motor_calibration_state_callback>>
    {
        public CancelMotorCalibration()
        {
            ManagesMemory = false;
            // Get a api callback instance to be supplied to the single argument to the API call.
            Arguments = new Tuple<IApiCallback<cancel_motor_calibration_state_callback>>(
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Motor_Calibration_Cancelled) as
                    IApiCallback<cancel_motor_calibration_state_callback>);
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Service.CancelCalibrationAPI(Arguments.Item1.Callback);
        }
    }
}
