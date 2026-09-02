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
using System.Text.RegularExpressions;
using Serilog;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Framework.TrayIcon;

internal static class DeviceFormFactorDetector
{
    private static readonly Regex IconPathRegex =
        new(@"mmres\.dll,-(?<index>\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex HeadphoneRegex =
        new(@"\b(headphone|earbud|earphone|airpods|qc\d|wh-\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex HeadsetRegex =
        new(@"\b(headset|game[ ]?com)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly IReadOnlyDictionary<int, IconKind> IconPathMap = new Dictionary<int, IconKind>
    {
        {5004, IconKind.Speaker},
        {5005, IconKind.Headphone},
        {5051, IconKind.Headset},
        {5044, IconKind.Headset},
        {5052, IconKind.Headset},
    };

    private static readonly HashSet<int> SeenUnmappedIndexes = new();

    internal static IconKind From(DeviceFullInfo deviceInfo)
    {
        if (deviceInfo == null)
            return IconKind.Speaker;

        if (deviceInfo.Type == EDataFlow.eCapture)
            return IconKind.Microphone;

        var kind = FromIconPath(deviceInfo.IconPath);
        if (kind.HasValue)
            return kind.Value;

        if (HeadphoneRegex.IsMatch(deviceInfo.NameClean))
            return IconKind.Headphone;

        if (HeadsetRegex.IsMatch(deviceInfo.NameClean))
            return IconKind.Headset;

        return IconKind.Speaker;
    }

    private static IconKind? FromIconPath(string iconPath)
    {
        if (string.IsNullOrEmpty(iconPath))
            return null;

        var match = IconPathRegex.Match(iconPath);
        if (!match.Success)
            return null;

        if (!int.TryParse(match.Groups["index"].Value, out var index))
            return null;

        if (IconPathMap.TryGetValue(index, out var kind))
            return kind;

        LogUnmappedIndex(index);
        return null;
    }

    private static void LogUnmappedIndex(int index)
    {
        lock (SeenUnmappedIndexes)
        {
            if (!SeenUnmappedIndexes.Add(index))
                return;
        }

        Log.Information("Unknown mmres.dll icon index {Index} for tray icon form factor detection, falling back to device name matching", index);
    }
}
