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
using System.Drawing;
using System.Resources;
using Serilog;
using SoundSwitch.Common.Framework.Icon;

namespace SoundSwitch.Framework.TrayIcon;

internal static class ThemeIcons
{
    private static readonly ResourceManager ResourceLoader =
        new("SoundSwitch.Common.Properties.Resources", typeof(IconExtractor).Assembly);

    private static readonly object LoadLock = new();
    private static readonly object FallbackLock = new();
    private static IconHandle[] _blackIcons;
    private static IconHandle[] _whiteIcons;
    private static IconHandle _applicationFallback;
    private static IconHandle _informationFallback;

    /// <summary>
    /// Returns a permanent <see cref="IconHandle"/> for the requested form factor and taskbar theme.
    /// The handle is application-lifetime and must NOT be disposed by callers; it is owned by the
    /// permanent cache. <see cref="TrayIcon.ReplaceIcon"/> will clone the underlying <see cref="Icon"/>
    /// internally, so callers should not clone again.
    /// </summary>
    public static IconHandle GetIcon(IconKind kind, bool isDarkTaskbar)
    {
        try
        {
            var icons = isDarkTaskbar ? _whiteIcons : _blackIcons;
            if (icons == null)
                icons = LoadIcons(isDarkTaskbar);

            var handle = icons[(int) kind];
            if (handle != null)
                return handle;
        }
        catch (Exception e)
        {
            Log.Error(e, "Can't load theme tray icon for {Kind}, using system icon fallback", kind);
        }

        return GetFallbackIcon(kind);
    }

    private static IconHandle[] LoadIcons(bool isDarkTaskbar)
    {
        lock (LoadLock)
        {
            if (isDarkTaskbar)
            {
                _whiteIcons ??= CreateIcons(isDarkTaskbar);
                return _whiteIcons;
            }

            _blackIcons ??= CreateIcons(isDarkTaskbar);
            return _blackIcons;
        }
    }

    private static IconHandle[] CreateIcons(bool isDarkTaskbar)
    {
        var suffix = isDarkTaskbar ? "White" : "";
        var resourceNames = new[]
        {
            "themeIconSpeaker" + suffix,
            "themeIconHeadphone" + suffix,
            "themeIconHeadset" + suffix,
            "themeIconMicrophone" + suffix,
        };

        var handles = new IconHandle[resourceNames.Length];
        for (var i = 0; i < resourceNames.Length; i++)
        {
            handles[i] = CreatePermanentIcon(resourceNames[i]);
        }

        return handles;
    }

    private static IconHandle CreatePermanentIcon(string resourceName)
    {
        try
        {
            return IconExtractor.CreatePermanent((Icon) ResourceLoader.GetObject(resourceName));
        }
        catch (Exception e)
        {
            Log.Error(e, "Can't load bundled theme icon {ResourceName}, using system icon fallback", resourceName);
            return null;
        }
    }

    private static IconHandle GetFallbackIcon(IconKind kind)
    {
        lock (FallbackLock)
        {
            if (kind == IconKind.Microphone)
            {
                _informationFallback ??= IconExtractor.CreatePermanent((Icon) SystemIcons.Information.Clone());
                return _informationFallback;
            }

            _applicationFallback ??= IconExtractor.CreatePermanent((Icon) SystemIcons.Application.Clone());
            return _applicationFallback;
        }
    }
}