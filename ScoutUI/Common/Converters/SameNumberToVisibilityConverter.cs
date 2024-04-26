using System;
using System.Globalization;
using System.Windows;

namespace ScoutUI.Common.Converters
{
    public class SameNumberToVisibilityConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i && int.TryParse(parameter?.ToString(), out var param))
            {
                if (i.Equals(param)) return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }
    }
}