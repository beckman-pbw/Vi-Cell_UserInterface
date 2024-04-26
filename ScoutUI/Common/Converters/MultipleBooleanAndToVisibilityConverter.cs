using System;
using System.Globalization;
using System.Windows;


namespace ScoutUI.Common.Converters
{
    public class MultipleBooleanAndToVisibilityConverter : BaseMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var value in values)
            {
                if (value is bool b && !b)
                    return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
