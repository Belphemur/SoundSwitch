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
using Serilog;
using SoundSwitch.Framework.Banner.Position;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.Banner
{
    /// <summary>
    /// Specialized banner manager for microphone mute notifications.
    /// Displays persistent banners for muted microphones and removes them when unmuted.
    /// </summary>
    public class MicrophoneMuteBannerManager
    {
        private static System.Threading.SynchronizationContext _syncContext;
        private readonly Dictionary<string, BannerForm> _activeBanners = new();
        private const int SPACING = 10;
        
        // Remove the local position factory and accessor
        private IPosition BannerPosition => AppModel.Instance.BannerPositionImpl;

        /// <summary>
        /// Updates or creates a banner when a microphone's mute state changes
        /// </summary>
        /// <param name="microphoneId">Unique identifier for the microphone</param>
        /// <param name="microphoneName">User-friendly name of the microphone</param>
        /// <param name="isMuted">True if the microphone is now muted, false if unmuted</param>
        public void UpdateMicrophoneMuteState(string microphoneId, string microphoneName, bool isMuted)
        {
            _syncContext.Send(_ =>
            {
                if (isMuted)
                {
                    // Create or update a persistent banner for the muted microphone
                    ShowMuteBanner(microphoneId, microphoneName);
                }
                else
                {
                    // Show temporary unmute notification and remove the persistent banner
                    ShowTempUnmuteNotification(microphoneId, microphoneName);
                }
            }, null);
        }

        /// <summary>
        /// Shows a persistent banner that indicates a microphone is muted
        /// </summary>
        /// <param name="microphoneId">Unique identifier for the microphone</param>
        /// <param name="microphoneName">User-friendly name of the microphone</param>
        private void ShowMuteBanner(string microphoneId, string microphoneName)
        {
            // Create banner data with very long TTL (persistent)
            var data = new BannerData
            {
                Priority = 3, // Higher than regular notifications
                Image = Resources.MicrophoneOff,
                Text = microphoneName,
                Title = SettingsStrings.microphone_off,
                Position = BannerPosition,
                Ttl = TimeSpan.MaxValue, // Effectively "infinite" until explicitly dismissed
                CompactMode = true,
                OnClick = (sender, args) => AppModel.Instance.SetMicrophoneMuteState(microphoneId, false)
            };

            if (!_activeBanners.TryGetValue(microphoneId, out var existingBanner))
            {
                // Create new banner
                var newBanner = new BannerForm();
                newBanner.SetData(data);
                // Arrange existing banners vertically
                _activeBanners.Add(microphoneId, newBanner);
                RearrangeBanners();
            }
            else
            {
                // Update existing banner
                existingBanner.SetData(data);
            }
        }

        /// <summary>
        /// Shows a temporary unmute notification and removes the persistent mute banner
        /// </summary>
        /// <param name="microphoneId">Unique identifier for the microphone</param>
        /// <param name="microphoneName">User-friendly name of the microphone</param>
        private void ShowTempUnmuteNotification(string microphoneId, string microphoneName)
        {
            // Create unmute notification data
            var unmuteBannerData = new BannerData
            {
                Priority = 3,
                Image = Resources.MicrophoneOn,
                Text = microphoneName,
                Title = SettingsStrings.microphone_on,
                Position = BannerPosition,
                Ttl = TimeSpan.FromSeconds(3),
                CompactMode = true
            };

            // Check if we have an existing banner for this microphone
            if (_activeBanners.TryGetValue(microphoneId, out var existingBanner))
            {
                // Update the existing banner
                existingBanner.SetData(unmuteBannerData);
                
                // Remove from our tracking dictionary
                _activeBanners.Remove(microphoneId);
                
                // Banner will auto-dispose after TTL expires
                existingBanner.Disposed += (s, e) => RearrangeBanners();
            }
            else
            {
                // Create a new temporary banner
                var newBanner = new BannerForm();
                newBanner.SetData(unmuteBannerData);
                newBanner.Disposed += (s, e) => RearrangeBanners();
                
                // No need to track in _activeBanners since it's temporary
                RearrangeBanners();
            }
        }

        /// <summary>
        /// Rearranges all active banners vertically
        /// </summary>
        private void RearrangeBanners()
        {
            var offset = 0;
            
            foreach (var banner in _activeBanners.Values)
            {
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
                throw new InvalidOperationException("MicrophoneMuteManager must be called in the context of the UI thread.");
            Log.Information("Microphone mute banner manager initialized");
        }
    }
}