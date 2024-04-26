using System;

namespace ScoutDomains
{
    public class CalibrationConsumableDomain
    {
        public string Label { get; set; }

        public DateTime ExpirationDate { get; set; }

        public string LotId { get; set; }

        public double AssayValue { get; set; }
    }
}