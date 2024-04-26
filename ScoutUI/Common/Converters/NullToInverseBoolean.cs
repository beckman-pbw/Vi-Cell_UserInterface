using System;
using System.Globalization;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
    public class NullToInverseBoolean : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value == null) || value.Equals("Empty") || value.Equals("") || ((int) value == 0) ||
                string.IsNullOrWhiteSpace(value.ToString()))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}