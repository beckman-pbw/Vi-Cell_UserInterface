using ScoutLanguageResources;
using ScoutUtilities.Common;
using ScoutUtilities.Helper;
using System;
using System.Windows;

namespace ScoutModels.Common
{
    public class ApplicationLanguage : BaseNotifyPropertyChanged
    {
        #region Properties

        private static string _selectedLanguage;
        public string SelectedLanguage
        {
            get { return _selectedLanguage; }
            private set
            {
                if (string.IsNullOrEmpty(value) || string.Equals(_selectedLanguage, value))
                    return;
                _selectedLanguage = value;
                NotifyPropertyChanged(nameof(ResourceKeys));
            }
        }

        private ResourceHelper _resourceKeys;
        public ResourceHelper ResourceKeys
        {
            get { return _resourceKeys; }
            private set
            {
                if (value == _resourceKeys)
                    return;
                _resourceKeys = value;
                NotifyPropertyChanged(nameof(ResourceKeys));
            }
        }

        #endregion

        public ApplicationLanguage() // called by some XAML files (as a resource)
        {
            // We need to check the settings for the default language to use
            LanguageType lang;
            // Only get the lang once, after it's set don't read from persitent storage 
            if (string.IsNullOrEmpty(_selectedLanguage))
            {
                lang = (LanguageType)Enum.Parse(typeof(LanguageType), XmlHelper.GetSelectedLanguageForCulture());
            } else
            {
                lang = LanguageEnums.GetLangType(SelectedLanguage);
            }
            ResourceKeys = new ResourceHelper();
            LanguageResourceHelper.SetInputLanguage(lang);
            LanguageResourceHelper.SetResourceManager(lang);
            SelectedLanguage = LanguageEnums.GetLanguageCulture(lang);
        }

        public static void SetLanguage(LanguageType newLanguage) // called by GeneralSettings
        {
            LanguageResourceHelper.SetInputLanguage(newLanguage);
            LanguageResourceHelper.SetResourceManager(newLanguage);
            var cultureCode = LanguageEnums.GetLanguageCulture(newLanguage);
            // reset any existing XAML resource(s) (usually for the currently loaded page)
            var applicationLanguageObj = (ApplicationLanguage) Application.Current.Resources[LanguageResourceHelper.LanguageResourceName];
            applicationLanguageObj.SelectedLanguage = cultureCode;
        }

        public static string GetLanguage()
        {
            var applicationLanguageObj = (ApplicationLanguage)Application.Current.Resources[LanguageResourceHelper.LanguageResourceName];
            return applicationLanguageObj.SelectedLanguage;
        }
    }
}