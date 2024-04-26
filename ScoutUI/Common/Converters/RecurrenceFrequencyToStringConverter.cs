using ScoutLanguageResources;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ScoutUI.Common.Converters
{
    public class RecurrenceFrequencyToStringConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList<RecurrenceFrequency> freqList)
            {
                var stringList = new List<string>();
                foreach (var item in freqList)
                {
                    stringList.Add(EnumToLocalization.GetLocalizedDescription(item));
                }
                return stringList;
            }

            if (value is RecurrenceFrequency frequency)
            {
                return EnumToLocalization.GetLocalizedDescription(frequency);
            }

            return string.Empty;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                if (strValue.Equals(LanguageResourceHelper.Get("LID_Label_RecurrenceFreq_Once")))
                    return RecurrenceFrequency.Once;
                if (strValue.Equals(LanguageResourceHelper.Get("LID_Label_RecurrenceFreq_Daily")))
                    return RecurrenceFrequency.Daily;
                if (strValue.Equals(LanguageResourceHelper.Get("LID_Label_RecurrenceFreq_Weekly")))
                    return RecurrenceFrequency.Weekly;
                if (strValue.Equals(LanguageResourceHelper.Get("LID_Label_RecurrenceFreq_Monthly")))
                    return RecurrenceFrequency.Monthly;
            }

            return null;
        }
    }
}