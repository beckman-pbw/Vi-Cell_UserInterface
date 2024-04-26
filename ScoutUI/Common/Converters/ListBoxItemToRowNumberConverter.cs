using System;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
    public class ListBoxItemToRowNumberConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CollectionViewSource collectionViewSource = parameter as CollectionViewSource;
            int counter = 1;
            if (collectionViewSource != null)
            {
                foreach (object item in collectionViewSource.View)
                {
                    if (item == value)
                    {
                        return counter.ToString();
                    }
                    counter++;
                }
            }
            return string.Empty;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
