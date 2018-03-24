using System;
using System.Text;
using static TimeMonkey.Core.WinAPI;

namespace TimeMonkey.Core
{
    public static class KeyboardNative
    {

        //Used to pass Unicode characters as if they were keystrokes. The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods
        static int lastVirtualKeyCode;
        static int lastScanCode;
        static byte[] lastKeyState = new byte[255];
        static bool lastIsDead;


        /// <summary>
        ///     Translates a virtual key to its character equivalent using the current keyboard layout
        /// </summary>
        /// <param name="virtualKeyCode"></param>
        /// <param name="scanCode"></param>
        /// <param name="fuState"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        internal static void TryGetCharFromKeyboardState(int virtualKeyCode, int scanCode, int fuState, out char[] chars)
        {
            var dwhkl = GetActiveKeyboard(); //get the active keyboard layout
            TryGetCharFromKeyboardState(virtualKeyCode, scanCode, fuState, dwhkl, out chars);
        }


        /// <summary>
        ///     Translates a virtual key to its character equivalent using a specified keyboard layout
        /// </summary>
        /// <param name="virtualKeyCode"></param>
        /// <param name="scanCode"></param>
        /// <param name="fuState"></param>
        /// <param name="dwhkl"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        internal static void TryGetCharFromKeyboardState(int virtualKeyCode, int scanCode, int fuState, IntPtr dwhkl, out char[] chars)
        {
            var pwszBuff = new StringBuilder(64);
            var keyboardState = KeyboardState.GetCurrent();
            var currentKeyboardState = keyboardState.GetNativeState();
            var isDead = false;


            //to indicate upper case
            if (keyboardState.IsDown(VKeys.SHIFT))
                currentKeyboardState[(byte)VKeys.SHIFT] = 0x80;

            if (keyboardState.IsToggled(VKeys.CAPITAL))
                currentKeyboardState[(byte)VKeys.CAPITAL] = 0x01;

            var relevantChars = User32.ToUnicodeEx(virtualKeyCode, scanCode, currentKeyboardState, pwszBuff, pwszBuff.Capacity, fuState, dwhkl);

            switch (relevantChars)
            {
                case -1:
                    isDead = true;
                    ClearKeyboardBuffer(virtualKeyCode, scanCode, dwhkl);
                    chars = null;
                    break;

                case 0:
                    chars = null;
                    break;

                case 1:
                    if (pwszBuff.Length > 0) chars = new[] { pwszBuff[0] };
                    else chars = null;
                    break;

                // Two or more (only two of them is relevant)
                default:
                    if (pwszBuff.Length > 1) chars = new[] { pwszBuff[0], pwszBuff[1] };
                    else chars = new[] { pwszBuff[0] };
                    break;
            }

            if (lastVirtualKeyCode != 0 && lastIsDead)
            {
                if (chars != null)
                {
                    var sbTemp = new StringBuilder(5);
                    User32.ToUnicodeEx(lastVirtualKeyCode, lastScanCode, lastKeyState, sbTemp, sbTemp.Capacity, 0, dwhkl);
                    lastIsDead = false;
                    lastVirtualKeyCode = 0;
                }

                return;
            }

            lastScanCode = scanCode;
            lastVirtualKeyCode = virtualKeyCode;
            lastIsDead = isDead;
            lastKeyState = (byte[])currentKeyboardState.Clone();
        }


        private static IntPtr GetActiveKeyboard()
        {
            var hActiveWnd = User32.GetForegroundWindow(); //handle to focused window
            var hCurrentWnd = User32.GetWindowThreadProcessId(hActiveWnd, out int dwProcessId);
            //thread of focused window
            return User32.GetKeyboardLayout((int)hCurrentWnd); //get the layout identifier for the thread whose window is focused
        }

        private static void ClearKeyboardBuffer(int vk, int sc, IntPtr hkl)
        {
            var sb = new StringBuilder(10);

            int rc;
            do
            {
                var lpKeyStateNull = new byte[255];
                rc = User32.ToUnicodeEx(vk, sc, lpKeyStateNull, sb, sb.Capacity, 0, hkl);
            } while (rc < 0);
        }


    }
}
