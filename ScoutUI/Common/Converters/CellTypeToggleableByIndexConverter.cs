using System;

namespace ScoutUI.Common.Converters
{
    public class CellTypeToggleableByIndexConverter : BaseMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (values == null || values.Length < 2 || !(values[0] is bool b))
                return false;

            if (b && values[1] is uint u)
            {
                return u != 0;
            }

            return false;
        }

        public override object[] ConvertBack(object values, Type[] targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}


