using System;
using System.Globalization;
using System.Windows.Data;
using ScoutUtilities.Enums;
using System.Windows.Media;

namespace ScoutUI.Common.Converters
{
    public class PlateStatusToColorConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Int32)
            {
                return (SolidColorBrush)new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription((SampleStatusColor)value));
            }

            if (value != null)

            {
                return (SolidColorBrush)new BrushConverter().ConvertFrom(GetEnumDescription.GetDescription((SampleStatusColor)value));
            }

            return null;
        }


        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}