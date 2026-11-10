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
using System.Drawing;
using System.IO;
using System.Linq;

using SoundSwitch.Audio.Manager.Interop.Enum;

using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.Banner.BannerPosition;
using SoundSwitch.Framework.Banner.BannerPosition.Position;
using SoundSwitch.Framework.Banner.MicrophoneMute;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.NotificationManager.Notification;

internal class NotificationBanner : INotification
{
    public NotificationType TypeEnum => NotificationType.BannerNotification;
    public string Label => SettingsStrings.notification_option_banner;

    public INotificationConfiguration Configuration { get; set; }
    private readonly BannerManager _bannerManager = new();
    private readonly BannerPositionFactory _bannerPositionFactory = new();
    private readonly MicrophoneMuteBannerManager _microphoneMuteBannerManager = new();

    internal void ClearPersistentMuteBanners() => _microphoneMuteBannerManager.ClearPersistentMuteBanners();

    /// <summary>
    /// Reconciles the persistent mute banners with the current mute target and banner scope:
    /// banners for microphones that left the scope are dismissed and in-scope microphones
    /// whose current state uses Persistent get their banner back.
    /// </summary>
    /// <param name="inScopeMicrophoneIds">Identifiers of the microphones that remain in scope</param>
    /// <param name="inScopeMicrophones">Current states of in-scope microphones</param>
    internal void SyncMuteBanners(IReadOnlyCollection<string> inScopeMicrophoneIds,
        IReadOnlyCollection<(string Id, string Name, bool IsMuted)> inScopeMicrophones)
    {
        var persistentMicrophones = new List<(string Id, string Name, bool IsMuted)>();
        foreach (var microphone in inScopeMicrophones)
        {
            if (!inScopeMicrophoneIds.Contains(microphone.Id, StringComparer.OrdinalIgnoreCase)) continue;

            var mode = GetMicrophoneMode(microphone.IsMuted);
            if (mode != MicrophoneMute.Fading)
                _bannerManager.RemoveMicrophoneNotifications(microphone.Id);
            if (mode == MicrophoneMute.Persistent)
                persistentMicrophones.Add(microphone);
        }

        _microphoneMuteBannerManager.RetainPersistentMuteBanners(persistentMicrophones.Select(mic => mic.Id).ToArray());
        foreach (var (id, name, isMuted) in persistentMicrophones)
            _microphoneMuteBannerManager.UpdateMicrophoneMuteState(id, name, isMuted);
    }

    private IPosition BannerPosition => _bannerPositionFactory.Get(Configuration.BannerPosition);

    /// <summary>
    /// Creates banner data pre-filled with the shared notification configuration
    /// (position, TTL, opacity, display mode).
    /// </summary>
    private BannerData CreateBannerData() => new()
    {
        Position = BannerPosition,
        Ttl = Configuration.Ttl,
        Opacity = Configuration.Opacity,
        DisplayInfo = Configuration.DisplayInfo
    };

    public void NotifyDefaultChanged(DeviceFullInfo audioDevice)
    {
        var content = NotificationContentBuilder.BuildDefaultChanged(audioDevice, NotificationContentBuilder.DeviceChangeWording.Banner);
        var toastData = CreateBannerData();
        toastData.Image = content.Image;
        toastData.Title = content.Title;
        toastData.Text = content.Text;
        if (CustomSoundCheck(audioDevice))
        {
            toastData.SoundFile = Configuration.CustomSound;
            toastData.CurrentDeviceId = audioDevice.Id;
        }

        _bannerManager.ShowNotification(toastData);
    }

    public void NotifyProfileChanged(Profile.Profile profile, Bitmap icon, uint? processId)
    {
        var content = NotificationContentBuilder.BuildProfileChanged(profile, icon);
        var bannerData = CreateBannerData();
        bannerData.Priority = content.Priority;
        bannerData.Image = content.Image;
        bannerData.Title = content.Title;
        bannerData.Text = content.Text;
        _bannerManager.ShowNotification(bannerData);
    }

    public void NotifyAppRuleMatched(AppSoundRule rule, DeviceFullInfo playback, DeviceFullInfo recording, Bitmap icon, uint processId)
    {
        var content = NotificationContentBuilder.BuildAppRuleMatched(playback, recording, icon);
        var bannerData = CreateBannerData();
        bannerData.Priority = content.Priority;
        bannerData.Image = content.Image;
        bannerData.Title = content.Title;
        bannerData.Text = content.Text;
        _bannerManager.ShowNotification(bannerData);
    }

    public void NotifyAppRuleNoProcess(AppSoundRule rule)
    {
        var content = NotificationContentBuilder.BuildAppRuleNoProcess();
        var bannerData = CreateBannerData();
        bannerData.Priority = content.Priority;
        bannerData.Image = content.Image;
        bannerData.Title = content.Title;
        bannerData.Text = content.Text;
        _bannerManager.ShowNotification(bannerData);
    }

    public void NotifyMicrophoneMuteChanged(string deviceId, string microphoneName, bool newMuteState)
    {
        var microphoneMuteBanner = GetMicrophoneMode(newMuteState);

        if (microphoneMuteBanner != MicrophoneMute.Fading)
            _bannerManager.RemoveMicrophoneNotifications(deviceId);
        if (microphoneMuteBanner != MicrophoneMute.Persistent)
            _microphoneMuteBannerManager.RemovePersistentMuteBanner(deviceId);

        switch (microphoneMuteBanner)
        {
            case MicrophoneMute.Persistent:
                _microphoneMuteBannerManager.UpdateMicrophoneMuteState(deviceId, microphoneName, newMuteState);
                return;
            case MicrophoneMute.Fading:
                FadingBanner(deviceId, microphoneName, newMuteState);
                return;
            case MicrophoneMute.None:
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private MicrophoneMute GetMicrophoneMode(bool isMuted) =>
        isMuted ? Configuration.MicrophoneMuteBanner : Configuration.MicrophoneUnmuteBanner;

    private void FadingBanner(string deviceId, string microphoneName, bool newMuteState)
    {
        var content = NotificationContentBuilder.BuildMicrophoneBanner(microphoneName, newMuteState);

        var bannerData = CreateBannerData();
        bannerData.MicrophoneId = deviceId;
        bannerData.Priority = content.Priority;
        bannerData.Image = content.Image;
        bannerData.Title = content.Title;
        bannerData.Text = content.Text;
        bannerData.CompactContent = content.CompactContent;
        _bannerManager.ShowNotification(bannerData);
    }

    public bool SupportIcon => true;

    public void OnSoundChanged(CachedSound newSound) => Configuration.CustomSound = newSound;

    public bool SupportCustomSound() => true;

    // Available in all Windows versions
    public bool IsAvailable() => true;

    public bool CustomSoundCheck(DeviceFullInfo audioDevice) =>
        audioDevice.Type == EDataFlow.eRender && Configuration.CustomSound != null && File.Exists(Configuration.CustomSound.FilePath);
}
