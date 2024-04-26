using System;
using System.Globalization;

namespace ScoutUI.Common.Converters
{
    public class ParamMatchesBindingConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && parameter == null) return true;
            if (value == null || parameter == null) return false;

            return value.ToString().Equals(parameter.ToString());
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;

            return !value.ToString().Equals(parameter.ToString());
        }
    }
}