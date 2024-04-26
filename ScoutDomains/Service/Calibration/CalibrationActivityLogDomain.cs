using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;

namespace ScoutDomains
{
    public class CalibrationActivityLogDomain : CalibrationConsumableDomain
    {
        public DateTime Date { get; set; }

        public string UserId { get; set; }

        public calibration_type CalibrationType { get; set; }

        public int NumberOfConsumables { get; set; }

        private List<CalibrationConsumableDomain> _consumable;

        public List<CalibrationConsumableDomain> Consumable
        {
            get { return _consumable ?? (_consumable = new List<CalibrationConsumableDomain>()); }
            set { _consumable = value; }
        }

        public double Slope { get; set; }

        public double Intercept { get; set; }

        public int ImageCount { get; set; }
    }
}