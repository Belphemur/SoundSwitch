using System;
using System.Runtime.InteropServices;
using SoundSwitch.Audio.Manager.Interop.Com.Threading;

namespace SoundSwitch.Audio.Manager.Interop.Com.User
{
    internal static class User32
    {
        private static class NativeMethods
        {
            [DllImport("user32.dll", PreserveSig = false)]
            public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

            [DllImport("user32.dll", PreserveSig = false)]
            public static extern IntPtr GetForegroundWindow();
        }

        public static int ForegroundProcessId
        {
            get
            {
                var activeWindowHandle = NativeMethods.GetForegroundWindow();
                NativeMethods.GetWindowThreadProcessId(activeWindowHandle, out var processId);
                return (int)processId;
            }
        }
    }
}