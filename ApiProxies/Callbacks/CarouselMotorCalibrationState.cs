using System;
using System.Diagnostics.CodeAnalysis;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;

namespace ApiProxies.Callbacks
{
    public class CarouselMotorCalibrationState : ApiCallbackEvent<CalibrationState>,
        IApiCallback<carousel_motor_calibration_state_callback>
    {
        public CarouselMotorCalibrationState() : base(typeof(CarouselMotorCalibrationState).Name)
        {
            EventType = ApiEventType.Carousel_Motor_Calibration_State;
            Callback = DoCallback;
        }

        // Gets a delegate to the callback handling method of this IApiCallback.
        public carousel_motor_calibration_state_callback Callback { get; }

        private CalibrationState CallbackArg1 { get; set; }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<CalibrationState>(CallbackArg1);
        }

        private void DoCallback(CalibrationState motorCalibrationState)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = motorCalibrationState;

                OnCallback();
            }
        }
    }
}