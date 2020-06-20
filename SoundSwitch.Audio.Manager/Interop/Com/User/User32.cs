using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SoundSwitch.Audio.Manager.Interop.Com.User
{
    public static class User32
    {
        internal static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowThreadProcessId([In] IntPtr hWnd, [Out] out uint ProcessId);

            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

            [DllImport("user32.dll")]
            public static extern int GetClassName(IntPtr hWnd, StringBuilder text, int count);

            internal delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

            [DllImport("user32.dll")]
            public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

            internal const uint WINEVENT_OUTOFCONTEXT    = 0;
            internal const uint EVENT_SYSTEM_FOREGROUND  = 0x0003;
            internal const uint EVENT_SYSTEM_MINIMIZEEND = 0x0017;
        }

        public static uint ForegroundProcessId
        {
            get
            {
                var activeWindowHandle = NativeMethods.GetForegroundWindow();
                NativeMethods.GetWindowThreadProcessId(activeWindowHandle, out var processId);
                return processId;
            }
        }

        /// <summary>
        /// Get text of the window
        /// </summary>
        public static string GetWindowText(IntPtr window)
        {
            var sb = new StringBuilder(256);
            NativeMethods.GetWindowText(window, sb, 256);
            return sb.ToString();
        }
        
        /// <summary>
        /// Get class of the window
        /// </summary>
        public static string GetWindowClass(IntPtr window)
        {
            var sb = new StringBuilder(256);
            NativeMethods.GetClassName(window, sb, 256);
            return sb.ToString();
        }
    }
}