using System;
using System.Globalization;
using System.Windows;

namespace ScoutUI.Common.Converters
{
    public class ObjectToBoolConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value != null);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException($"{nameof(ObjectToVisibilityConverter)}.{nameof(ConvertBack)}");
        }
    }
}