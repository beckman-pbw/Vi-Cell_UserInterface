using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ScoutUtilities.Common;

namespace ScoutUtilities.Structs
{
    public enum eMotorFlags : UInt16
    {
        mfUnknown = 0x0000,
        mfConfigured = 0x0001, // if !(flags & mfConfigured) - motor information not important
        mfHomed = 0x0002, // has motor been homed / zeroed
        mfInMotion = 0x0004,
        mfAtPosition = 0x0008,
        mfErrorState = 0x0010, // if (flags & mfErrorState) - motor is in a faulted condition
        mfPoweredOn = 0x0020, // if (flags & mfPoweredOn) - drive/holding power is applied to motor.
        mfPositionKnown = 0x0040
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MotorStatus
    {
        public UInt16 flags;
        public Int32 position;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string position_description; // up, down, "A5", in, out...
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SystemStatusData
    {
        public SystemStatus status;

        // Placeholder for the "desktop" version of the code that's coming eventually for reanalysis, etc. More or less the same as simulated mode.
        [MarshalAs(UnmanagedType.U1)] public bool is_standalone_mode;

        // Total since the beginning of time.
        public UInt32 system_total_sample_count;

        /* SENSOR STATES */
        public eSensorStatus sensor_carousel_detect;
        public eSensorStatus sensor_carousel_tube_detect;
        public eSensorStatus sensor_reagent_pack_door_closed;
        public eSensorStatus sensor_reagent_pack_in_place;

        /* SENSOR STATES - MOTORS */
        public eSensorStatus sensor_radiusmotor_home;
        public eSensorStatus sensor_thetamotor_home;
        public eSensorStatus sensor_probemotor_home;
        public eSensorStatus sensor_focusmotor_home;
        public eSensorStatus sensor_reagentmotor_upper;
        public eSensorStatus sensor_reagentmotor_lower;
        public eSensorStatus sensor_flopticsmotor1_home;
        public eSensorStatus sensor_flopticsmotor2_home;

        /* MOTOR STATES */
        public MotorStatus motor_Radius;
        public MotorStatus motor_Theta;
        public MotorStatus motor_Probe;
        public MotorStatus motor_Focus;
        public MotorStatus motor_Reagent;
        public MotorStatus motor_FLRack1;
        public MotorStatus motor_FLRack2;

        /* SAMPLE STAGE - may not always be a valid location */
        public SamplePosition sampleStageLocation;

        /* Carousel events remaining until tray should be emptied.
         * This is a HARD CAP.  Because of the risk of a mechanical
         * jam, the system will refuse to process Carousel samples
         * until the sample tray has been emptied and the API "SampleTubeDiscardTrayEmptied()"
         * function has been called.
         */
        public UInt16 sample_tube_disposal_remaining_capacity;
        public UInt16 remainingReagentPackUses;

        /* FOCUS SYSTEM */
        [MarshalAs(UnmanagedType.U1)] public bool focus_IsFocused;
        public Int32 focus_DefinedFocusPosition;

        /* ACTIVE ERROR STATES */
        public UInt16 active_error_count;
        //public List<UInt32> active_error_codes;
        public IntPtr active_error_codes;

        /* Syringe */
        public UInt16 syringeValvePosition;
        public UInt32 syringePosition;

        /* Camera */
        public float brightfieldLedPercentPower;

        /* Voltage in Volts*/
        public float voltage_neg_3V;
        public float voltage_3_3V;
        public float voltage_5V_Sensor;
        public float voltage_5V_Circuit;
        public float voltage_12V;
        public float voltage_24V;

        /* Temperature in Degree Celsius*/
        public float temperature_ControllerBoard;
        public float temperature_CPU;
        public float temperature_OpticalCase;

        /* Calibrations - days since Epoch (1 January 1970) */
        public UInt64 last_calibrated_date_size;
        public UInt64 last_calibrated_date_concentration;
		public UInt64 last_calibrated_date_acup_concentration;

        /* Nightly clean cycle status*/
        public eNightlyCleanStatus nightly_clean_cycle;

        public Int16 instrumentType;
    }
}
