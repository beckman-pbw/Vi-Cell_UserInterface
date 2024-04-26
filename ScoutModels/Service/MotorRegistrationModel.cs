using ApiProxies;
using ApiProxies.Commands.Service;
using ApiProxies.Misc;
using JetBrains.Annotations;
using ScoutModels.Home.QueueManagement;
using ScoutUtilities.Enums;
using System;

namespace ScoutModels.Service
{
    public class MotorRegistrationModel
    {
        public event EventHandler<ApiEventArgs<CalibrationState>> CarouselMotorCalibrationStateChanged;

        public event EventHandler<ApiEventArgs<CalibrationState>> PlateMotorCalibrationStateChanged;

        public event EventHandler<ApiEventArgs<CalibrationState>> MotorCalibrationCancelled;

        public MotorRegistrationModel()
        {
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError MoveProbe(bool upDown)
        {
            return LowLevelModel.svc_MoveProbe(upDown);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetProbePosition(bool upDown, uint stepsToMove)
        {
            return LowLevelModel.svc_SetProbePosition(upDown, stepsToMove);
        }

        public static void SetSampleWellPosition(char row, uint pos)
        {
            var _ = LowLevelModel.svc_SetSampleWellPosition(row, pos);
        }

        [MustUseReturnValue("Use HawkeyeError")] 
        public HawkeyeError PerformCarouselMotorCalibration()
        {
            ApiEventBroker.Instance.Subscribe<CalibrationState>(ApiEventType.Carousel_Motor_Calibration_State, HandleCarouselMotorCalibrationStateChanged);
            var apiCommand = new PerformCarouselCalibration();
            return apiCommand.Invoke();
        }

        [MustUseReturnValue("Use HawkeyeError")] 
        public HawkeyeError PerformPlateMotorCalibration()
        {
            ApiEventBroker.Instance.Subscribe<CalibrationState>(ApiEventType.Plate_Motor_Calibration_State, HandlePlateMotorCalibrationStateChanged);
            var apiCommand = new PerformPlateCalibration();
            return apiCommand.Invoke();
        }

        [MustUseReturnValue("Use HawkeyeError")] 
        public HawkeyeError CancelMotorCalibration()
        {
            ApiEventBroker.Instance.Subscribe<CalibrationState>(ApiEventType.Motor_Calibration_Cancelled, HandleCancelMotorCalibrationStateChanged);
            var apiCommand = new CancelMotorCalibration();
            return apiCommand.Invoke();
        }

        private void HandleCarouselMotorCalibrationStateChanged(object sender, ApiEventArgs<CalibrationState> e)
        {
            CarouselMotorCalibrationStateChanged?.Invoke(this, e);
            if (e.Arg1 == CalibrationState.eCompleted || e.Arg1 == CalibrationState.eFault)
            {
                ApiEventBroker.Instance.Unsubscribe<CalibrationState>(ApiEventType.Carousel_Motor_Calibration_State, HandleCarouselMotorCalibrationStateChanged);
            }
        }

        private void HandlePlateMotorCalibrationStateChanged(object sender, ApiEventArgs<CalibrationState> e)
        {
            PlateMotorCalibrationStateChanged?.Invoke(this, e);
            if (e.Arg1 == CalibrationState.eCompleted || e.Arg1 == CalibrationState.eFault)
            {
                ApiEventBroker.Instance.Unsubscribe<CalibrationState>(ApiEventType.Plate_Motor_Calibration_State, HandlePlateMotorCalibrationStateChanged);
            }
        }

        private void HandleCancelMotorCalibrationStateChanged(object sender, ApiEventArgs<CalibrationState> e)
        {
            MotorCalibrationCancelled?.Invoke(this, e);
            if (e.Arg1 == CalibrationState.eCompleted || e.Arg1 == CalibrationState.eFault)
            {
                ApiEventBroker.Instance.Unsubscribe<CalibrationState>(ApiEventType.Motor_Calibration_Cancelled, HandleCancelMotorCalibrationStateChanged);
            }
        }
    }
}
