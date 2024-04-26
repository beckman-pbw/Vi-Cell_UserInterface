using ScoutLanguageResources;
using ScoutUtilities.Enums;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
    public class EnumToLocalizedDescriptionConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString())) return string.Empty;

            var myEnum = (Enum)value;
            var description = EnumToLocalization.GetLocalizedDescription(myEnum);
            return description;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}