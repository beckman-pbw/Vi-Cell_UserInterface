using JetBrains.Annotations;
using log4net;
using ScoutUtilities.Enums;
using System;
using System.Runtime.InteropServices;
using ScoutUtilities;

namespace HawkeyeCoreAPI
{
    public partial class GenericFree
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

#region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError FreeTaggedBuffer(NativeDataType tag, IntPtr ptr);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError FreeListOfTaggedBuffers(NativeDataType tag, IntPtr ptr, UInt32 numitems);

        [DllImport("HawkeyeCore.dll")]
        public static extern void FreeCharBuffer(IntPtr ptr);

        [DllImport("HawkeyeCore.dll")]
        public static extern void FreeListOfCharBuffers(IntPtr ptr, UInt32 numitems);

#endregion


#region API_Calls

        public static void FreeTaggedBufferAPI(NativeDataType tag, IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
#if _DEBUG
                Log.Warn("FreeTaggedBufferAPI: pointer IntPtr.Zero");
#endif
            }
            else
            {
	            Misc.LogOnHawkeyeError("FreeListOfTaggedBuffers::", FreeTaggedBuffer(tag, ptr));
            }
        }

        public static void FreeListOfTaggedBuffersAPI(NativeDataType tag, IntPtr ptr, UInt32 numitems)
        {
            Misc.LogOnHawkeyeError("FreeListOfTaggedBuffers::", FreeListOfTaggedBuffers(tag, ptr, numitems));
        }

        public static void FreeCharBufferAPI(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                Log.Warn("FreeCharBufferAPI: pointer IntPtr.Zero");
            }
            else
            {
                FreeCharBuffer(ptr);
            }
        }

        public static void FreeListOfCharBuffersAPI(IntPtr ptr, UInt32 numitems)
        {
            FreeListOfCharBuffers(ptr, numitems);
        }
#endregion


#region Private Methods

#endregion

    }
}
