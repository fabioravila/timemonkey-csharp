using System;
using System.Diagnostics;
using static TimeMonkey.Core.WinAPI;
using static TimeMonkey.Core.WinAPI.User32;

namespace TimeMonkey.Core
{
    /// <summary>
    /// Simplest Keyboard ans Moude low system Hook, this class capture only the activiti without details
    /// </summary>
    public class NanoHook
    {
        HookHandler key_hookHandler;
        HookHandler mouse_hookHandler;

        public delegate void NanoHookCallback(NanoHookEventArgs args);
        public event NanoHookCallback Event;

        IntPtr key_hookID = IntPtr.Zero;
        IntPtr mouse_hookID = IntPtr.Zero;


        public void Install()
        {
            Install(HookEventType.KeyBoard | HookEventType.Mouse);
        }

        public void Install(HookEventType type)
        {
            key_hookHandler = key_HookFunc;
            mouse_hookHandler = mouse_HookFunc;

            if ((type & HookEventType.KeyBoard) == HookEventType.KeyBoard)
            {
                key_hookID = SetHook(key_hookHandler, WH_KEYBOARD_LL);
            }

            if ((type & HookEventType.Mouse) == HookEventType.Mouse)
            {
                mouse_hookID = SetHook(mouse_hookHandler, WH_MOUSE_LL);
            }
        }

        public void Uninstall()
        {
            UnhookWindowsHookEx(key_hookID);
            UnhookWindowsHookEx(mouse_hookID);
        }



        IntPtr SetHook(HookHandler proc, int idHook)
        {
            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                return SetWindowsHookEx(idHook, proc, module.BaseAddress, 0);
            }
        }

        private IntPtr key_HookFunc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                Event?.Invoke(new NanoHookEventArgs(HookEventType.KeyBoard));
            }

            return CallNextHookEx(key_hookID, nCode, wParam, lParam);
        }

        private IntPtr mouse_HookFunc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                Event?.Invoke(new NanoHookEventArgs(HookEventType.Mouse));
            }

            return CallNextHookEx(mouse_hookID, nCode, wParam, lParam);
        }


        ~NanoHook()
        {
            Uninstall();
        }
    }

    public enum HookEventType
    {
        KeyBoard = 1,
        Mouse = 2
    }

    public class NanoHookEventArgs : EventArgs
    {
        public HookEventType EventType { get; set; }

        public NanoHookEventArgs(HookEventType eventType)
        {
            EventType = eventType;
        }
    }
}


