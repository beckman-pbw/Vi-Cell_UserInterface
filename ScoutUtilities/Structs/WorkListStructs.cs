using System;
using System.Runtime.InteropServices;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;

namespace ScoutUtilities.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WorkList
    {
        public uuidDLL uuid;
        public IntPtr username;
        public IntPtr runUserId;
        public IntPtr label;
        public ulong timestamp;
        public SubstrateType carrier;
        public Precession precession; // Only applies to plate processing
        public SampleParameters defaultParameterSettings;
        
        public ushort numSampleSets;
        public IntPtr sampleSets;

        public byte UseSequencing;
        public byte SequencingTextFirst;
        public IntPtr SequencingBaseLabel;
        public ushort SequencingStartingDigit;
        public ushort SequencingNumberOfDigits;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SampleSet
    {
        public uuidDLL uuid;
        public ushort index; // used to map to the backend object before a uuid is generated
        public ulong timestamp;
        public ushort numSamples;
        public SubstrateType carrier;
        public IntPtr samples; // SampleDefinition list

        public IntPtr name;
        public IntPtr username;
        public SampleSetStatus setStatus;

        public Precession platePrecession;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SampleDefinition
    {
        public uuidDLL sampleSetUuid;
        public uuidDLL sampleDefUuid;
        public uuidDLL sampleDataUuid;
        public ushort index; // used to map to the backend object before a uuid is generated
        public ushort sampleSetIndex;
        public IntPtr username;
        public ulong timestamp;
        public SamplePosition position;
        public SampleStatus status;
        public SampleParameters parameters;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SampleParameters
    {
        public IntPtr Label;
        public IntPtr Tag;
        public IntPtr bp_qc_name;
        public ushort AnalysisIndex;
        public uint CellTypeIndex;
        public uint DilutionFactor;
        public SamplePostWash WashType;
        public uint SaveEveryNthImage;
    }
}