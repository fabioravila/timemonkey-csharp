using System;
using System.Diagnostics;
using static TimeMonkey.Core.WinAPI;
using static TimeMonkey.Core.WinAPI.User32;

namespace TimeMonkey.Core
{

    public class NanoMouseHook
    {
        HookHandler hookHandler;
        public delegate void MouseHookCallback(EventArgs args);
        public event MouseHookCallback Event;
        private IntPtr hookID = IntPtr.Zero;
        public void Install()
        {
            hookHandler = HookFunc;
            hookID = SetHook(hookHandler);
        }

        public void Uninstall()
        {
            if (hookID == IntPtr.Zero)
                return;

            UnhookWindowsHookEx(hookID);
            hookID = IntPtr.Zero;
        }

        ~NanoMouseHook()
        {
            Uninstall();
        }

        private IntPtr SetHook(HookHandler proc)
        {
            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, module.BaseAddress, 0);
            }
        }

        private IntPtr HookFunc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // parse system messages
            if (nCode >= 0)
            {
                Event?.Invoke(new EventArgs());
            }
            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }
    }
}
