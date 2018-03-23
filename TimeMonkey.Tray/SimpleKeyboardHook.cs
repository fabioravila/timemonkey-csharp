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
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
            {
                return SetWindowsHookEx(WinAPI.WH_KEYBOARD_LL, proc, module.BaseAddress, 0);
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

                var keyHookStruct = (CWPSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPSTRUCT));

                var isExtendedKey = (keyHookStruct.flags & 0x1) > 0;
                var keyData = (VKeys)Marshal.ReadInt32(lParam);

                var isDown = (iwParam == KeyboardMessages.WM_KEYDOWN || iwParam == KeyboardMessages.WM_SYSKEYDOWN);
                var isUp = (iwParam == KeyboardMessages.WM_KEYUP || iwParam == KeyboardMessages.WM_SYSKEYUP);

                //check modifiers
                // Is Control being held down?
                var control = CheckModifier(VKeys.CONTROL);
                // Is Shift being held down?
                var shift = CheckModifier(VKeys.SHIFT);
                // Is Alt being held down?
                var alt = CheckModifier(VKeys.MENU);



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
}
