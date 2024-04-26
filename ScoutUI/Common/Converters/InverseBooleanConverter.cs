using System;
using System.Globalization;

namespace ScoutUI.Common.Converters
{
    public class InverseBooleanConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return InverseValue(value);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return InverseValue(value);
        }
      
        private bool InverseValue(object value)
        {
            bool val = (bool) value;
            return !val;
        }
    }
}