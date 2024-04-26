﻿using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
   
    public class CountToInverseVisibility : BaseValueConverter
    {
       
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var items = value as IEnumerable;

            var result = items == null || !items.Cast<object>().Any() ? Visibility.Collapsed : Visibility.Visible;
            return result;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}