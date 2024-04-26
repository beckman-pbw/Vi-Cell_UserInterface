using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ScoutUtilities.Common
{
    public static class CommonExtensions
    {
        public static IntPtr ToIntPtr(this string str)
        {
            if(string.IsNullOrEmpty(str))
            {
                return IntPtr.Zero;
            }

            var tempBytes = Encoding.UTF8.GetBytes(str);
            var length = tempBytes.Length;
            if(length <= 0)
            {
                return IntPtr.Zero;
            }

            // Make last entry as null
            if (tempBytes[length - 1] != 0)
            {
                Array.Resize(ref tempBytes, length + 1);
                tempBytes[length] = 0;
                length = tempBytes.Length;
            }

            IntPtr unmanagedPointer = Marshal.AllocCoTaskMem(length);
            Marshal.Copy(tempBytes, 0, unmanagedPointer, tempBytes.Length);
            return unmanagedPointer;
        }

        public static void ReleaseIntPtr(this IntPtr ptr)
        {
            Marshal.FreeCoTaskMem(ptr);
        }

        public static string ToSystemString(this IntPtr ptr)
        {
            if(ptr == IntPtr.Zero)
            {
                return string.Empty;
            }

            int size = GetActualIntPrtDataSize(ptr);
            byte[] array = new byte[size];
            Marshal.Copy(ptr, array, 0, size);
            if (array.Length > 0 && array[array.Length - 1] == 0)
            {
                array = array.Take(array.Length - 1).ToArray();
            }
            return Encoding.UTF8.GetString(array);
        }

        private static int GetActualIntPrtDataSize(IntPtr ptr)
        {
            int size;
            for (size = 0; Marshal.ReadByte(ptr, size) > 0; size++) ;
            return size;
        }

        public static string ToUTF8(this string str)
        {
            byte[] tempBytes = Encoding.UTF8.GetBytes(str);
            return Encoding.UTF8.GetString(tempBytes);
        }
    }
}
