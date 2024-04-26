using JetBrains.Annotations;
using log4net;
using ScoutDataAccessLayer.DAL;
using ScoutDomains;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using ScoutUtilities.UIConfiguration;
using System;
using ScoutUtilities;

namespace ScoutModels.Settings
{
    public class HardwareSettingsModel : IHardwareSettingsModel
    {
        public HardwareSettingsModel()
        {
        }

        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string SerialNumber { get; set; }

        public string ConfirmSerialNumber { get; set; }

        public string Password { get; set; }

        private HardwareSettingsDomain _hardwareSettings;
        public HardwareSettingsDomain HardwareSettingsDomain
        {
            get { return _hardwareSettings ?? (_hardwareSettings = new HardwareSettingsDomain()); }
            set { _hardwareSettings = value; }
        }

        #region public Method

        public void GetInstrumentSettings(string username)
        {
            bool userFound;
            if (username == null)
                return;
            var hwSetting = XMLDataAccess.Instance.ReadConfigurationData<HardwareSettingsDomain>(username, nameof(HardwareSettingsDomain), out userFound);
            GetVersionInformation();
            HardwareSettingsDomain.IsMonitorOn = hwSetting.IsMonitorOn;
        }

        public void SaveInstrument(string username)
        {
            XMLDataAccess.Instance.WriteToConfigurationXML(HardwareSettingsDomain, username, null);
        }

        public HardwareSettingsDomain GetVersionInformation()
        {
            var systemVersion = new SystemVersion();

            HawkeyeCoreAPI.SystemStatus.GetVersionInformationAPI(ref systemVersion);

            Log.Debug("GetVersionInformation systemVersion: " + systemVersion);
			Log.Debug("	HawkeyeCore Version: " + systemVersion.software_version);
			Log.Debug("	Firmware Version: " + systemVersion.controller_firmware_version);
            Log.Debug("	SyringePump FW Version: " + systemVersion.syringepump_firmware_version);
            Log.Debug("	ImageAnalysis Software Version: " + systemVersion.img_analysis_version);
            Log.Debug("	CameraFirmware Version: " + systemVersion.camera_firmware_version);

            var serialNumber = string.Empty;
            var getSerialStatus = GetSystemSerialNumber(ref serialNumber);
            if (getSerialStatus.Equals(HawkeyeError.eSuccess))
            {
                HardwareSettingsDomain.SerialNumber = serialNumber;
                SerialNumber = serialNumber;
            }
            else
            {
                SerialNumber = HardwareSettingsDomain.SerialNumber;
            }

            HardwareSettingsDomain = GetHardWareSetting(systemVersion);
            return HardwareSettingsDomain;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError SetSystemSerialNumber(string serialNumber, string servicePassword)
        {
            var hawkeyeError = HawkeyeCoreAPI.Service.SetSystemSerialNumberAPI(serialNumber, servicePassword);
            Misc.LogOnHawkeyeError("SetSystemSerialNumber", hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError ValidateMe(string password)
        {
            var hawkeyeError = HawkeyeCoreAPI.User.ValidateMeAPI(password);
            Misc.LogOnHawkeyeError("ValidateMe", hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError GetSystemSerialNumber(ref string serialNumber)
        {
            var hawkeyeError = HawkeyeCoreAPI.Service.GetSystemSerialNumberAPI(ref serialNumber);
            Misc.LogOnHawkeyeError("GetSystemSerialNumber",hawkeyeError);
            return hawkeyeError;
        }

        #endregion

        private HardwareSettingsDomain GetHardWareSetting(SystemVersion sysVersion)
        {
            var hwdSetting = new HardwareSettingsDomain()
            {
                SerialNumber = sysVersion.system_serial_number,
                UIVersion = UISettings.SoftwareVersion,
				HawkeyeCoreVersion = sysVersion.software_version,
				FirmwareVersion = sysVersion.controller_firmware_version,
                SyringePumpFWVersion = sysVersion.syringepump_firmware_version,
                ImageAnalysisSoftwareVersion = sysVersion.img_analysis_version,
                CameraFirmwareVersion = sysVersion.camera_firmware_version,
                DeviceName = Environment.MachineName
            };
            return hwdSetting;
        }

    }
}