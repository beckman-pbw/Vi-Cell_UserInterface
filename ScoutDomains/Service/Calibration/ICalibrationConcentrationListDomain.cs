using System;
using ScoutUtilities.Enums;

namespace ScoutDomains
{
    public interface ICalibrationConcentrationListDomain
    {
        string KnownConcentration { get; set; }
        int StartPosition { get; set; }
        double AssayValue { get; set; }
        DateTime ExpiryDate { get; set; }
        string Lot { get; set; }
        int NumberOfTubes { get; set; }
        SampleStatusColor Status { get; set; }
        int EndPosition { get; set; }
        AssayValueEnum AssayValueType { get; set; }

        bool IsCorrectAssayValue // This gets checked and set in AssayValueTextBox.AssayValueTextChange
        {
            get;
            set;
        }

        bool IsActiveRow { get; set; }
    }
}