#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Serilog;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.Telemetry;
using SoundSwitch.Model;

namespace SoundSwitch.Services
{
    /// <summary>
    /// Reconciler between the desired routing state (the enabled <see cref="AppSoundRule"/>s) and
    /// the actual state (Windows' per-app persisted endpoint store, read through the
    /// <see cref="PersistedDefaultEndpointStore"/>) — the ownership ledger of the app-rules
    /// identity ADR (§6.4).
    ///
    /// A store record is <em>ours</em> when a ledger entry covers it; entries are stamped
    /// whenever SoundSwitch writes a route (any enforcement kind) and adopted on first reconcile
    /// for pre-ledger records matching an enabled rule. Records with neither are the user's/Win-
    /// dows' own and are never touched — they only surface in the rule list's read-only
    /// "Windows assignments" section. Entries whose record vanished under them are dropped
    /// silently; entries of deleted or disabled assignment rules drive the restore-default clear
    /// (immediately for a running app, deferred to the app's next sighting for a closed one).
    ///
    /// All Windows-side mutations ride the existing engine-mediated paths only
    /// (<see cref="AudioSwitcher"/> on its <c>ComThread</c>); this class never writes the
    /// registry store. The caller persists the configuration after a pass when
    /// <see cref="TakeDirty"/> reports ledger changes.
    /// </summary>
    public sealed class AppRuleAssignmentReconciler
    {
        private static readonly ILogger _logger = Log.ForContext<AppRuleAssignmentReconciler>();

        private readonly ISoundSwitchConfiguration _configuration;
        private readonly AudioSwitcher _audioSwitcher;
        private readonly PersistedDefaultEndpointStore _store;
        private readonly Func<DateTime> _clock;
        private readonly object _sync = new();
        private bool _dirty;

        public AppRuleAssignmentReconciler(ISoundSwitchConfiguration configuration, AudioSwitcher audioSwitcher,
            PersistedDefaultEndpointStore? store = null, Func<DateTime>? clock = null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _audioSwitcher = audioSwitcher;
            _store = store ?? new PersistedDefaultEndpointStore();
            _clock = clock ?? (() => DateTime.UtcNow);
        }

        /// <summary>
        /// One merged view of an identity's records in Windows' store: the persisted endpoint per
        /// flow, as shown in the rule list's read-only "Windows assignments" section.
        /// </summary>
        public sealed class StoreAssignment
        {
            /// <summary>NT device path (<c>\Device\HarddiskVolumeN\...</c>) as stored by Windows.</summary>
            public string ExePath { get; init; } = string.Empty;

            /// <summary>Volume-relative path; the row display name derives from it.</summary>
            public string RelativePath { get; init; } = string.Empty;

            /// <summary>Persisted playback endpoint (preferred role), or null.</summary>
            public string? PlaybackDeviceId { get; init; }

            /// <summary>Persisted recording endpoint (preferred role), or null.</summary>
            public string? RecordingDeviceId { get; init; }
        }

        /// <summary>One identity's records merged from the store, with ownership annotations.</summary>
        private sealed class StoreGroup
        {
            public string NtExePath = string.Empty;
            public string RelativePath = string.Empty;
            public string? PlaybackDeviceId;
            public string? RecordingDeviceId;
            public bool CoveredByLedger;
            public AppSoundRule? CoveringRule;
        }

        /// <summary>
        /// Records of Windows' store not owned by any SoundSwitch rule (ADR §6.5, merge rule of
        /// §11.5 — one identity, one place). Pure read; ownership materializes in the passes.
        /// </summary>
        public IReadOnlyList<StoreAssignment> GetWindowsAssignments()
        {
            var rules = _configuration.AppSoundRules?.ToList() ?? new List<AppSoundRule>();
            List<StoreGroup> groups;
            lock (_sync)
            {
                groups = ReadGroups(rules);
            }

            return groups
                .Where(group => !group.CoveredByLedger && group.CoveringRule == null)
                .Select(group => new StoreAssignment
                {
                    ExePath = group.NtExePath,
                    RelativePath = group.RelativePath,
                    PlaybackDeviceId = group.PlaybackDeviceId,
                    RecordingDeviceId = group.RecordingDeviceId
                })
                .ToList();
        }

        /// <summary>Whether an entry of <paramref name="rule"/> already owns the identity.</summary>
        public bool HasEntry(AppSoundRule rule, string processPath)
        {
            var identity = IdentityOf(rule, processPath);
            lock (_sync)
            {
                return Ledger(rule.Id, identity).Count > 0;
            }
        }

        /// <summary>
        /// Stamp the ledger with the route <paramref name="rule"/> just wrote (or re-asserted) for
        /// the identity it matched — the ownership record of §6.4, for any enforcement kind.
        /// </summary>
        public void EnsureEntry(AppSoundRule rule, string processPath, AppSoundRuleDevicePair pair)
        {
            var identity = IdentityOf(rule, processPath);
            lock (_sync)
            {
                var entry = Ledger(rule.Id, identity).FirstOrDefault();
                if (entry != null)
                {
                    if (UpdateEntry(entry, rule, pair))
                        MarkDirty();
                    return;
                }

                _configuration.AssignmentLedger.Add(new AssignmentLedgerEntry
                {
                    RuleId = rule.Id,
                    ExePath = identity,
                    Enforcement = rule.Enforcement,
                    PlaybackDeviceId = pair.Playback?.Id,
                    RecordingDeviceId = pair.Recording?.Id,
                    WrittenAt = _clock()
                });
                MarkDirty();
            }
        }

        /// <summary>
        /// Stop routing like the (now removed, disabled or edited) rule did. Assignment rules
        /// clear the records they wrote right away for running apps; a closed app keeps its
        /// entry as pending and is cleared on its next sighting. Dynamic rules only release
        /// their entries: their leftover records keep today's delete behaviour (route stays) and
        /// are surfaced as Windows-managed rows instead (ADR §11.5, §11.6).
        /// </summary>
        public void ClearRuleAssignments(AppSoundRule rule)
        {
            lock (_sync)
            {
                var entries = _configuration.AssignmentLedger.Where(entry => entry.RuleId == rule.Id).ToList();
                foreach (var entry in entries)
                {
                    if (entry.Enforcement == AppRuleEnforcement.Assignment)
                    {
                        ClearAssignmentRoute(entry);
                        continue;
                    }

                    _configuration.AssignmentLedger.Remove(entry);
                    MarkDirty();
                }
            }
        }

        /// <summary>
        /// One reconcile pass: adoption, stale-drop, deferred restore-default clears (ADR §6.4,
        /// §6.6). Runs on start, on rule save/enable/disable/delete, on process sightings and on
        /// device changes.
        /// </summary>
        public void Reconcile()
        {
            var rules = _configuration.AppSoundRules?.ToList() ?? new List<AppSoundRule>();
            lock (_sync)
            {
                var groups = ReadGroups(rules);

                // 1. Adoption: a store record matching an enabled rule but missing from the
                //    ledger is ours (written before the ledger existed) — claim it without
                //    touching Windows' store (ADR §6.4 upgrade path).
                foreach (var group in groups.Where(g => !g.CoveredByLedger && g.CoveringRule != null))
                {
                    var rule = group.CoveringRule!;
                    if (Ledger(rule.Id, group.NtExePath).Count > 0)
                        continue;

                    _configuration.AssignmentLedger.Add(new AssignmentLedgerEntry
                    {
                        RuleId = rule.Id,
                        ExePath = group.NtExePath,
                        Enforcement = rule.Enforcement,
                        PlaybackDeviceId = group.PlaybackDeviceId,
                        RecordingDeviceId = group.RecordingDeviceId,
                        WrittenAt = _clock()
                    });
                    MarkDirty();
                    _logger.Information("Adopted persisted assignment {ExePath} for rule {ProcessPattern}",
                        group.NtExePath, rule.ProcessPath);
                }

                foreach (var entry in _configuration.AssignmentLedger.ToList())
                {
                    var group = groups.FirstOrDefault(g => AppRuleIdentityMatcher.SameApp(g.NtExePath, entry.ExePath));

                    // 2. Stale-drop: the record disappeared under the entry (user cleaned it in
                    //    Windows settings, or the engine dropped it) — converge silently.
                    if (group == null)
                    {
                        _configuration.AssignmentLedger.Remove(entry);
                        MarkDirty();
                        continue;
                    }

                    var rule = rules.FirstOrDefault(r => r.Id == entry.RuleId);
                    var stillOwned = rule != null &&
                                     rule.Enabled &&
                                     rule.Enforcement == entry.Enforcement &&
                                     AppRuleIdentityMatcher.SameApp(IdentityOf(rule, entry.ExePath), entry.ExePath);

                    // 3. A Dynamic entry whose rule is gone: the route keeps today's delete
                    //    behaviour (left in place, displayed as Windows-managed), so the ledger
                    //    simply releases its claim.
                    if (entry.Enforcement == AppRuleEnforcement.Dynamic && !stillOwned)
                    {
                        _configuration.AssignmentLedger.Remove(entry);
                        MarkDirty();
                        continue;
                    }

                    // 4. Deleted/disabled/edited-away assignment rule: restore the system
                    //    default. Nothing to do while the app is closed — the entry stays
                    //    pending and the sighting clears it (ADR §6.3 deferred restore).
                    if (entry.Enforcement == AppRuleEnforcement.Assignment && !stillOwned)
                    {
                        ClearAssignmentRoute(entry);
                    }
                }
            }
        }

        /// <summary>Whether any ledger change happened since the previous drain.</summary>
        public bool TakeDirty()
        {
            lock (_sync)
            {
                return TakeDirtyLocked();
            }
        }

        private bool TakeDirtyLocked()
        {
            var wasDirty = _dirty;
            _dirty = false;
            return wasDirty;
        }

        private void MarkDirty()
        {
            _dirty = true;
        }

        private static string IdentityOf(AppSoundRule rule, string seenPath)
        {
            return rule.Enforcement == AppRuleEnforcement.Assignment
                ? rule.AssignmentExePath ?? rule.ProcessPath
                : seenPath;
        }

        private List<AssignmentLedgerEntry> Ledger(Guid ruleId, string identity)
        {
            return _configuration.AssignmentLedger
                .Where(entry => entry.RuleId == ruleId && AppRuleIdentityMatcher.SameApp(entry.ExePath, identity))
                .ToList();
        }

        private bool UpdateEntry(AssignmentLedgerEntry entry, AppSoundRule rule, AppSoundRuleDevicePair pair)
        {
            var changed = false;
            var playbackId = pair.Playback?.Id;
            var recordingId = pair.Recording?.Id;
            if (!string.Equals(entry.PlaybackDeviceId, playbackId, StringComparison.OrdinalIgnoreCase))
            {
                entry.PlaybackDeviceId = playbackId;
                changed = true;
            }

            if (!string.Equals(entry.RecordingDeviceId, recordingId, StringComparison.OrdinalIgnoreCase))
            {
                entry.RecordingDeviceId = recordingId;
                changed = true;
            }

            if (entry.Enforcement != rule.Enforcement)
            {
                entry.Enforcement = rule.Enforcement;
                changed = true;
            }

            entry.WrittenAt = _clock();
            return changed;
        }

        /// <summary>Group the store's records per identity and annotate ledger/rule coverage.</summary>
        private List<StoreGroup> ReadGroups(List<AppSoundRule> rules)
        {
            var groups = new List<StoreGroup>();
            foreach (var groupOfRecords in _store.ReadAll()
                         .GroupBy(record => PersistedDefaultEndpointStore.GetVolumeRelativePath(record.ExePath),
                             StringComparer.OrdinalIgnoreCase)
                         .Where(g => g.Key.Length > 0))
            {
                var group = new StoreGroup
                {
                    NtExePath = groupOfRecords.First().ExePath,
                    RelativePath = groupOfRecords.Key,
                    PlaybackDeviceId = PreferredEndpoint(groupOfRecords, EDataFlow.eRender),
                    RecordingDeviceId = PreferredEndpoint(groupOfRecords, EDataFlow.eCapture)
                };
                if (groups.Any(existing => AppRuleIdentityMatcher.SameApp(existing.NtExePath, group.NtExePath)))
                    continue; // volume-relative grouping can still yield two spellings of one identity

                group.CoveredByLedger = _configuration.AssignmentLedger
                    .Any(entry => AppRuleIdentityMatcher.SameApp(entry.ExePath, group.NtExePath));
                group.CoveringRule = rules.FirstOrDefault(rule => RuleCovers(rule, group));
                groups.Add(group);
            }

            return groups;
        }

        /// <summary>First record of the flow in preferred-role order (console, then others).</summary>
        private static string? PreferredEndpoint(IEnumerable<PersistedDefaultEndpointStore.Record> records, EDataFlow flow)
        {
            foreach (var role in new[] { ERole.eConsole, ERole.eMultimedia, ERole.eCommunications })
            {
                var deviceId = records
                    .Where(record => record.Flow == flow)
                    .Select(record => record.DeviceIds.TryGetValue(role, out var id) ? id : null)
                    .FirstOrDefault(id => id != null);
                if (deviceId != null)
                    return deviceId;
            }

            return null;
        }

        /// <summary>
        /// Whether the rule targets the record's identity: assignment rules compare the
        /// assignment exe identity exactly; other rules through their usual match expression
        /// against the store's exe path (no window title available in the store).
        /// </summary>
        private static bool RuleCovers(AppSoundRule rule, StoreGroup group)
        {
            if (!rule.Enabled)
                return false;

            var bundleName = Path.GetFileNameWithoutExtension(group.RelativePath);
            return rule.Enforcement == AppRuleEnforcement.Assignment
                ? AppRuleIdentityMatcher.SameApp(IdentityOf(rule, group.NtExePath), group.NtExePath)
                : AppSoundLockManager.IsMatch(rule, bundleName, group.NtExePath, string.Empty);
        }

        /// <summary>Flows the entry's route covered, or both when the entry carries no device.</summary>
        private static List<EDataFlow> FlowsToClear(AssignmentLedgerEntry entry)
        {
            var flows = new List<EDataFlow>();
            if (entry.PlaybackDeviceId != null || entry.RecordingDeviceId == null)
                flows.Add(EDataFlow.eRender);
            if (entry.RecordingDeviceId != null || entry.PlaybackDeviceId == null)
                flows.Add(EDataFlow.eCapture);
            return flows;
        }

        /// <summary>
        /// Clear a pending assignment route right away where the engine accepts the write (only
        /// for a pid with a live audio session). Returns whether the whole route was cleared —
        /// a closed app keeps its entry pending for its next sighting (ADR §6.3).
        /// </summary>
        private bool ClearAssignmentRoute(AssignmentLedgerEntry entry)
        {
            var flows = FlowsToClear(entry);
            if (flows.Count == 0)
            {
                _configuration.AssignmentLedger.Remove(entry);
                MarkDirty();
                return true;
            }

            var runningPids = RunningProcessesFor(entry.ExePath);
            if (runningPids.Count == 0)
                return false; // stays pending

            var allCleared = flows.All(flow =>
                runningPids.Exists(tuple => _audioSwitcher.ClearProcessDefaultEndpoint(flow, tuple.Pid)));
            if (!allCleared)
                return false; // partially failed or no live session: stay pending

            _configuration.AssignmentLedger.Remove(entry);
            TrackDeferredClear(entry);
            MarkDirty();
            return true;
        }

        /// <summary>Running processes whose exe matches the identity (the app itself excluded).</summary>
        private static List<(uint Pid, string ProcessPath)> RunningProcessesFor(string identity)
        {
            var matches = new List<(uint Pid, string ProcessPath)>();
            foreach (var process in Process.GetProcesses())
            {
                using (process)
                {
                    if (process.Id <= 4 || process.Id == Environment.ProcessId)
                        continue;

                    try
                    {
                        var path = process.MainModule?.FileName;
                        if (string.IsNullOrEmpty(path))
                            continue;

                        if (AppRuleIdentityMatcher.SameApp(identity, path))
                            matches.Add(((uint)process.Id, path));
                    }
                    catch
                    {
                        // Unreadable module path (elevated/exited process) — expected.
                    }
                }
            }

            return matches;
        }

        private void TrackDeferredClear(AssignmentLedgerEntry entry)
        {
            var elapsed = _clock() - entry.WrittenAt;
            _logger.Information("Assignment route of {ExePath} (rule {RuleId}) restored to the default after {Seconds:N0}s",
                entry.ExePath, entry.RuleId, elapsed.TotalSeconds);
            TelemetryService.TrackAppRuleAssignmentDeferredClear(elapsed.TotalSeconds);
        }
    }
}
