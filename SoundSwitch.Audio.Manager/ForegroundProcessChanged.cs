using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SoundSwitch.Audio.Manager.Interop.Com.Threading;
using SoundSwitch.Audio.Manager.Interop.Com.User;

namespace SoundSwitch.Audio.Manager
{
    public class ForegroundProcessChanged
    {
        public class ForegroundProcessChangedEvent : EventArgs
        {
            /// <summary>
            /// ID of the process that is now active
            /// </summary>
            public uint ProcessId { get; }

            /// <summary>
            /// Name of the process that is active
            /// </summary>
            public string ProcessName { get; }


            public ForegroundProcessChangedEvent(uint processId, string processName)
            {
                ProcessId = processId;
                ProcessName = processName;
            }

            public override string ToString()
            {
                return $"{nameof(ProcessId)}: {ProcessId}, {nameof(ProcessName)}: {ProcessName}";
            }
        }

        public event EventHandler<ForegroundProcessChangedEvent> Event;

        private readonly User32.NativeMethods.WinEventDelegate _winEventDelegate;


        public ForegroundProcessChanged()
        {
            _winEventDelegate = (hook, type, hwnd, idObject, child, thread, time) =>
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
                
                if (windowProcessId == 0)
                {
                    return;
                }

                Task.Factory.StartNew(() =>
                {
                    var process = Process.GetProcessById((int) windowProcessId);
                    var processName = process.MainModule?.FileName;
                    Event?.Invoke(this, new ForegroundProcessChangedEvent(windowProcessId, processName));
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