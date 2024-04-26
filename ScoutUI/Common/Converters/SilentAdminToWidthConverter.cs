using System;
using System.Globalization;

namespace ScoutUI.Common.Converters
{
    public class SilentAdminToWidthConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? 180 : (object)((bool)value ? 130 : 180);
        }


        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
