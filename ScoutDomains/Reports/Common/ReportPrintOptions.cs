using ScoutUtilities.Common;
using ScoutUtilities.Enums;

namespace ScoutDomains.Reports.Common
{
    public class ReportPrintOptions : BaseNotifyPropertyChanged
    {
        public HorizontalAlignmentType AlignmentType { get; set; }

        public RunSummaryColumnType ColumnType { get; set; }

        public RunSummaryParameterType ParameterType { get; set; }

        public string ParameterName { get; set; }

        public InstrumentStatusOptionType InstrumentoptionType { get; set; }

        private bool _isParameterChecked;
        public bool IsParameterChecked
        {
            get { return _isParameterChecked; }
            set
            {
                _isParameterChecked = value;
                NotifyPropertyChanged(nameof(IsParameterChecked));
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                NotifyPropertyChanged(nameof(IsEnabled));
            }
        }
    }
}
