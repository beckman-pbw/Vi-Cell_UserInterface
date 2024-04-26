// ***********************************************************************
// Assembly         : ScoutModels
// Author           : 20128398
// Created          : 19-02-2019
//
// Last Modified By : 20115954
// Last Modified On : 19-02-2019
// ***********************************************************************
// <copyright file="SampleStatusToVisibilityConverter.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using ScoutUtilities.Enums;
using System;
using System.Globalization;
using System.Windows;

namespace ScoutUI.Common.Converters
{
    public class SampleStatusToVisibilityConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var status = (SampleStatusColor)value;
                switch (status)
                {
                    case SampleStatusColor.Empty:
                        return Visibility.Collapsed;
                    default:
                        return Visibility.Visible;
                }
            }
            return Visibility.Visible;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
