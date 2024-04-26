using ScoutUtilities.Common;

namespace ScoutDomains
{
    public class ProgressDomain : BaseNotifyPropertyChanged
    {
        public bool IsResponseAvailable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsProgressComplete
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string ProgressLabel { get; set; }
    }
}
