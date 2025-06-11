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
using NuGet.Versioning;
using SoundSwitch.Framework.Updater.Releases.Models;

namespace SoundSwitch.Framework.Updater.Releases;

public class AppRelease(SemanticVersion releaseVersion, Asset asset, string name)
{
    public SemanticVersion ReleaseVersion { get; private set; } = releaseVersion;
    public Asset Asset { get; } = asset;
    public List<string> Changelog { get; } = new List<string>();
    public string Name { get; private set; } = name;

    public override string ToString()
    {
        return $"{nameof(ReleaseVersion)}: {ReleaseVersion}, {nameof(Name)}: {Name}";
    }
}