using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
    public class WindowsStateConverter : BaseValueConverter
    {
       
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && (bool) value ? WindowState.Maximized : WindowState.Minimized;
        }

     
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value.Equals(WindowState.Maximized);
        }
    }
}