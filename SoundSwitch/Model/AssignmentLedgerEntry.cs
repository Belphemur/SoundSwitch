#nullable enable

using System;

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

        public override string ToString()
        {
            return $"{ExePath} (rule {RuleId} {Enforcement})";
        }
    }
}
