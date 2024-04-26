using System.Runtime.InteropServices;

namespace ScoutUtilities.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SmtpConfig
    {
        public uint Port;
        public string Server;
        public string Username;
        public string Password;
        public byte AuthEnabled; //boolean

        public SmtpConfig(uint port, string server, string username, string password, bool authEnabled)
        {
            Port = port;
            Server = server;
            Username = username;
            Password = password;
            AuthEnabled = Misc.BoolToByte(authEnabled);
        }
    }
}