using ScoutUtilities.Enums;
using System;
using System.Globalization;
using System.Windows;

namespace ScoutUI.Common.Converters
{
    public class FilterItemParamToVisibilityConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return Visibility.Collapsed;

            var parsedFilterParam = Enum.Parse(typeof(eFilterItem), parameter?.ToString(), true);

            if (value is eFilterItem eFilterItemValue && parsedFilterParam is eFilterItem eFilterItemParam &&
                eFilterItemValue.Equals(eFilterItemParam))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}