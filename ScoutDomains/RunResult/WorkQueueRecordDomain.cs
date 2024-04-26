using System;
using ScoutUtilities.Common;
using System.Collections.ObjectModel;
using ScoutUtilities.Structs;

namespace ScoutDomains
{
    public class WorkQueueRecordDomain : BaseNotifyPropertyChanged
    {

        public bool HasValue
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string WorkQueueName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public uuidDLL UUID { get; set; }

        public string UserId { get; set; }

        public int TimeStamp { get; set; }

        public DateTime SampleDateTime
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public uint NumSampleRecords { get; set; }

        private ObservableCollection<SampleRecordDomain> _sampleRecords;

        public ObservableCollection<SampleRecordDomain> SampleRecords
        {
            get { return _sampleRecords ?? (_sampleRecords = new ObservableCollection<SampleRecordDomain>()); }
            set
            {
                _sampleRecords = value;
                NotifyPropertyChanged(nameof(SampleRecords));
            }
        }

        public uuidDLL[] SampleRecordIDs { get; set; }

    }
}
