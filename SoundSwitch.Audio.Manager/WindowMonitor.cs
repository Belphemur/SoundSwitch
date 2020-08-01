using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SoundSwitch.Audio.Manager.Interop.Com.Threading;
using SoundSwitch.Audio.Manager.Interop.Com.User;

namespace SoundSwitch.Audio.Manager
{
    public class WindowMonitor
    {
        public class Event : EventArgs
        {
            /// <summary>
            /// ID of the process that is now active
            /// </summary>
            public uint ProcessId { get; }

            /// <summary>
            /// Name of the process that is active
            /// </summary>
            public string ProcessName { get; }

            /// <summary>
            /// Name of the active window
            /// </summary>
            public string WindowName { get; }

            /// <summary>
            /// Class of the window
            /// </summary>
            public string WindowClass { get; }

            public Event(uint processId, string processName, string windowName, string windowClass)
            {
                ProcessId   = processId;
                ProcessName = processName;
                WindowName  = windowName;
                WindowClass = windowClass;
            }

            public override string ToString()
            {
                return $"{nameof(ProcessId)}: {ProcessId}, {nameof(ProcessName)}: {ProcessName}, {nameof(WindowName)}: {WindowName}, {nameof(WindowClass)}: {WindowClass}";
            }
        }

        public event EventHandler<Event> ForegroundChanged;
        // public event EventHandler<Event> WindowClosed;

        private readonly User32.NativeMethods.WinEventDelegate _foregroundWindowChanged;
        // private readonly User32.NativeMethods.WinEventDelegate _windowClosed;

        public WindowMonitor()
        {
            _foregroundWindowChanged = (hook, type, hwnd, idObject, child, thread, time) =>
            {
                // ignore any event not pertaining directly to the window
                if (idObject != User32.NativeMethods.OBJID_WINDOW)
                    return;
 
                // Ignore if this is a bogus hwnd (shouldn't happen)
                if (hwnd == IntPtr.Zero)
                    return;
                var windowProcessId = ProcessWindowInformation(hwnd, out var wndText, out var wndClass);

                Task.Factory.StartNew(() =>
                {
                    var process     = Process.GetProcessById((int) windowProcessId);
                    var processName = process.MainModule?.FileName ?? "N/A";
                    ForegroundChanged?.Invoke(this, new Event(windowProcessId, processName, wndText, wndClass));
                });
            };
            //Window close != Window destroyed
            // _windowClosed = (hook, type, hwnd, idObject, child, thread, time) =>
            // {
            //     // Ignore if this is a bogus hwnd (shouldn't happen)
            //     if (hwnd == IntPtr.Zero || !User32.NativeMethods.IsWindow(hwnd))
            //         return;
            //     var windowProcessId = ProcessWindowInformation(hwnd, out var wndText, out var wndClass);
            //
            //     Task.Factory.StartNew(() =>
            //     {
            //         var process     = Process.GetProcessById((int) windowProcessId);
            //         var processName = process.MainModule?.FileName ?? "N/A";
            //         WindowClosed?.Invoke(this, new Event(windowProcessId, processName, wndText, wndClass));
            //     });
            // };

            ComThread.Invoke(() =>
            {
                User32.NativeMethods.SetWinEventHook(User32.NativeMethods.EVENT_SYSTEM_MINIMIZEEND,
                    User32.NativeMethods.EVENT_SYSTEM_MINIMIZEEND,
                    IntPtr.Zero, _foregroundWindowChanged,
                    0,
                    0,
                    User32.NativeMethods.WINEVENT_OUTOFCONTEXT);

                User32.NativeMethods.SetWinEventHook(User32.NativeMethods.EVENT_SYSTEM_FOREGROUND,
                    User32.NativeMethods.EVENT_SYSTEM_FOREGROUND,
                    IntPtr.Zero, _foregroundWindowChanged,
                    0,
                    0,
                    User32.NativeMethods.WINEVENT_OUTOFCONTEXT);
                // User32.NativeMethods.SetWinEventHook(User32.NativeMethods.EVENT_OBJECT_DESTROY,
                //     User32.NativeMethods.EVENT_OBJECT_DESTROY,
                //     IntPtr.Zero, _windowClosed,
                //     0,
                //     0,
                //     User32.NativeMethods.WINEVENT_OUTOFCONTEXT);
            });
        }

        private static uint ProcessWindowInformation(User32.NativeMethods.HWND hwnd, out string wndText, out string wndClass)
        {
            var windowProcessId = ComThread.Invoke(() =>
            {
                try
                {
                    User32.NativeMethods.GetWindowThreadProcessId(hwnd, out var processId);
                    return processId;
                }
                catch
                {
                    return (uint) 0;
                }
            });

            wndText = ComThread.Invoke(() =>
            {
                try
                {
                    return User32.GetWindowText(hwnd);
                }
                catch (Exception)
                {
                    return "";
                }
            });
            wndClass = ComThread.Invoke(() =>
            {
                try
                {
                    return User32.GetWindowClass(hwnd);
                }
                catch (Exception)
                {
                    return "";
                }
            });
            return windowProcessId;
        }
    }
}