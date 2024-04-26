using ScoutUtilities.Enums;

namespace ScoutDomains.EnhancedSampleWorkflow
{
    public class SampleSetTemplateDomain
    {
        public uint CellTypeIndex { get; set; }
        public string QualityControlName { get; set; } // string.Empty if the Template uses a Cell Type and not a Quality Control
        public uint Dilution { get; set; }
        public string SampleTag { get; set; }
        public SamplePostWash WashType { get; set; }

        public string SampleId { get; set; }
        
        public bool UseSequencing { get; set; }
        public bool SequencingTextFirst { get; set; }
        public string SequencingBaseLabel { get; set; }
        public uint SequencingNumberOfDigits { get; set; }
        public uint SequencingStartingDigit { get; set; }
        public uint SaveEveryNthImage { get; set; }

        public SampleSetTemplateDomain()
        {

        }

        public SampleSetTemplateDomain(uint cellTypeIndex, string qcName, uint dilution, string sampleTag, SamplePostWash wash, 
            string sampleId, uint saveEveryNthImage, bool useSequencing, bool sequencingTextFirst = true, string sequencingBaseLabel = null,
            uint sequencingNumberOfDigits = 0, uint sequencingStartingDigit = 0)
        {
            CellTypeIndex = cellTypeIndex;
            QualityControlName = qcName ?? string.Empty;
            Dilution = dilution;
            SampleTag = sampleTag ?? string.Empty;
            WashType = wash;
            SampleId = sampleId ?? string.Empty;
            SaveEveryNthImage = saveEveryNthImage;
            UseSequencing = useSequencing;
            SequencingTextFirst = sequencingTextFirst;
            SequencingBaseLabel = sequencingBaseLabel ?? string.Empty;
            SequencingNumberOfDigits = sequencingNumberOfDigits;
            SequencingStartingDigit = sequencingStartingDigit;
        }
    }
}