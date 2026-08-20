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

using SoundSwitch.Framework.Telemetry;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.Banner;

/// <summary>
/// Class to manage the banners. This class is the entrypoint to show a notification banner.
/// </summary>
public class BannerManager
{
    private static System.Threading.SynchronizationContext _syncContext;
    private readonly Dictionary<Guid, BannerForm> _bannerForms = new();
    private BannerForm _singleBanner;
    private BannerForm _customPositionBanner;
    private string _lastSingleBannerText;
    private DateTime _lastSingleBannerTimeUtc;
    private const int SingleBannerCoalesceMs = 500;
    private const int SPACING = 10;

    /// <summary>
    /// Show a banner notification with the given data
    /// </summary>
    /// <param name="data"></param>
    public void ShowNotification(BannerData data)
    {
        TelemetryService.TrackNotificationBanner("shown");
        TelemetryService.AddBreadcrumb("notification", "Banner shown");

        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763) && ExclusiveFullscreenDetector.IsForegroundInExclusiveFullscreen())
        {
            Log.Debug("Foreground window is in exclusive fullscreen — routing to Toast notification");
            ToastBannerAdapter.Show(data);
            return;
        }

        if (data.CustomPositionMode)
        {
            ShowOrUpdateCustomPositionNotification(data);
            return;
        }

        if (AppModel.Instance.MaxNumberNotification > 1)
        {
            MultipleBannerShow(data);
            return;
        }

        // Single-banner mode reuses one form. Coalesce rapid notifications for the
        // same device (e.g. "Switch default communication device" raises console,
        // multimedia and communications role changes for the same device in quick
        // succession) so only one banner is shown. See issue #2355.
        if (_lastSingleBannerText != null && _lastSingleBannerText == data.Text &&
            (DateTime.UtcNow - _lastSingleBannerTimeUtc).TotalMilliseconds < SingleBannerCoalesceMs)
        {
            return;
        }

        _lastSingleBannerText = data.Text;
        _lastSingleBannerTimeUtc = DateTime.UtcNow;

        _syncContext.Send(_ =>
        {
            if (_singleBanner == null)
            {
                _singleBanner = new BannerForm();
                _singleBanner.Disposed += (s, e) =>
                {
                    _singleBanner = null;
                    _lastSingleBannerText = null;
                };
            }

            _singleBanner.SetData(data);
        }, null);
    }

    private void ShowOrUpdateCustomPositionNotification(BannerData data)
    {
        _syncContext.Send(_ =>
        {
            if (_customPositionBanner == null)
            {
                _customPositionBanner = new BannerForm();
                _customPositionBanner.Disposed += (s, e) => { _customPositionBanner = null; };
            }

            _customPositionBanner.SetData(data);
        }, null);
    }

    private void MultipleBannerShow(BannerData data)
    {
        // Execute the banner in the context of the UI thread
        _syncContext.Send(_ =>
        {
            var banner = new BannerForm();
            banner.Disposed += (s, e) => { _bannerForms.Remove(banner.Id); };
            banner.SetData(data);
            foreach (var bannerForm in _bannerForms.Values)
            {
                bannerForm.UpdateLocationOpacity(banner.Height + SPACING,  0.10, 100/AppModel.Instance.MaxNumberNotification);
            }

            _bannerForms.Add(banner.Id, banner);
        }, null);
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
            throw new InvalidOperationException("BannerManager must be called in the context of the UI thread.");
        Log.Information("Banner manager initialized");

        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            ToastBannerAdapter.EnsureRegistered();
        }
    }
}
