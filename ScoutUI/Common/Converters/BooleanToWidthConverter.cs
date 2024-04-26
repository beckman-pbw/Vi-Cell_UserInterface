using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
    class BooleanToWidthConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && ((bool) value) ? (object) GridLength.Auto : 0;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}