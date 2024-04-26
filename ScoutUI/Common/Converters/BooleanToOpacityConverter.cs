using System;
using System.Globalization;

namespace ScoutUI.Common.Converters
{
    public class BooleanToOpacityConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? 1.0 : 0.5;
            }

            return 1.0;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(nameof(BooleanToOpacityConverter));
        }
    }
}