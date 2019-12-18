using System;
using System.Diagnostics;
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
                var @event = ComThread.Invoke(() =>
                {
                    User32.NativeMethods.GetWindowThreadProcessId(hwnd, out var processId);
                    var process = Process.GetProcessById((int) processId);
                    var processName = process.MainModule?.FileName;
                    return new ForegroundProcessChangedEvent(processId, processName);
                });
                Event?.Invoke(this, @event);
            };

            User32.NativeMethods.SetWinEventHook(User32.NativeMethods.EVENT_SYSTEM_FOREGROUND,
                User32.NativeMethods.EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero, _winEventDelegate,
                0,
                0,
                User32.NativeMethods.WINEVENT_OUTOFCONTEXT);
        }
    }
}