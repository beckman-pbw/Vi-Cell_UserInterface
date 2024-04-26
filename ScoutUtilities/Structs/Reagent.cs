// ***********************************************************************
// Assembly         : ScoutUtilities
// Author           : 20128398
// Created          : 07-05-2017
//
// Last Modified By : 20128398
// Last Modified On : 07-05-2017
// ***********************************************************************
// <copyright file="Reagent.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using ScoutUtilities.Enums;
using System.Runtime.InteropServices;

namespace ScoutUtilities.Structs
{
    /// <summary>
    /// Struct ReagentState
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ReagentState
    {
        public UInt16 reagent_index; // Index to ReagentDefinition::reagent_index
        public IntPtr lot_information;

        public UInt16 events_possible;
        public UInt16 events_remaining;

        public byte valve_location; // Which valve selection on the syringe pump accesses this reagent
    }

    /// <summary>
    /// Struct ReagentDefinition
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ReagentDefinition
    {
        /// <summary>
        /// The reagent index
        /// </summary>
        public UInt16 reagent_index;

        /// <summary>
        /// The label
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
        public string label;

        /// <summary>
        /// The mixing cycles
        /// </summary>
        /// 
        public byte mixing_cycles;
    }


    /// <summary>
    /// Struct ReagentContainerState
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ReagentContainerState
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] identifier;

        public IntPtr bci_part_number;

        public UInt32 in_service_date;
        public UInt32 exp_date; // Either shelf-life date OR service-life date if in_service_date != 0

        public IntPtr lot_information;

        public ReagentContainerStatus status;

        public UInt16 events_remaining; // set to min (reagent_states[n].events_remaining);

        public ReagentContainerPosition position;
        public byte num_reagents;
        public IntPtr reagent_states;
    }


    public struct AutofocusResults
    {
        [MarshalAs(UnmanagedType.U1)] public bool focus_successful;
        public UInt32 nFocusDatapoints;
        public IntPtr dataset;
        public Int32 bestfocus_af_position; // Position identified as that of sharpest focus (motor position)
        public Int32 offset_from_bestfocus_um; // Configured offset (in microns) from sharpest focus
        public Int32 final_af_position; // Final position of autofocus after offset (motor position);

        public imagewrapper_t
            bestfocus_af_image; // Final image at the "bestfocus_af_position" position (motor position);
    }


    public struct AutofocusDatapoint
    {
        public Int32 position;
        public UInt32 focalvalue;
    }
}