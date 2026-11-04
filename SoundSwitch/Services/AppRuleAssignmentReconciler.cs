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
            public bool CoveredByLiveLedger;
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

            if (groups == null)
                return Array.Empty<StoreAssignment>();

            // Coverage by an entry whose rule no longer exists (e.g. a pending restore-default
            // clear for a closed app) is not ownership for display: the record belongs to no
            // rule row anymore, so the read-only Windows section is where it stays visible.
            return groups
                .Where(group => !group.CoveredByLiveLedger && group.CoveringRule == null)
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
        /// Only flows the write actually reproduced are claimed: a side whose device could not
        /// resolve (or already matched) may hold the app's pre-existing Windows assignment, and
        /// the ledger must not later clear a route SoundSwitch never wrote.
        /// </summary>
        public void EnsureEntry(AppSoundRule rule, string processPath, AppSoundRuleDevicePair pair, IReadOnlyCollection<EDataFlow>? flowsWritten = null)
        {
            var identity = IdentityOf(rule, processPath);
            var playbackId = flowsWritten == null || flowsWritten.Contains(EDataFlow.eRender)
                ? pair.Playback?.Id
                : null;
            var recordingId = flowsWritten == null || flowsWritten.Contains(EDataFlow.eCapture)
                ? pair.Recording?.Id
                : null;

            lock (_sync)
            {
                var entry = Ledger(rule.Id, identity).FirstOrDefault();
                if (entry != null)
                {
                    if (UpdateEntry(entry, rule, playbackId, recordingId))
                        MarkDirty();
                    return;
                }

                _configuration.AssignmentLedger.Add(new AssignmentLedgerEntry
                {
                    RuleId = rule.Id,
                    ExePath = identity,
                    Enforcement = rule.Enforcement,
                    PlaybackDeviceId = playbackId,
                    RecordingDeviceId = recordingId,
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
                        // The restore-default measurement (§7) starts when the rule retracted
                        // its route, not when the route was last written.
                        entry.ClearRequestedAt ??= _clock();
                        MarkDirty();
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

                // An unreadable store is not an empty store: without the real state the pass
                // cannot distinguish "record vanished" from "read failed", so drop nothing and
                // adopt nothing this round (ADR §6.6 idempotence) — ownership survives to the
                // next pass instead of being deleted because a read hiccup returned nothing.
                if (groups == null)
                    return;

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
                        // A surviving assignment rule replaced the retracted one for the same
                        // identity (the rule was deleted while the app was closed, then a new
                        // rule for the exe was created): the sighting that brought the app back
                        // already wrote the new route, and a clear would wipe it moments later.
                        // Release the stale entry — the new rule's own ledger entry owns now.
                        if (ReplacedByLiveAssignmentRule(rules, entry))
                        {
                            _logger.Information("Released pending clear of {ExePath}: rule {OldRuleId} was replaced by another assignment rule",
                                entry.ExePath, entry.RuleId);
                            _configuration.AssignmentLedger.Remove(entry);
                            MarkDirty();
                            continue;
                        }

                        if (entry.ClearRequestedAt == null)
                        {
                            entry.ClearRequestedAt = _clock();
                            MarkDirty();
                        }

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
            return AppRuleAssignmentValidator.IsEligible(rule)
                ? rule.AssignmentExePath ?? rule.ProcessPath
                : seenPath;
        }

        private List<AssignmentLedgerEntry> Ledger(Guid ruleId, string identity)
        {
            return _configuration.AssignmentLedger
                .Where(entry => entry.RuleId == ruleId && AppRuleIdentityMatcher.SameApp(entry.ExePath, identity))
                .ToList();
        }

        private bool UpdateEntry(AssignmentLedgerEntry entry, AppSoundRule rule, string? playbackId, string? recordingId)
        {
            var changed = false;
            // Only a flow this write actually reproduced replaces the stored endpoint id: a side
            // the switch didn't touch (device unavailable, already matching) keeps the id the
            // previously written route carried — the flows-to-clear set (§6.3/§6.4) stays honest.
            if (playbackId != null && !string.Equals(entry.PlaybackDeviceId, playbackId, StringComparison.OrdinalIgnoreCase))
            {
                entry.PlaybackDeviceId = playbackId;
                changed = true;
            }

            if (recordingId != null && !string.Equals(entry.RecordingDeviceId, recordingId, StringComparison.OrdinalIgnoreCase))
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

        /// <summary>
        /// Group the store's records per identity and annotate ledger/rule coverage. Returns
        /// <c>null</c> when Windows' store could not be read this pass (not "no records").
        /// </summary>
        private List<StoreGroup>? ReadGroups(List<AppSoundRule> rules)
        {
            var records = _store.TryReadAll();
            if (records == null)
                return null;

            var liveRuleIds = rules.Select(rule => rule.Id).ToHashSet();
            var groups = new List<StoreGroup>();
            foreach (var groupOfRecords in records
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
                group.CoveredByLiveLedger = _configuration.AssignmentLedger
                    .Any(entry => liveRuleIds.Contains(entry.RuleId) &&
                                  AppRuleIdentityMatcher.SameApp(entry.ExePath, group.NtExePath));
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
        /// assignment exe identity exactly; other rules through their usual match expression —
        /// the store has no window titles and its paths are NT-device form, so the dynamic
        /// expression is evaluated against the record's volume-relative path in addition to the
        /// NT path and the bundle name.
        /// </summary>
        private static bool RuleCovers(AppSoundRule rule, StoreGroup group)
        {
            if (!rule.Enabled)
                return false;

            if (AppRuleAssignmentValidator.IsEligible(rule))
                return AppRuleIdentityMatcher.SameApp(IdentityOf(rule, group.NtExePath), group.NtExePath);

            // The pattern must also see the bare exe name: a rule like "spotify*" or
            // "^Spotify\.exe$" covers its own store record even though the record only carries
            // paths (NT and volume-relative). Evaluating just the paths would leave that record
            // looking Windows-managed: a duplicate row, and never cleaned up on rule deletion.
            var bareName = Path.GetFileName(group.NtExePath);
            return AppSoundLockManager.IsMatch(rule, bareName, group.RelativePath, string.Empty) ||
                   AppSoundLockManager.IsMatch(rule, bareName, group.NtExePath, string.Empty);
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
        /// Whether another enabled assignment rule (not the entry's own) targets the same exe
        /// identity — the rule that own the route the pending clear is about to run against.
        /// </summary>
        private static bool ReplacedByLiveAssignmentRule(List<AppSoundRule> rules, AssignmentLedgerEntry entry)
        {
            return rules.Any(other =>
                other.Id != entry.RuleId &&
                other.Enabled &&
                AppRuleAssignmentValidator.IsEligible(other) &&
                AppRuleIdentityMatcher.SameApp(IdentityOf(other, entry.ExePath), entry.ExePath));
        }

        /// <summary>
        /// Clear a pending assignment route right away where the engine accepts the write (only
        /// for a pid with a live audio session) — for every running instance of the identity, so
        /// zero-pid copies keep no leftover route. A claimed flow is cleared as soon as one
        /// running pid accepts the write: the engine drops the identity's record, and the
        /// session-less copies that no-op'd no longer hold the entry hostage. Only when no pid of
        /// a claimed flow accepts (the app is running but has no audio session yet) does the
        /// entry stay pending for its next sighting (ADR §6.3).
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

            // 1. Detach the live sessions. This does not touch the store record, so a running app
            //    needs it to leave the route immediately; a session-less pid is a no-op.
            var cleared = runningPids.Count == 0 || flows.All(flow =>
            {
                var anyAccepted = false;
                foreach (var tuple in runningPids)
                    anyAccepted |= _audioSwitcher.ClearProcessDefaultEndpoint(flow, tuple.Pid);
                return anyAccepted;
            });

            // 2. Delete the identity's persisted record. This is what restores the default for a
            //    closed app (no pid, so no engine call is possible) and what makes the app
            //    disappear from Windows Settings' per-app list instead of lingering with empty
            //    role values.
            var removed = _store.DeleteRecords(entry.ExePath);
            if (removed < 0)
                return false; // store not writable right now: keep the claim and retry

            if (!cleared && removed == 0)
                return false; // live session refused and nothing matched: retry next sighting

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
            // Measured from when the rule was retracted (deleted/disabled/edited), not from the
            // route's last write: a route written weeks earlier must not report weeks of
            // restore delay (ADR §7 — the metric watches the user-visible restore delay).
            var elapsed = _clock() - (entry.ClearRequestedAt ?? entry.WrittenAt);
            _logger.Information("Assignment route of {ExePath} (rule {RuleId}) restored to the default after {Seconds:N0}s",
                entry.ExePath, entry.RuleId, elapsed.TotalSeconds);
            TelemetryService.TrackAppRuleAssignmentDeferredClear(elapsed.TotalSeconds);
        }
    }
}
