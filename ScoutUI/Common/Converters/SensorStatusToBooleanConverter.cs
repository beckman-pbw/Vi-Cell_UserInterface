using ScoutUtilities.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
    public class SensorStatusToBooleanConverter : BaseValueConverter
    {
     
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool returnValue = false;
            if (value != null)
            {
                var result = (eSensorStatus) value;
                switch (result)
                {
                    case eSensorStatus.ssStateUnknown:
                        returnValue = false;
                        break;
                    case eSensorStatus.ssStateActive:
                        returnValue = true;
                        break;
                    case eSensorStatus.ssStateInactive:
                        returnValue = false;
                        break;
                    default:
                        returnValue = false;
                        break;
                }
            }

            return returnValue;
        }

    
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (bool) value)
                return eSensorStatus.ssStateActive;
            return eSensorStatus.ssStateInactive;
        }
    }
}