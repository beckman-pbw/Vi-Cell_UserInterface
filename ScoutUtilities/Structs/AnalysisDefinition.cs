// ***********************************************************************
// <copyright file="AnalysisDefinition.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Runtime.InteropServices;


namespace ScoutUtilities.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct FL_IlluminationSettings
    {
        public UInt16 illuminator_wavelength_nm;
        public UInt16 emission_wavelength_nm;
        public UInt16 exposure_time_ms;
    }


    public struct DetailedResultMeasurements
    {
        public uuidDLL uuid; // UUID of associated result record
        public UInt16 num_image_sets;
        public IntPtr blobs_by_image; // image_blobs_t structure
        public IntPtr large_clusters_by_image; // large_cluster_t structure
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct blob_measurements_t
    {
        public UInt16 x_coord;
        public UInt16 y_coord;
        public UInt32 num_measurements;
        public IntPtr measurements; // measurement_t structure
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct measurement_t
    {
        public Characteristic_t characteristic;
        public float value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct image_blobs_t
    {
        public UInt16 image_set_number;
        public UInt32 blob_count;
        public IntPtr blob_list; // blob_measurements_t structure
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct large_cluster_t
    {
        public UInt16 image_set_number;
        public UInt32 cluster_count;
        public IntPtr cluster_list; // large_cluster_data_t structure
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct large_cluster_data_t
    {
        public UInt16 num_cells_in_cluster;
        public UInt16 top_left_x;
        public UInt16 top_left_y;
        public UInt16 bottom_right_x;
        public UInt16 bottom_right_y;
    }


    /// <summary>
    /// Struct AnalysisDefinition
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AnalysisDefinition
    {
        public UInt16 Analysis_index;
        public IntPtr label;


        public byte num_reagents;
        public IntPtr reagent_indices;

        public byte mixing_cycles;

        public byte num_fl_illuminators;
        public IntPtr fl_illuminators;

        public byte num_analysis_parameters;
        public IntPtr analysis_parameters;
        public IntPtr population_parameter;

        public bool ContainsFluorescence()
        {
            return num_fl_illuminators > 0;
        }

        public bool IsBCIAnalysis()
        {
            return Analysis_index < 0x8000;
        }

        public bool IsCustomerAnalysis()
        {
            return !IsBCIAnalysis();
        }

        public IntPtr ToPtr()
        {
            IntPtr ptrAnalysisParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(this));
            Marshal.StructureToPtr(this, ptrAnalysisParam, false);
            return ptrAnalysisParam;
        }
    }
}