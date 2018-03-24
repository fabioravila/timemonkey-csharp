using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static TimeMonkey.Tray.WinAPI;
using static TimeMonkey.Tray.WinAPI.User32;

namespace TimeMonkey.Tray
{
    /// <summary>
    /// Class for intercepting low level Windows mouse hooks.
    /// </summary>
    public class SimpleMouseHook
    {
        /// <summary>
        /// Internal callback processing function
        /// </summary>
        HookHandler hookHandler;

        /// <summary>
        /// Function to be called when defined even occurs
        /// </summary>
        /// <param name="mouseStruct">MSLLHOOKSTRUCT mouse structure</param>
        public delegate void MouseHookCallback(MSLLHOOKSTRUCT mouseStruct);

        /// <summary>
        /// Function to be called when defined even occurs
        /// </summary>
        /// <param name="mouseStruct">MSLLHOOKSTRUCT mouse structure</param>
        public delegate void GenericHookCallback(MSLLHOOKSTRUCT mouseStruct, MouseMessages mouseEvent);


        private readonly int systemDoubleClickTime;

        #region Events
        public event MouseHookCallback LeftButtonDown;
        public event MouseHookCallback LeftButtonUp;
        public event MouseHookCallback RightButtonDown;
        public event MouseHookCallback RightButtonUp;
        public event MouseHookCallback MouseMove;
        public event MouseHookCallback MouseWheel;
        public event MouseHookCallback DoubleClick;
        public event MouseHookCallback MiddleButtonUp;
        public event MouseHookCallback MiddleButtonDown;
        public event GenericHookCallback MouseEvent;
        #endregion

        /// <summary>
        /// Low level mouse hook's ID
        /// </summary>
        private IntPtr hookID = IntPtr.Zero;

        /// <summary>
        /// Install low level mouse hook
        /// </summary>
        /// <param name="mouseHookCallbackFunc">Callback function</param>
        public void Install()
        {
            hookHandler = HookFunc;
            hookID = SetHook(hookHandler);
        }

        /// <summary>
        /// Remove low level mouse hook
        /// </summary>
        public void Uninstall()
        {
            if (hookID == IntPtr.Zero)
                return;

            UnhookWindowsHookEx(hookID);
            hookID = IntPtr.Zero;
        }

        public SimpleMouseHook()
        {
            systemDoubleClickTime = GetDoubleClickTime();
        }

        /// <summary>
        /// Destructor. Unhook current hook
        /// </summary>
        ~SimpleMouseHook()
        {
            Uninstall();
        }

        /// <summary>
        /// Sets hook and assigns its ID for tracking
        /// </summary>
        /// <param name="proc">Internal callback function</param>
        /// <returns>Hook ID</returns>
        private IntPtr SetHook(HookHandler proc)
        {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, module.BaseAddress, 0);
            }
        }

        /// <summary>
        /// Callback function
        /// </summary>
        private IntPtr HookFunc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // parse system messages
            if (nCode >= 0)
            {
                var data = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                var message = (MouseMessages)wParam;


                switch (message)
                {
                    case MouseMessages.WM_MOUSEMOVE:
                        break;
                    case MouseMessages.WM_LBUTTONDOWN:
                        break;
                    case MouseMessages.WM_RBUTTONDOWN:
                        break;
                    case MouseMessages.WM_MBUTTONDOWN:
                        break;
                    case MouseMessages.WM_LBUTTONUP:
                        break;
                    case MouseMessages.WM_RBUTTONUP:
                        break;
                    case MouseMessages.WM_MBUTTONUP:
                        break;
                    case MouseMessages.WM_MOUSEWHEEL:
                        break;
                    case MouseMessages.WM_MOUSEHWHEEL:
                        break;
                    case MouseMessages.WM_LBUTTONDBLCLK:
                        break;
                    case MouseMessages.WM_RBUTTONDBLCLK:
                        break;
                    case MouseMessages.WM_MBUTTONDBLCLK:
                        break;
                    case MouseMessages.WM_XBUTTONDOWN:
                        break;
                    case MouseMessages.WM_XBUTTONUP:
                        break;
                    case MouseMessages.WM_XBUTTONDBLCLK:
                        break;
                    default:
                        break;
                }


                if (MouseMessages.WM_LBUTTONDOWN == message)
                    LeftButtonDown?.Invoke(data);
                if (MouseMessages.WM_LBUTTONUP == message)
                    LeftButtonUp?.Invoke(data);
                if (MouseMessages.WM_RBUTTONDOWN == message)
                    RightButtonDown?.Invoke(data);
                if (MouseMessages.WM_RBUTTONUP == message)
                    RightButtonUp?.Invoke(data);
                if (MouseMessages.WM_MOUSEMOVE == message)
                    MouseMove?.Invoke(data);
                if (MouseMessages.WM_MOUSEWHEEL == message)
                    MouseWheel?.Invoke(data);
                if (MouseMessages.WM_LBUTTONDBLCLK == message)
                    DoubleClick?.Invoke(data);
                if (MouseMessages.WM_MBUTTONDOWN == message)
                    MiddleButtonDown?.Invoke(data);
                if (MouseMessages.WM_MBUTTONUP == message)
                    MiddleButtonUp?.Invoke(data);

                //Generic Event, fire with type
                MouseEvent?.Invoke(data, message);
            }
            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }
    }


    public class SimpleMouseEventArgs : EventArgs
    {
        public SimpleMouseButtons Button { get; }
        public int Clicks { get; }
        public int X { get; }
        public int Y { get; }
        public int Delta { get; }
        public int TimeSpan { get; }

        public bool IsButtonUp { get; }
        public bool IsButtonDown { get; }


        //public Point Location { get; }
        public SimpleMouseEventArgs(SimpleMouseButtons button, int clicks, int x, int y, int delta, int timespan, bool isButtonUp, bool isButtonDown)
        {
            Button = button;
            Clicks = clicks;
            X = x;
            Y = y;
            Delta = delta;
            TimeSpan = timespan;
            IsButtonUp = isButtonUp;
            IsButtonDown = isButtonDown;
        }
    }

    public enum SimpleMouseButtons
    {
        None = 0,
        Left = 1048576,
        Right = 2097152,
        Middle = 4194304,
        XButton1 = 8388608,
        XButton2 = 16777216
    }

}
