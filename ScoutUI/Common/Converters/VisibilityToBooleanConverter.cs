using System;
using System.Windows;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
  
    public class VisibilityToBooleanConverter : BaseValueConverter
    {
      
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Visibility && Visibility.Collapsed == (Visibility) value)
            {
                return false;
            }
            return true;
        }

      
        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool && (bool) value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }
    }
}