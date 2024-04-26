using System;
using System.Globalization;
using System.Windows;

namespace ScoutUI.Common.Converters
{
    public class InverseVisibilityConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;

            if (value is Visibility)
            {
                return ((Visibility)value == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
            }
            
            if (value is bool)
            {
                return (bool)value ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}