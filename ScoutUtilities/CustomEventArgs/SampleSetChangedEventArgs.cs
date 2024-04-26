using ScoutUtilities.Structs;
using System;

namespace ScoutUtilities.CustomEventArgs
{
    public class SampleSetChangedEventArgs : EventArgs
    {
        public uuidDLL SampleSetUid { get; set; }
        public ushort SampleSetIndex { get; set; }
        public string SampleSetName { get; set; }

        public SampleSetChangedEventArgs(uuidDLL sampleSetUid, string sampleSetName = null)
        {
            SampleSetUid = sampleSetUid;
            SampleSetName = sampleSetName;
        }

        public SampleSetChangedEventArgs(ushort setIndex, string sampleSetName = null)
        {
            SampleSetIndex = setIndex;
            SampleSetName = sampleSetName;
        }

        public SampleSetChangedEventArgs(uuidDLL sampleSetUid, ushort setIndex, string sampleSetName = null)
        {
            SampleSetUid = sampleSetUid;
            SampleSetIndex = setIndex;
            SampleSetName = sampleSetName;
        }
    }
}