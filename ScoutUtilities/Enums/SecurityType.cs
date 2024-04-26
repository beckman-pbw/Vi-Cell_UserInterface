using System;

namespace ScoutUtilities.Enums
{
    public enum SecurityType : uint
    {
        NoSecurity = 0,
        LocalSecurity,
        ActiveDirectory,
	}

	public enum ShutdownOrRebootEnum : Int16
	{
		Shutdown = 0,
		Reboot = 1,
	}
}
