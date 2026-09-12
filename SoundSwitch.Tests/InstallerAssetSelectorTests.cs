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

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.Updater.Releases.Models;

namespace SoundSwitch.Tests;

/// <summary>
/// Unit tests for the architecture-aware installer asset selection shared by
/// UpdateChecker and NightlyUpdateChecker.
///
/// Asset-name matrix:
/// - x64: unsuffixed installer preferred (new self-contained builds), legacy
///   *_x64.exe as fallback, _arm64 must never be selected.
/// - arm64: *_arm64.exe preferred, unsuffixed (legacy dual-payload) fallback.
/// </summary>
[TestFixture]
public class InstallerAssetSelectorTests
{
    private static Asset Asset(string name)
    {
        return new Asset { Name = name };
    }

    [Test]
    public void X64_ShouldPreferUnsuffixedInstaller()
    {
        var assets = new List<Asset>
        {
            Asset("SoundSwitch_v7.2.0_Stable_Installer.exe"),
            Asset("SoundSwitch_v7.2.0_Stable_Installer_arm64.exe"),
            Asset("SoundSwitch_v7.1.0_Stable_Installer_x64.exe"),
        };

        var selected = InstallerAssetSelector.SelectInstallerAsset(assets, System.Runtime.InteropServices.Architecture.X64);

        selected.Name.Should().Be("SoundSwitch_v7.2.0_Stable_Installer.exe");
    }

    [Test]
    public void X64_LegacySuffixedInstaller_UsedAsFallback()
    {
        var assets = new List<Asset>
        {
            Asset("SoundSwitch_v7.1.0_Stable_Installer_x64.exe"),
            Asset("SoundSwitch_v7.1.0_Stable_Installer_arm64.exe"),
        };

        var selected = InstallerAssetSelector.SelectInstallerAsset(assets, System.Runtime.InteropServices.Architecture.X64);

        selected.Name.Should().Be("SoundSwitch_v7.1.0_Stable_Installer_x64.exe");
    }

    [Test]
    public void X64_NeverSelectsArm64Installer()
    {
        var assets = new List<Asset>
        {
            Asset("SoundSwitch_v7.2.0_Stable_Installer_arm64.exe"),
            Asset("SoundSwitch_v7.2.0.zip"),
        };

        var selected = InstallerAssetSelector.SelectInstallerAsset(assets, System.Runtime.InteropServices.Architecture.X64);

        selected.Should().BeNull();
    }

    [Test]
    public void Arm64_ShouldPreferArm64Installer()
    {
        var assets = new List<Asset>
        {
            Asset("SoundSwitch_v7.2.0_Stable_Installer.exe"),
            Asset("SoundSwitch_v7.2.0_Stable_Installer_arm64.exe"),
        };

        var selected = InstallerAssetSelector.SelectInstallerAsset(assets, System.Runtime.InteropServices.Architecture.Arm64);

        selected.Name.Should().Be("SoundSwitch_v7.2.0_Stable_Installer_arm64.exe");
    }

    [Test]
    public void Arm64_UnsuffixedInstaller_UsedAsLegacyFallback()
    {
        var assets = new List<Asset>
        {
            Asset("SoundSwitch_v7.1.0_Stable_Installer.exe"),
        };

        var selected = InstallerAssetSelector.SelectInstallerAsset(assets, System.Runtime.InteropServices.Architecture.Arm64);

        selected.Name.Should().Be("SoundSwitch_v7.1.0_Stable_Installer.exe");
    }

    [Test]
    public void Arm64_NeverSelectsX64SuffixedInstaller()
    {
        var assets = new List<Asset>
        {
            Asset("SoundSwitch_v7.1.0_Stable_Installer_x64.exe"),
        };

        var selected = InstallerAssetSelector.SelectInstallerAsset(assets, System.Runtime.InteropServices.Architecture.Arm64);

        selected.Should().BeNull();
    }

    [Test]
    public void Selection_ShouldIgnoreNonExeAssets()
    {
        var assets = new List<Asset>
        {
            Asset("SoundSwitch-v7.2.0.zip"),
            Asset("CHECKSUMS.txt"),
            Asset("SoundSwitch_v7.2.0_Stable_Installer.exe"),
        };

        var selected = InstallerAssetSelector.SelectInstallerAsset(assets, System.Runtime.InteropServices.Architecture.X64);

        selected.Name.Should().Be("SoundSwitch_v7.2.0_Stable_Installer.exe");
    }

    [Test]
    public void EmptyAssetList_ReturnsNull()
    {
        var selected = InstallerAssetSelector.SelectInstallerAsset(new List<Asset>(), System.Runtime.InteropServices.Architecture.X64);

        selected.Should().BeNull();
    }
}