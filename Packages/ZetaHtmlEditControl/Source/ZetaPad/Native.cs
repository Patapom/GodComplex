using ZetaNative = ZetaHtmlEditControl.Code.PInvoke.NativeMethods;

namespace Test
{
    using System;
    using System.Runtime.InteropServices;

    class WinHook
    {
        static Native.HookProc MouseHookProcedure;
        static int hHook;
        public static Action OnDoubleCick;

        static int lastDblClickTimestamp;

        public static void Init()
        {
            MouseHookProcedure = MouseHookProc;
            hHook = Native.SetWindowsHookEx(Native.WH_GETMESSAGE, MouseHookProcedure, IntPtr.Zero, AppDomain.GetCurrentThreadId());
            if (hHook == 0)
            {
                //if we failed, then nothing we can do about it
            }
        }

        public static int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                return Native.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
            else
            {
                var msg = (ZetaNative.MSG)Marshal.PtrToStructure(lParam, typeof(ZetaNative.MSG));
                if (msg.message == Native.WM_LBUTTONDBLCLK && OnDoubleCick != null)
                {
                    if (Environment.TickCount - lastDblClickTimestamp > 500)
                    {
                        //ignore the second message of the same dbl-click event
                        lastDblClickTimestamp = Environment.TickCount;
                        OnDoubleCick();
                    }
                }

                return Native.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
        }
    }

    class Native
    {
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        //For other hook types, you can obtain these values from Winuser.h in the Microsoft SDK.
        public const int WH_MOUSE = 7;

        public const int WH_MOUSE_LL = 14;
        public const int WH_GETMESSAGE = 3;
        public const int WM_LBUTTONDBLCLK = 0x203;
        static int hHook = 0;

        //Use this function to install a thread-specific hook.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        //Call this function to uninstall the hook.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        //Use this function to pass the hook information to the next hook procedure in chain.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);
    }
}