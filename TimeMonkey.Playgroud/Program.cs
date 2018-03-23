using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeMonkey.Tray;

namespace TimeMonkey.Playgroud
{
    class Program
    {
        static Stopwatch afk_stopwatch = new Stopwatch();
        static SimpleMouseHook mouseHook = new SimpleMouseHook();
        static SimpleKeyboardHook keyboardHook = new SimpleKeyboardHook();
        static TimeSpan akf_treshold = TimeSpan.FromSeconds(20);

        static void Main(string[] args)
        {
            Application.ApplicationExit += Application_ApplicationExit;

            try
            {
                mouseHook.MouseEvent += MouseHook_MouseEvent;
                keyboardHook.KeyEvent += KeyboardHook_KeyEvent;

                //mouseHook.Install();
                keyboardHook.Install();

                Application.Run();
            }
            catch (Exception)
            {
                Console.WriteLine("[ERROR]");
            }
            finally
            {
                UninstallHooks();

                Console.WriteLine("[END]");
                Console.WriteLine("Press any key to close");
                Console.ReadKey(true);
            }
        }

        static void KeyboardHook_KeyEvent(WinAPI.VKeys key, SimpleKeyboardHook.KeyState mode)
        {
            Console.WriteLine($"KEYBOARD: {key} {mode}");
        }

        static void MouseHook_MouseEvent(WinAPI.MSLLHOOKSTRUCT mouseStruct, WinAPI.MouseMessages mouseEvent)
        {
            Console.WriteLine($"MOUSE: {mouseEvent} x:{mouseStruct.pt.x} y:{mouseStruct.pt.y}");
        }

        static void Application_ApplicationExit(object sender, EventArgs e)
        {
            UninstallHooks();
        }

        static void UninstallHooks()
        {

            if (mouseHook != null)
            {
                mouseHook.MouseEvent -= MouseHook_MouseEvent;
                mouseHook.Uninstall();
                mouseHook = null;
            }

            if (keyboardHook != null)
            {
                keyboardHook.KeyEvent -= KeyboardHook_KeyEvent;
                keyboardHook.Uninstall();
                keyboardHook = null;
            }

            afk_stopwatch?.Stop();
        }
    }
}
