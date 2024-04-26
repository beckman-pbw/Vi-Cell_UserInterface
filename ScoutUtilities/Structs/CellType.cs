// ***********************************************************************
// <copyright file="CellType.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Runtime.InteropServices;
using ScoutUtilities.Enums;

namespace ScoutUtilities.Structs
{
    /// <summary>
    /// Struct CellType
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CellType
    {
        /// Cell types 0x00000000 to 0x7FFFFFFF are BCI-supplied.
        /// Cell types 0x80000000 to 0xFFFFFFFF are customer-generated.
        public UInt32 celltype_index;

        public IntPtr label;

        // Number of images to acquire/analyze for a sample using this cell type
        public UInt16 max_image_count;

        // Number of aspiration cycles used to re-suspend particles in the sample
        public byte aspiration_cycles;

        // Exclusion zone on top/left of image to balance out partial cells on image borders.
        // These fields are no longer part of the actual CellType definition.
        // They are only here as a mechanism to pass them from the CellCouning configuration to the UI.
        public UInt16 roi_x_pixels;
        public UInt16 roi_y_pixels;

        /*NB: We would be wise to redefine these in terms of characteristic_t values
              so that we can have whatever set of things we want to 
         */

        // Cell morphology parameters:
        public float minimum_diameter_um;
        public float maximum_diameter_um;
        public float minimum_circularity; // 0.0-1.0 : 1.0 is perfectly circular
        public float sharpness_limit; // 0.0-1.0 : 1.0 is perfectly sharp

        /*
         * The collection of parameters (characteristic, threshold, polarity) that are used
         *  to identify this cell type within the collection of identified/measured blobs.
         * These REPLACE the traditional hard-coded set of ViCell cell morphology parameters
         *  and allow for flexibility and future expansion.
         */
        public byte num_cell_identification_parameters;
        public IntPtr cell_identification_parameters;

        // Declustering
        public eCellDeclusterSetting decluster_setting; // Effort given to identify individual cells within a cluster

        // For Fluorescent analysis:
        // Region of interest within the FL images in relation to the 
        // cell's boundaries defined in the brightfield image.
        // Multiplier: 1.0 = exact ROI as defined by bright field; 0.5 = half of BF ROI
        public float fl_roi_extent;

        // Specializations of factory/user analysis parameters
        // For each analysis type known to the instrument, the user may specialize it 
        //  to the cell type under test.
        // If an analysis is requested for a sample, that sample's cell-type instance is first checked for
        //  a specialization for that analysis.  If one exists, that parameter set will be used.  If no 
        //  specialization is found, then the default parameters from the factory list will be used.
        //
        // Note: When showing the list of analyses for a sample, go ONLY with the master list.  Do NOT rely
        // on this list as the source.

        public UInt32 num_analysis_specializations;
        public IntPtr analysis_specializations;

        // Adjustment factor used with automation to ensure values are what is expected when calculations are performed
        public float calculation_adjustment_factor;

        public override string ToString()
        {
            return Misc.ObjectToString(this);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct imagewrapper_t
    {
        public UInt16 rows;
        public UInt16 cols;
        public byte type; // OpenCV type ex: CV_8UC1 == 0 (see "types_c.h" from OpenCV
        public UInt32 step; // bytes per row, including padding
        public IntPtr data; // Data buffer.  For multi-channel, will be in BGR order
    }
}