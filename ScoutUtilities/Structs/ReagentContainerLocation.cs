using ScoutUtilities.Enums;
using System.Runtime.InteropServices;

namespace ScoutUtilities.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ReagentContainerLocation
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte identifier;

        public ReagentContainerPosition position;
    }
}