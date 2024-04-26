namespace ScoutDomains.RunResult
{
    public class RetrievedSampleRecord
    {
        public SampleImageRecordDomain Image { get; set; }

        public SampleRecordDomain Record { get; set; }

        public RetrievedSampleRecord(SampleRecordDomain record, SampleImageRecordDomain image)
        {
            Image = image;
            Record = record;
        }
    }
}
