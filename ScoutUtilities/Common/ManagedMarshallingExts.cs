using System;
using System.Runtime.InteropServices;
using ScoutUtilities.Structs;

namespace ScoutUtilities.Common
{
    public class CoTaskUuidPtr
    {
        private readonly IntPtr _data;

        public CoTaskUuidPtr(IntPtr data)
        {
            _data = data;
        }

        ~CoTaskUuidPtr()
        {
            Marshal.FreeCoTaskMem(_data);
        }

        public IntPtr Raw => _data;
    }

    public static class ManagedMarshallingExts
    {

        public static CoTaskUuidPtr MarshalToIntPtr(this uuidDLL[] uuids)
        {

            var uuidsPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(uuidDLL)) * uuids.Length);
            var uuidIterPtr = uuidsPtr;
            foreach (var uuid in uuids)
            {
                Marshal.StructureToPtr(uuid, uuidIterPtr, false);
                uuidIterPtr += Marshal.SizeOf(typeof(uuidDLL));
            }

            return new CoTaskUuidPtr(uuidsPtr);
        }
    }
}
