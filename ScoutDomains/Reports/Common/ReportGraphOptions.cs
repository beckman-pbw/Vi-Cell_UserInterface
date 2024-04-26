using ScoutUtilities.Common;

namespace ScoutDomains.Reports.Common
{
    public class ReportGraphOptions : BaseNotifyPropertyChanged
    {
        public string FirstParameterName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsFirstParameterChecked
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string SecondParameterName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsSecondParameterVisible
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsSecondParameterChecked
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
    }
}