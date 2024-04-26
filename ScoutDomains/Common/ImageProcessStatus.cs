using ScoutUtilities.Common;
using ScoutUtilities.Structs;

namespace ScoutDomains.Common
{
    public class ImageProcessStatus : BaseNotifyPropertyChanged
    {
        public E_ERRORCODE ImageErrorCode { get; }

        public string ImageErrorMessage
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string ImageErrorCount
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public ImageProcessStatus(E_ERRORCODE imageErrorCode, string imageErrorMessage, string imageErrorCount)
        {
            ImageErrorCode = imageErrorCode;
            ImageErrorMessage = imageErrorMessage;
            ImageErrorCount = imageErrorCount;
        }
    }
}
