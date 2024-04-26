using System;
using System.Runtime.InteropServices;
using ScoutUtilities.Structs;
using JetBrains.Annotations;
using ScoutUtilities.Enums;
using ScoutUtilities.Delegate;
using ScoutUtilities.Common;

namespace HawkeyeCoreAPI
{
    public static partial class Service
    {
        #region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_CameraAutoFocus(SamplePosition focusbead_location,
            [MarshalAs(UnmanagedType.FunctionPtr)] autofocus_state_callback_t on_status_change,
            [MarshalAs(UnmanagedType.FunctionPtr)] countdown_timer_callback_t on_timer_tick);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_CameraAutoFocus_FocusAcceptance(eAutofocusCompletion decision);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_CameraAutoFocus_Cancel();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_CameraAutoFocus_ServiceSkipDelay();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError StartBrightfieldDustSubtract([MarshalAs(UnmanagedType.FunctionPtr)] brightfield_dustsubtraction_callback on_status_change);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError AcceptDustReference(bool accepted);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError CancelBrightfieldDustSubtract();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_SetCameraLampState(bool lamp_on, float intensity_0_to_100);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_SetCameraLampIntensity(float intensity_0_to_100);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_GetCameraLampState(out bool lamp_on, out float intensity_0_to_100);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_GenerateSingleShotAnalysis(
        [MarshalAs(UnmanagedType.FunctionPtr)] service_analysis_result_callback callback);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_GenerateContinuousAnalysis([MarshalAs(UnmanagedType.FunctionPtr)] service_analysis_result_callback callback);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_StopContinuousAnalysis();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_ManualSample_Load();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_ManualSample_Nudge();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_ManualSample_Expel();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_StartLiveImageFeed([MarshalAs(UnmanagedType.FunctionPtr)] service_live_image_callback callback);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_StopLiveImageFeed();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_SetProbePostion(bool upDown, UInt32 stepsToMove);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_MoveProbe(bool upDown);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_MoveReagentArm(bool upDown);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_DispenseSample(UInt32 volume);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_AspirateSample(UInt32 volume);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_SetSampleWellPosition(SamplePosition samplePos);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_GetProbePostion(out Int32 pos);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_GetValvePort(out char port);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_SetValvePort(char port);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_GetFlowCellDepthSetting(out UInt16 flow_cell_depth);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_SetFlowCellDepthSetting(UInt16 flow_cell_depth);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError InitializeCarrier();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_CameraFocusAdjust(bool direction_up, bool adjustment_fine);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_CameraFocusRestoreToSavedLocation();

        [DllImport("HawkeyeCore.dll")]
        static extern void svc_FreeAutofocusResults(out IntPtr results, byte num_result);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError svc_PerformPlateCalibration([MarshalAs(UnmanagedType.FunctionPtr)] plate_motor_calibration_state_callback onMotorState);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_PerformCarouselCalibration([MarshalAs(UnmanagedType.FunctionPtr)] carousel_motor_calibration_state_callback onPerformCarouselChange);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_CancelCalibration([MarshalAs(UnmanagedType.FunctionPtr)] cancel_motor_calibration_state_callback onCalibStateChangeCb);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError EjectSampleStage(string username, string password);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RotateCarousel(out SamplePosition samplePos);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_GetSampleWellPosition(out SamplePosition pos);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError svc_SetSystemSerialNumber(string serial, string servicePassword);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetSystemSerialNumber(out IntPtr serialNumber);

	#endregion


	#region API_Calls

		[MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError CameraAutoFocusAPI(SamplePosition samplePosition,
        autofocus_state_callback_t on_status_change, countdown_timer_callback_t on_timer_tick)
        {
            return svc_CameraAutoFocus(samplePosition, on_status_change, on_timer_tick);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError PerformPlateCalibrationAPI(
            plate_motor_calibration_state_callback on_PerformPlateCalibration_state)
        {
            return svc_PerformPlateCalibration(on_PerformPlateCalibration_state);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError PerformCarouselCalibrationAPI(
            carousel_motor_calibration_state_callback on_motor_calibration_state)
        {
            return svc_PerformCarouselCalibration(on_motor_calibration_state);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError CancelCalibrationAPI(cancel_motor_calibration_state_callback on_CancelCalibration_state)
        {
            return svc_CancelCalibration(on_CancelCalibration_state);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError CameraAutoFocus_FocusAcceptanceAPI(eAutofocusCompletion decision)
        {
            return svc_CameraAutoFocus_FocusAcceptance(decision);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError CameraAutoFocus_CancelAPI()
        {
            return svc_CameraAutoFocus_Cancel();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError CameraFocusAdjustAPI(bool direction_up, bool adjustment_fine)
        {
            return svc_CameraFocusAdjust(direction_up, adjustment_fine);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError CameraFocusRestoreToSavedLocationAPI()
        {
            return svc_CameraFocusRestoreToSavedLocation();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError StartBrightfieldDustSubtractAPI(
        brightfield_dustsubtraction_callback on_Dustsubtraction_state)
        {
            return StartBrightfieldDustSubtract(on_Dustsubtraction_state);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError AcceptDustReferenceAPI(bool accepted)
        {
            return AcceptDustReference(accepted);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError CancelBrightfieldDustSubtractAPI()
        {
            return CancelBrightfieldDustSubtract();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ManualSample_LoadAPI()
        {
            return svc_ManualSample_Load();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ManualSample_NudgeAPI()
        {
            return svc_ManualSample_Nudge();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ManualSample_ExpelAPI()
        {
            return svc_ManualSample_Expel();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError StartLiveImageFeedAPI(service_live_image_callback on_service_live_image)
        {
            return svc_StartLiveImageFeed(on_service_live_image);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError StopLiveImageFeedAPI()
        {
            return svc_StopLiveImageFeed();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetProbePositionAPI(bool upDown, UInt32 stepsToMove)
        {
            return svc_SetProbePostion(upDown, stepsToMove);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError MoveProbeAPI(bool upDown)
        {
            return svc_MoveProbe(upDown);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError MoveReagentArmAPI(bool upDown)
        {
            return svc_MoveReagentArm(upDown);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError DispenseSampleAPI(UInt32 volume)
        {
            return svc_DispenseSample(volume);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError AspirateSampleAPI(UInt32 volume)
        {
            return svc_AspirateSample(volume);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetSampleWellPositionAPI(SamplePosition samplePos)
        {
            return svc_SetSampleWellPosition(samplePos);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetProbePositionAPI(out Int32 pos)
        {
            return svc_GetProbePostion(out pos);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetValvePortAPI(out char pos)
        {
            return svc_GetValvePort(out pos);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetValvePortAPI(char pos)
        {
            return svc_SetValvePort(pos);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError CameraAutoFocus_ServiceSkipDelayAPI()
        {
            return svc_CameraAutoFocus_ServiceSkipDelay();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetCameraLampStateAPI(bool lamp_on, float intensity_0_to_100)
        {
            return svc_SetCameraLampState(lamp_on, intensity_0_to_100);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetCameraLampIntensityAPI(float intensity_0_to_100)
        {
            return svc_SetCameraLampIntensity(intensity_0_to_100);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetCameraLampStateAPI(out bool lamp_on, out float intensity_0_to_100)
        {
            return svc_GetCameraLampState(out lamp_on, out intensity_0_to_100);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError InitializeCarrierAPI()
        {
            return InitializeCarrier();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GenerateContinuousAnalysisAPI(service_analysis_result_callback on_service_analysis)
        {
            return svc_GenerateContinuousAnalysis(on_service_analysis);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GenerateSingleShotAnalysisAPI(service_analysis_result_callback on_service_analysis)
        {
            return svc_GenerateSingleShotAnalysis(on_service_analysis);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError StopContinuousAnalysisAPI()
        {
            return svc_StopContinuousAnalysis();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetFlowCellDepthSettingAPI(out ushort flow_cell_depth)
        {
            return svc_GetFlowCellDepthSetting(out flow_cell_depth);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetFlowCellDepthSettingAPI(UInt16 flow_cell_depth)
        {
            return svc_SetFlowCellDepthSetting(flow_cell_depth);
        }

        public static void FreeAutofocusResultsAPI(ref IntPtr results, byte num_result)
        {
            svc_FreeAutofocusResults(out results, num_result);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RotateCarouselAPI(out SamplePosition samplePos)
        {
            return RotateCarousel(out samplePos);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError EjectSampleStageAPI(string username, string password)
        {
            return EjectSampleStage(username, password);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetSampleWellPositionAPI(out SamplePosition samplePos)
        {
            return svc_GetSampleWellPosition(out samplePos);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetSystemSerialNumberAPI(string serialNumber, string servicePassword)
        {
            return svc_SetSystemSerialNumber(serialNumber, servicePassword);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetSystemSerialNumberAPI(ref string serialNumber)
        {
            IntPtr ptr;
            var hawkeyeError = GetSystemSerialNumber(out ptr);
            serialNumber = ptr.ToSystemString();
            GenericFree.FreeCharBuffer(ptr);
            return hawkeyeError;
        }


        #endregion


        #region Private Methods

        #endregion

    }
}
