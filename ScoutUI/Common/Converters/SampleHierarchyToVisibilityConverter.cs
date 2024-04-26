using ScoutUtilities.Enums;
using System;
using System.Globalization;
using System.Windows;

namespace ScoutUI.Common.Converters
{
    public class SampleHierarchyToVisibilityConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SampleHierarchyType hierarchyType && parameter is string strParameter)
            {
                switch (hierarchyType)
                {
                    case SampleHierarchyType.None:
                        return Visibility.Collapsed;
                    case SampleHierarchyType.Parent:
                        return strParameter.Equals("ArrowDown") ? Visibility.Visible : Visibility.Collapsed;
                    case SampleHierarchyType.Child:
                        return strParameter.Equals("ArrowRight") ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            return Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(nameof(SampleHierarchyToVisibilityConverter) + "." + nameof(ConvertBack));
        }
    }
}