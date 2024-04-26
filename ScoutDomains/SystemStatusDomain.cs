using System;
using System.Collections.Generic;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ScoutDomains
{
    public class SystemStatusDomain : BaseNotifyPropertyChanged
    {
        public List<SystemErrorDomain> SystemErrorDomainList
        {
            get { return GetProperty<List<SystemErrorDomain>>(); }
            set { SetProperty(value); }
        }

        public SystemStatus SystemStatus
        {
            get { return GetProperty<SystemStatus>(); }
            set { SetProperty(value); }
        }

        public eSensorStatus FlOpticsmotor1
        {
            get { return GetProperty<eSensorStatus>(); }
            set { SetProperty(value); }
        }

        public eSensorStatus FlOpticsmotor2
        {
            get { return GetProperty<eSensorStatus>(); }
            set { SetProperty(value); }
        }

        public int MotorRadiusPosition
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int MotorThetaPosition
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int MotorProbePosition
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int MotorFocusPosition
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value);} 
        }

        public int MotorReagentPosition
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int MotorFlRack1Position
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int MotorFlRack2Position
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public eSensorStatus CarouselDetect
        {
            get { return GetProperty<eSensorStatus>(); }
            set { SetProperty(value); }
        }

        public eSensorStatus TubeDetect
        {
            get { return GetProperty<eSensorStatus>(); }
            set { SetProperty(value); }
        }

        public eSensorStatus ReagentDoor
        {
            get { return GetProperty<eSensorStatus>(); }
            set { SetProperty(value); }
        }

        public eSensorStatus ReagentPack
        {
            get { return GetProperty<eSensorStatus>(); }
            set { SetProperty(value); }
        }
        
        public eSensorStatus RadiusHome
        {
            get { return GetProperty<eSensorStatus>(); }
            set { SetProperty(value); }
        }

        public eSensorStatus ThetaHome
        {
            get { return GetProperty<eSensorStatus>(); }
            set { SetProperty(value); }
        }

        public eSensorStatus ProbeHome
        {
            get { return GetProperty<eSensorStatus>(); }
            set { SetProperty(value); }
        }

        public eSensorStatus FocusHome
        {
            get { return GetProperty<eSensorStatus>(); }
            set { SetProperty(value); }
        }

        public eSensorStatus ReagentUpper
        {
            get { return GetProperty<eSensorStatus>(); }
            set { SetProperty(value); }
        }

        public eSensorStatus ReagentLower
        {
            get { return GetProperty<eSensorStatus>(); }
            set { SetProperty(value); }
        }

        public ValvePosition ValvePosition
        {
            get { return GetProperty<ValvePosition>(); }
            set { SetProperty(value); }
        }

        public int SyringePosition
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public double BrightFieldLED
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double Voltage3V
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double Voltage3_3V
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double Voltage5vSensor
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double Voltage5vCircuit
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double Voltage12v
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double Voltage24v
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public uint SampleTubeDisposalRemainingCapacity
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }
        
        public uint SystemTotalSampleCount
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public ulong LastCalibratedDateConcentration
        {
            get { return GetProperty<ulong>(); }
            set { SetProperty(value); }
        }

		public ulong LastCalibratedDateACupConcentration
		{
			get { return GetProperty<ulong>(); }
			set { SetProperty(value); }
		}

		public uint RemainingReagentPackUses
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public Int32 DefinedFocusPosition
        {
            get { return GetProperty<Int32>(); }
            set { SetProperty(value); }
        }

        public double TemperatureOpticalCase
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double TemperatureCPU
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double TemperatureControlBoard
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public SamplePosition SamplePosition { get; set; }

        public eNightlyCleanStatus NightlyCleanStatus { get; set; }

        public string StagePositionString
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
    
        public InstrumentType InstrumentType
        {
	        get { return GetProperty<InstrumentType>(); }
	        set { SetProperty(value); }
        }
    }
}
