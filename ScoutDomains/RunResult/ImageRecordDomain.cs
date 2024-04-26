using ScoutUtilities.Common;
using ScoutUtilities.Structs;
using System;

namespace ScoutDomains
{
    public class ImageRecordDomain : BaseNotifyPropertyChanged
    {
        public uuidDLL UUID { get; set; }

        public string userId { get; set; }

        public UInt64 TimeStamp { get; set; }
    }
}
