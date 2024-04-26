using ScoutLanguageResources;
using ScoutUtilities.Common;

namespace ScoutDomains
{
    public class GeneralSettingsDomain : NotifyPropertyChanges
    {
        public string SelectedLanguage { get; set; }

        public string SelectedTimeZone { get; set; }

        public bool IsDayLightSavings { get; set; }

        public LanguageType LanguageID { get; set; }
    }
}
