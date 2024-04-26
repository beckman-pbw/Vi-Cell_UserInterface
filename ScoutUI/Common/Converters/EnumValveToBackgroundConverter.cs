using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ScoutUI.Common.Converters
{
    public class EnumValveToBackgroundConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.Equals(parameter))
            {
                var mySolidColorBrush = (SolidColorBrush) Application.Current.Resources["Level1Background"];
                return mySolidColorBrush;
            }
            else
            {
                var mySolidColorBrush = (SolidColorBrush) Application.Current.Resources["ValveColor"];
                return mySolidColorBrush;
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && ((bool) value) ? parameter : Binding.DoNothing;
        }
    }
}