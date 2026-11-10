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

using Serilog;

using SoundSwitch.Framework.NotificationManager.Notification;
using SoundSwitch.Framework.Telemetry;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.Banner.MicrophoneMute;

/// <summary>
/// Specialized banner manager for microphone mute notifications.
/// Tracks one persistent state banner per microphone, whether muted or unmuted.
/// </summary>
public class MicrophoneMuteBannerManager
{
    private static System.Threading.SynchronizationContext _syncContext;
    private readonly Dictionary<string, BannerForm> _activeBanners = new();
    private const int SPACING = 10;

    /// <summary>
    /// Whether the UI-thread synchronization context captured by <see cref="Setup"/> is
    /// available. <see cref="Setup"/> runs on the UI thread during application start; mute
    /// events arrive on audio threads and can fire before it, in which case banners must
    /// simply be skipped instead of crashing the caller.
    /// </summary>
    private static bool BannerContextUnavailable => _syncContext == null;

    /// <summary>
    /// Runs a complete microphone banner operation on the UI thread, including state lookup.
    /// </summary>
    internal void RunOnUiThread(Action action)
    {
        if (BannerContextUnavailable)
        {
            Log.Warning("Microphone mute banner manager not initialized yet; ignoring banner operation");
            return;
        }

        if (System.Threading.SynchronizationContext.Current == _syncContext)
            action();
        else
            _syncContext.Send(_ => action(), null);
    }

    /// <summary>
    /// Updates or creates a banner when a microphone's mute state changes
    /// </summary>
    /// <param name="microphoneId">Unique identifier for the microphone</param>
    /// <param name="microphoneName">User-friendly name of the microphone</param>
    /// <param name="isMuted">True if the microphone is now muted, false if unmuted</param>
    public void UpdateMicrophoneMuteState(string microphoneId, string microphoneName, bool isMuted)
    {
        if (BannerContextUnavailable)
        {
            Log.Warning("Microphone mute banner manager not initialized yet; ignoring mute change of {DeviceName} ({DeviceId}) to {MuteState}",
                microphoneName, microphoneId, isMuted);
            return;
        }

        _syncContext.Send(_ => ShowStateBanner(microphoneId, microphoneName, isMuted), null);
    }

    /// <summary>
    /// Shows the current microphone state until it is replaced or leaves scope.
    /// </summary>
    /// <param name="microphoneId">Unique identifier for the microphone</param>
    /// <param name="microphoneName">User-friendly name of the microphone</param>
    private void ShowStateBanner(string microphoneId, string microphoneName, bool isMuted)
    {
        var data = NotificationContentBuilder.BuildMicrophoneBanner(microphoneName, isMuted);
        data.Priority = 3;
        data.Position = AppModel.Instance.BannerPositionImpl;
        data.Ttl = TimeSpan.MaxValue;
        data.Opacity = AppModel.Instance.BannerOpacityPercentage;
        data.DisplayInfo = AppModel.Instance.BannerDisplayInfo;
        data.OnClick = (sender, args) =>
        {
            if (isMuted)
                TelemetryService.TrackNotificationBanner("unmute_clicked");
            var result = AppModel.Instance.SetMicrophoneMuteState(microphoneId, !isMuted);
            if (result != null)
            {
                TelemetryService.TrackMicMute("banner", !isMuted);
            }
        };

        if (_activeBanners.TryGetValue(microphoneId, out var existingBanner))
        {
            // Update existing banner
            existingBanner.SetData(data);
        }
        else
        {
            // Create new banner
            var newBanner = new BannerForm();
            newBanner.SetData(data);
            // Arrange existing banners vertically
            TrackBanner(microphoneId, newBanner);
            RearrangeBanners();
        }
    }

    /// <summary>
    /// Tracks the banner for a microphone and clears the tracked reference when the form is
    /// disposed, whatever path disposes it (manager call, fade-out after a click, ...). Without
    /// this, a form dying outside the manager would leave a stale disposed entry behind and the
    /// next rearrange pass would dereference it (Sentry SOUNDSWITCH-40H). Surviving banners are
    /// pulled up so the vacated spot does not leave a gap.
    /// </summary>
    private void TrackBanner(string microphoneId, BannerForm banner)
    {
        _activeBanners[microphoneId] = banner;
        banner.SizeChanged += (_, _) => RearrangeBanners();
        banner.Disposed += (_, _) =>
        {
            // Only clear the entry while it still points at this banner.
            if (!_activeBanners.TryGetValue(microphoneId, out var tracked) || !ReferenceEquals(tracked, banner))
                return;

            _activeBanners.Remove(microphoneId);
            // The form died on its own, without going through the manager: run a layout
            // pass so the surviving banners move up into the vacated spot.
            RearrangeBanners();
        };
    }

    /// <summary>
    /// Removes the persistent state banner for a microphone.
    /// </summary>
    /// <param name="microphoneId">Unique identifier for the microphone</param>
    public void RemovePersistentMuteBanner(string microphoneId)
    {
        // Live mute events arrive on an audio thread while the tracked banners are only
        // mutated on the UI thread, so marshal the mutation to keep _activeBanners safe.
        if (BannerContextUnavailable)
        {
            Log.Warning("Microphone mute banner manager not initialized yet; ignoring persistent banner removal of {DeviceId}",
                microphoneId);
            return;
        }

        _syncContext.Send(_ =>
        {
            if (!_activeBanners.TryGetValue(microphoneId, out var existingBanner)) return;
            _activeBanners.Remove(microphoneId);
            existingBanner.Dispose();
            RearrangeBanners();
        }, null);
    }

    /// <summary>
    /// Dismisses persistent mute banners for microphones that are no longer in scope,
    /// keeping the ones that remain relevant.
    /// </summary>
    /// <param name="inScopeMicrophoneIds">Identifiers of the microphones that may keep a banner</param>
    public void RetainPersistentMuteBanners(IReadOnlyCollection<string> inScopeMicrophoneIds)
    {
        if (BannerContextUnavailable)
        {
            Log.Warning("Microphone mute banner manager not initialized yet; ignoring persistent banner reconciliation");
            return;
        }

        _syncContext.Send(_ =>
        {
            var inScope = new HashSet<string>(inScopeMicrophoneIds, StringComparer.OrdinalIgnoreCase);
            foreach (var microphoneId in _activeBanners.Keys.Where(id => !inScope.Contains(id)).ToArray())
            {
                _activeBanners.Remove(microphoneId, out var banner);
                banner?.Dispose();
            }

            RearrangeBanners();
        }, null);
    }

    /// <summary>
    /// Dismisses all persistent mute banners.
    /// </summary>
    public void ClearPersistentMuteBanners() => RetainPersistentMuteBanners(Array.Empty<string>());

    /// <summary>
    /// Rearranges all active banners vertically
    /// </summary>
    private void RearrangeBanners()
    {
        var offset = 0;

        // Snapshot the tracked banners before iterating: a rearrange pass can be triggered from
        // a banner's Disposed event (asynchronous fade-out after a click), so the dictionary can
        // change between the moment this pass is scheduled and its execution.
        foreach (var (microphoneId, banner) in _activeBanners.ToArray())
        {
            // Expected stale-banner condition: the form died without going through the manager.
            // Drop the tracked reference instead of dereferencing it; not an error.
            if (banner == null || banner.IsDisposed)
            {
                Log.Debug("Rearranging mute banners: dropping stale banner for {MicrophoneId}", microphoneId);
                // Identity-checked: the dictionary may have changed while this pass ran.
                if (_activeBanners.TryGetValue(microphoneId, out var tracked) && ReferenceEquals(tracked, banner))
                    _activeBanners.Remove(microphoneId);
                continue;
            }

            banner.UpdatePosition(offset);
            offset += banner.Height + SPACING;
        }
    }

    /// <summary>
    /// Because notifications dispatched asynchronously, this method must be called in the context of the UI thread
    /// <remarks>This method requires that at least one System.Windows.Form.Control has been created or Application.Run() called</remarks>
    /// </summary>
    internal static void Setup()
    {
        // Grab the synchronization context of the UI thread!
        _syncContext = System.Threading.SynchronizationContext.Current;
        if (!(_syncContext is System.Windows.Forms.WindowsFormsSynchronizationContext))
            throw new InvalidOperationException("MicrophoneMuteBannerManager must be called in the context of the UI thread.");
        Log.Information("Microphone mute banner manager initialized");
    }
}
