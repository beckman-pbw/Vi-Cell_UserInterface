using ScoutUtilities.Enums;
using System;
using System.Runtime.InteropServices;
using ScoutUtilities.Common;

namespace ScoutUtilities.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WorkQueueItem
    {
        public IntPtr label;
        public IntPtr comment;
        public SamplePosition location;
        public UInt32 celltypeIndex;
        public UInt32 saveEveryNthImage;
        public UInt32 dilutionFactor;
        public SamplePostWash postWash;
        public IntPtr bp_qc_name;
        public byte numAnalyses;
        public UInt16 analysisIndices;
        public SampleStatus status;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct histogrambin_t
    {
        public float bin_nominal_value;
        public UInt32 count;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WorkQueue
    {
        public IntPtr label;

        public UInt16 numWQI;

        public UInt16 curWQIIndex;

        public IntPtr workQueueItems;

        public SubstrateType substrate;

        public Precession precession; // does not affect Carousel processing...

        // For Carousel: settings to use for processing of any additional samples encountered.
        public WorkQueueItem additionalWorkSettings;

        // .label will be used with an appended index ("label.1", "label.2"....)
        // .location will be ignored.
    }
}

