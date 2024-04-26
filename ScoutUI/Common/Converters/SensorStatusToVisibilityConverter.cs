using ScoutUtilities.Enums;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
    class SensorStatusToVisibilityConverter : BaseValueConverter
    {
       
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Visible;
            var result = (eSensorStatus) value;
            return result.Equals(eSensorStatus.ssStateInactive) ? Visibility.Hidden : Visibility.Visible;

        }

     
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var result = (Visibility) value;

                switch (result)
                {
                    case Visibility.Visible:
                        return eSensorStatus.ssStateActive;
                    case Visibility.Hidden:
                        return eSensorStatus.ssStateInactive;
                    case Visibility.Collapsed:
                        return eSensorStatus.ssStateInactive;
                    default:
                        return eSensorStatus.ssStateInactive;
                }
            }

            return eSensorStatus.ssStateInactive;
        }
    }
}