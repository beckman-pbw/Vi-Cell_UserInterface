using ScoutDataAccessLayer.DAL;
using ScoutLanguageResources;
using ScoutDomains;
using ScoutUtilities;
using ScoutUtilities.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace ScoutModels.Settings
{
    public class GeneralSettingsModel
    {
        public GeneralSettingsModel()
        {
            XMLDataAccess = XMLDataAccess.Instance;
        }

        public XMLDataAccess XMLDataAccess { get; set; }

        public ReadOnlyCollection<TimeZoneInfo> TimeZoneList => TimeZoneInfo.GetSystemTimeZones();

        public GeneralSettingsDomain GeneralSettings { get; set; }

        public DateTime CurrentDate { get; set; } = DateTime.Now;

        private List<Language> _languageList;
        public List<Language> LanguageList
        {
            get { return _languageList ?? (_languageList = new List<Language>()); }
            set { _languageList = value; }
        }

        public string SystemTimeHours { get; set; } = Misc.ConvertToString(DateTime.Now.Hour);

        public string SystemTimeMinutes { get; set; } = Misc.ConvertToString(DateTime.Now.Minute);

        public string SystemTimeSeconds { get; set; } = Misc.ConvertToString(DateTime.Now.Second);


        public void UpdateLanguageList(string userId)
        {
            GeneralSettings = GetGeneralSettings(userId);
            LanguageList = GetAvailableLanguages();
            LanguageList.Where(x => x.LanguageID == GeneralSettings.LanguageID).ToList()
                .ForEach(x => x.IsActive = true);
        }

        private GeneralSettingsDomain GetGeneralSettings(string userId)
        {
            bool userFound = false;
            if ((userId != null) && (userId.Length > 0))
            {
                var statusData = XMLDataAccess.ReadConfigurationData<GeneralSettingsDomain>(userId,
                    typeof(GeneralSettingsDomain).Name.Replace("Domain", string.Empty), out userFound);
                if (!userFound)
                    XMLDataAccess.WriteToConfigurationXML(GeneralSettings, userId, null);
                return statusData;
            }
            else
            {
                var statusData = XMLDataAccess.ReadConfigurationData<GeneralSettingsDomain>(ApplicationConstants.DefaultSettings,
                    typeof(GeneralSettingsDomain).Name.Replace("Domain", string.Empty), out userFound);

                return statusData;
            }
        }

        public LanguageType GetUserLanguage(string userId)
        {
            GeneralSettingsDomain data = GetGeneralSettings(userId);
            return data.LanguageID;
        }

        public List<Language> GetAvailableLanguages()
        {
            return XMLDataAccess.ReadMasterData<List<Language>>();
        }

        public bool SaveUserLanguage(string UserID)
        {
            // this happens when a user logs in and when the settings change
            if (UserID.Length > 0)
            {
                XMLDataAccess.WriteToConfigurationXML(GeneralSettings, UserID, null);
                Common.ApplicationLanguage.SetLanguage(GeneralSettings.LanguageID);
                return true;
            }
            return false;
        }
    }
}