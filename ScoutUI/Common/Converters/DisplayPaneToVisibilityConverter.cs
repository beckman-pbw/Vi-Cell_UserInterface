using ScoutUtilities.Enums;
using System;
using System.Globalization;
using System.Windows;

namespace ScoutUI.Common.Converters
{
    public class DisplayPaneToVisibilityConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DisplayPane displayPane && !string.IsNullOrEmpty(parameter?.ToString()))
            {
                if (Enum.TryParse(parameter.ToString(), out DisplayPane parameterDisplayPane))
                {
                    return displayPane == parameterDisplayPane ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            return Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}