/********************************************************************
 * Copyright (C) 2015-2024 Antoine Aflalo
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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Serilog;

namespace SoundSwitch.Framework.Banner;

/// <summary>
/// Renders a <see cref="BannerData"/> as a Windows Toast notification
/// (app notification). Unlike a Win32 overlay window, the toast is
/// composited by the Windows notification surface and does NOT send
/// WM_ACTIVATEAPP to any foreground application — making it safe to
/// display over exclusive fullscreen games.
/// </summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows10.0.17763.0")]
internal static class ToastBannerAdapter
{
    // SoundSwitch's registered AppUserModelID.
    // Must match the value set by the installer (NSIS/Inno sets this on the
    // Start Menu shortcut). If SoundSwitch is not installed (dev/portable),
    // we fall back to a generic ID — Windows will still show the toast but
    // may use a generic icon.
    private const string AppId = "aaflalo.SoundSwitch.Application";

    /// <summary>
    /// Ensures that SoundSwitch is registered as a toast notifier AppId in the registry.
    /// This is a fallback/helper for development or portable scenarios so that standard
    /// toasts display correctly even if the installer's Start Menu shortcut isn't active.
    /// </summary>
    public static void EnsureRegistered()
    {
        try
        {
            const string regPath = @"SOFTWARE\Classes\AppUserModelId\" + AppId;

            using var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(regPath);
            if (key != null)
            {
                key.SetValue("DisplayName", "SoundSwitch");
                var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue("IconUri", exePath);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to ensure toast registration in registry");
        }
    }

    /// <summary>
    /// Shows a Windows Toast notification built from the provided
    /// <see cref="BannerData"/>. Safe to call from any thread.
    /// </summary>
    public static void Show(BannerData data)
    {
        try
        {
            var xml = BuildXml(data);
            var toast = new ToastNotification(xml);

            // Mirror the BannerData TTL as the toast expiry.
            // Expired toasts are removed from Action Center automatically.
            if (data.Ttl != TimeSpan.MaxValue)
                toast.ExpirationTime = DateTimeOffset.Now + data.Ttl;

            // Suppress the pop-up but still add to Action Center when TTL
            // is very short (< 4 s) — avoids a flash the user can't read.
            // For normal TTLs, show the pop-up banner.
            toast.SuppressPopup = data.Ttl < TimeSpan.FromSeconds(4);

            ToastNotificationManager
                .CreateToastNotifier(AppId)
                .Show(toast);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "ToastBannerAdapter failed to show notification for '{Title}'", data.Title);
        }
    }

    // -------------------------------------------------------------------------
    // XML builder
    // -------------------------------------------------------------------------

    private static XmlDocument BuildXml(BannerData data)
    {
        // Save the Image to a temp PNG so the toast can reference it via
        // a file:// URI. System.Drawing.Image cannot be passed directly to
        // the WinRT toast XML; it requires a file path or ms-appx URI.
        string? imagePath = SaveImageToTemp(data.Image);

        // Build the toast XML by hand — no dependency on
        // Microsoft.Toolkit.Uwp.Notifications needed for net10.0-windows.
        //
        // Schema: https://learn.microsoft.com/en-us/uwp/schemas/tiles/toastschema/schema-root
        var imageElement = imagePath != null
            ? $"""<image placement="appLogoOverride" hint-crop="circle" src="file:///{Uri.EscapeDataString(imagePath).Replace("%2F", "/").Replace("%5C", "/")}" />"""
            : string.Empty;

        var xml = $"""
            <toast>
              <visual>
                <binding template="ToastGeneric">
                  {imageElement}
                  <text>{EscapeXml(data.Title)}</text>
                  <text>{EscapeXml(data.Text)}</text>
                </binding>
              </visual>
            </toast>
            """;

        var doc = new XmlDocument();
        doc.LoadXml(xml);
        return doc;
    }

    /// <summary>
    /// Saves a <see cref="System.Drawing.Image"/> to a temp file and returns
    /// the full path, or <c>null</c> if the image is null or saving fails.
    /// Files are written to the user's temp folder and are cleaned up by the
    /// OS on reboot. For long-running apps you may want a more targeted
    /// cleanup strategy, but for a ~2–5 s banner lifetime this is fine.
    /// </summary>
    private static string? SaveImageToTemp(Image? image)
    {
        if (image == null)
            return null;

        try
        {
            // Use a stable hash of the image reference so the same Image
            // object reuses the same temp file across repeated notifications.
            var path = Path.Combine(
                Path.GetTempPath(),
                $"SoundSwitch_toast_{image.GetHashCode():X8}.png");

            if (!File.Exists(path))
                image.Save(path, ImageFormat.Png);

            return path;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "ToastBannerAdapter could not save image to temp");
            return null;
        }
    }

    private static string EscapeXml(string? text) =>
        (text ?? string.Empty)
            .Replace("&",  "&amp;")
            .Replace("<",  "&lt;")
            .Replace(">",  "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'",  "&apos;");
}
