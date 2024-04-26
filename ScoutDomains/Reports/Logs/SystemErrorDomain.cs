namespace ScoutDomains
{
    public class SystemErrorDomain
    {
        private uint _errorCode;
        public uint ErrorCode
        {
            get { return _errorCode; }
            set { _errorCode = value; }
        }

        private string _severityKey;
        public string SeverityKey
        {
            get { return _severityKey; }
            set { _severityKey = value; }
        }

        private string _severityDisplayValue;
        public string SeverityDisplayValue
        {
            get { return _severityDisplayValue; }
            set { _severityDisplayValue = value; }
        }

        private string _failureMode;
        public string FailureMode
        {
            get { return _failureMode; }
            set { _failureMode = value; }
        }

        private string _sysytem;
        public string System
        {
            get { return _sysytem; }
            set { _sysytem = value; }
        }

        private string _subSystem;
        public string SubSystem
        {
            get { return _subSystem; }
            set { _subSystem = value; }
        }

        private string _instance;
        public string Instance
        {
            get { return _instance; }
            set { _instance = value; }
        }

        private string _cellHealthErrorCode;
        public string CellHealthErrorCode
		{
	        get { return _cellHealthErrorCode; }
	        set { _cellHealthErrorCode = value; }
        }

        // CellHealthErrorCode has not bee added here since this is called to post messages to the MsgHub in the UI.
        public string GetDescription()
        {
            var instanceStr = string.IsNullOrEmpty(Instance) ? "" : (" - " + Instance);
            return string.Format("[{0}] {1} - {2}{3}:{4}", SeverityDisplayValue, System, SubSystem, instanceStr, FailureMode);
        }
    }
}