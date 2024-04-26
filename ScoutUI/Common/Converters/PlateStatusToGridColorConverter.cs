using ScoutUtilities.Enums;
using System;
using System.Globalization;
using System.Windows.Media;

namespace ScoutUI.Common.Converters
{
    public class PlateStatusToGridColorConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (SampleStatusColor) value == SampleStatusColor.Empty)
            {
                return (SolidColorBrush) new BrushConverter().ConvertFrom("#0000");
            }

            if (value != null)
            {
                return (SolidColorBrush)
                    new BrushConverter().ConvertFrom(
                        GetEnumDescription.GetDescription((SampleStatus) value));
            }

            return null;
        }


        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}