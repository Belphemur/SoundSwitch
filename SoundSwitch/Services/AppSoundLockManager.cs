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
        private readonly AppRuleAssignmentReconciler _assignmentReconciler;

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

        /// <summary>
        /// The rules as they were at the last <see cref="OnRulesChanged"/>, used to detect
        /// deletions, disables and edits of assignment rules so their written route gets
        /// restored to the default (deferred to the next sighting for a closed app).
        /// </summary>
        private List<AppSoundRule> _rulesSnapshot = new();

        public AppSoundLockManager(ISoundSwitchConfiguration configuration, AudioSwitcher audioSwitcher, WindowMonitor windowMonitor, ProcessMonitor processMonitor, NotificationManager notificationManager)
        {
            _configuration = configuration;
            _audioSwitcher = audioSwitcher;
            _windowMonitor = windowMonitor;
            _processMonitor = processMonitor;
            _notificationManager = notificationManager;
            _assignmentReconciler = new AppRuleAssignmentReconciler(configuration, audioSwitcher);
        }

        public void Start()
        {
            _logger.Information("Starting AppSoundLockManager");
            _windowMonitor.ForegroundChanged += OnForegroundWindowChanged;
            _processMonitor.ProcessesDetected += OnProcessesDetected;
            _processMonitor.Start();
            WindowsAPIAdapter.HotKeyPressed += OnHotKeyPressed;
            ReloadHotkeys();
            // Adopt pre-ledger records for existing rules and resume pending restore-default
            // clears from the previous run, before anything is sighted.
            ReconcileAssignments("start");
            _rulesSnapshot = _configuration.AppSoundRules?.ToList() ?? new List<AppSoundRule>();
        }

        /// <summary>
        /// Device arrival/removal can change which device an assigned route resolves to
        /// (replugged/re-enumerated endpoint, device back after it was unavailable at the last
        /// sighting). Reconcile the ledger's convergence and re-apply every live assignment rule
        /// to the running processes � SwitchRuleDevice resolves the stored device against the
        /// fresh device list and self-heals the stored info (ADR �6.6). Publishes nothing on
        /// its own.
        /// </summary>
        public void OnDevicesChanged()
        {
            ReconcileAssignments("device-change");

            if (_configuration.AppSoundRules == null)
                return;

            List<ProcessSnapshot>? snapshot = null;
            foreach (var rule in _configuration.AppSoundRules.ToList().Where(rule =>
                         rule.Enabled && AppRuleAssignmentValidator.IsEligible(rule)))
            {
                try
                {
                    // A self-healing re-apply is not a user-triggered switch: no banner per
                    // running instance (the docstring promises silence on this path). The table
                    // is read once, and only when at least one rule actually needs it.
                    ApplyAssignmentRuleToRunningProcesses(rule, snapshot ??= SnapshotProcesses(), notify: false);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Failed to re-apply assignment rule {RuleId} after a device change", rule.Id);
                }
            }

            // The re-apply may have stamped new entries: persist them here instead of waiting
            // for an unrelated save, or a restart would lose the ownership of those routes.
            SaveLedgerIfDirty("device-change");
        }

        /// <summary>
        /// The read-only "Windows assignments" rows for the rule list: persisted per-app routes
        /// that no SoundSwitch rule owns (ADR �6.5).
        /// </summary>
        public IReadOnlyList<AppRuleAssignmentReconciler.StoreAssignment> GetWindowsAssignments()
        {
            return _assignmentReconciler.GetWindowsAssignments();
        }

        /// <summary>
        /// The rules changed in Settings (add/edit/delete/enable/disable): retract the routing
        /// the change did away with � restoring the default for assignment rules (immediately
        /// for running apps, deferred for closed ones), releasing the ledger claim of removed
        /// dynamic rules � then apply still-live assignment rules to the running apps (ADR
        /// �6.2, �6.3) and reconcile the ledger.
        /// </summary>
        public void OnRulesChanged()
        {
            var current = _configuration.AppSoundRules?.ToList() ?? new List<AppSoundRule>();

            foreach (var previous in _rulesSnapshot)
            {
                var now = current.FirstOrDefault(rule => rule.Id == previous.Id);

                if (now == null)
                {
                    // Removed: an assignment route gets restored to the default (kept pending
                    // while the app is closed); a dynamic route keeps today's delete behaviour
                    // and the ledger just releases its claim (ADR �11.5, �11.6).
                    _assignmentReconciler.ClearRuleAssignments(previous);
                    continue;
                }

                if (!AppRuleAssignmentValidator.IsEligible(previous))
                    continue; // Dynamic en/disable keeps the route alive (�11.6)

                if (!previous.Enabled)
                    continue; // already retracted when it was disabled

                var stillRoutesTheSame =
                    now.Enabled &&
                    AppRuleAssignmentValidator.IsEligible(now) &&
                    AppRuleIdentityMatcher.SameApp(previous.AssignmentExePath ?? previous.ProcessPath, now.AssignmentExePath ?? now.ProcessPath) &&
                    SameRoute(previous, now);

                // An edit that changed identity, route or enforcement, or a disable: clear the
                // written route; the apply loop below re-applies while the rule still is a live
                // assignment.
                if (!stillRoutesTheSame)
                    _assignmentReconciler.ClearRuleAssignments(previous);
            }

            _rulesSnapshot = current;

            foreach (var rule in current.Where(rule => rule.Enabled && AppRuleAssignmentValidator.IsEligible(rule)))
            {
                ApplyAssignmentRuleToRunningProcesses(rule);
            }

            ReconcileAssignments("rules-changed");
        }

        private static bool SameRoute(AppSoundRule left, AppSoundRule right)
        {
            var leftPair = left.DevicePairs?.FirstOrDefault();
            var rightPair = right.DevicePairs?.FirstOrDefault();
            return leftPair?.Playback?.Id == rightPair?.Playback?.Id &&
                   leftPair?.Recording?.Id == rightPair?.Recording?.Id;
        }

        /// <summary>
        /// Write the assignment rule's route into every running process of its exe identity
        /// through the existing switch pipeline (device self-heal, notify, telemetry included);
        /// a Set is authoritative, so this also replaces stale routing from a deleted rule.
        /// Nothing is written while the app is closed � it routes on its next sighting (�6.2).
        /// </summary>
        private void ApplyAssignmentRuleToRunningProcesses(
            AppSoundRule rule,
            IReadOnlyList<ProcessSnapshot>? snapshot = null,
            bool notify = true)
        {
            var identity = rule.AssignmentExePath ?? rule.ProcessPath;
            var pair = rule.DevicePairs?.FirstOrDefault();
            if (string.IsNullOrEmpty(identity) || pair == null)
            {
                _logger.Warning("Assignment rule {RuleId} has no identity or device pair, skipping apply", rule.Id);
                return;
            }

            // One banner and one activation metric per rule and batch, not per running pid
            // (mirrors the hotkey batch, which notifies for the first match only).
            var notifiedRules = new HashSet<Guid>();
            string? firstMatchPath = null;
            string? firstMatchName = null;

            // A device-change burst re-applies every live assignment rule: enumerate the process
            // table once for all of them instead of once per rule (O(rules x processes)).
            snapshot ??= SnapshotProcesses();

            foreach (var running in snapshot)
            {
                if (!AppRuleIdentityMatcher.SameApp(identity, running.Path))
                    continue;

                // The snapshot can be a moment old and the engine writes by pid: re-read the
                // path so a recycled pid is not switched as the identity of a process that
                // exited since the enumeration.
                if (!StillSameApp(identity, running.Pid))
                    continue;

                // The process may have exited between enumeration and the switch.
                try
                {
                    if (ApplyRuleToProcess(rule, pair, running.Pid, running.Name, running.Path, notifiedRules, "assignment", notify, out var flowsWritten))
                    {
                        firstMatchPath ??= running.Path;
                        firstMatchName ??= running.Name;
                        _assignmentReconciler.EnsureEntry(rule, running.Path, pair, flowsWritten);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Couldn't apply assignment rule to process PID {PID}", running.Pid);
                }
            }

            if (firstMatchPath != null)
            {
                TrackRuleActivated("engine", firstMatchPath, firstMatchName);
            }
        }

        /// <summary>
        /// Whether the pid still runs the expected exe. A pid captured earlier can be recycled by
        /// another process, and the policy engine writes by pid; unreadable/protected processes
        /// are treated as no longer matching (they cannot be switched meaningfully anyway).
        /// </summary>
        private bool StillSameApp(string identity, uint pid)
        {
            try
            {
                using var process = Process.GetProcessById((int)pid);
                return AppRuleIdentityMatcher.SameApp(identity, process.MainModule?.FileName ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Couldn't recheck process PID {PID} before switching it", pid);
                return false;
            }
        }

        /// <summary>One process of the machine's table, captured once for a batch of rules.</summary>
        private readonly record struct ProcessSnapshot(uint Pid, string Name, string Path);

        /// <summary>
        /// Read the process table once. Unreadable/protected processes are skipped: they cannot
        /// be matched against an identity, so they never receive a switch either way.
        /// </summary>
        private List<ProcessSnapshot> SnapshotProcesses()
        {
            var snapshot = new List<ProcessSnapshot>();
            foreach (var process in Process.GetProcesses())
            {
                using (process)
                {
                    if (process.Id <= 4 || process.Id == Environment.ProcessId) continue;

                    try
                    {
                        snapshot.Add(new ProcessSnapshot((uint)process.Id, process.ProcessName, process.MainModule?.FileName ?? string.Empty));
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug(ex, "Couldn't inspect process PID {PID} for an assignment rule", process.Id);
                    }
                }
            }

            return snapshot;
        }

        /// <summary>Reconcile the ownership ledger, persisting it when the pass changed anything.</summary>
        private void ReconcileAssignments(string trigger)
        {
            try
            {
                _assignmentReconciler.Reconcile();
            }
            catch (Exception ex)
            {
                // A failure partway through a pass must not lose the ledger edits it already
                // made: they are persisted below before the error is surfaced.
                _logger.Warning(ex, "Assignment reconciliation ({Trigger}) failed", trigger);
            }
            finally
            {
                SaveLedgerIfDirty(trigger);
            }
        }

        /// <summary>Persist the ledger when a pass changed it.</summary>
        private void SaveLedgerIfDirty(string trigger)
        {
            if (!_assignmentReconciler.IsDirty)
                return;

            _logger.Debug("Assignment ledger changed ({Trigger}), saving", trigger);
            try
            {
                _configuration.Save();
                // Only a successful save may consume the flag: a failed one must keep the
                // ledger dirty so the next pass retries instead of losing ownership edits.
                _assignmentReconciler.TakeDirty();
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to save the assignment ledger ({Trigger})", trigger);
            }
        }

        private void OnProcessesDetected(object? sender, List<ProcessMonitor.Event> events)
        {
            var notifiedRulesInBatch = new HashSet<Guid>();
            foreach (var @event in events)
            {
                ApplyRulesToProcess(@event.ProcessId, @event.ProcessName, @event.ProcessPath, @event.WindowTitle, notifiedRulesInBatch, "process");
            }

            ReconcileAssignments("sighting");
        }

        private void OnForegroundWindowChanged(object? sender, WindowMonitor.Event e)
        {
            if (e.ProcessId == 0 || e.ProcessId == Environment.ProcessId) return;

            _logger.Verbose("Foreground window changed: {WindowTitle} (PID: {ProcessId})", e.WindowName, e.ProcessId);

            // For foreground changes, we re-apply immediately as it often means the app is interactive
            ApplyRulesToProcess(e.ProcessId, e.ProcessName, e.ProcessPath, e.WindowName, null, "foreground");
            ReconcileAssignments("foreground");
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

            return WindowsAPIAdapter.IsHotKeyAvailable(hotKey);
        }

        /// <summary>
        /// Two hotkey rules may not target the same application/window, otherwise the
        /// cycle would be ambiguous. Comparison honours each rule's <see cref="AppSoundRule.CaseSensitive"/>
        /// flag the same way <see cref="IsMatch"/> does: only when both rules are case-sensitive
        /// are the patterns compared case-sensitively.
        /// </summary>
        public bool IsHotkeyTargetAvailable(AppSoundRule rule)
        {
            if (_configuration.AppSoundRules == null) return true;

            lock (_lock)
            {
                return !_configuration.AppSoundRules.Any(other =>
                    other.Id != rule.Id &&
                    other.Mode == AppRuleMode.Hotkey &&
                    PatternsEqual(other.ProcessPath, rule.ProcessPath, rule.CaseSensitive && other.CaseSensitive) &&
                    PatternsEqual(other.WindowName, rule.WindowName, rule.CaseSensitive && other.CaseSensitive));
            }
        }

        private static bool PatternsEqual(string left, string right, bool caseSensitive) =>
            string.Equals(left, right, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

        private void ApplyRulesToProcess(uint processId, string processName, string processPath, string windowTitle, HashSet<Guid>? notifiedRules, string triggerSource)
        {
            if (_configuration.AppSoundRules == null || _configuration.AppSoundRules.Count == 0) return;

            foreach (var rule in _configuration.AppSoundRules.ToList().Where(r => r.Enabled && r.Mode == AppRuleMode.Automatic))
            {
                var pair = rule.DevicePairs?.FirstOrDefault();
                if (pair == null) continue;

                // Assignment rules match one exe identity exactly; other rules keep their
                // regex/glob/window-title matching (ADR �6.1).
                var identityRule = AppRuleAssignmentValidator.IsEligible(rule);
                var identity = rule.AssignmentExePath ?? rule.ProcessPath;
                var matched = identityRule
                    ? !string.IsNullOrEmpty(identity) && AppRuleIdentityMatcher.SameApp(identity, processPath)
                    : IsMatch(rule, processName, processPath, windowTitle);
                if (!matched) continue;

                if (ApplyRuleToProcess(rule, pair, processId, processName, processPath, notifiedRules, triggerSource, true, out var flowsWritten))
                {
                    // An assignment's engine re-applies the route itself after the first write;
                    // keep that observable in the trigger telemetry (ADR �7).
                    TrackRuleActivated(identityRule ? "engine" : triggerSource, processPath, processName);

                    // Own the route in the ledger � for any enforcement kind (ADR �6.4). Only
                    // the flows actually written are claimed; adoption covers pre-existing
                    // records and later successful writes extend the claim.
                    _assignmentReconciler.EnsureEntry(rule, processPath, pair, flowsWritten);
                }
            }
        }

        /// <summary>
        /// Switch the given process to the pair's devices, notify and record telemetry.
        /// Shared by the automatic (process/foreground) path and the rule hotkey.
        /// </summary>
        private bool ApplyRuleToProcess(AppSoundRule rule, AppSoundRuleDevicePair pair, uint processId, string processName, string processPath, HashSet<Guid>? notifiedRules, string triggerSource, bool notify, out IReadOnlyCollection<EDataFlow> flowsWritten)
        {
            _logger.Information("MATCH: Rule for {ProcessPattern} matched process {ProcessName} (PID: {PID})", rule.ProcessPath, processName, processId);

            var playbackSwitched = SwitchRuleDevice(pair, p => p.Playback, (p, v) => p.Playback = v, EDataFlow.eRender, processId);
            var recordingSwitched = SwitchRuleDevice(pair, p => p.Recording, (p, v) => p.Recording = v, EDataFlow.eCapture, processId);
            var changed = playbackSwitched || recordingSwitched;

            var flows = new List<EDataFlow>();
            if (playbackSwitched) flows.Add(EDataFlow.eRender);
            if (recordingSwitched) flows.Add(EDataFlow.eCapture);
            flowsWritten = flows;

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
        /// Emit the activation telemetry once per event or press. Prefer the basename from the
        /// process path; if it's empty (ProcessMonitor sets the path to <c>string.Empty</c>) or a
        /// placeholder (WindowMonitor uses "N/A" when it can't read the path), fall back to the
        /// process name so we still capture an anonymized hash instead of hashing "N/A".
        /// </summary>
        private static void TrackRuleActivated(string triggerSource, string processPath, string processName)
        {
            var basename = Path.GetFileName(processPath);
            if (string.IsNullOrEmpty(basename) || basename == "N/A")
            {
                basename = processName;
            }
            TelemetryService.TrackAppRuleActivated(basename, triggerSource);
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

            var pairIndex = TakeNextPairIndex(rule.Id, pairs.Count);
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
                    var processPath = GetModulePath(process);
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
                RevertPairIndex(rule.Id, pairIndex, pairs.Count);
                return;
            }

            // A pair that resolves to no live device can't be applied; keep the cycle where
            // it is so the user can plug the device back in and press again.
            if (!CanApplyPair(pair))
            {
                _logger.Warning("App rule hotkey pressed but no device of pair {Index} could be resolved for {ProcessPattern}", pairIndex, rule.ProcessPath);
                RevertPairIndex(rule.Id, pairIndex, pairs.Count);
                return;
            }

            var changed = false;
            foreach (var match in matches)
            {
                try
                {
                    var applied = ApplyRuleToProcess(rule, pair, match.ProcessId, match.ProcessName, match.ProcessPath, null, "hotkey", false, out var flowsWritten);
                    changed |= applied;

                    // Only an actually written route is claimed: a repeat press that already
                    // matches writes nothing, and stamping it would create (or blank out) an
                    // entry whose missing devices read as "clear both flows".
                    // Every written route owns its record in the ledger for any enforcement
                    // kind (ADR �6.4): a hotkey write under a window-title condition could
                    // otherwise never be re-adopted by reconcile (the store carries no titles)
                    // and the app would surface under Windows assignments while its rule lives.
                    if (applied)
                        _assignmentReconciler.EnsureEntry(rule, match.ProcessPath, pair, flowsWritten);
                }
                catch (Exception ex)
                {
                    // The process may have exited between enumeration and the switch.
                    _logger.Debug(ex, "Couldn't apply app rule hotkey to process PID {PID}", match.ProcessId);
                }
            }

            // Persist the claims this press stamped: nothing later in the hotkey flow saves, so
            // a restart would otherwise load a ledger that does not own the route in the store.
            SaveLedgerIfDirty("hotkey");

            // Notify even when the endpoint already matched: the hotkey did apply this pair,
            // and the banner is the feedback that tells the user which pair is now active.
            // The cycle index was already reserved by TakeNextPairIndex, so a notification
            // failure cannot block the cycle.
            if (rule.Notify)
            {
                _notificationManager.NotifyAppRuleMatched(rule, pair, matches[0].ProcessId);
            }

            // A single press is one activation: record telemetry once for the first match
            // rather than once per switched process, mirroring the notify behaviour above.
            if (changed)
            {
                TrackRuleActivated("hotkey", matches[0].ProcessPath, matches[0].ProcessName);
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

        private int TakeNextPairIndex(Guid ruleId, int pairCount)
        {
            lock (_lock)
            {
                var index = _nextPairIndex.TryGetValue(ruleId, out var i) && i >= 0 && i < pairCount ? i : 0;
                _nextPairIndex[ruleId] = pairCount <= 0 ? 0 : (index + 1) % pairCount;
                return index;
            }
        }

        private void RevertPairIndex(Guid ruleId, int appliedIndex, int pairCount)
        {
            lock (_lock)
            {
                // Only rewind when this press still owns the cursor. A newer press that
                // already took the next index must not be clobbered.
                if (pairCount > 0 &&
                    _nextPairIndex.TryGetValue(ruleId, out var next) &&
                    next == (appliedIndex + 1) % pairCount)
                {
                    _nextPairIndex[ruleId] = appliedIndex;
                }
            }
        }

        private string GetWindowTitle(Process process)
        {
            try
            {
                return process.MainWindowTitle ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Verbose(ex, "Couldn't read window title for PID {PID}", process.Id);
                return string.Empty;
            }
        }

        /// <summary>
        /// Best-effort module path for a process. Mirroring the automatic path, an
        /// unreadable path degrades to an empty string so name-based rules can still
        /// match a process whose module path is inaccessible.
        /// </summary>
        private string GetModulePath(Process process)
        {
            try
            {
                return process.MainModule?.FileName ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Verbose(ex, "Couldn't read module path for PID {PID}", process.Id);
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

        /// <summary>
        /// The full rule match expression (process regex/glob plus optional window pattern).
        /// Internal static so the assignment reconciler can evaluate the same expression for
        /// adoption/ownership of store records (ADR �6.4).
        /// </summary>
        internal static bool IsMatch(AppSoundRule rule, string processName, string processPath, string windowTitle)
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
                    Log.Warning(ex, "Invalid Regex for ProcessPath: {Pattern}. Falling back to glob.", rule.ProcessPath);
                    processMatched = targets.Any(t => !string.IsNullOrEmpty(t) && IsGlobMatch(t, rule.ProcessPath, options));
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Unexpected error matching pattern {Pattern}", rule.ProcessPath);
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
                    ex => Log.Warning(ex, "Invalid Regex for WindowName: {Pattern}. Falling back to glob.", rule.WindowName)
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
