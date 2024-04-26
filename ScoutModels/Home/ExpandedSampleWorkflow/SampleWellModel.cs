namespace ScoutModels.ExpandedSampleWorkflow
{
    public class SampleWellModel
    {
        public int Index { get; set; }
        public SampleModel Sample { get; set; }
        public bool IsEmpty => Sample == null;

        public SampleWellModel(int index, SampleModel sample = null)
        {
            Index = index;
            Sample = sample;
        }
    }
}