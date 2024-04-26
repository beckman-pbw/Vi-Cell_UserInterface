// ***********************************************************************
// <copyright file="EnumUtilities.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using ScoutUtilities.Enums;

namespace ScoutUtilities.Helper
{
    public class EnumUtilities
    {
        public static MotorRegHighlightType ConvertStringToEnum(string value)
        {
            return (MotorRegHighlightType) Enum.Parse(typeof(MotorRegHighlightType), value);
        }
    }
}
