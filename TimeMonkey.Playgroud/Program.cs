using System;
using System.Diagnostics;
using System.Windows.Forms;
using TimeMonkey.Core;

namespace TimeMonkey.Playgroud
{
    class Program
    {
        static Stopwatch afk_stopwatch = new Stopwatch();
        static SimpleMouseHook mouseHook = new SimpleMouseHook();
        static SimpleKeyboardHook keyboardHook = new SimpleKeyboardHook();
        static NanoHook nanoHook = new NanoHook();
        static TimeSpan akf_treshold = TimeSpan.FromSeconds(20);

        static void Main(string[] args)
        {
            Application.ApplicationExit += Application_ApplicationExit;

            try
            {
                mouseHook.MouseEvent += MouseHook_MouseEvent;
                keyboardHook.KeyEvent += KeyboardHook_KeyEvent;
                keyboardHook.KeyPressEvent += KeyboardHook_KeyPressEvent;
                nanoHook.Event += NanoHook_Event;

                //mouseHook.Install();
                keyboardHook.Install();
                //nanoHook.Install();
                //nanoHook.Install(HookEventType.KeyBoard);
                //nanoHook.Install(HookEventType.Mouse);
                nanoHook.Install(HookEventType.KeyBoard | HookEventType.Mouse);

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

        private static void KeyboardHook_KeyPressEvent(SimpleKeyPressEventArgs args)
        {
            //Console.WriteLine($"KEYBOARD: KEYPRESS:{args.KeyChar}");
        }

        private static void NanoHook_Event(NanoHookEventArgs args)
        {
            Console.WriteLine($"NANO: {args.EventType.ToString().ToUpper()}");
        }

        private static void KeyboardHook_KeyEvent(SimpleKeyEventArgs args)
        {
            //Console.WriteLine($"KEYBOARD: KEY:{args.Key} IS:{(args.IsUp ? "UP" : "DOWN")} WITH:{args.Modifiers}");
        }


        static void MouseHook_MouseEvent(WinAPI.MSLLHOOKSTRUCT mouseStruct, WinAPI.MouseMessages mouseEvent)
        {
            Console.WriteLine($"MOUSE: {mouseEvent} x:{mouseStruct.pt.x} y:{mouseStruct.pt.y} data:{mouseStruct.mouseData} wheeldelta: {mouseStruct.wheelDelta}");
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

            if (nanoHook != null)
            {
                nanoHook.Event -= NanoHook_Event;
                nanoHook.Uninstall();
                nanoHook = null;
            }

            afk_stopwatch?.Stop();
        }
    }
}
