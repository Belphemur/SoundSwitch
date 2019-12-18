using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

            internal delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

            [DllImport("user32.dll")]
            public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

            internal const uint WINEVENT_OUTOFCONTEXT = 0;
            internal const uint EVENT_SYSTEM_FOREGROUND = 3;
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
    }
}