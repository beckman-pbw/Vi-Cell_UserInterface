using System;
using System.Diagnostics.CodeAnalysis;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;

namespace ApiProxies.Callbacks
{
    public class CancelMotorCalibrationState : ApiCallbackEvent<CalibrationState>,
        IApiCallback<cancel_motor_calibration_state_callback>
    {
        public CancelMotorCalibrationState() : base(typeof(CancelMotorCalibrationState).Name)
        {
            EventType = ApiEventType.Motor_Calibration_Cancelled;
            Callback = DoCallback;
        }

        /// <summary>
        /// Gets a delegate to the callback handling method of this IApiCallback.
        /// </summary>
        public cancel_motor_calibration_state_callback Callback { get; }

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