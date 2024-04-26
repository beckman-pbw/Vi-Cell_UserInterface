using ScoutUtilities.Common;

namespace ScoutDomains
{
   
    public class ExportRunResultDomain : BaseNotifyPropertyChanged
    {
      
        public bool IsSelected
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string Position { get; set; }

        public string SampleId { get; set; }

        public string BpQcCellType { get; set; }

        public string Dilution { get; set; }

        public string Wash { get; set; }

        public string Comments { get; set; }
    }
}