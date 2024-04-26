using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
    public class EmptyStringToCollapsedConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.ToString() == string.Empty)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
