using ScoutUtilities.Enums;
using System;
using System.Globalization;

namespace ScoutUI.Common.Converters
{

    public class ViableConcentrationConverter : BaseValueConverter
    {
       
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null
                              || !int.TryParse(parameter.ToString(),out var divider)
                              || !double.TryParse(value.ToString(), out var data))
                return ScoutUtilities.Misc.UpdateTrailingPoint(default(double), ScoutUtilities.Misc.ConcDisplayDigits);
            var result = data / divider;
            return ScoutUtilities.Misc.UpdateTrailingPoint(result, ScoutUtilities.Misc.ConcDisplayDigits);
        }

       
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
