using System.Runtime.InteropServices;

namespace ScoutUtilities.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DbConfig
    {
        public uint Port;
        public string IpAddress;
        public string Name;

        public DbConfig(uint port, string ipAddress, string hostName)
        {
            Port = port;
            IpAddress = ipAddress;
            Name = hostName;
        }
    }
}