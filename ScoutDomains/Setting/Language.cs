using ScoutLanguageResources;

namespace ScoutDomains
{
    public class Language
    {
        public LanguageType LanguageID { get; set; }

        public string LanguageName { get; set; }

        public string LanguageCulture { get; set; }

        public bool IsActive { get; set; }
    }
}
