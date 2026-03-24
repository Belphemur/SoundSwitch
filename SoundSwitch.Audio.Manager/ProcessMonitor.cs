using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Serilog;

namespace SoundSwitch.Audio.Manager
{
    public class ProcessMonitor : IDisposable
    {
        public class Event : EventArgs
        {
            public uint ProcessId { get; }
            public string ProcessName { get; }
            public string ProcessPath { get; }
            public string WindowTitle { get; }

            public Event(uint processId, string processName, string processPath, string windowTitle)
            {
                ProcessId = processId;
                ProcessName = processName;
                ProcessPath = processPath;
                WindowTitle = windowTitle;
            }
        }

        public event EventHandler<List<Event>> ProcessesDetected;

        private readonly Timer _pollTimer;
        private readonly HashSet<int> _processedProcessIds = new();
        private readonly HashSet<int> _pendingProcesses = new();
        private readonly object _lock = new();
        private readonly ILogger _logger = Log.ForContext<ProcessMonitor>();

        public ProcessMonitor(double intervalMs = 2000)
        {
            _pollTimer = new Timer(intervalMs);
            _pollTimer.Elapsed += OnPollTimerElapsed;
        }

        public void Start()
        {
            _logger.Information("Starting ProcessMonitor");
            _pollTimer.Start();
            PollProcesses();
        }

        public void Stop()
        {
            _pollTimer.Stop();
        }

        private void OnPollTimerElapsed(object sender, ElapsedEventArgs e)
        {
            PollProcesses();
        }

        private void PollProcesses()
        {
            lock (_lock)
            {
                var currentPids = new HashSet<int>();
                var toProcess = new List<int>();

                foreach (var process in Process.GetProcesses())
                {
                    var pid = process.Id;
                    if (pid <= 4) continue;
                    currentPids.Add(pid);

                    if (_processedProcessIds.Contains(pid)) continue;

                    if (_pendingProcesses.Contains(pid))
                    {
                        toProcess.Add(pid);
                    }
                    else
                    {
                        try
                        {
                            _pendingProcesses.Add(pid);
                            _logger.Verbose("Detected new process {ProcessName} ({PID}), waiting for next iteration", process.ProcessName, pid);
                        }
                        catch { }
                    }
                }

                // Cleanup exited processes
                _processedProcessIds.RemoveWhere(pid => !currentPids.Contains(pid));
                _pendingProcesses.RemoveWhere(pid => !currentPids.Contains(pid));

                if (!toProcess.Any()) return;

                var events = new List<Event>();
                foreach (var pid in toProcess)
                {
                    try
                    {
                        using var process = Process.GetProcessById(pid);
                        if (!process.HasExited)
                        {
                            var processName = process.ProcessName;
                            var processPath = string.Empty;
                            try { processPath = process.MainModule?.FileName ?? string.Empty; } catch { }

                            events.Add(new Event((uint)pid, processName, processPath, process.MainWindowTitle));
                        }
                    }
                    catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 5)
                    {
                        _logger.Debug("Access is denied for PID {PID}, skipping.", pid);
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug(ex, "Failed to process throttled PID {PID}", pid);
                    }
                    finally
                    {
                        _processedProcessIds.Add(pid);
                        _pendingProcesses.Remove(pid);
                    }
                }

                if (events.Any())
                {
                    ProcessesDetected?.Invoke(this, events);
                }
            }
        }

        public void Dispose()
        {
            _pollTimer.Stop();
            _pollTimer.Dispose();
        }
    }
}
