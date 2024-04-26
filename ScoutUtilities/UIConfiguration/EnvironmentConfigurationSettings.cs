using System.Configuration;

namespace ScoutUtilities.UIConfiguration
{
    /// <summary> 
    /// Loads and maps the configuration properties from the Environment Configuration Section defined in the loaded configuration.
    /// The Configuration Properties marked as "IsRequired = true" are the only required fields in the loaded configuration. 
    /// </summary>
    internal class EnvironmentConfigurationSettings : ConfigurationSection
    {
        [ConfigurationProperty("isFromHardware", IsRequired = true)]
        internal string IsFromHardware
        {
            get { return this["isFromHardware"].ToString(); }
            private set { this["isFromHardware"] = value; }
        }
    }
}