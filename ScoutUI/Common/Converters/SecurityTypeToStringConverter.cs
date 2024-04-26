using ScoutLanguageResources;
using ScoutUtilities.Enums;
using System;
using System.Globalization;

namespace ScoutUI.Common.Converters
{
    public class SecurityTypeToStringConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SecurityType securityType)
            {
                switch (securityType)
                {
                    case SecurityType.NoSecurity: return LanguageResourceHelper.Get("LID_Enum_SecuritySettings_None");
                    case SecurityType.LocalSecurity: return LanguageResourceHelper.Get("LID_Enum_SecuritySettings_LocalUsers");
                    case SecurityType.ActiveDirectory: return LanguageResourceHelper.Get("LID_Enum_SecuritySettings_ActiveDirectory");
                }
            }

            return string.Empty;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                if (str.Equals(LanguageResourceHelper.Get("LID_Enum_SecuritySettings_None"))) return SecurityType.NoSecurity;
                if (str.Equals(LanguageResourceHelper.Get("LID_Enum_SecuritySettings_LocalUsers"))) return SecurityType.LocalSecurity;
                if (str.Equals(LanguageResourceHelper.Get("LID_Enum_SecuritySettings_ActiveDirectory"))) return SecurityType.ActiveDirectory;
            }

            return null;
        }
    }
}