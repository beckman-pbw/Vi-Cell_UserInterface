using ScoutUtilities.Common;

namespace ScoutDomains
{
    public class LiveImageDataDomain : BaseNotifyPropertyChanged
    {
        public string TestName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string TestResult
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
    }
}