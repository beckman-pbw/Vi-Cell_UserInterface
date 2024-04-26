using System;
using System.Globalization;
using System.Windows.Data;
using ScoutUtilities.Enums;

namespace ScoutUI.Common.Converters
{
    public class ExpanderToBooleanConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter != null && value != null && (ExpanderType) value == (ExpanderType) parameter;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.Convert.ToBoolean(value))
                return parameter;
            return ExpanderType.None;
        }
    }
}