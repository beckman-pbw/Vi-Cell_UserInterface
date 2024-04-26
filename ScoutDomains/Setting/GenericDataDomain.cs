using System;
using ScoutUtilities.Enums;

namespace ScoutDomains
{
    public class GenericDataDomain
    {
        public string BpQc { get; set; }

        public SamplePostWash Wash { get; set; }

        public string Dilution { get; set; }

        public string Tag { get; set; }

        public string AnalysisBy { get; set; }

        public string ReanalysisBy { get; set; }

        public DateTime? AnalysisDateTime { get; set; }

        public DateTime? ReanalysisDateTime { get; set; }
    }
}