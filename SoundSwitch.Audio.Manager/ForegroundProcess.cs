using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SoundSwitch.Audio.Manager.Interop.Com.Threading;
using SoundSwitch.Audio.Manager.Interop.Com.User;

namespace SoundSwitch.Audio.Manager
{
    public class ForegroundProcess
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

        public event EventHandler<Event> Changed;

        private readonly User32.NativeMethods.WinEventDelegate _winEventDelegate;


        public ForegroundProcess()
        {
            _winEventDelegate = (hook, type, hwnd, idObject, child, thread, time) =>
            {
                // ignore any event not pertaining directly to the window
                if (idObject != User32.NativeMethods.OBJID_WINDOW)
                    return;
 
                // Ignore if this is a bogus hwnd (shouldn't happen)
                if (hwnd == IntPtr.Zero)
                    return;
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

                var wndText = ComThread.Invoke(() =>
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
                var wndClass = ComThread.Invoke(() =>
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

                Task.Factory.StartNew(() =>
                {
                    var process     = Process.GetProcessById((int) windowProcessId);
                    var processName = process.MainModule?.FileName ?? "N/A";
                    Changed?.Invoke(this, new Event(windowProcessId, processName, wndText, wndClass));
                });
            };

            ComThread.Invoke(() =>
            {
                User32.NativeMethods.SetWinEventHook(User32.NativeMethods.EVENT_SYSTEM_MINIMIZEEND,
                    User32.NativeMethods.EVENT_SYSTEM_MINIMIZEEND,
                    IntPtr.Zero, _winEventDelegate,
                    0,
                    0,
                    User32.NativeMethods.WINEVENT_OUTOFCONTEXT);

                User32.NativeMethods.SetWinEventHook(User32.NativeMethods.EVENT_SYSTEM_FOREGROUND,
                    User32.NativeMethods.EVENT_SYSTEM_FOREGROUND,
                    IntPtr.Zero, _winEventDelegate,
                    0,
                    0,
                    User32.NativeMethods.WINEVENT_OUTOFCONTEXT);
            });
        }
    }
}