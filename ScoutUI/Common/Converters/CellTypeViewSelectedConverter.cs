using System;
using System.Globalization;
using ScoutViewModels.ViewModels.CellTypes;

namespace ScoutUI.Common.Converters
{
    public class CellTypeViewSelectedConverter : BaseValueConverter
    {
        /// <summary>
        /// If the value and parameter are CellTypeViewSelection enums and they match,
        /// then that UIElement is the checked one.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>UIElement's IsChecked value</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CellTypeViewSelection selection && parameter is string paramStr)
            {
                if (Enum.TryParse(paramStr, out CellTypeViewSelection parsed))
                {
                    return selection.Equals(parsed);
                }
            }

            return false;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}