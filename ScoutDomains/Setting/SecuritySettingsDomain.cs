using ScoutUtilities.Common;
using ScoutUtilities.Enums;

namespace ScoutDomains
{
    public class SecuritySettingsDomain : BaseNotifyPropertyChanged
    {
        public SecurityType SecurityType
        {
            get { return GetProperty<SecurityType>(); }
            set { SetProperty(value); }
        }

        public string InActivityTimeOutMins
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string PasswordExpiryDays
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        private TrulyObservableCollection<SignatureDomain> _signatures;

        public TrulyObservableCollection<SignatureDomain> Signatures
        {
            get => _signatures ?? (_signatures = new TrulyObservableCollection<SignatureDomain>());
            set => _signatures = value;
        }
    }
}

