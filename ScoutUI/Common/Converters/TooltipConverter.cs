// ***********************************************************************
// Assembly         : ScoutUI
// Author           : 20128398
// Created          : 28-10-2018
//
// Last Modified By : 20128398
// Last Modified On : 28-10-2018
// ***********************************************************************
// <copyright file="TooltipConverter.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using ScoutLanguageResources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
    public class TooltipConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var selectedItem = (KeyValuePair<string, string>) value;
                var parameterString = LanguageResourceHelper.Get("LID_UsersLabel_CellType");
                return selectedItem.Key.Equals(parameterString);
            }
            return false;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
