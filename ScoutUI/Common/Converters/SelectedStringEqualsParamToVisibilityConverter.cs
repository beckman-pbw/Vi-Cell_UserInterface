using System;
using System.Globalization;
using System.Windows;
using ScoutLanguageResources;

namespace ScoutUI.Common.Converters
{
    public class SelectedStringEqualsParamToVisibilityConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue && parameter is string strParameter)
            {
                if (strValue.Equals(strParameter)) return Visibility.Visible;

                var localStrValue = LanguageResourceHelper.Get(strValue) ?? string.Empty;
                var localStrParam = LanguageResourceHelper.Get(strParameter) ?? string.Empty;
                if (strValue.Equals(localStrParam)) return Visibility.Visible;
                if (localStrValue.Equals(strParameter)) return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}