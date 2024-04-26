using ScoutUtilities.Enums;

namespace ScoutDomains
{
    public class WorkQueueSampleLogDomain
    {
        private string _sampleLabel;

        public string SampleLabel
        {
            get { return _sampleLabel; }
            set { _sampleLabel = value; }
        }

        private string _cellTypeName;

        public string CellTypeName
        {
            get { return _cellTypeName; }
            set { _cellTypeName = value; }
        }

        private string _analysisName;

        public string AnalysisName
        {
            get { return _analysisName; }
            set { _analysisName = value; }
        }

        private sample_completion_status _sampleStatus;

        public sample_completion_status SampleStatus
        {
            get { return _sampleStatus; }
            set { _sampleStatus = value; }
        }
    }
}