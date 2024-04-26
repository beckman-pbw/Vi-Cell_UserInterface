using ScoutUtilities.Enums;
using System;
using System.Globalization;
using System.Windows;

namespace ScoutUI.Common.Converters
{
    public class SeqNamingToVisibilityConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SequentialNamingType seq && parameter is string strParam)
            {
                switch (seq)
                {
                    case SequentialNamingType.Text:
                        if (strParam.Equals("Text")) return Visibility.Visible;
                        break;
                    case SequentialNamingType.Integer:
                        if (strParam.Equals("Integer")) return Visibility.Visible;
                        break;
                }
            }

            return Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException($"{nameof(SeqNamingToVisibilityConverter)}.{nameof(ConvertBack)}");
        }
    }
}