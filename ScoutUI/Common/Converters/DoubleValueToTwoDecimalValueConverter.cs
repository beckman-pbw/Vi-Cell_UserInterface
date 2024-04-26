using System;
using System.Globalization;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
    public class DoubleValueToTwoDecimalValueConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double resultValue = 0.0;
            if (value == null)
                return null;
            if (double.TryParse(value.ToString(), out resultValue))
            {
                resultValue = Math.Round(resultValue, 4);
                return resultValue;
            }

            return value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}