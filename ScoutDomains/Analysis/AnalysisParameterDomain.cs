using ScoutUtilities.Structs;
using System;

namespace ScoutDomains
{
    public class AnalysisParameterDomain : ICloneable
    {
        public string Label { get; set; }

        public Characteristic_t Characteristic { get; set; }

        public float? ThresholdValue { get; set; }

        public bool AboveThreshold { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
