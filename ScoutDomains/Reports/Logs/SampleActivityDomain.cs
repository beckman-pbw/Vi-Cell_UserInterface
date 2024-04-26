using System;
using System.Collections.Generic;

namespace ScoutDomains
{
    public class SampleActivityDomain : WorkQueueSampleLogDomain
    {
        private DateTime _timeStamp;

        public DateTime Timestamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        private string _userId;

        public string UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        private List<WorkQueueSampleLogDomain> _workQueue;

        public List<WorkQueueSampleLogDomain> WorkQueue
        {
            get
            {
                if (_workQueue == null)
                {
                    _workQueue = new List<WorkQueueSampleLogDomain>();
                }

                return _workQueue;
            }
            set { _workQueue = value; }
        }
    }
}