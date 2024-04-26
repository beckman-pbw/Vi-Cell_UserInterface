using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ScoutUtilities.EventDomain
{
    public class StageStatus : BaseNotifyPropertyChanged
    {
        public eSensorStatus IsCarouselStatus
        {
            get { return GetProperty<eSensorStatus>(); }
            set { SetProperty(value); }
        }

        public SamplePosition SamplePosition
        {
            get { return GetProperty<SamplePosition>(); }
            set { SetProperty(value); }
        }
    }
}