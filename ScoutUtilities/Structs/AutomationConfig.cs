using System.Runtime.InteropServices;

namespace ScoutUtilities.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AutomationConfig
    {
        public uint Port;
        public uint Padding;   // This is required for the structure to be xferred correctly from the UI to the Backend.
        public byte AutomationIsEnabled; //boolean
        public byte ACupIsEnabled; //boolean

        public AutomationConfig(bool automationEnabled, bool acupEnabled, uint port)
        {
            Port = port;
            Padding = 0;
            AutomationIsEnabled = Misc.BoolToByte(automationEnabled);
            ACupIsEnabled = Misc.BoolToByte(acupEnabled);
        }
    }
}
