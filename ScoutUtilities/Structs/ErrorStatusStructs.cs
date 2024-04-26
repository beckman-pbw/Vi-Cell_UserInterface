using System;
using System.Runtime.InteropServices;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;

namespace ScoutUtilities.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ErrorStatusType
    {
        public string ErrorCode;
        public string Severity;
        public string System;
        public string SubSystem;
        public string Instance;
        public string FailureMode;
	}
}
