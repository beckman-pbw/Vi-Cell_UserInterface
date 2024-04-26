using ScoutLanguageResources;
using System;
using System.Globalization;

namespace ScoutUI.Common.Converters
{
    public class CellTypeToTextConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty((string)value))
            {
                return LanguageResourceHelper.Get("LID_Label_CellTypeRemoved");
            }

            return value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
