using System;
using System.Globalization;
using System.Windows.Data;
using ScoutUtilities;

namespace ScoutUI.Common.Converters
{
   
    public class DoubleToPowerSixConverter : BaseValueConverter
    {
      
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double result = 0;
            if (value != null)
            {
                 result = (double) value;
            }
            return Misc.ConvertToPower(result, culture);
        }

      
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
