using ScoutUtilities.Enums;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
    class SensorStatusInActiveToVisibilityConverter : BaseValueConverter
    {
       
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var result = (eSensorStatus) value;
                switch (result)
                {
                    case eSensorStatus.ssStateUnknown:
                        return Visibility.Hidden;
                    case eSensorStatus.ssStateActive:
                        return Visibility.Hidden;
                    case eSensorStatus.ssStateInactive:
                        return Visibility.Visible;
                    default:
                        return Visibility.Hidden;
                }
            }

            return Visibility.Hidden;
        }

      
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var result = (Visibility) value;

                switch (result)
                {
                    case Visibility.Visible:
                        return eSensorStatus.ssStateInactive;
                    case Visibility.Hidden:
                        return eSensorStatus.ssStateActive;
                    case Visibility.Collapsed:
                        return eSensorStatus.ssStateActive;
                    default:
                        return eSensorStatus.ssStateActive;
                }
            }

            return eSensorStatus.ssStateActive;
        }
    }
}