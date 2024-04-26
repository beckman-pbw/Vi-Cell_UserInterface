using ScoutUtilities.Enums;
using System;

namespace ScoutDomains
{
    public class AuditLogDomain
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

        private audit_event_type _auditEventType;

        public audit_event_type AuditEventType
        {
            get { return _auditEventType; }
            set { _auditEventType = value; }
        }

        private string _message;

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
    }
}