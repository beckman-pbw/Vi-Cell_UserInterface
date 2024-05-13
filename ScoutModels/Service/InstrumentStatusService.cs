using JetBrains.Annotations;
using ScoutDomains;
using ScoutLanguageResources;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using HawkeyeCoreAPI.Interfaces;
using ScoutModels.Interfaces;
using Ninject.Extensions.Logging;
using SystemStatus = ScoutUtilities.Enums.SystemStatus;
using System.Reactive.Subjects;
using System.Timers;
using HawkeyeCoreAPI.Facade;
using ScoutUtilities.UIConfiguration;
using ScoutModels.Settings;
using ScoutDomains.Analysis;
using ScoutModels.Common;

namespace ScoutModels
{
    internal class SystemStatusTimer : System.Timers.Timer
    {
        public InstrumentStatusService InstrumentStatusService { get; }

        public SystemStatusTimer(double interval, InstrumentStatusService instrumentStatusService) : base(interval)
        {
            InstrumentStatusService = instrumentStatusService;
        }
    }

    public class InstrumentStatusService : Disposable, IInstrumentStatusService
    {
        #region Constructor
        
        public InstrumentStatusService(
	        ISystemStatus systemStatus, 
	        IErrorLog errorLogService,
	        ILogger logger, 
			IApplicationStateService applicationStateService)
        {
            _systemStatusCallbackSubject = new Subject<SystemStatusDomain>();
            _cellTypesCallbackSubject = new Subject<List<CellTypeDomain>>();
            _viCellIdentifierCallbackSubject = new Subject<string>();
            _softwareVersionCallbackSubject = new Subject<string>();
            _firmwareVersionCallbackSubject = new Subject<string>();
            _reagentUseRemainingCallbackSubject = new Subject<int>();
            _wasteTubeCapacityCallbackSubject = new Subject<int>();
            _errorStatusCallbackSubject = new Subject<ErrorStatusType>();
            _systemStatus = systemStatus;
            _errorLogService = errorLogService;
            _logger = logger;
            _applicationStateService = applicationStateService;
            _cellTypesObtained = false;
            _previousReagentUseRemaining = UInt32.MaxValue;
            _previousWasteTubeCapacity = UInt32.MaxValue;
            _applicationStateServiceSubscription = _applicationStateService.SubscribeStateChanges(ApplicationStateHandler);
            _updateSystemStatusTimer = new SystemStatusTimer(UISettings.SystemStatusTimerInterval, this);
            _instanceGUID = Guid.NewGuid();
        }

        #endregion

        #region Properties & Fields

        private bool _cellTypesObtained;
        private uint _previousReagentUseRemaining;
        private uint _previousWasteTubeCapacity;
        private static ILogger _logger;
        private readonly IApplicationStateService _applicationStateService;
        private readonly SystemStatusTimer _updateSystemStatusTimer;
        private readonly Subject<SystemStatusDomain> _systemStatusCallbackSubject;
        private readonly Subject<List<CellTypeDomain>> _cellTypesCallbackSubject;
        private readonly Subject<string> _viCellIdentifierCallbackSubject;
        private readonly Subject<string> _softwareVersionCallbackSubject;
        private readonly Subject<string> _firmwareVersionCallbackSubject;
        private readonly Subject<Int32> _reagentUseRemainingCallbackSubject;
        private readonly Subject<Int32> _wasteTubeCapacityCallbackSubject;
        private readonly Subject <ErrorStatusType> _errorStatusCallbackSubject;
        private readonly ISystemStatus _systemStatus;
        private readonly IErrorLog _errorLogService;

        private readonly object _lockObject = new object();
        private readonly object _lockWasteTrayObject = new object();

        public SystemStatus SystemStatus => SystemStatusDom?.SystemStatus ?? SystemStatus.Idle;

        // Keep for testing...
        //private int temp = 0;
        private readonly Guid _instanceGUID;
        
        private SystemStatusDomain _systemStatusDom;
        public SystemStatusDomain SystemStatusDom
        {
            get
            {
                lock (_lockObject)
                {
                    return _systemStatusDom;
                }
            }
            private set
            {
                lock (_lockObject)
                {
                    _systemStatusDom = value;
                }
            }
        }

        #endregion

        #region Sample Status Callback Subscription

        public IObservable<SystemStatusDomain> SubscribeToSystemStatusCallback()
        {
			//TODO: removed the "lock" statements to see if the UI lockup go away.
			// Need to research why the "lock" statements cause the UI to lockup in various ways.
			// See:
			//		PC3549-4706 UI Lockup when Signing On(External Keyboard)
			//		PC3549-4645 UI Lockup when Searching via Sample Set Name using Filter Icon

			//lock (_sampleStatusLock)
			{
				return _systemStatusCallbackSubject;
            }
        }

        public void PublishSystemStatusCallback(SystemStatusDomain systemStatus)
        {
            try
            {                
                SystemStatusDom = systemStatus;
				//lock (_sampleStatusLock)
                {
                    _systemStatusCallbackSubject.OnNext(systemStatus);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to publish sample status callback", e);

                //lock (_sampleStatusLock)
                {
                    _systemStatusCallbackSubject.OnError(e);
                }
            }
        }

        #endregion

        #region Cell Types Changed Callback Subscription

        private readonly object _cellTypesLock = new object();

        public IObservable<List<CellTypeDomain>> SubscribeToCellTypesCallback()
        {
            lock (_cellTypesLock)
            {
                return _cellTypesCallbackSubject;
            }
        }

        public void PublishCellTypesCallback(List<CellTypeDomain> cellTypes)
        {
            try
            {
                lock (_cellTypesLock)
                {
                    _cellTypesCallbackSubject.OnNext(cellTypes);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to publish cell types changed callback", e);

                lock (_cellTypesLock)
                {
                    _cellTypesCallbackSubject.OnError(e);
                }
            }
        }

        #endregion

        #region ViCell Identifier changed subscription
        private readonly object _viCellIdentifierLock = new object();

        private void ApplicationStateHandler(ApplicationStateChange stateChange)
        {
            switch (stateChange.State)
            {
                case ApplicationStateEnum.Startup:
                    _logger.Debug($"ApplicationStateHandler {_instanceGUID} : state change : startup");
                    if (!_updateSystemStatusTimer.Enabled)
                    {
                        _logger.Debug($"ApplicationStateHandler {_instanceGUID} : state change : startup : added Elapsed Handler");
                        _updateSystemStatusTimer.Elapsed += OnTimedEventUpdateSystemStatus;
                        _updateSystemStatusTimer.AutoReset = true;
                        _updateSystemStatusTimer.Enabled = true;
                    }
                    break;
                case ApplicationStateEnum.Shutdown:
                    _logger.Debug($"ApplicationStateHandler {_instanceGUID} : state change : shutdown");
                    if (_updateSystemStatusTimer.Enabled)
                    {
                        _logger.Debug($"ApplicationStateHandler {_instanceGUID} : state change : shutdown : removed Elapsed Handler");
                        _updateSystemStatusTimer.Elapsed -= OnTimedEventUpdateSystemStatus;
                        _updateSystemStatusTimer.Enabled = false;
                    }
                    break;
            }
        }

        private static void OnTimedEventUpdateSystemStatus(object source, ElapsedEventArgs e)
        {
            if (source is SystemStatusTimer timer)
            {
                timer.InstrumentStatusService.GetAndPublishSystemStatus();
            }
        }

        public IObservable<string> SubscribeViCellIdentifierCallback()
        {
            lock (_viCellIdentifierLock)
            {
                return _viCellIdentifierCallbackSubject;
            }
        }

        public void PublishViCellIdentifierCallback(string serialNumber)
        {
            try
            {
                lock (_viCellIdentifierLock)
                {
	                _viCellIdentifierCallbackSubject.OnNext(serialNumber);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to publish Vi Cell ID callback", e);

                lock (_viCellIdentifierLock)
                {
                    _viCellIdentifierCallbackSubject.OnError(e);
                }
            }
        }

        private readonly object _softwareVersionLock = new object();
        public IObservable<string> SubscribeSoftwareVersionCallback()
        {
	        lock (_softwareVersionLock)
	        {
		        return _softwareVersionCallbackSubject;
	        }
        }

        public void PublishSoftwareVersionCallback(string version)
        {
	        try
	        {
		        lock (_softwareVersionLock)
		        {
			        _softwareVersionCallbackSubject.OnNext(version);
		        }
	        }
	        catch (Exception e)
	        {
		        _logger.Error($"Failed to publish software version callback", e);

		        lock (_softwareVersionLock)
		        {
			        _softwareVersionCallbackSubject.OnError(e);
		        }
	        }
        }

        private readonly object _firmwareVersionLock = new object();
        public IObservable<string> SubscribeFirmwareVersionCallback()
        {
	        lock (_firmwareVersionLock)
	        {
		        return _firmwareVersionCallbackSubject;
	        }
        }

        public void PublishFirmwareVersionCallback(string version)
        {
	        try
	        {
		        lock (_firmwareVersionLock)
		        {
			        _firmwareVersionCallbackSubject.OnNext(version);
		        }
	        }
	        catch (Exception e)
	        {
		        _logger.Error($"Failed to publish firmware version callback", e);

		        lock (_firmwareVersionLock)
		        {
			        _firmwareVersionCallbackSubject.OnError(e);
		        }
	        }
        }

        private readonly object _errorStatusLock = new object();
        public IObservable <ErrorStatusType> SubscribeErrorStatusCallback()
        {
	        lock (_errorStatusLock)
	        {
		        return _errorStatusCallbackSubject;
	        }
        }

		public void PublishErrorStatusCallback(ErrorStatusType status)
        {
	        try
	        {
		        lock (_errorStatusLock)
		        {
			        _errorStatusCallbackSubject.OnNext(status);
		        }
	        }
	        catch (Exception e)
	        {
		        _logger.Error($"Failed to publish error status callback", e);

		        lock (_errorStatusLock)
		        {
			        _errorStatusCallbackSubject.OnError(e);
		        }
	        }
        }

	#endregion

	#region Reagent Use Remaining subscription
        private readonly object _reagentUseRemainingLock = new object();

        public IObservable<Int32> SubscribeReagentUseRemainingCallback()
        {
            lock (_reagentUseRemainingLock)
            {
                return _reagentUseRemainingCallbackSubject;
            }
        }
        public void PublishReagentUseRemainingCallback(Int32 reagentUseRemaining)
        {
            try
            {
                lock (_reagentUseRemainingLock)
                {
                    _reagentUseRemainingCallbackSubject.OnNext(reagentUseRemaining);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to publish reagent use remaining callback", e);

                lock (_reagentUseRemainingLock)
                {
                    _reagentUseRemainingCallbackSubject.OnError(e);
                }
            }
        }

        #endregion

        #region Waste Tube Capacity subscription
        private readonly object _wasteTubeCapacityLock = new object();

        public IObservable<Int32> SubscribeWasteTubeCapacityCallback()
        {
            lock (_wasteTubeCapacityLock)
            {
                return _wasteTubeCapacityCallbackSubject;
            }
        }

        public void PublishWasteTubeCapacityCallback(Int32 wasteTubeCapacity)
        {
            try
            {
                lock (_wasteTubeCapacityLock)
                {
                    _wasteTubeCapacityCallbackSubject.OnNext(wasteTubeCapacity);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to publish waste tube capacity callback", e);

                lock (_reagentUseRemainingLock)
                {
                    _wasteTubeCapacityCallbackSubject.OnError(e);
                }
            }
        }

        #endregion

        #region Public Methods and Dispose

        protected override void DisposeManaged()
        {
            if (_updateSystemStatusTimer != null)
            {
                _updateSystemStatusTimer.Elapsed -= OnTimedEventUpdateSystemStatus;
                _updateSystemStatusTimer.Enabled = false;
                _updateSystemStatusTimer.Dispose();
            }
            _systemStatusCallbackSubject?.OnCompleted();
            _cellTypesCallbackSubject?.OnCompleted();
            _reagentUseRemainingCallbackSubject?.OnCompleted();
            _viCellIdentifierCallbackSubject?.OnCompleted();
            _wasteTubeCapacityCallbackSubject?.OnCompleted();
            _errorStatusCallbackSubject.OnCompleted();
            base.DisposeManaged();
        }

        protected override void DisposeUnmanaged()
        {
            _applicationStateServiceSubscription?.Dispose();
            base.DisposeUnmanaged();
        }

        public void GetAndPublishSystemStatus()
        {
            var oldSystemStatus = SystemStatusDom?.SystemStatus;

            var newSystemStatusData = new SystemStatusData();
            var intPtr = _systemStatus.GetSystemStatusAPI(ref newSystemStatusData);
            var newSystemStatusDomain = GetSystemStatusDomain(newSystemStatusData);
            // Freeing the SystemStatus is done here after the newSystemStatusDomain has been initialized,
            // otherwise the free destroys the pointer to the error codes.
            _systemStatus.FreeSystemStatusAPI(intPtr);
            
            if (oldSystemStatus == null || oldSystemStatus != newSystemStatusDomain.SystemStatus)
	            // "Callback" added so the log file can be sorted and include this along with sample status callbacks
				_logger.Debug($"Instrument Status: {newSystemStatusDomain.SystemStatus}{Environment.NewLine}My instance: {_instanceGUID}[Callback]");

            PublishSystemStatusCallback(newSystemStatusDomain);

			PublishViCellIdentifierCallback(HardwareManager.HardwareSettingsModel.SerialNumber);

            if (_cellTypesObtained == false)
            {
                var allCellTypes = CellTypeFacade.Instance.GetAllCellTypes_BECall();
                PublishCellTypesCallback(allCellTypes);
                _cellTypesObtained = true;
            }

            if (_previousReagentUseRemaining != SystemStatusDom.RemainingReagentPackUses)
            {
                PublishReagentUseRemainingCallback((int)SystemStatusDom.RemainingReagentPackUses);
                _previousReagentUseRemaining = SystemStatusDom.RemainingReagentPackUses;
            }

            if (_previousWasteTubeCapacity != SystemStatusDom.SampleTubeDisposalRemainingCapacity)
            {
                PublishWasteTubeCapacityCallback((int) SystemStatusDom.SampleTubeDisposalRemainingCapacity);
                _previousWasteTubeCapacity = SystemStatusDom.SampleTubeDisposalRemainingCapacity;
            }
        }

        private readonly object _systemLock = new object();
        private readonly IDisposable _applicationStateServiceSubscription;

        private SystemStatusDomain GetSystemStatusDomain(SystemStatusData newSystemStatusData)
        {
            var systemStatusDomain = new SystemStatusDomain
            {
                SystemStatus = newSystemStatusData.status,
                CarouselDetect = newSystemStatusData.sensor_carousel_detect,
                TubeDetect = newSystemStatusData.sensor_carousel_tube_detect,
                ReagentPack = newSystemStatusData.sensor_reagent_pack_in_place,
                ReagentDoor = newSystemStatusData.sensor_reagent_pack_door_closed,
                RadiusHome = newSystemStatusData.sensor_radiusmotor_home,
                ThetaHome = newSystemStatusData.sensor_thetamotor_home,
                ProbeHome = newSystemStatusData.sensor_probemotor_home,
                FocusHome = newSystemStatusData.sensor_focusmotor_home,
                ReagentUpper = newSystemStatusData.sensor_reagentmotor_upper,
                ReagentLower = newSystemStatusData.sensor_reagentmotor_lower,
                FlOpticsmotor1 = newSystemStatusData.sensor_flopticsmotor1_home,
                FlOpticsmotor2 = newSystemStatusData.sensor_flopticsmotor2_home,
                MotorFlRack1Position = newSystemStatusData.motor_FLRack1.position,
                MotorFlRack2Position = newSystemStatusData.motor_FLRack2.position,
                MotorFocusPosition = newSystemStatusData.motor_Focus.position,
                MotorProbePosition = newSystemStatusData.motor_Probe.position,
                BrightFieldLED = newSystemStatusData.brightfieldLedPercentPower,
                MotorRadiusPosition = newSystemStatusData.motor_Radius.position,
                MotorReagentPosition = newSystemStatusData.motor_Reagent.position,
                MotorThetaPosition = newSystemStatusData.motor_Theta.position,
                TemperatureControlBoard = newSystemStatusData.temperature_ControllerBoard,
                TemperatureCPU = newSystemStatusData.temperature_CPU,
                TemperatureOpticalCase = newSystemStatusData.temperature_OpticalCase,
                Voltage3V = newSystemStatusData.voltage_neg_3V,
                Voltage3_3V = newSystemStatusData.voltage_3_3V,
                Voltage5vCircuit = newSystemStatusData.voltage_5V_Circuit,
                Voltage5vSensor = newSystemStatusData.voltage_5V_Sensor,
                Voltage12v = newSystemStatusData.voltage_12V,
                Voltage24v = newSystemStatusData.voltage_24V,
                SamplePosition = newSystemStatusData.sampleStageLocation,
                NightlyCleanStatus = newSystemStatusData.nightly_clean_cycle,
			};


            if (systemStatusDomain.NightlyCleanStatus == eNightlyCleanStatus.ncsInProgress ||
                systemStatusDomain.NightlyCleanStatus == eNightlyCleanStatus.ncsAutomationInProgress)
            {
                systemStatusDomain.SystemStatus = SystemStatus.NightlyClean;
            }

            switch (systemStatusDomain.CarouselDetect)
            {
                case eSensorStatus.ssStateUnknown:
                    systemStatusDomain.StagePositionString = "--";
                    break;
                case eSensorStatus.ssStateActive:
                    if (newSystemStatusData.sampleStageLocation.IsValid())
                        systemStatusDomain.StagePositionString = newSystemStatusData.sampleStageLocation.Column.ToString();
                    else
                        systemStatusDomain.StagePositionString = "--";
                    break;
                case eSensorStatus.ssStateInactive:
                    if (newSystemStatusData.sampleStageLocation.IsValid())
                        systemStatusDomain.StagePositionString = newSystemStatusData.sampleStageLocation.Row + newSystemStatusData.sampleStageLocation.Column.ToString();
                    else
                        systemStatusDomain.StagePositionString = "--";
                    break;
            }

            systemStatusDomain.SyringePosition = (int)newSystemStatusData.syringePosition;
            systemStatusDomain.ValvePosition = ValvePositionMap.BackendToValvePosition(newSystemStatusData.syringeValvePosition);
            systemStatusDomain.SampleTubeDisposalRemainingCapacity = newSystemStatusData.sample_tube_disposal_remaining_capacity;
            systemStatusDomain.DefinedFocusPosition = newSystemStatusData.focus_DefinedFocusPosition;
            systemStatusDomain.LastCalibratedDateConcentration = newSystemStatusData.last_calibrated_date_concentration;
			systemStatusDomain.LastCalibratedDateACupConcentration = newSystemStatusData.last_calibrated_date_acup_concentration;
            systemStatusDomain.RemainingReagentPackUses = newSystemStatusData.remainingReagentPackUses;
            systemStatusDomain.SystemErrorDomainList = GetSystemErrorDomains(newSystemStatusData);
            systemStatusDomain.SystemTotalSampleCount = newSystemStatusData.system_total_sample_count;

            return systemStatusDomain;
        }


        private List<SystemErrorDomain> GetSystemErrorDomains(SystemStatusData sysStatusData)
        {
            var errorCodeList = new List<uint>();
            var systemErrorDomain = new List<SystemErrorDomain>();
            if (sysStatusData.active_error_count > 0 && sysStatusData.active_error_codes != IntPtr.Zero)
            {
                int[] indices = new int[sysStatusData.active_error_count];
                Marshal.Copy(sysStatusData.active_error_codes, indices, 0, sysStatusData.active_error_count);
                errorCodeList = indices.Select(l => (uint) l).ToList();
            }

            errorCodeList.ForEach(errorCode =>
            {
	            var systemErrorAsResource = SystemErrorCodeToExpandedResourceStrings(errorCode);
	            
				// Build decoded (English) error message for OPCUA event.
				var systemError = SystemErrorCodeToExpandedStrings(errorCode);

				// Only report "Warning" and "Error" events.
				if (systemError.SeverityKey != "Notification")
				{
					ErrorStatusType temp = new ErrorStatusType()
					{
						ErrorCode = systemError.CellHealthErrorCode,
						Severity = systemError.SeverityDisplayValue,
						System = systemError.System,
						SubSystem = systemError.SubSystem,
						Instance = systemError.Instance,
						FailureMode = systemError.FailureMode,
					};

					PublishErrorStatusCallback(temp);
					var status = ClearSystemErrorCode(errorCode);
					if (!status.Equals(HawkeyeError.eSuccess))
					{
						_logger.Debug("GetSystemErrorDomains:: failed to clear error:" + errorCode);
					}
				}

				systemErrorDomain.Add(systemErrorAsResource);
				systemErrorDomain[systemErrorDomain.Count - 1].ErrorCode = errorCode;
            });

            // Keep for testing...
            //temp++; 
            //var tempStr = temp.ToString();

            //ErrorStatusType status = new ErrorStatusType()
            //{
            //	Identifier = "CH" + tempStr,
            //	ErrorCode = "CH" + tempStr,
	        //	Severity = "Warning" + tempStr,
            //	System = "System" + tempStr,
	        //	SubSystem = "SubSystem" + tempStr,
            //	Instance = "Instance" + tempStr,
            //	FailureMode = "FailureMode" + tempStr,
            //};

	        //PublishErrorStatusCallback(status);

			return systemErrorDomain;
        }


        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError SampleTubeDiscardTrayEmptied()
        {
            var hawkeyeError = HawkeyeCoreAPI.Reagent.SampleTubeDiscardTrayEmptiedAPI();
            return hawkeyeError;
        }

        #endregion

        #region Static Methods

        public bool IsRunning()
        {
            return SystemStatus == SystemStatus.ProcessingSample || 
                   SystemStatus == SystemStatus.SearchingTube;
        }

        public bool IsPaused()
        {
            return SystemStatus == SystemStatus.Paused || SystemStatus == SystemStatus.Pausing;
        }

        public bool IsStopped()
        {
            return SystemStatus == SystemStatus.Faulted || SystemStatus == SystemStatus.Idle ||
                   SystemStatus == SystemStatus.Stopped || SystemStatus == SystemStatus.Stopping;
        }

        /// <summary>
        /// The system is not running. It is either Idle, Stopped, or has Faulted.
        /// </summary>
        /// <returns>true if not running, otherwise false, included stopping.</returns>
        public bool IsNotRunning()
        {
            return SystemStatus == SystemStatus.Faulted || SystemStatus == SystemStatus.Idle ||
                   SystemStatus == SystemStatus.Stopped;
        }

        public bool IsIdle()
        {
            return SystemStatus == SystemStatus.Idle;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError ClearSystemErrorCode(uint active_error)
        {
            _logger.Debug("ClearSystemErrorCode:: active_error: " + active_error);
            var hawkeyeError = HawkeyeCoreAPI.ErrorLog.ClearSystemErrorCodeAPI(active_error);
            _logger.Debug("ClearSystemErrorCode:: hawkeyeError:" + hawkeyeError);
            return hawkeyeError;
        }

        public SystemErrorDomain SystemErrorCodeToExpandedResourceStrings(UInt32 system_error_code)
        {
            var severityResourceKey = string.Empty;
            var systemResourceKey = string.Empty;
            var subsystemResourceKey = string.Empty;
            var instanceResourceKey = string.Empty;
            var failureModeResourceKey = string.Empty;
            var cellHealthErrorCodeKey = string.Empty;

            _errorLogService.SystemErrorCodeToExpandedResourceStringsAPI(
                system_error_code,
                ref severityResourceKey,
                ref systemResourceKey,
                ref subsystemResourceKey,
                ref instanceResourceKey,
                ref failureModeResourceKey,
	            ref cellHealthErrorCodeKey);

            LogWarning(system_error_code, severityResourceKey, systemResourceKey, subsystemResourceKey, instanceResourceKey, failureModeResourceKey, cellHealthErrorCodeKey);
            
            var errorDomain = new SystemErrorDomain
            {
                SeverityKey = severityResourceKey,
                SeverityDisplayValue = LanguageResourceHelper.Get(severityResourceKey),
                System = LanguageResourceHelper.Get(systemResourceKey),
                SubSystem = LanguageResourceHelper.Get(subsystemResourceKey),
                Instance = LanguageResourceHelper.Get(instanceResourceKey),
                FailureMode = LanguageResourceHelper.Get(failureModeResourceKey),
                CellHealthErrorCode = LanguageResourceHelper.Get(cellHealthErrorCodeKey)
            };

            return errorDomain;
        }

        public SystemErrorDomain SystemErrorCodeToExpandedStrings(UInt32 system_error_code)
        {
            var severity = string.Empty;
	        var system = string.Empty;
            var subsystem = string.Empty;
	        var instance = string.Empty;
            var failureMode = string.Empty;
            var cellHealthErrorCode = string.Empty;

	        _errorLogService.SystemErrorCodeToExpandedStringsAPI(
		        system_error_code,
		        ref severity,
				ref system,
		        ref subsystem,
		        ref instance,
		        ref failureMode,
		        ref cellHealthErrorCode);

	        LogWarning(system_error_code, severity, system, subsystem, instance, failureMode, cellHealthErrorCode);

	        var errorDomain = new SystemErrorDomain
	        {
		        SeverityKey = severity,
		        SeverityDisplayValue = severity,
		        System = system,
		        SubSystem = subsystem,
		        Instance = instance,
		        FailureMode = failureMode,
		        CellHealthErrorCode = cellHealthErrorCode
	        };

	        return errorDomain;
        }

		private void LogWarning(uint system_error_code, string severityResourceKey, 
	        string systemResourceKey, string subsystemResourceKey, string instanceResourceKey,
            string failureModeResourceKey, string cellHealthErrorCodeKey)
        {
            if (!UISettings.EnableLoggingForStatusWarnings)
                return;

            var msg = $"{nameof(SystemErrorCodeToExpandedResourceStrings)}::{Environment.NewLine}" +
                      $"\tcode: {system_error_code}{Environment.NewLine}" +
                      $"\tseverity: {LanguageResourceHelper.Get(severityResourceKey)}{Environment.NewLine}" +
                      $"\tsystem: {LanguageResourceHelper.Get(systemResourceKey)}{Environment.NewLine}" +
                      $"\tsubsystem: {LanguageResourceHelper.Get(subsystemResourceKey)}{Environment.NewLine}" +
                      $"\tinstance: {LanguageResourceHelper.Get(instanceResourceKey)}{Environment.NewLine}" +
                      $"\tfailure_mode: {LanguageResourceHelper.Get(failureModeResourceKey)}" +
                      $"\tcellHealthErrorCode: {LanguageResourceHelper.Get(cellHealthErrorCodeKey)}";

            // log with the appropriate logging severity
            switch (severityResourceKey?.ToUpper())
            {
                case "LID_API_SYSTEMERRORCODE_SEVERITY_ERROR":
                case "ERROR":
                    _logger.Error(msg);
                    break;
                case "LID_API_SYSTEMERRORCODE_SEVERITY_WARNING":
                case "WARNING":
                    _logger.Warn(msg);
                    break;
                case "LID_API_SYSTEMERRORCODE_SEVERITY_NOTIFICATION":
                case "NOTIFICATION":
                    _logger.Info(msg);
                    break;
                case "CRITICAL":
                    _logger.Fatal(msg);
                    break;
                default:
                    _logger.Debug(msg);
                    break;
            }
        }

        #endregion
    }
}
