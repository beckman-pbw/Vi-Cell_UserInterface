using System.ComponentModel;

namespace ScoutUtilities.Enums
{
    public enum eFilterItem : ushort
    {
        [Description("LID_Label_SampleSetFilter")] eSampleSet = 0,
        [Description("LID_Label_SampleFilter")] eSample
    }
}