using System;
// ReSharper disable InconsistentNaming

namespace ScoutOpcUaTests
{
    public class TestClientException : Exception
    {
        public const int ERROR_CODE_UNEXPECTED = 10;
        public const int ERROR_CODE_PERMISSION_DENIED = 20;
        public const int ERROR_CODE_BAD_LOCK_STATE = 30;
        public const string ERROR_TEXT_UNEXPECTED = "Unexpected error";
        public const string ERROR_TEXT_PERMISSION_DENIED = "Permission denied";
        public const string ERROR_TEXT_BAD_LOCK_STATE = "Bad Lock State";

        public TestClientException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public TestClientException(string message, Exception innerException, int errorCode) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public int ErrorCode { get; }
    }
}