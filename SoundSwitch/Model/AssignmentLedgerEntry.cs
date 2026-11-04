#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SoundSwitch.Model
{
    /// <summary>
    /// One entry in SoundSwitch's ownership ledger for Windows' per-app persisted endpoints
    /// (design doc §6.4). A record in <c>HKCU\...\Multimedia\Audio\DefaultEndpoint</c> covered by
    /// an entry was written by SoundSwitch (through <c>SwitchProcessTo</c>, for any enforcement
    /// kind); on rule deletion/disable the entry drives the deferred restore-default clear.
    /// Entries with no corresponding store record anymore are dropped silently on reconcile.
    /// </summary>
    public class AssignmentLedgerEntry
    {
        /// <summary>The rule that wrote (or adopted) the record.</summary>
        public Guid RuleId { get; set; }

        /// <summary>The exe identity the record is keyed on, as seen when written.</summary>
        public string ExePath { get; set; } = string.Empty;

        /// <summary>Enforcement kind of the owning rule at write time; drives the delete semantics.</summary>
        public AppRuleEnforcement Enforcement { get; set; } = AppRuleEnforcement.Dynamic;

        /// <summary>Playback endpoint written, or null when the route covered none.</summary>
        public string? PlaybackDeviceId { get; set; }

        /// <summary>Recording endpoint written, or null when the route covered none.</summary>
        public string? RecordingDeviceId { get; set; }

        /// <summary>Diagnostics: when the last write/adoption happened.</summary>
        public DateTime WrittenAt { get; set; }

        /// <summary>
        /// When the owning rule retracted the route (deleted/disabled/edited away), if it did:
        /// the restore-default delay (ADR §7) and the pending clear are measured from here, not
        /// from <see cref="WrittenAt"/> — a route written weeks earlier must not report weeks of
        /// restore delay. <c>null</c> while the rule still owns the route.
        /// </summary>
        public DateTime? ClearRequestedAt { get; set; }

        public override string ToString()
        {
            return $"{ExePath} (rule {RuleId} {Enforcement})";
        }
    }

    /// <summary>
    /// The ledger's backing collection. Reconcile passes mutate it on background threads
    /// (process sightings, device changes, foreground changes) while the configuration
    /// serializer and the UI (rule list, settings save) enumerate it on other threads; every
    /// operation takes one gate and every enumeration returns a snapshot of the entries, so an
    /// overlapping save can neither throw "collection was modified" nor persist a half-mutated
    /// ledger.
    /// </summary>
    public class AssignmentLedgerCollection : IList<AssignmentLedgerEntry>
    {
        private static readonly object _sync = new();
        private readonly List<AssignmentLedgerEntry> _items = new();

        public AssignmentLedgerEntry this[int index]
        {
            get
            {
                lock (_sync)
                    return _items[index];
            }
            set
            {
                lock (_sync)
                    _items[index] = value;
            }
        }

        public int Count
        {
            get
            {
                lock (_sync)
                    return _items.Count;
            }
        }

        public bool IsReadOnly => false;

        public void Add(AssignmentLedgerEntry item)
        {
            lock (_sync)
                _items.Add(item);
        }

        public void Clear()
        {
            lock (_sync)
                _items.Clear();
        }

        public bool Contains(AssignmentLedgerEntry item)
        {
            lock (_sync)
                return _items.Contains(item);
        }

        public void CopyTo(AssignmentLedgerEntry[] array, int arrayIndex)
        {
            lock (_sync)
                _items.CopyTo(array, arrayIndex);
        }

        public int IndexOf(AssignmentLedgerEntry item)
        {
            lock (_sync)
                return _items.IndexOf(item);
        }

        public void Insert(int index, AssignmentLedgerEntry item)
        {
            lock (_sync)
                _items.Insert(index, item);
        }

        public bool Remove(AssignmentLedgerEntry item)
        {
            lock (_sync)
                return _items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            lock (_sync)
                _items.RemoveAt(index);
        }

        public IEnumerator<AssignmentLedgerEntry> GetEnumerator()
        {
            lock (_sync)
            {
                var snapshot = new AssignmentLedgerEntry[_items.Count];
                _items.CopyTo(snapshot);
                return ((IEnumerable<AssignmentLedgerEntry>)snapshot).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
