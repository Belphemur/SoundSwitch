#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Model;

namespace SoundSwitch.Services
{
    public class AppSoundLockManager
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<AppSoundLockManager>();
        private readonly ISoundSwitchConfiguration _configuration;
        private readonly AudioSwitcher _audioSwitcher;
        private readonly WindowMonitor _windowMonitor;
        private readonly ProcessMonitor _processMonitor;
        private readonly NotificationManager _notificationManager;
        private readonly ILogger _logger = Log.ForContext<AppSoundLockManager>();
        private readonly object _lock = new();

        public AppSoundLockManager(ISoundSwitchConfiguration configuration, AudioSwitcher audioSwitcher, WindowMonitor windowMonitor, ProcessMonitor processMonitor, NotificationManager notificationManager)
        {
            _configuration = configuration;
            _audioSwitcher = audioSwitcher;
            _windowMonitor = windowMonitor;
            _processMonitor = processMonitor;
            _notificationManager = notificationManager;
        }

        public void Start()
        {
            _logger.Information("Starting AppSoundLockManager");
            _windowMonitor.ForegroundChanged += OnForegroundWindowChanged;
            _processMonitor.ProcessesDetected += OnProcessesDetected;
            _processMonitor.Start();
        }

        private void OnProcessesDetected(object? sender, List<ProcessMonitor.Event> events)
        {
            var notifiedRulesInBatch = new HashSet<Guid>();
            foreach (var @event in events)
            {
                ApplyRulesToProcess(@event.ProcessId, @event.ProcessName, @event.ProcessPath, @event.WindowTitle, notifiedRulesInBatch);
            }
        }

        private void OnForegroundWindowChanged(object? sender, WindowMonitor.Event e)
        {
            if (e.ProcessId == 0 || e.ProcessId == Environment.ProcessId) return;

            _logger.Verbose("Foreground window changed: {WindowTitle} (PID: {ProcessId})", e.WindowName, e.ProcessId);
            
            // For foreground changes, we re-apply immediately as it often means the app is interactive
            ApplyRulesToProcess(e.ProcessId, e.ProcessName, e.ProcessPath, e.WindowName, null);
        }

        private void ApplyRulesToProcess(uint processId, string processName, string processPath, string windowTitle, HashSet<Guid>? notifiedRules)
        {
            if (_configuration.AppSoundRules == null || !_configuration.AppSoundRules.Any()) return;

            foreach (var rule in _configuration.AppSoundRules.Where(r => r.Enabled))
            {
                if (IsMatch(rule, processName, processPath, windowTitle))
                {
                    _logger.Information("MATCH: Rule for {ProcessPattern} matched process {ProcessName} (PID: {PID})", rule.ProcessPath, processName, processId);
                    
                    var changed = false;
                    if (!string.IsNullOrEmpty(rule.PlaybackDeviceId))
                    {
                        changed |= _audioSwitcher.SwitchProcessTo(rule.PlaybackDeviceId, ERole.ERole_enum_count, EDataFlow.eRender, processId);
                    }

                    if (!string.IsNullOrEmpty(rule.RecordingDeviceId))
                    {
                        changed |= _audioSwitcher.SwitchProcessTo(rule.RecordingDeviceId, ERole.ERole_enum_count, EDataFlow.eCapture, processId);
                    }

                    if (rule.Notify && changed)
                    {
                        if (notifiedRules == null || !notifiedRules.Contains(rule.Id))
                        {
                            _notificationManager.NotifyAppRuleMatched(rule, processId);
                            notifiedRules?.Add(rule.Id);
                        }
                    }
                }
            }
        }

        private bool IsMatch(AppSoundRule rule, string processName, string processPath, string windowTitle)
        {
            var options = rule.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            // 1. Match Process Pattern (against Path ?? Name ?? Name.exe)
            var processMatched = string.IsNullOrEmpty(rule.ProcessPath); // If empty, consider matched
            if (!processMatched)
            {
                try
                {
                    var targets = new[] { processPath, processName, processName + ".exe" };
                    processMatched = targets.Any(t => !string.IsNullOrEmpty(t) && Regex.IsMatch(t, rule.ProcessPath, options));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Invalid Regex for ProcessPath: {Pattern}", rule.ProcessPath);
                }
            }

            // 2. Match Window Pattern
            var windowMatched = string.IsNullOrEmpty(rule.WindowName); // If empty, consider matched
            if (!windowMatched)
            {
                try
                {
                    windowMatched = !string.IsNullOrEmpty(windowTitle) && Regex.IsMatch(windowTitle, rule.WindowName, options);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Invalid Regex for WindowName: {Pattern}", rule.WindowName);
                }
            }

            return processMatched && windowMatched;
        }

        public void Dispose()
        {
            _processMonitor.Stop();
            _processMonitor.ProcessesDetected -= OnProcessesDetected;
            _windowMonitor.ForegroundChanged -= OnForegroundWindowChanged;
        }
    }
}
