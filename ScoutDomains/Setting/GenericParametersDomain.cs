using ScoutUtilities.Common;
using ScoutUtilities.Enums;

namespace ScoutDomains
{
    public class GenericParametersDomain : BaseNotifyPropertyChanged
    {
        public string ParameterName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsVisible
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsDefault
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public int Order { get; set; }

        public ResultParameter ParameterID { get; set; }
    }
}
