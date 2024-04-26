using System;
using System.Globalization;
using System.Windows;

namespace ScoutUI.Common.Converters
{
  
    public class BooleanToVisibilityConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value != null && (bool) value ? Visibility.Visible : Visibility.Collapsed;
            return result;
        }

    
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value != null && ((Visibility) value == Visibility.Visible);
            return result;
        }
    }
}