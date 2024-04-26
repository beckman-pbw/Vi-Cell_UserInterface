using System.Configuration;

namespace ScoutUtilities.UIConfiguration
{
    /// <summary> 
    /// Loads the configuration properties from the UI Configuration Section defined in the loaded configuration.
    /// The Configuration Properties marked as "IsRequired = true" are the only required fields in the loaded configuration. 
    /// </summary> 
    internal class UIConfigurationSettings : ConfigurationSection
    {
        [ConfigurationProperty("xmlDBConfigurationXmlFilePath", IsRequired = true)]
        internal string XMLDBConfigurationXmlFilePath
        {
            get { return this["xmlDBConfigurationXmlFilePath"].ToString(); }
            private set { this["xmlDBConfigurationXmlFilePath"] = value; }
        }

        [ConfigurationProperty("deploymentConfigurationSource", IsRequired = true)]
        internal string DeploymentConfigurationSource
        {
            get { return this["deploymentConfigurationSource"].ToString(); }
            private set { this["deploymentConfigurationSource"] = value; }
        }

        [ConfigurationProperty("driveName", IsRequired = true)]
        internal string DriveName
        {
            get { return this["driveName"].ToString(); }
            private set { this["driveName"] = value; }
        }

//NOTE: not currently implemented...
        //[ConfigurationProperty("enableKioskMode", IsRequired = true)]
        //internal bool EnableKioskMode
        //{
        //    get { return (bool)this["enableKioskMode"]; }
        //    private set { this["enableKioskMode"] = value; }
        //}

        [ConfigurationProperty("environmentConfigurationSource", IsRequired = true)]
        internal string EnvironmentConfigurationSource
        {
            get { return this["environmentConfigurationSource"].ToString(); }
            private set { this["environmentConfigurationSource"] = value; }
        }

        [ConfigurationProperty("exportPath", IsRequired = true)]
        internal string ExportPath
        {
            get { return this["exportPath"].ToString(); }
            private set { this["exportPath"] = value; }
        }

        [ConfigurationProperty("instrumentPath", IsRequired = true)]
        internal string InstrumentPath
        {
            get { return this["instrumentPath"].ToString(); }
            private set { this["instrumentPath"] = value; }
        }

        [ConfigurationProperty("isMinimizeButtonVisible", IsRequired = true)]
        internal bool IsMinimizeButtonVisible
        {
            get { return (bool)this["isMinimizeButtonVisible"]; }
            private set { this["isMinimizeButtonVisible"] = value; }
        }

        [ConfigurationProperty("loggingConfigurationSource", IsRequired = true)]
        internal string LoggingConfigurationSource
        {
            get { return this["loggingConfigurationSource"].ToString(); }
            private set { this["loggingConfigurationSource"] = value; }
        }

        [ConfigurationProperty("masterDataXmlFilePath", IsRequired = true)]
        internal string MasterDataXmlFilePath
        {
            get { return this["masterDataXmlFilePath"].ToString(); }
            private set { this["masterDataXmlFilePath"] = value; }
        }

        [ConfigurationProperty("responseTimeOutSeconds", IsRequired = true)]
        internal int ResponseTimeOutSeconds
        {
            get { return (int)this["responseTimeOutSeconds"]; }
            private set { this["responseTimeOutSeconds"] = value; }
        }

        [ConfigurationProperty("maximumSearchCountForOpenSample", IsRequired = true)]
        internal int MaximumSearchCountForOpenSample
        {
            get { return (int)this["maximumSearchCountForOpenSample"]; }
            private set { this["maximumSearchCountForOpenSample"] = value; }
        }

        [ConfigurationProperty("shutDownTimeoutSecond", IsRequired = true)]
        internal int ShutDownTimeoutSecond
        {
            get { return (int)this["shutDownTimeoutSecond"]; }
            private set { this["shutDownTimeoutSecond"] = value; }
        }

        [ConfigurationProperty("systemBrightness", IsRequired = true)]
        internal string SystemBrightness
        {
            get { return this["systemBrightness"].ToString(); }
            private set { this["systemBrightness"] = value; }
        }

        [ConfigurationProperty("systemStatusTimerInterval", IsRequired = true)]
        internal int SystemStatusTimerInterval
        {
            get { return (int)this["systemStatusTimerInterval"]; }
            private set { this["systemStatusTimerInterval"] = value; }
        }

        [ConfigurationProperty("timeoutHour", IsRequired = true)]
        internal int TimeoutHour
        {
            get { return (int) this["timeoutHour"]; }
            private set { this["timeoutHour"] = value; }
        }

        [ConfigurationProperty("timeoutMinute", IsRequired = true)]
        internal int TimeoutMinute
        {
            get { return (int)this["timeoutMinute"]; }
            private set { this["timeoutMinute"] = value; }
        }

        [ConfigurationProperty("timeoutSecond", IsRequired = true)]
        internal int TimeoutSecond
        {
            get { return (int)this["timeoutSecond"]; }
            private set { this["timeoutSecond"] = value; }
        }

        [ConfigurationProperty("workQueueStatusTimer", IsRequired = true)]
        internal int WorkQueueStatusTimer
        {
            get { return (int)this["workQueueStatusTimer"]; }
            private set { this["workQueueStatusTimer"] = value; }
        }

        [ConfigurationProperty("diameterHistogramBinWidth", IsRequired = true)]
        internal double DiameterHistogramBinWidth
        {
            get { return (double)this["diameterHistogramBinWidth"]; }
            private set { this["diameterHistogramBinWidth"] = value; }
        }

        [ConfigurationProperty("licensesHtmlPath", IsRequired = true)]
        internal string LicensesHtmlPath
        {
            get { return (string)this["licensesHtmlPath"]; }
            private set { this["licensesHtmlPath"] = value; }
        }
        
        [ConfigurationProperty("helpHtmlPath", IsRequired = true)]
        internal string HelpHtmlPath
        {
            get { return (string)this["helpHtmlPath"]; }
            private set { this["helpHtmlPath"] = value; }
        }
       
        [ConfigurationProperty("nightlyCleanTimeOutSeconds", IsRequired = true)]
        internal int NightlyCleanTimeOutSeconds
        {
            get { return (int)this["nightlyCleanTimeOutSeconds"]; }
            private set { this["nightlyCleanTimeOutSeconds"] = value; }
        }

        [ConfigurationProperty("setFocusTimeOutSec", IsRequired = true)]
        internal int SetFocusTimeOutSec
        {
            get { return (int)this["setFocusTimeOutSec"]; }
            private set { this["setFocusTimeOutSec"] = value; }
        }
        [ConfigurationProperty("dustReffTimeOutSec", IsRequired = true)]
        internal int DustReffTimeOutSec
        {
            get { return (int)this["dustReffTimeOutSec"]; }
            private set { this["dustReffTimeOutSec"] = value; }
        }

        [ConfigurationProperty("maximumRecentMessages", IsRequired = true)]
        internal int MaximumRecentMessages
        {
            get { return (int)this["maximumRecentMessages"]; }
            private set { this["maximumRecentMessages"] = value; }
        }

        [ConfigurationProperty("useWindowedMode", IsRequired = false)]
        internal bool WindowedMode
        {
            get { return (bool)this["useWindowedMode"]; }
            private set { this["useWindowedMode"] = value; }
        }

        [ConfigurationProperty("overrideEssVcrControls", IsRequired = false)]
        internal bool EnableSampleSetVcrControlsInOfflineMode
        {
            get { return (bool)this["overrideEssVcrControls"]; }
            private set { this["overrideEssVcrControls"] = value; }
        }

        [ConfigurationProperty("enableLoggingForStatusWarnings", IsRequired = false)]
        internal bool EnableLoggingForStatusWarnings
        {
            get { return (bool)this["enableLoggingForStatusWarnings"]; }
            private set { this["enableLoggingForStatusWarnings"] = value; }
        }

        [ConfigurationProperty("useCarouselSimulation", IsRequired = true)]
        internal bool UseCarouselSimulation
		{
	        get { return (bool)this["useCarouselSimulation"]; }
	        private set { this["useCarouselSimulation"] = value; }
        }

    }
}