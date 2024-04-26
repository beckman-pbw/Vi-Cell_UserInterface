using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using ScoutLanguageResources;
using ScoutUtilities.Enums;

namespace ScoutUI.Common.Converters
{
    public class ClockFormatToStringConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList<ClockFormat> clockList)
            {
                var stringList = new List<string>();
                foreach (var item in clockList)
                {
                    stringList.Add(EnumToLocalization.GetLocalizedDescription(item));
                }
                return stringList;
            }

            if (value is ClockFormat clockFormat)
            {
                return EnumToLocalization.GetLocalizedDescription(clockFormat);
            }

            return string.Empty;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                if (strValue.Equals(LanguageResourceHelper.Get("LID_Enum_Hour24")))
                    return ClockFormat.Hour24;
                if (strValue.Equals(LanguageResourceHelper.Get("LID_Enum_AM")))
                    return ClockFormat.AM;
                if (strValue.Equals(LanguageResourceHelper.Get("LID_Enum_PM")))
                    return ClockFormat.PM;
            }

            return null;
        }
    }
}