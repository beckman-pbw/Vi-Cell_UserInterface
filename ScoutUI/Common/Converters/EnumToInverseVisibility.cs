using System;
using System.Globalization;
using System.Windows;

namespace ScoutUI.Common.Converters
{
    public class EnumToInverseVisibility : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.Equals(parameter))
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}