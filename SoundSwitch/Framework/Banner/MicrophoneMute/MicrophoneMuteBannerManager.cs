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

using System.Collections.Generic;
using SoundSwitch.Banner;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.Banner.MicrophoneMute;

public class MicrophoneMuteBannerManager
{
    public static void Setup() { }

    private readonly Dictionary<string, BannerForm> _persistentBanners = new();
    private readonly BannerService _bannerService = new(new BannerConfigurationBridge(), new BannerAudioServiceBridge());

    public void UpdateMicrophoneMuteState(string deviceId, string microphoneName, bool isMuted)
    {
        if (_persistentBanners.TryGetValue(deviceId, out var existingBanner))
        {
            UpdateBanner(existingBanner, microphoneName, isMuted);
            return;
        }

        var request = CreateRequest(microphoneName, isMuted);
        var banner = _bannerService.Show(request, (BannerPosition)AppModel.Instance.BannerPosition, true);
        if (banner != null)
        {
            _persistentBanners[deviceId] = banner;
            banner.Disposed += (s, e) => _persistentBanners.Remove(deviceId);
        }
    }

    private BannerRequest CreateRequest(string microphoneName, bool isMuted)
    {
        return new BannerRequest
        {
            Priority = 2,
            Image = isMuted ? Resources.microphone_muted : Resources.microphone_unmuted,
            Title = isMuted ?
                string.Format(SettingsStrings.notification_microphone_muted, microphoneName) :
                string.Format(SettingsStrings.notification_microphone_unmuted, microphoneName),
            CompactMode = false
        };
    }

    private void UpdateBanner(BannerForm banner, string microphoneName, bool isMuted)
    {
        var request = CreateRequest(microphoneName, isMuted);
        banner.SetData(request, persistent: false);
    }

    public void RemovePersistentMuteBanner(string deviceId)
    {
        if (_persistentBanners.TryGetValue(deviceId, out var banner))
        {
            banner.Close();
        }
    }
}
