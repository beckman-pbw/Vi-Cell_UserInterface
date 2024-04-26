using ScoutUtilities.Common;

namespace ScoutDomains.EnhancedSampleWorkflow
{
    public class SequenceParametersDomain : BaseNotifyPropertyChanged
    {
        public bool UseSequencing;
        public bool SequencingTextFirst;
        public string SequencingBaseLabel;
        public ushort SequencingStartingDigit;
        public ushort SequencingNumberOfDigits;
    }
}