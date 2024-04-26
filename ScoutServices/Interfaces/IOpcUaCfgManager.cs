using System;

namespace ScoutServices.Interfaces
{
    /// <summary>
    /// Interface used to get and set the OpcUa server port number
    /// </summary>
    public interface IOpcUaCfgManager
    {
        UInt32 GetOrSetOpcUaPort(UInt32 newPort = 0);
    }
}
