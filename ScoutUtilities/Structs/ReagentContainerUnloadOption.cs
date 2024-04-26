// ***********************************************************************
// Assembly         : ScoutUtilities
// Author           : 20128398
// Created          : 07-11-2017
//
// Last Modified By : 20128398
// Last Modified On : 07-11-2017
// ***********************************************************************
// <copyright file="ReagentContainerUnloadOption.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using ScoutUtilities.Enums;
using System.Runtime.InteropServices;

namespace ScoutUtilities.Structs
{
    /// <summary>
    /// Struct ReagentContainerUnloadOption
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ReagentContainerUnloadOption
    {
        /// <summary>
        /// The container identifier
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] container_id;

        /// <summary>
        /// The container action
        /// </summary>
        public ReagentUnloadOption container_action;
    }
}