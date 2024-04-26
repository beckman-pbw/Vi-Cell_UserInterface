// ***********************************************************************
// Assembly         : ScoutUtilities
// Author           : 20128398
// Created          : 07-05-2017
//
// Last Modified By : 20128398
// Last Modified On : 07-05-2017
// ***********************************************************************
// <copyright file="AdminUser.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Runtime.InteropServices;
using ScoutUtilities.Enums;
using System.ComponentModel;

namespace ScoutUtilities.Structs
{
    /// <summary>
    /// Struct SYSTEMTIME
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTime
    {
        /// <summary>
        /// The w year
        /// </summary>
        public short wYear;

        /// <summary>
        /// The w month
        /// </summary>
        public short wMonth;

        /// <summary>
        /// The w day of week
        /// </summary>
        public short wDayOfWeek;

        /// <summary>
        /// The w day
        /// </summary>
        public short wDay;

        /// <summary>
        /// The w hour
        /// </summary>
        public short wHour;

        /// <summary>
        /// The w minute
        /// </summary>
        public short wMinute;

        /// <summary>
        /// The w second
        /// </summary>
        public short wSecond;

        /// <summary>
        /// The w milliseconds
        /// </summary>
        public short wMilliseconds;
    }

    /// <summary>
    /// Struct SystemVersion
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemVersion
    {
        /// <summary>
        /// The software version
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string software_version;

        /// <summary>
        /// The img analysis version
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string img_analysis_version;

        /// <summary>
        /// The controller firmware version
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string controller_firmware_version;

        /// <summary>
        /// The camera firmware version
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string camera_firmware_version;

        /// <summary>
        /// The syringepump firmware version
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string syringepump_firmware_version;

        /// <summary>
        /// The system_serial_number
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string system_serial_number;
    }

    /// <summary>
    /// Struct UserProperty
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct UserProperty
    {
        /// <summary>
        /// The propertyname
        /// </summary>
        public IntPtr propertyname;

        /// <summary>
        /// The members
        /// </summary>
        public IntPtr members;

        /// <summary>
        /// The number members
        /// </summary>
        public UInt16 num_members;
    }

    /// <summary>
    /// Struct qualitycontrol_t
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct qualitycontrol_t
    {
        /// <summary>
        /// The qc name
        /// </summary>
        public IntPtr qc_name;

        /// <summary>
        /// The cell type index
        /// </summary>
        public UInt32 cell_type_index;

        /// <summary>
        /// The assay type
        /// </summary>
        public assay_parameter assay_type;

        /// <summary>
        /// The lot information
        /// </summary>
        public IntPtr lot_information;

        /// <summary>
        /// The assay value
        /// </summary>
        public double assay_value;

        /// <summary>
        /// The plusminus percentage
        /// </summary>
        public double plusminus_percentage;

        /// <summary>
        /// The expiration date
        /// </summary>
        public UInt64 expiration_date; // days since 1/1/1970 UTC>

        public IntPtr comment_text;

        public override string ToString()
        {
            return Misc.ObjectToString(this);
        }
    }
}