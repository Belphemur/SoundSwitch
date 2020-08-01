using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SoundSwitch.Audio.Manager.Interop.Com.User
{
    public static class User32
    {
        public static class NativeMethods
        {
            
                    
            [StructLayout(LayoutKind.Sequential)]
            public struct HWND
            {
                public IntPtr h;
 
                public static HWND Cast(IntPtr h)
                {
                    HWND hTemp = new HWND();
                    hTemp.h = h;
                    return hTemp;
                }
 
                public static implicit operator IntPtr(HWND h)
                {
                    return h.h;
                }
 
                public static HWND NULL
                {
                    get
                    {
                        HWND hTemp = new HWND();
                        hTemp.h = IntPtr.Zero;
                        return hTemp;
                    }
                }
 
                public static bool operator==(HWND hl, HWND hr)
                {
                    return hl.h == hr.h;
                }
 
                public static bool operator!=(HWND hl, HWND hr)
                {
                    return hl.h != hr.h;
                }
 
                override public bool Equals(object oCompare)
                {
                    HWND hr = Cast((HWND)oCompare);
                    return h == hr.h;
                }
 
                public override int GetHashCode()
                {
                    return (int) h;
                }
            }

            
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr GetWindowThreadProcessId([In] HWND hWnd, [Out] out uint ProcessId);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern HWND GetForegroundWindow();

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern int GetWindowText(HWND hWnd, StringBuilder text, int count);

            [DllImport("user32.dll")]
            public static extern int GetClassName(HWND hWnd, StringBuilder text, int count);

            public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, HWND hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

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
        public static string GetWindowText(NativeMethods.HWND window)
        {
            var sb = new StringBuilder(256);
            NativeMethods.GetWindowText(window, sb, 256);
            return sb.ToString();
        }
        
        /// <summary>
        /// Get class of the window
        /// </summary>
        public static string GetWindowClass(NativeMethods.HWND window)
        {
            var sb = new StringBuilder(256);
            NativeMethods.GetClassName(window, sb, 256);
            return sb.ToString();
        }
    }
}