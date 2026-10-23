#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;

using Serilog;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Telemetry;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Model;

namespace SoundSwitch.Services
{
    public class AppSoundLockManager : IDisposable
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<AppSoundLockManager>();
        private readonly ISoundSwitchConfiguration _configuration;
        private readonly AudioSwitcher _audioSwitcher;
        private readonly WindowMonitor _windowMonitor;
        private readonly ProcessMonitor _processMonitor;
        private readonly NotificationManager _notificationManager;
        private readonly ILogger _logger = Log.ForContext<AppSoundLockManager>();
        private readonly object _lock = new();

        /// <summary>
        /// The rule bound to each currently registered hotkey.
        /// </summary>
        private readonly Dictionary<HotKey, AppSoundRule> _rulesByHotkey = new();

        /// <summary>
        /// Hotkeys this manager registered with the OS, so they can be unregistered again.
        /// </summary>
        private readonly HashSet<HotKey> _ruleHotkeys = new();

        /// <summary>
        /// In-memory index of the next device pair for each hotkey rule. Reset whenever
        /// the rules change or the app restarts; it is deliberately not persisted.
        /// </summary>
        private readonly Dictionary<Guid, int> _nextPairIndex = new();

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
            WindowsAPIAdapter.HotKeyPressed += OnHotKeyPressed;
            ReloadHotkeys();
        }

        private void OnProcessesDetected(object? sender, List<ProcessMonitor.Event> events)
        {
            var notifiedRulesInBatch = new HashSet<Guid>();
            foreach (var @event in events)
            {
                ApplyRulesToProcess(@event.ProcessId, @event.ProcessName, @event.ProcessPath, @event.WindowTitle, notifiedRulesInBatch, "process");
            }
        }

        private void OnForegroundWindowChanged(object? sender, WindowMonitor.Event e)
        {
            if (e.ProcessId == 0 || e.ProcessId == Environment.ProcessId) return;

            _logger.Verbose("Foreground window changed: {WindowTitle} (PID: {ProcessId})", e.WindowName, e.ProcessId);

            // For foreground changes, we re-apply immediately as it often means the app is interactive
            ApplyRulesToProcess(e.ProcessId, e.ProcessName, e.ProcessPath, e.WindowName, null, "foreground");
        }

        private void OnHotKeyPressed(object? sender, WindowsAPIAdapter.KeyPressedEventArgs e)
        {
            AppSoundRule? rule;
            lock (_lock)
            {
                _rulesByHotkey.TryGetValue(e.HotKey, out rule);
            }

            if (rule == null) return;

            _logger.Information("Hotkey {HotKey} pressed, applying app rule for {ProcessPattern}", e.HotKey, rule.ProcessPath);
            ApplyRuleToRunningProcesses(rule);
        }

        /// <summary>
        /// (Re-)register the hotkey of every enabled hotkey-mode rule. Called on start and
        /// whenever the rules change in Settings, so a hotkey takes effect without an app restart.
        /// </summary>
        public void ReloadHotkeys()
        {
            UnregisterRuleHotkeys();

            lock (_lock)
            {
                if (_configuration.AppSoundRules == null) return;

                foreach (var rule in _configuration.AppSoundRules.ToList().Where(IsHotkeyRule))
                {
                    var hotKey = rule.HotKey!;
                    if (_rulesByHotkey.ContainsKey(hotKey))
                    {
                        _logger.Warning("App rule hotkey {HotKey} is used by more than one rule, keeping the first", hotKey);
                        continue;
                    }

                    if (!WindowsAPIAdapter.RegisterHotKey(hotKey))
                    {
                        _logger.Warning("Couldn't register app rule hotkey {HotKey}; it may conflict with another hotkey", hotKey);
                        continue;
                    }

                    _ruleHotkeys.Add(hotKey);
                    _rulesByHotkey[hotKey] = rule;
                }
            }
        }

        private static bool IsHotkeyRule(AppSoundRule rule) => rule.Enabled && rule.Mode == AppRuleMode.Hotkey && rule.HotKey is { Enabled: true };

        private void UnregisterRuleHotkeys()
        {
            lock (_lock)
            {
                foreach (var hotKey in _ruleHotkeys)
                {
                    WindowsAPIAdapter.UnRegisterHotKey(hotKey);
                }

                _ruleHotkeys.Clear();
                _rulesByHotkey.Clear();
                _nextPairIndex.Clear();
            }
        }

        /// <summary>
        /// Check whether the given hotkey can be used by the rule identified by
        /// <paramref name="ruleId"/>: it must not already be bound to another app rule,
        /// a profile, or another application.
        /// </summary>
        public bool IsHotkeyAvailable(HotKey hotKey, Guid ruleId)
        {
            lock (_lock)
            {
                // A disabled Hotkey rule is not registered, so _rulesByHotkey alone
                // would let another rule claim the same shortcut. Check the configured
                // rules as well, excluding the rule being edited.
                if (_configuration.AppSoundRules != null &&
                    _configuration.AppSoundRules.Any(other =>
                        other.Id != ruleId &&
                        other.Mode == AppRuleMode.Hotkey &&
                        other.HotKey != null &&
                        other.HotKey.Equals(hotKey)))
                {
                    return false;
                }

                if (_rulesByHotkey.TryGetValue(hotKey, out var owner))
                {
                    // The rule being edited already owns this hotkey: it stays valid.
                    return owner.Id == ruleId;
                }
            }

            return WindowsAPIAdapter.RegisterHotKey(hotKey) && WindowsAPIAdapter.UnRegisterHotKey(hotKey);
        }

        /// <summary>
        /// Two hotkey rules may not target the same application/window, otherwise the
        /// cycle would be ambiguous.
        /// </summary>
        public bool IsHotkeyTargetAvailable(AppSoundRule rule)
        {
            if (_configuration.AppSoundRules == null) return true;

            lock (_lock)
            {
                return !_configuration.AppSoundRules.Any(other =>
                    other.Id != rule.Id &&
                    other.Mode == AppRuleMode.Hotkey &&
                    string.Equals(other.ProcessPath, rule.ProcessPath, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(other.WindowName, rule.WindowName, StringComparison.OrdinalIgnoreCase));
            }
        }

        private void ApplyRulesToProcess(uint processId, string processName, string processPath, string windowTitle, HashSet<Guid>? notifiedRules, string triggerSource)
        {
            if (_configuration.AppSoundRules == null || _configuration.AppSoundRules.Count == 0) return;

            foreach (var rule in _configuration.AppSoundRules.ToList().Where(r => r.Enabled && r.Mode == AppRuleMode.Automatic))
            {
                if (!IsMatch(rule, processName, processPath, windowTitle)) continue;

                var pair = rule.DevicePairs?.FirstOrDefault();
                if (pair == null) continue;

                ApplyRuleToProcess(rule, pair, processId, processName, processPath, notifiedRules, triggerSource, true);
            }
        }

        /// <summary>
        /// Switch the given process to the pair's devices, notify and record telemetry.
        /// Shared by the automatic (process/foreground) path and the rule hotkey.
        /// </summary>
        private bool ApplyRuleToProcess(AppSoundRule rule, AppSoundRuleDevicePair pair, uint processId, string processName, string processPath, HashSet<Guid>? notifiedRules, string triggerSource, bool notify)
        {
            _logger.Information("MATCH: Rule for {ProcessPattern} matched process {ProcessName} (PID: {PID})", rule.ProcessPath, processName, processId);

            var changed = false;
            changed |= SwitchRuleDevice(pair, p => p.Playback, (p, v) => p.Playback = v, EDataFlow.eRender, processId);
            changed |= SwitchRuleDevice(pair, p => p.Recording, (p, v) => p.Recording = v, EDataFlow.eCapture, processId);

            if (changed)
            {
                // Prefer the basename from the process path. If it's empty (ProcessMonitor
                // sets the path to string.Empty) or a placeholder (WindowMonitor uses "N/A"
                // when it can't read the path), fall back to the process name so we still
                // capture an anonymized hash instead of hashing the literal "N/A".
                var basename = Path.GetFileName(processPath);
                if (string.IsNullOrEmpty(basename) || basename == "N/A")
                {
                    basename = processName;
                }
                TelemetryService.TrackAppRuleActivated(basename, triggerSource);
            }

            if (notify && rule.Notify && changed)
            {
                if (notifiedRules == null || !notifiedRules.Contains(rule.Id))
                {
                    _notificationManager.NotifyAppRuleMatched(rule, pair, processId);
                    notifiedRules?.Add(rule.Id);
                }
            }

            return changed;
        }

        /// <summary>
        /// Apply the rule's next device pair to every currently running process matching it.
        /// Triggered by the rule's hotkey; only the apps' own audio is switched, the default
        /// device is never touched.
        /// </summary>
        private void ApplyRuleToRunningProcesses(AppSoundRule rule)
        {
            var pairs = rule.DevicePairs;
            if (pairs == null || pairs.Count == 0)
            {
                _logger.Warning("App rule hotkey pressed but rule {ProcessPattern} has no device pair", rule.ProcessPath);
                return;
            }

            var pairIndex = GetNextPairIndex(rule.Id, pairs.Count);
            var pair = pairs[pairIndex];

            // Find every matching process first so "nothing running" always wins over an
            // unavailable device: the user gets the actionable no-process message.
            var matches = new List<(uint ProcessId, string ProcessName, string ProcessPath)>();
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.Id <= 4 || process.HasExited) continue;

                    var processName = process.ProcessName;
                    var processPath = process.MainModule?.FileName ?? string.Empty;
                    var windowTitle = GetWindowTitle(process);

                    if (!IsMatch(rule, processName, processPath, windowTitle)) continue;

                    matches.Add(((uint)process.Id, processName, processPath));
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Couldn't inspect process PID {PID} for an app rule hotkey", process.Id);
                }
                finally
                {
                    process.Dispose();
                }
            }

            if (matches.Count == 0)
            {
                _logger.Information("App rule hotkey pressed but no running process matches {ProcessPattern}", rule.ProcessPath);
                _notificationManager.NotifyAppRuleNoProcess(rule);
                return;
            }

            // A pair that resolves to no live device can't be applied; keep the cycle where
            // it is so the user can plug the device back in and press again.
            if (!CanApplyPair(pair))
            {
                _logger.Warning("App rule hotkey pressed but no device of pair {Index} could be resolved for {ProcessPattern}", pairIndex, rule.ProcessPath);
                return;
            }

            var changed = false;
            foreach (var match in matches)
            {
                try
                {
                    changed |= ApplyRuleToProcess(rule, pair, match.ProcessId, match.ProcessName, match.ProcessPath, null, "hotkey", false);
                }
                catch (Exception ex)
                {
                    // The process may have exited between enumeration and the switch.
                    _logger.Debug(ex, "Couldn't apply app rule hotkey to process PID {PID}", match.ProcessId);
                }
            }

            // Notify even when the endpoint already matched: the hotkey did apply this pair,
            // and the banner is the feedback that tells the user which pair is now active.
            // Advance first so a notification failure cannot block the cycle.
            AdvancePairIndex(rule.Id, pairIndex, pairs.Count);

            if (rule.Notify)
            {
                _notificationManager.NotifyAppRuleMatched(rule, pair, matches[0].ProcessId);
            }

            if (!changed)
            {
                _logger.Debug("App rule pair {Index} for {ProcessPattern} was already applied", pairIndex, rule.ProcessPath);
            }
        }

        /// <summary>
        /// Whether at least one side of the pair resolves to a live device.
        /// </summary>
        private static bool CanApplyPair(AppSoundRuleDevicePair pair)
        {
            return Resolves(pair.Playback) || Resolves(pair.Recording);

            static bool Resolves(DeviceInfo? device) =>
                device != null && AppRuleDeviceResolver.Resolve(device, AppModel.Instance.AudioDeviceLister) != null;
        }

        private int GetNextPairIndex(Guid ruleId, int pairCount)
        {
            lock (_lock)
            {
                return _nextPairIndex.TryGetValue(ruleId, out var index) && index >= 0 && index < pairCount ? index : 0;
            }
        }

        private void AdvancePairIndex(Guid ruleId, int appliedIndex, int pairCount)
        {
            lock (_lock)
            {
                _nextPairIndex[ruleId] = pairCount <= 0 ? 0 : (appliedIndex + 1) % pairCount;
            }
        }

        private string GetWindowTitle(Process process)
        {
            try
            {
                return process.MainWindowTitle;
            }
            catch (Exception ex)
            {
                _logger.Verbose(ex, "Couldn't read window title for PID {PID}", process.Id);
                return string.Empty;
            }
        }

        /// <summary>
        /// Resolve the pair's stored device against the active devices, switch the process to it and
        /// self-heal the stored <see cref="DeviceInfo"/> when the live device differs (new id or name,
        /// e.g. after a driver update). This is what keeps rules working and turns the migrated
        /// name=id placeholder rules back into properly named rules after the first successful match.
        /// </summary>
        /// <returns>`true` when the process was switched to the device.</returns>
        private bool SwitchRuleDevice(AppSoundRuleDevicePair pair, Func<AppSoundRuleDevicePair, DeviceInfo?> getter, Action<AppSoundRuleDevicePair, DeviceInfo> setter, EDataFlow flow, uint processId)
        {
            var stored = getter(pair);
            if (stored == null) return false;

            var resolved = AppRuleDeviceResolver.Resolve(stored, AppModel.Instance.AudioDeviceLister);
            if (resolved == null)
            {
                _logger.Warning("App rule device {NameClean} ({DeviceId}) not found among active {Flow} devices, skipping switch", stored.NameClean, stored.Id, flow);
                return false;
            }

            var healed = false;
#pragma warning disable CS0618 // Type or member is obsolete
            if (resolved.Id != stored.Id || resolved.Name != stored.Name)
            {
                setter(pair, new DeviceInfo(resolved.Name, resolved.Id, resolved.Type, resolved.IsUsb, DateTime.UtcNow));
                healed = true;
            }
#pragma warning restore CS0618 // Type or member is obsolete

            var switched = false;
            try
            {
                switched = _audioSwitcher.SwitchProcessTo(resolved.Id, ERole.ERole_enum_count, flow, processId);
            }
            finally
            {
                // Persist in `finally` so memory and disk stay consistent even if the
                // switch throws; the save itself never blocks or aborts audio routing.
                if (healed)
                {
                    _logger.Information("Self-healing app rule device: {Old} -> {New}", stored.NameClean, resolved.NameClean);
                    try
                    {
                        _configuration.Save();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "Failed to save self-healed app rule device {NameClean}", resolved.NameClean);
                    }
                }
            }

            return switched;
        }

        private bool IsMatch(AppSoundRule rule, string processName, string processPath, string windowTitle)
        {
            var options = rule.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            // 1. Match Process Pattern (against Path ?? Name ?? Name.exe)
            var processMatched = string.IsNullOrEmpty(rule.ProcessPath); // If empty, consider matched
            if (!processMatched)
            {
                var targets = new[] { processPath, processName, processName + ".exe" };
                try
                {
                    // Validate regex once
                    _ = Regex.Match(string.Empty, rule.ProcessPath, options);
                    processMatched = targets.Any(t => !string.IsNullOrEmpty(t) && Regex.IsMatch(t, rule.ProcessPath, options));
                }
                catch (RegexParseException ex)
                {
                    _logger.Warning(ex, "Invalid Regex for ProcessPath: {Pattern}. Falling back to glob.", rule.ProcessPath);
                    processMatched = targets.Any(t => !string.IsNullOrEmpty(t) && IsGlobMatch(t, rule.ProcessPath, options));
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Unexpected error matching pattern {Pattern}", rule.ProcessPath);
                }
            }

            // 2. Match Window Pattern
            var windowMatched = string.IsNullOrEmpty(rule.WindowName); // If empty, consider matched
            if (!windowMatched)
            {
                windowMatched = IsRegexOrGlobMatch(
                    windowTitle,
                    rule.WindowName,
                    options,
                    ex => _logger.Warning(ex, "Invalid Regex for WindowName: {Pattern}. Falling back to glob.", rule.WindowName)
                );
            }

            return processMatched && windowMatched;
        }

        /// <summary>
        /// Attempts to match a value against a pattern using regex, falling back to glob pattern matching if the regex is invalid.
        /// </summary>
        /// <param name="value">The value to match against. Returns false if null or empty.</param>
        /// <param name="pattern">The pattern to match (regex or glob). Returns false if null or empty.</param>
        /// <param name="options">Regex options to apply during matching.</param>
        /// <param name="onInvalidRegex">Optional callback invoked when the pattern is invalid as regex before falling back to glob.</param>
        /// <returns>True if the value matches the pattern (as regex or glob); otherwise false.</returns>
        internal static bool IsRegexOrGlobMatch(string? value, string pattern, RegexOptions options, Action<RegexParseException>? onInvalidRegex = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            
            if (string.IsNullOrEmpty(pattern))
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(value, pattern, options);
            }
            catch (RegexParseException ex)
            {
                onInvalidRegex?.Invoke(ex);
                return IsGlobMatch(value, pattern, options);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Unexpected error matching pattern {Pattern} against value {Value}", pattern, value);
                return false;
            }
        }

        /// <summary>
        /// Matches a value against a glob-style pattern (* for any characters, ? for single character).
        /// </summary>
        /// <param name="value">The value to match against.</param>
        /// <param name="pattern">The glob pattern to match.</param>
        /// <param name="options">Regex options to apply during matching.</param>
        /// <returns>True if the value matches the glob pattern; otherwise false.</returns>
        internal static bool IsGlobMatch(string value, string pattern, RegexOptions options)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern))
            {
                return false;
            }
            
            var globAsRegex = $"^{Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".")}$";
            return Regex.IsMatch(value, globAsRegex, options);
        }

        public void Dispose()
        {
            _processMonitor.Stop();
            _processMonitor.ProcessesDetected -= OnProcessesDetected;
            _windowMonitor.ForegroundChanged -= OnForegroundWindowChanged;
            WindowsAPIAdapter.HotKeyPressed -= OnHotKeyPressed;
            UnregisterRuleHotkeys();
        }
    }
}
