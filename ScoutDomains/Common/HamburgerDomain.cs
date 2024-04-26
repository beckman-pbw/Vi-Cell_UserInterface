using System.Windows.Media;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;

namespace ScoutDomains.Common
{
    public class HamburgerDomain : BaseNotifyPropertyChanged
    {

        public HamburgerItem Item
        {
            get { return GetProperty<HamburgerItem>(); }
            set { SetProperty(value); }
        }

        public string Path
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Title
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public Geometry Image
        {
            get { return GetProperty<Geometry>(); }
            set { SetProperty(value); }
        }

        public bool IsItemVisible
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
       
        public bool IsItemSelected
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsItemEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
    }
}