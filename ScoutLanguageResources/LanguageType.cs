using log4net;
using System;

namespace ScoutLanguageResources
{
    public enum LanguageType
    {
        eEnglishUS,
        eFrench,
        eJapanese,
        eGerman,
        eChinese,
        eSpanish,
    }

    public class LanguageEnums
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetLanguageCulture(LanguageType languageId)
        {
            switch (languageId)
            {
                case LanguageType.eEnglishUS:
                    return "en-US";
                case LanguageType.eFrench:
                    return "fr-FR";
                case LanguageType.eJapanese:
                    return "ja-JP";
                case LanguageType.eGerman:
                    return "de-DE";
                case LanguageType.eChinese:
                    return "zh-CN";
                case LanguageType.eSpanish:
                    return "es-ES";
                default:
                    Log.Error("Invalid Language Type!", new NotImplementedException(Environment.StackTrace));
                    return "en-US"; // should we throw an exception instead???
            }
        }

        public static LanguageType GetLangType(string lang)
        {
            switch (lang)
            {
                case "en-US":
                    return LanguageType.eEnglishUS;
                case "fr-FR":
                    return LanguageType.eFrench;
                case "ja-JP":
                    return LanguageType.eJapanese;
                case "de-DE":
                    return LanguageType.eGerman;
                case "zh-CN":
                    return LanguageType.eChinese;
                case "es-ES":
                    return LanguageType.eSpanish;
            }
            return LanguageType.eEnglishUS;
        }
    }
}