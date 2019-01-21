using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static TimeMonkey.Core.WinAPI;
using static TimeMonkey.Core.WinAPI.User32;

namespace TimeMonkey.Core
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
        public delegate void KeyboardHookCallback(SimpleKeyEventArgs args);
        public delegate void KeyboardPressHookCallback(SimpleKeyPressEventArgs args);

        #region Events
        public event KeyboardHookCallback KeyDown;
        public event KeyboardHookCallback KeyUp;
        public event KeyboardHookCallback KeyEvent;

        private int keyPressEventCount = 0;
        private event KeyboardPressHookCallback keyPressEvent;
        public event KeyboardPressHookCallback KeyPressEvent
        {
            add
            {
                keyPressEvent += value;
                keyPressEventCount++;
            }

            remove
            {
                keyPressEvent -= value;
                keyPressEventCount--;
            }
        }

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
                var keyStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

                var keyData = (VKeys)keyStruct.vkCode;
                var keyModifiers = ReturnModifiers();

                //NOTE: It is possible to key isDown && isUp OR !isDown && !isUp
                var isUp = (iwParam == KeyboardMessages.WM_KEYUP || iwParam == KeyboardMessages.WM_SYSKEYUP);
                var isDown = (iwParam == KeyboardMessages.WM_KEYDOWN || iwParam == KeyboardMessages.WM_SYSKEYDOWN);

                //NOT NOW
                //var isExtendedKey = (keyHookStruct.flags & 0x1) > 0;

                var keyArgs = new SimpleKeyEventArgs(keyData, isUp, isDown, (int)keyStruct.time, keyModifiers);


                if (isUp)
                {
                    KeyUp?.Invoke(keyArgs);
                    KeyEvent?.Invoke(keyArgs);
                }

                if (isDown)
                {
                    KeyDown?.Invoke(keyArgs);
                    KeyEvent?.Invoke(keyArgs);
                }


                //Has key press handler
                if (isDown && keyPressEventCount > 0)
                {
                    if (keyStruct.vkCode == (int)VKeys.PACKET)
                    {
                        var ch = (char)keyStruct.scanCode;
                        if (!ch.IsNonChar())
                            keyPressEvent?.Invoke(new SimpleKeyPressEventArgs(ch, (int)keyStruct.time));
                    }
                    else
                    {
                        KeyboardNative.TryGetCharFromKeyboardState((int)keyStruct.vkCode, keyStruct.scanCode, (int)keyStruct.flags, out char[] chars);

                        if (chars != null)
                        {
                            foreach (var ch in chars)
                            {
                                if (!ch.IsNonChar())
                                {
                                    keyPressEvent?.Invoke(new SimpleKeyPressEventArgs(ch, (int)keyStruct.time));
                                }
                            }
                        }
                    }
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
        public VKeys Key { get; }
        public bool IsUp { get; }
        public bool IsDown { get; }
        public int Timestamp { get; }
        public VKeys Modifiers { get; }

        public bool Alt { get { return (Modifiers & VKeys.MENU) == VKeys.MENU; } }
        public bool Shift { get { return (Modifiers & VKeys.SHIFT) == VKeys.SHIFT; } }
        public bool Control { get { return (Modifiers & VKeys.CONTROL) == VKeys.CONTROL; } }

        public SimpleKeyEventArgs(VKeys key, bool isUp, bool isDown, int timestamp, VKeys modifiers) : this(key, isUp, isDown, timestamp)
        {
            Modifiers = modifiers;
        }

        public SimpleKeyEventArgs(VKeys key, bool isUp, bool isDown, int timestamp)
        {
            Key = key;
            IsUp = isUp;
            IsDown = isDown;
            Timestamp = timestamp;
            Modifiers = VKeys.NONE;
        }
    }

    public class SimpleKeyPressEventArgs : EventArgs
    {
        public char KeyChar { get; }
        public int Timestamp { get; }

        public SimpleKeyPressEventArgs(char keyChar, int timestamp)
        {
            KeyChar = keyChar;
            Timestamp = timestamp;
        }
    }

}


