using ScoutUtilities.Common;

namespace ScoutDomains
{
    public class SetFocusDomain : BaseNotifyPropertyChanged
    {
        public int Position
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int InFocusCount
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }
    }
}
