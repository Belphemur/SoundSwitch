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
///   - All Track* methods gate on _enabled first (volatile read, no allocation)
///     so a disabled-telemetry call path is a single branch + return.
/// </summary>
public static class TelemetryService
{
    /// <summary>
    /// Sentry DSN used by both the main application and the CLI.
    /// </summary>
    public const string SentryDsn = "https://7d52dfb4f6554bf0b58b256337835332@o631137.ingest.sentry.io/5755327";

    private static volatile bool _enabled;

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
    /// Returns IEnumerable (lazy) instead of List to avoid an allocation per call;
    /// the Sentry SDK enumerates it once internally.
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
