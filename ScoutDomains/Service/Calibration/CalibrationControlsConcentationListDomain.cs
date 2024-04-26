using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System;

namespace ScoutDomains
{
    public class CalibrationConcentrationListDomain : BaseNotifyPropertyChanged, ICalibrationConcentrationListDomain
    {
        public string KnownConcentration { get; set; }
        public int StartPosition { get; set; }

        public double AssayValue
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public DateTime ExpiryDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public string Lot
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public int NumberOfTubes
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public SampleStatusColor Status
        {
            get { return GetProperty<SampleStatusColor>(); }
            set { SetProperty(value); }
        }

        public int EndPosition
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public AssayValueEnum AssayValueType
        {
            get { return GetProperty<AssayValueEnum>(); }
            set { SetProperty(value); }
        }

        public bool IsCorrectAssayValue // This gets checked and set in AssayValueTextBox.AssayValueTextChange
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsActiveRow
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

    }
}