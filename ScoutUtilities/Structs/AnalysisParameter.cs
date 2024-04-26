// ***********************************************************************
// Assembly         : ScoutUtilities
// Author           : 20115954
// Created          : 04-28-2017
//
// Last Modified By : 20115954
// Last Modified On : 04-28-2017
// ***********************************************************************
// <copyright file="AnalysisParameter.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Runtime.InteropServices;


namespace ScoutUtilities.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AnalysisParameter
    {
        public IntPtr label; // for char label[30]
        public Characteristic_t characteristic;
        public float threshold_value; // 32bits on 64bit system.
        [MarshalAs(UnmanagedType.U1)] public bool above_threshold;
    }
}