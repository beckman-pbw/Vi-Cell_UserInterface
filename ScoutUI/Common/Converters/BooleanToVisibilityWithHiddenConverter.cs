using System;
using System.Windows;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
    class BooleanToVisibilityWithHiddenConverter : BaseValueConverter
    {
      
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool && (bool) value)
            {
                return Visibility.Visible;
            }
            return Visibility.Hidden;
        }

        public override object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is Visibility && Visibility.Hidden == (Visibility) value)
            {
                return false;
            }
            return true;
        }
    }
}