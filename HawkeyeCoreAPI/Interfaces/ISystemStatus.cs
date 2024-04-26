using ScoutUtilities.Structs;
using System;

namespace HawkeyeCoreAPI.Interfaces
{
    public interface ISystemStatus
    {
        IntPtr GetSystemStatusAPI(ref SystemStatusData systemStatusData);
        void FreeSystemStatusAPI(IntPtr ptrSystemStatus);
    }
}