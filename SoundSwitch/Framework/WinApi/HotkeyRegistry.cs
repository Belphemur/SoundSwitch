#nullable enable

using System;
using System.Collections.Generic;

using Serilog;

using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.WinApi;

/// <summary>
/// The SoundSwitch system that owns a hotkey combination.
/// </summary>
public enum HotkeyOwnerKind
{
    /// <summary>The global playback switch hotkey (settings).</summary>
    PlaybackSwitch,

    /// <summary>The global recording switch hotkey (settings).</summary>
    RecordingSwitch,

    /// <summary>The global mute hotkey (settings).</summary>
    Mute,

    /// <summary>A profile's hotkey trigger; <see cref="HotkeyOwner.Name"/> is the profile name.</summary>
    Profile,

    /// <summary>An app rule's hotkey; <see cref="HotkeyOwner.Name"/> identifies the rule's process.</summary>
    AppRule
}

/// <summary>
/// The system a hotkey combination belongs to, with the name that identifies the
/// individual owner when several can exist (a profile, an app rule).
/// </summary>
public readonly record struct HotkeyOwner(HotkeyOwnerKind Kind, string? Name = null);

/// <summary>
///     The single point through which every SoundSwitch hotkey owner registers its
///     combination (ADR docs/design/architecture/2026-11-21-shared-mechanism-consolidation.md
///     §D3): the global switch/mute hotkeys, profile hotkeys and app-rule hotkeys.
///     <see cref="WindowsAPIAdapter"/> remains the Win32 layer underneath; this registry
///     owns the owner→hotkey knowledge, so a failed registration can name the system that
///     already holds the combination instead of surfacing a raw RegisterHotKey failure.
/// </summary>
public sealed class HotkeyRegistry
{
    private static readonly ILogger Log = Serilog.Log.ForContext<HotkeyRegistry>();

    private readonly Dictionary<HotKey, HotkeyOwner> _owners = new();
    private readonly object _lock = new();

    /// <summary>
    ///     Try to register the hotkey on behalf of the given owner.
    /// </summary>
    /// <param name="hotKey">The hotkey to register</param>
    /// <param name="owner">The system registering the hotkey</param>
    /// <returns>
    ///     Null on success; otherwise a localized message naming the owner that already
    ///     holds the combination, or describing the external failure. The conflict is
    ///     logged here once, so no failed registration stays silent.
    /// </returns>
    public string? TryRegister(HotKey hotKey, HotkeyOwner owner)
    {
        string conflict;
        // Invariant: exactly one owner per combination, decided atomically. The
        // ownership check, the adapter call and the owner recording share one
        // critical section, so two systems claiming the same combination
        // concurrently cannot both succeed (the adapter would otherwise re-base
        // the native id under the second claim, leaving both believing they own
        // it). Holding the lock across RegisterHotKey is safe: the adapter
        // dispatches to its own STA thread, whose subscribers (theme repaints,
        // session-unlock profile switching) never call back into the registry,
        // and registrations are rare — startup and explicit user changes — so
        // serializing them costs nothing measurable.
        lock (_lock)
        {
            if (_owners.TryGetValue(hotKey, out var current) && !current.Equals(owner))
            {
                conflict = ConflictMessage(hotKey, current);
                Log.Warning("{HotkeyConflict}", conflict);
                return conflict;
            }

            if (!WindowsAPIAdapter.RegisterHotKey(hotKey))
            {
                conflict = string.Format(SettingsStrings.hotkey_conflict_external, hotKey);
                Log.Warning("{HotkeyConflict}", conflict);
                return conflict;
            }

            _owners[hotKey] = owner;
        }

        return null;
    }

    /// <summary>
    ///     Unbind a hotkey combination owned by the given owner. A combination held by
    ///     a different owner is left untouched — nothing of the caller's is bound —
    ///     and so is one that never registered through this registry (e.g. rejected
    ///     at startup); both return true, meaning "you hold nothing to unbind".
    /// </summary>
    /// <returns>
    ///     False only when the caller's own binding couldn't be removed; the
    ///     ownership record then stays, keeping the registry truthful.
    /// </returns>
    public bool Unregister(HotKey hotKey, HotkeyOwner owner)
    {
        lock (_lock)
        {
            if (!_owners.TryGetValue(hotKey, out var current))
            {
                return true;
            }

            if (!current.Equals(owner))
            {
                // Another SoundSwitch system owns the combination: leave its binding alone.
                Log.Debug("Leaving hotkey {HotKey} registered for owner {Owner}", hotKey, current);
                return true;
            }

            if (WindowsAPIAdapter.UnRegisterHotKey(hotKey))
            {
                _owners.Remove(hotKey);
                return true;
            }

            // The native unregistration failed. When the adapter still tracks the
            // binding it may still be live, so the owner stays recorded and later
            // conflict checks stay truthful; a binding that is already gone at the
            // OS level is forgotten instead of immortalizing an owner that holds
            // nothing.
            if (!WindowsAPIAdapter.IsHotKeyRegistered(hotKey))
            {
                _owners.Remove(hotKey);
            }
            else
            {
                Log.Warning("Couldn't unregister hotkey {HotKey}; keeping its ownership record", hotKey);
            }

            return false;
        }
    }

    /// <summary>
    ///     Rewrite the owner recorded for a hotkey without touching the OS registration.
    ///     Used when the recorded representative of a shared profile hotkey changes
    ///     (e.g. the first claiming profile was deleted while others still use the
    ///     combination), so conflict messages keep naming a profile the user can
    ///     still find. A combination with no record is left alone.
    /// </summary>
    public void UpdateOwner(HotKey hotKey, HotkeyOwner owner)
    {
        lock (_lock)
        {
            if (_owners.ContainsKey(hotKey))
            {
                _owners[hotKey] = owner;
            }
        }
    }

    /// <summary>
    ///     Localized message naming the owner that already holds the combination,
    ///     or null when no SoundSwitch system owns it.
    /// </summary>
    public string? DescribeConflict(HotKey hotKey)
    {
        lock (_lock)
        {
            return _owners.TryGetValue(hotKey, out var owner) ? ConflictMessage(hotKey, owner) : null;
        }
    }

    private static string ConflictMessage(HotKey hotKey, HotkeyOwner owner)
    {
        var ownerName = owner.Kind switch
        {
            HotkeyOwnerKind.PlaybackSwitch => SettingsStrings.hotkey_conflict_owner_playback,
            HotkeyOwnerKind.RecordingSwitch => SettingsStrings.hotkey_conflict_owner_recording,
            HotkeyOwnerKind.Mute => SettingsStrings.hotkey_conflict_owner_mute,
            HotkeyOwnerKind.Profile => string.Format(SettingsStrings.hotkey_conflict_owner_profile, owner.Name),
            HotkeyOwnerKind.AppRule => string.Format(SettingsStrings.hotkey_conflict_owner_appRule, owner.Name),
            _ => throw new ArgumentOutOfRangeException(nameof(owner), owner.Kind, null)
        };

        return string.Format(SettingsStrings.hotkey_conflict, hotKey, ownerName);
    }
}
