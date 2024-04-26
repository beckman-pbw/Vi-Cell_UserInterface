using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
    public class EnumToVisibility : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.Equals(parameter))
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}