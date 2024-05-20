using JetBrains.Annotations;
using log4net;
using ScoutDataAccessLayer.DAL;
using ScoutDomains;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using ScoutUtilities;
using ScoutLanguageResources;
using ScoutUtilities.UIConfiguration;

namespace ScoutModels.Settings
{
    public class HardwareSettingsModel
    {
        public HardwareSettingsModel()
        {
        }

        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string ApplicationName { get; set; }
        public string ApplicationVersion { get; set; }

        public string SerialNumber { get; set; }

        public string ConfirmSerialNumber { get; set; }

//TODO: what is this used for ???
        public string Password { get; set; }

        public InstrumentType InstrumentType { get; set; }

        private HardwareSettingsDomain _hardwareSettings;
        public HardwareSettingsDomain HardwareSettingsDomain
        {
            get { return _hardwareSettings ?? (_hardwareSettings = new HardwareSettingsDomain()); }
            set { _hardwareSettings = value; }
        }

        #region public Method

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

            switch (InstrumentType)
            {
                case InstrumentType.CellHealth_ScienceModule:
                    ApplicationName = LanguageResourceHelper.Get("LID_Title_CHM_Name");
                    break;
                case InstrumentType.ViCELL_BLU_Instrument:
                    ApplicationName = LanguageResourceHelper.Get("LID_Title_ViCell_BLU_Name");
                    break;
                case InstrumentType.ViCELL_GO_Instrument:
                    ApplicationName = LanguageResourceHelper.Get("LID_Title_ViCell_GO_Name");
                    break;
                default:
                    ApplicationName = "Unknown application name";
                    break;
            }

            switch (HardwareManager.HardwareSettingsModel.InstrumentType)
            {
                case InstrumentType.CellHealth_ScienceModule:
                    ApplicationVersion = LanguageResourceHelper.Get("LID_Label_CHM_SW_Version") + UISettings.SoftwareVersion;
                    break;
                case InstrumentType.ViCELL_BLU_Instrument:
                    ApplicationVersion = LanguageResourceHelper.Get("LID_Label_ViCell_BLU_SW_Version") + UISettings.SoftwareVersion;
                    break;
                case InstrumentType.ViCELL_GO_Instrument:
                    ApplicationVersion = LanguageResourceHelper.Get("LID_Label_ViCell_GO_SW_Version") + UISettings.SoftwareVersion;
                    break;
                default:
                    ApplicationVersion = "Unknown application name: " + UISettings.SoftwareVersion;
                    break;
            }

            return HardwareSettingsDomain;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError SetSystemSerialNumber(string serialNumber, string servicePassword)
        {
            Log.Debug("SetSystemSerialNumber:: serial #: " + serialNumber);
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