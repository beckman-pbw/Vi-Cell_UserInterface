using System;
using System.Globalization;
using System.Windows.Data;
using ScoutUtilities.Enums;

namespace ScoutUI.Common.Converters
{
    public class TrailingPointConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return UpdateTrailingPoint(value, parameter);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return UpdateTrailingPoint(value, parameter);
        }

        private static object UpdateTrailingPoint(object value, object parameter)
        {
            double data;
            if (value == null || parameter == null 
                              || !double.TryParse(value.ToString(), out data))
                return null;
            var point = (TrailingPoint) parameter;
            return ScoutUtilities.Misc.UpdateTrailingPoint(data, point);
        }
    }
}