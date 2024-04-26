using System;

namespace ScoutDomains
{
    public class ErrorLogDomain
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

        private string _message;

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        private uint errorCode;

        public uint ErrorCode
        {
            get { return errorCode; }
            set { errorCode = value; }
        }
    }
}