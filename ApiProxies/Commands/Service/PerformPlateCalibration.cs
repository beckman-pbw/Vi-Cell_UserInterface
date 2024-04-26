using System;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;

namespace ApiProxies.Commands.Service
{
    public class PerformPlateCalibration : ApiCommand<IApiCallback<plate_motor_calibration_state_callback>>
    {
        public PerformPlateCalibration()
        {
            ManagesMemory = false;
            // Get a api callback instance to be supplied to the single argument to the API call.
            Arguments = new Tuple<IApiCallback<plate_motor_calibration_state_callback>>(
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.Plate_Motor_Calibration_State) as
                    IApiCallback<plate_motor_calibration_state_callback>);
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Service.PerformPlateCalibrationAPI(Arguments.Item1.Callback);
        }
    }
}