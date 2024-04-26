using ScoutUtilities.Enums;
using System;

namespace ScoutUtilities.CustomEventArgs
{
    public class FilterSampleSetsEventArgs : BaseDialogEventArgs
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string User { get; set; }
        public string SearchString { get; set; }
        public string TagSearchString { get; set; }
        public bool IsAllCellTypesSelected { get; set; }
        public string CellTypeOrQualityControlName { get; set; }
        public eFilterItem FilteringItem { get; set; }

        public override string ToString()
        {
            return Misc.ObjectToString(this);
        }
    }
}