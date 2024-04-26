using System;
using System.Collections.Generic;
using System.Globalization;
using ScoutLanguageResources;
using ScoutUtilities.Enums;

namespace ScoutUI.Common.Converters
{
    public class WeekdayEnumToStringConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList<Weekday> weekdays)
            {
                var stringList = new List<string>();
                foreach (var day in weekdays)
                {
                    stringList.Add(EnumToLocalization.GetLocalizedDescription(day));
                }
                return stringList;
            }

            if (value is Weekday frequency)
            {
                return EnumToLocalization.GetLocalizedDescription(frequency);
            }

            return string.Empty;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                if (strValue.Equals(LanguageResourceHelper.Get("LID_Label_Weekday_Sunday")))
                    return Weekday.Sunday;
                if (strValue.Equals(LanguageResourceHelper.Get("LID_Label_Weekday_Monday")))
                    return Weekday.Monday;
                if (strValue.Equals(LanguageResourceHelper.Get("LID_Label_Weekday_Tuesday")))
                    return Weekday.Tuesday;
                if (strValue.Equals(LanguageResourceHelper.Get("LID_Label_Weekday_Wednesday")))
                    return Weekday.Wednesday;
                if (strValue.Equals(LanguageResourceHelper.Get("LID_Label_Weekday_Thursday")))
                    return Weekday.Thursday;
                if (strValue.Equals(LanguageResourceHelper.Get("LID_Label_Weekday_Friday")))
                    return Weekday.Friday;
                if (strValue.Equals(LanguageResourceHelper.Get("LID_Label_Weekday_Saturday")))
                    return Weekday.Saturday;
            }

            return null;
        }
    }
}