using ScoutUtilities.Structs;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;

namespace ScoutModels.Common
{
    public class KioskHelper
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod,
            uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr _ptrHook;

        private LowLevelKeyboardProc _objKeyboardProcess;

        private IntPtr CaptureKey(int nCode, IntPtr wp, IntPtr lp)
        {
            if (nCode >= 0)
            {
                var objKeyInfo = (KbdllHooks) Marshal.PtrToStructure(lp, typeof(KbdllHooks));
                // Disabling Windows keys 
                if (objKeyInfo.key == Keys.RWin || objKeyInfo.key == Keys.LWin ||
                    objKeyInfo.key == Keys.Tab && HasAltModifier(objKeyInfo.flags) ||
                    objKeyInfo.key == Keys.Escape && (Keyboard.Modifiers & ModifierKeys.Control) != 0 ||
                    objKeyInfo.key == Keys.F4 && HasAltModifier(objKeyInfo.flags))
                {
                    return (IntPtr) 1;
                }
            }

            return CallNextHookEx(_ptrHook, nCode, wp, lp);
        }

        private bool HasAltModifier(int flags)
        {
            return (flags & 0x20) == 0x20;
        }


        public void EnableKioskMode()
        {
            var objCurrentModule = Process.GetCurrentProcess().MainModule;
            _objKeyboardProcess = CaptureKey;
            _ptrHook = SetWindowsHookEx(13, _objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0);
        }
    }
}
