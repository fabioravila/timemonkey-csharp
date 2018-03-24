

namespace TimeMonkey.Tray
{
    internal static class ExtensionsMethods
    {
        const char NON_CHAR = (char)0x0;

        public static bool IsNonChar(this char ch)
        {
            return ch == NON_CHAR;
        }

    }
}
