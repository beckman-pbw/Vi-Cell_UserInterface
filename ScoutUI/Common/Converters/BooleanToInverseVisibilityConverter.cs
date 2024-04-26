using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
  
    public class BooleanToInverseVisibilityConverter : BaseValueConverter
    {
      
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value != null && (bool) value ? Visibility.Collapsed : Visibility.Visible;
            return result;
        }

     
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}