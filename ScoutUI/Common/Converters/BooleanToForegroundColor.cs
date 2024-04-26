using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ScoutUI.Common.Converters
{
  
    public class BooleanToForegroundColor : BaseValueConverter
    {
    
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (bool) value)
                return new SolidColorBrush(Colors.Black);
            return new SolidColorBrush(Colors.Gray);
        }

       
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}