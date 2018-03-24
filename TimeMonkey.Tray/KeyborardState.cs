using System;
using System.Collections.Generic;
using System.Linq;
using static TimeMonkey.Tray.WinAPI;

namespace TimeMonkey.Tray
{
    /// <summary>
    ///     Contains a snapshot of a keyboard state at certain moment and provides methods
    ///     of querying whether specific keys are pressed or locked.
    /// </summary>
    /// <remarks>
    ///     This class is basically a managed wrapper of GetKeyboardState API function
    ///     http://msdn.microsoft.com/en-us/library/ms646299
    /// </remarks>
    public class KeyboardState
    {
        private readonly byte[] m_KeyboardStateNative;

        private KeyboardState(byte[] keyboardStateNative)
        {
            m_KeyboardStateNative = keyboardStateNative;
        }

        /// <summary>
        ///     Makes a snapshot of a keyboard state to the moment of call and returns an
        ///     instance of <see cref="KeyboardState" /> class.
        /// </summary>
        /// <returns>An instance of <see cref="KeyboardState" /> class representing a snapshot of keyboard state at certain moment.</returns>
        public static KeyboardState GetCurrent()
        {
            var keyboardStateNative = new byte[256];
            User32.GetKeyboardState(keyboardStateNative);
            return new KeyboardState(keyboardStateNative);
        }

        internal byte[] GetNativeState()
        {
            return m_KeyboardStateNative;
        }

        public bool IsDown(VKeys key)
        {
            if ((int)key < 256) return IsDownRaw(key);
            if (key == VKeys.MENU) return IsDownRaw(VKeys.LMENU) || IsDownRaw(VKeys.RMENU);
            if (key == VKeys.SHIFT) return IsDownRaw(VKeys.LSHIFT) || IsDownRaw(VKeys.RSHIFT);
            if (key == VKeys.CONTROL) return IsDownRaw(VKeys.LCONTROL) || IsDownRaw(VKeys.RCONTROL);
            return false;
        }

        private bool IsDownRaw(VKeys key)
        {
            var keyState = GetKeyState(key);
            var isDown = GetHighBit(keyState);
            return isDown;
        }

        /// <summary>
        ///     Indicate weather specified key was toggled at the moment when snapshot was created or not.
        /// </summary>
        /// <param name="key">Key (corresponds to the virtual code of the key)</param>
        /// <returns>
        ///     <b>true</b> if toggle key like (CapsLock, NumLocke, etc.) was on. <b>false</b> if it was off.
        ///     Ordinal (non toggle) keys return always false.
        /// </returns>
        public bool IsToggled(VKeys key)
        {
            var keyState = GetKeyState(key);
            var isToggled = GetLowBit(keyState);
            return isToggled;
        }

        /// <summary>
        ///     Indicates weather every of specified keys were down at the moment when snapshot was created.
        ///     The method returns false if even one of them was up.
        /// </summary>
        /// <param name="keys">Keys to verify whether they were down or not.</param>
        /// <returns><b>true</b> - all were down. <b>false</b> - at least one was up.</returns>
        public bool AreAllDown(IEnumerable<VKeys> keys)
        {
            return keys.All(IsDown);
        }

        private byte GetKeyState(VKeys key)
        {
            var virtualKeyCode = (int)key;
            if (virtualKeyCode < 0 || virtualKeyCode > 255)
                throw new ArgumentOutOfRangeException("key", key, "The value must be between 0 and 255.");
            return m_KeyboardStateNative[virtualKeyCode];
        }

        private static bool GetHighBit(byte value)
        {
            return value >> 7 != 0;
        }

        private static bool GetLowBit(byte value)
        {
            return (value & 1) != 0;
        }
    }


   
}
