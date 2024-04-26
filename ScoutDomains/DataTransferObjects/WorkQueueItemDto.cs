using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System;

namespace ScoutDomains.DataTransferObjects
{
    public class WorkQueueItemDto
    {
        public string Label { get; set; }
        public string Comment { get; set; }
        public SamplePosition Location { get; set; }
        public UInt32 CelltypeIndex { get; set; }
        public UInt32 SaveEveryNthImage { get; set; }
        public UInt32 DilutionFactor { get; set; }
        public SamplePostWash PostWash { get; set; }
        public string BpQcName { get; set; }
        public byte NumAnalyses { get; set; }
        public UInt16 AnalysisIndices { get; set; }
        public SampleStatus Status { get; set; }
    }
}