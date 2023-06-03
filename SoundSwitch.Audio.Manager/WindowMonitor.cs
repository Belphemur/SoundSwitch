using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog;
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

            /// <summary>
            /// Handle of the window
            /// </summary>
            public User32.NativeMethods.HWND Hwnd { get; }

            public Event(uint processId, string processName, string windowName, string windowClass, User32.NativeMethods.HWND hwnd)
            {
                ProcessId   = processId;
                ProcessName = processName;
                WindowName  = windowName;
                WindowClass = windowClass;
                Hwnd        = hwnd;
            }

            public override string ToString()
            {
                return $"{nameof(ProcessId)}: {ProcessId}, {nameof(ProcessName)}: {ProcessName}, {nameof(WindowName)}: {WindowName}, {nameof(WindowClass)}: {WindowClass}";
            }
        }

        public event EventHandler<Event> ForegroundChanged;

        private readonly User32.NativeMethods.WinEventDelegate _foregroundWindowChanged;

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
                var (processId, windowText, windowClass) = ProcessWindowInformation(hwnd);

                //Couldn't find the processId of the window
                if (processId == 0) return;

                if (processId == Environment.ProcessId)
                {
                    Log.Information("Foreground = SoundSwitch, don't save.");
                    return;
                }

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var process = Process.GetProcessById((int) processId);
                        var processName = process.MainModule?.FileName ?? "N/A";
                        ForegroundChanged?.Invoke(this, new Event(processId, processName, windowText, windowClass, hwnd));
                    }
                    catch (Exception)
                    {
                      //Ignored
                    }
                });
            };

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
            });
        }

        public static (uint ProcessId, string WindowText, string WindowClass) ProcessWindowInformation(User32.NativeMethods.HWND hwnd)
        {
            return ComThread.Invoke(() =>
            {
                uint processId = 0;
                var  wndText   = "";
                var  wndClass  = "";
                try
                {
                    wndText = User32.GetWindowText(hwnd);
                }
                catch (Exception)
                {
                    // ignored
                }

                try
                {
                    wndClass = User32.GetWindowClass(hwnd);
                }
                catch (Exception)
                {
                    // ignored
                }

                try
                {
                    User32.NativeMethods.GetWindowThreadProcessId(hwnd, out processId);
                }
                catch (Exception)
                {
                    // ignored
                }


                return (processId, wndText, wndClass);
            });
        }
    }
}