using ScoutUtilities.Enums;
using System;
using System.Runtime.InteropServices;

namespace ScoutUtilities.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ActiveDirConfig
    {
        public ushort AdPort;
        public IntPtr AdServer;
        public IntPtr BaseDn;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ActiveDirGroup
    {
        public UserPermissionLevel UserRole;
        public IntPtr ActiveDirGroupMap;
    }
}