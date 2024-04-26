// ***********************************************************************
// Assembly         : ScoutUtilities
// Author           : 20115954
// Created          : 05-15-2018
//
// Last Modified By : 20115954
// Last Modified On : 05-15-2018
// ***********************************************************************
// <copyright file="KbdllHooks.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScoutUtilities.Structs
{
    /// <summary>
    /// Struct KbdllHooks
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct KbdllHooks
    {
        /// <summary>
        /// The key
        /// </summary>
        public readonly Keys key;

        /// <summary>
        /// The scan code
        /// </summary>
        private readonly int scanCode;

        /// <summary>
        /// The flags
        /// </summary>
        public readonly int flags;

        /// <summary>
        /// The time
        /// </summary>
        private readonly int time;

        /// <summary>
        /// The extra
        /// </summary>
        private readonly IntPtr extra;
    }
}