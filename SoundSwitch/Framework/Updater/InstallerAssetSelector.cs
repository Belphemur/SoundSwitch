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
using System.Linq;
using System.Runtime.InteropServices;

using SoundSwitch.Framework.Updater.Releases.Models;

namespace SoundSwitch.Framework.Updater;

/// <summary>
/// Selects the installer asset that matches the current process architecture
/// from a release's asset list. Shared by the stable and nightly update
/// checkers.
/// </summary>
/// <remarks>
/// <para>
/// Asset-name matrix (new self-contained dual-arch installers):
/// <list type="bullet">
/// <item>x64: prefer the unsuffixed installer (new releases), fall back to a
/// legacy <c>*_x64.exe</c> asset. <c>_arm64</c> installers must NEVER be
/// selected on x64 — an arm64 installer cannot run on an x64 machine.</item>
/// <item>arm64: prefer <c>*_arm64.exe</c>, fall back to the unsuffixed
/// installer (legacy installers bundled both arch payloads).</item>
/// </para>
/// </remarks>
internal static class InstallerAssetSelector
{
    /// <summary>
    /// Selects the installer asset for the current process architecture.
    /// </summary>
    /// <returns>The matching asset, or <c>null</c> when no compatible installer exists.</returns>
    public static Asset SelectInstallerAsset(System.Collections.Generic.IEnumerable<Asset> assets)
    {
        return SelectInstallerAsset(assets, RuntimeInformation.ProcessArchitecture);
    }

    /// <summary>
    /// Selects the installer asset for an explicit architecture.
    /// </summary>
    /// <returns>The matching asset, or <c>null</c> when no compatible installer exists.</returns>
    internal static Asset SelectInstallerAsset(System.Collections.Generic.IEnumerable<Asset> assets, Architecture architecture)
    {
        var assetList = (assets ?? Array.Empty<Asset>()).ToArray();

        if (architecture == Architecture.Arm64)
        {
            // Prefer the arm64 installer; fall back to the unsuffixed installer
            // (legacy installers bundled both arch payloads and run on arm64).
            return assetList.FirstOrDefault(a => a.Name.Contains("_arm64") && a.Name.EndsWith(".exe"))
                   ?? assetList.FirstOrDefault(a => a.Name.EndsWith(".exe")
                                                    && !a.Name.Contains("_arm64")
                                                    && !a.Name.Contains("_x64"));
        }

        // x64: prefer the unsuffixed installer (new self-contained x64 builds),
        // fall back to the legacy explicitly-suffixed x64 asset.
        // _arm64 assets are excluded on every pass.
        return assetList.FirstOrDefault(a => a.Name.EndsWith(".exe")
                                             && !a.Name.Contains("_arm64")
                                             && !a.Name.Contains("_x64"))
               ?? assetList.FirstOrDefault(a => a.Name.Contains("_x64") && a.Name.EndsWith(".exe"));
    }
}