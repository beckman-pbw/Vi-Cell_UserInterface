namespace ScoutDomains
{
    public class HardwareSettingsDomain
    {
        public string SerialNumber { get; set; }

        public string HawkeyeCoreVersion { get; set; }

        public string FirmwareVersion { get; set; }

        public string SyringePumpFWVersion { get; set; }

        public string CameraFirmwareVersion { get; set; }

        public string UIVersion { get; set; }

        public string ImageAnalysisSoftwareVersion { get; set; }

        public string DeviceName { get; set; }

        public bool IsMonitorOn { get; set; }
    }
}
