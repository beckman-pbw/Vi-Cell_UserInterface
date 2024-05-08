using System;
using System.Configuration;
using System.IO;

namespace ScoutUtilities.UIConfiguration
{
    public partial class UISettings
    {
        #region Properties

        public static string XmlDbConfigurationXmlFilePath { get; private set; }
        public static string DriveName { get; private set; }

        public static string ExportPath { get; private set; }
        public static string InstrumentPath { get; private set; }
        public static bool IsFromHardware { get; set; }
        public static bool IsMinimizeButtonVisible { get; private set; }
        public static string LoggingConfigurationSource { get; private set; }
        public static string MasterDataXmlFilePath { get; private set; }
        public static int MaximumSearchCountForOpenSample { get; private set; }
        public static int ResponseTimeOutSeconds { get; private set; }
        public static int ShutDownTimeoutSecond { get; private set; }
        public static string SystemBrightness { get; private set; }
        public static int SystemStatusTimerInterval { get; private set; }
        public static int TimeoutHour { get; private set; }
        public static int TimeoutMinute { get; private set; }
        public static int TimeoutSecond { get; private set; }
        public static int WorkQueueStatusTimer { get; private set; }
        public static double DiameterHistogramBinWidth { get; private set; }
        public static string LicensesHtmlPath { get; private set; }
        public static string HelpHtmlPath { get; private set; }
        public static int NightlyCleanTimeOutSeconds { get; private set; }
        public static int SetFocusTimeOutSec { get; private set; }
        public static int DustReffTimeOutSec { get; private set; }
        public static int MaximumRecentMessages { get; private set; }
        public static bool UseWindowedMode { get; private set; }
        public static bool EnableSampleSetVcrControlsInOfflineMode { get; set; }
        public static bool EnableLoggingForStatusWarnings { get; private set; }
        public static bool UseCarouselSimulation { get; private set; }
        #endregion Properties

        #region Class Members

        private static string _deploymentConfigurationSource;
        private static string _environmentConfigurationSource;
        private static UIConfigurationSettings _uiConfigurationSettings;
        private static EnvironmentConfigurationSettings _environmentConfigurationSettings;

        #endregion Class Members

        public static bool IsHardwareOrSimulated()
        {
            if (IsFromHardware || EnableSampleSetVcrControlsInOfflineMode)
                return true;
            return false;
        }

        static UISettings()
        {
            LoadUiConfigurationSettings();
            LoadEnvironmentConfigurationSettings();
        }

        private static void LoadUiConfigurationSettings()
        {
            var uiConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"UIConfiguration\ui.config");
            if (!File.Exists(uiConfig))
            {
                throw new FileNotFoundException(uiConfig);
            }

            ExeConfigurationFileMap map = new ExeConfigurationFileMap
            {
                ExeConfigFilename = uiConfig
            };

            Configuration uiConfiguration = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            _uiConfigurationSettings = uiConfiguration.Sections.Get("uiConfiguration") as UIConfigurationSettings;

            if (_uiConfigurationSettings == null)
            {
                throw new ConfigurationErrorsException("UI Configuration Failed To Load Successfully.");
            }


            // The other configuration sources
            LoggingConfigurationSource = _uiConfigurationSettings.LoggingConfigurationSource;
            _environmentConfigurationSource = _uiConfigurationSettings.EnvironmentConfigurationSource;
            _deploymentConfigurationSource = _uiConfigurationSettings.DeploymentConfigurationSource;
            
            XmlDbConfigurationXmlFilePath = _uiConfigurationSettings.XMLDBConfigurationXmlFilePath;
            DriveName = _uiConfigurationSettings.DriveName;
            ExportPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _uiConfigurationSettings.ExportPath));
            InstrumentPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _uiConfigurationSettings.InstrumentPath));
            IsMinimizeButtonVisible = _uiConfigurationSettings.IsMinimizeButtonVisible;
            MasterDataXmlFilePath = _uiConfigurationSettings.MasterDataXmlFilePath;
            MaximumSearchCountForOpenSample = _uiConfigurationSettings.MaximumSearchCountForOpenSample;
            ResponseTimeOutSeconds = _uiConfigurationSettings.ResponseTimeOutSeconds;
            ShutDownTimeoutSecond = _uiConfigurationSettings.ShutDownTimeoutSecond;
            SystemBrightness = _uiConfigurationSettings.SystemBrightness;
            SystemStatusTimerInterval = _uiConfigurationSettings.SystemStatusTimerInterval;
            TimeoutHour = _uiConfigurationSettings.TimeoutHour;
            TimeoutMinute = _uiConfigurationSettings.TimeoutMinute;
            TimeoutSecond = _uiConfigurationSettings.TimeoutSecond;
            WorkQueueStatusTimer = _uiConfigurationSettings.WorkQueueStatusTimer;
            DiameterHistogramBinWidth = _uiConfigurationSettings.DiameterHistogramBinWidth;
            LicensesHtmlPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _uiConfigurationSettings.LicensesHtmlPath));
            HelpHtmlPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _uiConfigurationSettings.HelpHtmlPath));
            NightlyCleanTimeOutSeconds = _uiConfigurationSettings.NightlyCleanTimeOutSeconds;
            SetFocusTimeOutSec = _uiConfigurationSettings.SetFocusTimeOutSec;
            DustReffTimeOutSec = _uiConfigurationSettings.DustReffTimeOutSec;
            MaximumRecentMessages = _uiConfigurationSettings.MaximumRecentMessages;
            UseWindowedMode = _uiConfigurationSettings.WindowedMode;
            EnableSampleSetVcrControlsInOfflineMode = _uiConfigurationSettings.EnableSampleSetVcrControlsInOfflineMode;
            EnableLoggingForStatusWarnings = _uiConfigurationSettings.EnableLoggingForStatusWarnings;
            UseCarouselSimulation = _uiConfigurationSettings.UseCarouselSimulation;
        }

        private static void LoadEnvironmentConfigurationSettings()
        {
            var envirConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _environmentConfigurationSource);
            if (!File.Exists(envirConfig))
            {
                throw new FileNotFoundException(envirConfig);
            }

            ExeConfigurationFileMap map = new ExeConfigurationFileMap
            {
                ExeConfigFilename = envirConfig
            };

            Configuration environmentConfiguration = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            _environmentConfigurationSettings = environmentConfiguration.Sections.Get("environmentConfiguration") as EnvironmentConfigurationSettings;

            if (_environmentConfigurationSettings == null)
            {
                throw new ConfigurationErrorsException("Environment Configuration Failed To Load Successfully.");
            }

            IsFromHardware = Convert.ToBoolean(_environmentConfigurationSettings.IsFromHardware);
        }
    }
}