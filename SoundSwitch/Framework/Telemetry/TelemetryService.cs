/********************************************************************
 * Copyright (C) 2015-2017 Antoine Aflalo
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 ********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

using Sentry;

using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.Profile.Trigger;

namespace SoundSwitch.Framework.Telemetry;

/// <summary>
/// Centralized feature-usage telemetry. All calls go through this static class.
/// When AppConfigs.Configuration.Telemetry is false, every method is a no-op.
///
/// Threading / non-blocking design:
///   - SentrySdk.Metrics.Emit* and SentrySdk.AddBreadcrumb are buffered by the
///     Sentry SDK and flushed on its own background transport thread; the public
///     API calls themselves return immediately and do not block the caller.
///   - ProfileHash uses a per-name cache to avoid recomputing SHA256 on the
///     hot path. Cache entries are never evicted (profile names are stable).
///   - All Track* methods gate on _enabled first (volatile read, no allocation)
///     so a disabled-telemetry call path is a single branch + return.
/// </summary>
public static class TelemetryService
{
    /// <summary>
    /// Sentry DSN used by both the main application and the CLI.
    /// </summary>
    public const string SentryDsn = "https://7d52df...5332@o631137.ingest.sentry.io/5755327";

    private static volatile bool _enabled;

    /// <summary>
    /// Cache of already-hashed profile names. Avoids re-hashing the same
    /// profile on every activation. Never evicted — profile names are
    /// stable across the lifetime of an installation.
    /// </summary>
    private static readonly Dictionary<string, string> _profileHashCache = new();

    /// <summary>
    /// Call once at startup and whenever the Telemetry setting changes.
    /// </summary>
    public static void Reload()
    {
        _enabled = AppConfigs.Configuration.Telemetry;
    }

    public static bool IsEnabled() => _enabled;

    /// <summary>
    /// Build the attribute list for a metric call.
    /// Inline instead of a separate method to avoid a separate allocation;
    /// the caller supplies the exact key-value pairs it needs.
    /// </summary>
    private static IEnumerable<KeyValuePair<string, object>> Attributes(params (string Key, object Value)[] tags) =>
        tags.Select(t => new KeyValuePair<string, object>(t.Key, t.Value));

    // ── Core switching ──────────────────────────────────────────────

    public static void TrackPlaybackSwitch(string trigger)
    {
        if (!_enabled) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.playback.switched", 1,
            Attributes(("trigger", trigger)), null);
    }

    public static void TrackRecordingSwitch(string trigger)
    {
        if (!_enabled) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.recording.switched", 1,
            Attributes(("trigger", trigger)), null);
    }

    public static void TrackMicMute(string trigger, bool muted)
    {
        if (!_enabled) return;
        SentrySdk.Metrics.EmitCounter(muted ? "soundswitch.mic.muted" : "soundswitch.mic.unmuted", 1,
            Attributes(("trigger", trigger)), null);
    }

    // ── Profiles ────────────────────────────────────────────────────

    /// <summary>
    /// Hash the profile name to an 8-char hex so we can count activations
    /// per profile without sending the actual name.
    ///
    /// Result is cached per name so repeated activations of the same profile
    /// don't re-compute SHA256. First call for a given name does the
    /// one-time hash; subsequent calls return the cached value.
    /// </summary>
    private static string ProfileHash(string name)
    {
        if (string.IsNullOrEmpty(name)) return "unknown";

        if (_profileHashCache.TryGetValue(name, out var cached))
            return cached;

        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(name));
        var result = Convert.ToHexString(hash).Substring(0, 8).ToLowerInvariant();
        lock (_profileHashCache)
        {
            _profileHashCache[name] = result;
        }
        return result;
    }

    public static void TrackProfileActivated(TriggerFactory.Enum triggerType, string profileName)
    {
        if (!_enabled) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.profile.activated", 1,
            Attributes(("trigger_type", triggerType.ToString()), ("profile_id", ProfileHash(profileName))), null);
    }

    public static void TrackProfileCreated()
    {
        if (!_enabled) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.profile.created", 1);
    }

    public static void TrackProfileDeleted()
    {
        if (!_enabled) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.profile.deleted", 1);
    }

    public static void TrackProfileActivationFailed(string reason)
    {
        if (!_enabled) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.profile.activation_failed", 1,
            Attributes(("reason", reason)), null);
    }

    // ── Notifications ───────────────────────────────────────────────

    public static void TrackNotificationBanner(string action)
    {
        if (!_enabled) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.notification.banner", 1,
            Attributes(("action", action)), null);
    }

    public static void TrackNotificationWindows()
    {
        if (!_enabled) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.notification.windows_shown", 1);
    }

    public static void TrackNotificationSound()
    {
        if (!_enabled) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.notification.sound_played", 1);
    }

    // ── CLI ──────────────────────────────────────────────────────────

    public static void TrackCliCommand(string command, int exitCode)
    {
        if (!_enabled) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.cli.command", 1,
            Attributes(("command", command), ("exit_code", exitCode.ToString())), null);
    }

    // ── System ──────────────────────────────────────────────────────

    public static void TrackDevicesEnumerated(string deviceType, int count)
    {
        if (!_enabled) return;
        SentrySdk.Metrics.EmitDistribution("soundswitch.devices.count", count, MeasurementUnit.None,
            Attributes(("device_type", deviceType)), null);
    }

    // ── Breadcrumbs ─────────────────────────────────────────────────

    /// <summary>
    /// Add a breadcrumb. SentrySdk.AddBreadcrumb is buffered by the SDK and
    /// does not block the caller — it pushes to an in-memory queue that the
    /// transport thread drains asynchronously.
    /// </summary>
    public static void AddBreadcrumb(string category, string message)
    {
        if (!_enabled) return;
        SentrySdk.AddBreadcrumb(message, category, null, null, BreadcrumbLevel.Info);
    }
}
