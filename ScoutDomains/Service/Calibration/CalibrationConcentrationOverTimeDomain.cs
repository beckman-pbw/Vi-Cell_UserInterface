using ScoutUtilities.Common;
using System;

namespace ScoutDomains
{
    public class CalibrationConcentrationOverTimeDomain : BaseNotifyPropertyChanged
    {
        public string ConsumableLabel { get; set; }

        public string ConsumableLotId { get; set; }

        public DateTime Date
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public double AssayValue
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public int R3
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }
    }
}