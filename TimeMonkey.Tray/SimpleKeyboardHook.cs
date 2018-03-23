using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static TimeMonkey.Tray.WinAPI;
using static TimeMonkey.Tray.WinAPI.User32;

namespace TimeMonkey.Tray
{
    public class SimpleKeyboardHook
    {
        /// <summary>
        /// Internal callback processing function
        /// </summary>
        HookHandler hookHandler;

        /// <summary>
        /// Function that will be called when defined events occur
        /// </summary>
        /// <param name="key">VKeys</param>
        public delegate void KeyboardHookCallback(VKeys key);
        public delegate void GenericKeyboardHookCallback(VKeys key, KeyState mode);

        #region Events
        public event KeyboardHookCallback KeyDown;
        public event KeyboardHookCallback KeyUp;

        public event GenericKeyboardHookCallback KeyEvent;
        #endregion

        /// <summary>
        /// Hook ID
        /// </summary>
        private IntPtr hookID = IntPtr.Zero;

        /// <summary>
        /// Install low level keyboard hook
        /// </summary>
        public void Install()
        {
            hookHandler = HookFunc;
            hookID = SetHook(hookHandler);
        }

        /// <summary>
        /// Remove low level keyboard hook
        /// </summary>
        public void Uninstall()
        {
            UnhookWindowsHookEx(hookID);
        }

        /// <summary>
        /// Registers hook with Windows API
        /// </summary>
        /// <param name="proc">Callback function</param>
        /// <returns>Hook ID</returns>
        private IntPtr SetHook(HookHandler proc)
        {
            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, module.BaseAddress, 0);
            }
        }

        /// <summary>
        /// Default hook call, which analyses pressed keys
        /// </summary>
        private IntPtr HookFunc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {

                var iwParam = (KeyboardMessages)wParam;
                var keyHookStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

                var keyData = (VKeys)keyHookStruct.vkCode;

                var keyModifiers = ReturnModifiers();

                var isDown = (iwParam == KeyboardMessages.WM_KEYDOWN || iwParam == KeyboardMessages.WM_SYSKEYDOWN);
                var isUp = (iwParam == KeyboardMessages.WM_KEYUP || iwParam == KeyboardMessages.WM_SYSKEYUP);


                var isExtendedKey = (keyHookStruct.flags & 0x1) > 0;

                if (isDown)
                {
                    KeyDown?.Invoke(keyData);
                    KeyEvent?.Invoke(keyData, KeyState.DOWN);
                }

                if (isUp)
                {
                    KeyUp?.Invoke(keyData);
                    KeyEvent?.Invoke(keyData, KeyState.UP);
                }
            }

            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        static VKeys ReturnModifiers()
        {
            //check modifiers
            // Is Control being held down?
            var control = CheckModifier(VKeys.CONTROL);
            // Is Shift being held down?
            var shift = CheckModifier(VKeys.SHIFT);
            // Is Alt being held down?
            var alt = CheckModifier(VKeys.MENU);

            //Append with flags
            return (control ? VKeys.CONTROL : VKeys.NONE) |
                   (shift ? VKeys.SHIFT : VKeys.NONE) |
                   (alt ? VKeys.MENU : VKeys.NONE);

        }

        static bool CheckModifier(VKeys vKey)
        {
            return (GetKeyState((int)vKey) & 0x8000) > 0;
        }

        /// <summary>
        /// Destructor. Unhook current hook
        /// </summary>
        ~SimpleKeyboardHook()
        {
            Uninstall();
        }
        public enum KeyState
        {
            DOWN = 0,
            UP = 1
        }
    }

    public class SimpleKeyEventArgs : EventArgs
    {
        public VKeys Key { get; private set; }

        VKeys modifiers = VKeys.NONE;

        public bool Alt { get { return (modifiers & VKeys.MENU) == VKeys.MENU; } }
        public bool Shift { get { return (modifiers & VKeys.SHIFT) == VKeys.SHIFT; } }
        public bool Control { get { return (modifiers & VKeys.CONTROL) == VKeys.CONTROL; } }

        public SimpleKeyEventArgs(VKeys key, VKeys modifiers) : this(key)
        {
            this.modifiers = modifiers;
        }

        public SimpleKeyEventArgs(VKeys key)
        {
            Key = key;
        }
    }

    //public class SimpleKeyPressEventArgs : SimpleKeyEventArgs
    //{

    //}

}
