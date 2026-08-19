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
///   - Every Track* method gates on AppConfigs.Configuration.Telemetry directly —
///     the setting is the single source of truth, owned by AppModel and persisted
///     to the user's config file. There is no local cache to keep in sync.
/// </summary>
public static class TelemetryService
{
    /// <summary>
    /// Build the attribute list for a metric call.
    /// Returns IEnumerable (lazy) instead of List to avoid an allocation per call;
    /// the Sentry SDK enumerates it once internally.
    /// </summary>
    private static IEnumerable<KeyValuePair<string, object>> Attributes(params (string Key, object Value)[] tags) =>
        tags.Select(t => new KeyValuePair<string, object>(t.Key, t.Value));

    // ── Core switching ──────────────────────────────────────────────

    public static void TrackPlaybackSwitch(string trigger)
    {
        if (!AppConfigs.Configuration.Telemetry) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.playback.switched", 1,
            Attributes(("trigger", trigger)), null);
    }

    public static void TrackRecordingSwitch(string trigger)
    {
        if (!AppConfigs.Configuration.Telemetry) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.recording.switched", 1,
            Attributes(("trigger", trigger)), null);
    }

    public static void TrackMicMute(string trigger, bool muted)
    {
        if (!AppConfigs.Configuration.Telemetry) return;
        SentrySdk.Metrics.EmitCounter(muted ? "soundswitch.mic.muted" : "soundswitch.mic.unmuted", 1,
            Attributes(("trigger", trigger)), null);
    }

    // ── Profiles ────────────────────────────────────────────────────

    /// <summary>
    /// Hash the profile name to an 8-char hex so we can count activations
    /// per profile without sending the actual name.
    ///
    /// SHA256 of a short profile name is cheap enough to recompute on every
    /// call — the cost is well below the noise floor of UI/event processing,
    /// so a cache is not warranted. Recomputing also avoids the memory and
    /// contention overhead of maintaining a dictionary.
    /// </summary>
    private static string ProfileHash(string name)
    {
        if (string.IsNullOrEmpty(name)) return "unknown";

        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(name));
        return Convert.ToHexString(hash).Substring(0, 8).ToLowerInvariant();
    }

    public static void TrackProfileActivated(TriggerFactory.Enum triggerType, string profileName)
    {
        if (!AppConfigs.Configuration.Telemetry) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.profile.activated", 1,
            Attributes(("trigger_type", triggerType.ToString()), ("profile_id", ProfileHash(profileName))), null);
    }

    public static void TrackProfileCreated()
    {
        if (!AppConfigs.Configuration.Telemetry) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.profile.created", 1);
    }

    public static void TrackProfileDeleted()
    {
        if (!AppConfigs.Configuration.Telemetry) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.profile.deleted", 1);
    }

    public static void TrackProfileActivationFailed(string reason)
    {
        if (!AppConfigs.Configuration.Telemetry) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.profile.activation_failed", 1,
            Attributes(("reason", reason)), null);
    }

    // ── Notifications ───────────────────────────────────────────────

    public static void TrackNotificationBanner(string action)
    {
        if (!AppConfigs.Configuration.Telemetry) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.notification.banner", 1,
            Attributes(("action", action)), null);
    }

    public static void TrackNotificationWindows()
    {
        if (!AppConfigs.Configuration.Telemetry) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.notification.windows_shown", 1);
    }

    public static void TrackNotificationSound()
    {
        if (!AppConfigs.Configuration.Telemetry) return;
        SentrySdk.Metrics.EmitCounter("soundswitch.notification.sound_played", 1);
    }

    // ── System ──────────────────────────────────────────────────────

    public static void TrackDevicesEnumerated(string deviceType, int count)
    {
        if (!AppConfigs.Configuration.Telemetry) return;
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
        if (!AppConfigs.Configuration.Telemetry) return;
        SentrySdk.AddBreadcrumb(message, category, null, null, BreadcrumbLevel.Info);
    }
}
