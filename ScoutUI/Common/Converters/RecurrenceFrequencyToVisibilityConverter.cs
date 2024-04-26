using ScoutUtilities.Enums;
using System;
using System.Globalization;
using System.Windows;

namespace ScoutUI.Common.Converters
{
    public class RecurrenceFrequencyToVisibilityConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var paramStr = parameter?.ToString();
            if (string.IsNullOrEmpty(paramStr))
                return Visibility.Collapsed;

            if (paramStr.StartsWith("!"))
            {
                if (Enum.TryParse<RecurrenceFrequency>(paramStr.Substring(1), out var recFreqParam))
                {
                    if (value is RecurrenceFrequency frequency)
                    {
                        if (!frequency.Equals(recFreqParam))
                            return Visibility.Visible;
                    }
                }
            }
            else
            {
                if (Enum.TryParse<RecurrenceFrequency>(paramStr, out var recFreqParam))
                {
                    if (value is RecurrenceFrequency frequency)
                    {
                        if (frequency.Equals(recFreqParam))
                            return Visibility.Visible;
                    }
                }
            }
            

            return Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
            // @todo implement this function 
            //if (value is Visibility visibility && parameter is RecurrenceFrequency freqParam)
            //{
            //    if (visibility == Visibility.Visible)
            //        return freqParam;
            //}

            //return null;
        }
    }
}