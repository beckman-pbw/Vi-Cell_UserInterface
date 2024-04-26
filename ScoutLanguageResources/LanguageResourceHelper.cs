using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using InputLanguage = System.Windows.Forms.InputLanguage;

namespace ScoutLanguageResources
{
    public class LanguageResourceHelper
    {
        static LanguageResourceHelper()
        {
            // By default, WPF uses en-US as the culture, regardless of the system settings.
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage(CurrentFormatCulture.IetfLanguageTag)));
        }

        public static string Get(string key) => string.IsNullOrEmpty(key) ? string.Empty : ResourceManager.GetString(key);

        public static void SetResourceManager(LanguageType languageId)
        {
            var cultureCode = LanguageEnums.GetLanguageCulture(languageId);
            var resourceFileName = languageId == LanguageType.eEnglishUS ? ResourceFile : cultureCode;

            ResourceBaseName = $"{ResourceLocation}.{resourceFileName}";

            CurrentDisplayCulture = CultureInfo.CurrentCulture;
            CurrentFormatCulture = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentDisplayCulture;
            Thread.CurrentThread.CurrentCulture = CurrentFormatCulture;

            ResourceManager = new ResourceManager(ResourceBaseName, ResourceAssembly);
        }

        public static void SetInputLanguage(LanguageType languageType)
        {
            var updatedLanguage = LanguageEnums.GetLanguageCulture(languageType);

            var culture = CultureInfo.GetCultureInfo(updatedLanguage);
            var language = InputLanguage.FromCulture(culture);
            InputLanguage.CurrentInputLanguage = InputLanguage.InstalledInputLanguages.IndexOf(language) >= 0
                ? language
                : InputLanguage.DefaultInputLanguage;
        }

        #region Properties

        // constants:
        public const string DefaultUserId = "DefaultSettings";
        public const string LanguageResourceName = "ScoutUILanguageResource"; // This should match the key for 'model:ApplicationLanguage' in App.xaml

        // lazy-loaded static properties:
        private static string _resourceBaseName;
        public static string ResourceBaseName
        {
            get
            {
                if (string.IsNullOrEmpty(_resourceBaseName)) _resourceBaseName = ResourceLocation + "." + "ResourceFile";
                return _resourceBaseName;
            }
            private set { _resourceBaseName = value; }
        }

        private static ResourceManager _resourceManager;
        public static ResourceManager ResourceManager
        {
            get
            {
                if (_resourceManager == null) _resourceManager = new ResourceManager(ResourceBaseName, ResourceAssembly);
                return _resourceManager;
            }
            private set { _resourceManager = value; }
        }

        private static CultureInfo _currentFormatCulture;
        public static CultureInfo CurrentFormatCulture
        {
            get
            {
                if (_currentFormatCulture == null) _currentFormatCulture = CultureInfo.CurrentCulture;
                return _currentFormatCulture;
            }
            private set { _currentFormatCulture = value; }
        }

        private static CultureInfo _currentDisplayCulture;
        public static CultureInfo CurrentDisplayCulture
        {
            get
            {
                if (_currentDisplayCulture == null) _currentDisplayCulture = CultureInfo.CurrentCulture;
                return _currentDisplayCulture;
            }
            private set { _currentDisplayCulture = value; }
        }

        private static string _resourceFile;
        public static string ResourceFile
        {
            get
            {
                if (string.IsNullOrEmpty(_resourceFile)) _resourceFile = nameof(LanguageResources.ResourceFile);
                return _resourceFile;
            }
            private set { _resourceFile = value; }
        }

        private static string _resourceLocation;
        public static string ResourceLocation
        {
            get
            {
                if (string.IsNullOrEmpty(_resourceLocation)) _resourceLocation = $"{ScoutLanguageResources}.{nameof(LanguageResources)}";
                return _resourceLocation;
            }
            private set { _resourceLocation = value; }
        }

        private static string _scoutLanguageResources;
        public static string ScoutLanguageResources
        {
            get
            {
                if (string.IsNullOrEmpty(_scoutLanguageResources)) _scoutLanguageResources = ResourceAssembly.GetName().Name;
                return _scoutLanguageResources;
            }
            private set { _scoutLanguageResources = value; }
        }

        private static Assembly _resourceAssembly;
        public static Assembly ResourceAssembly
        {
            get
            {
                if (_resourceAssembly == null) _resourceAssembly = typeof(LanguageResourceHelper).Assembly;
                return _resourceAssembly;
            }
            private set { _resourceAssembly = value; }
        }

        #endregion
    }
}
