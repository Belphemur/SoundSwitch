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

namespace SoundSwitch.Framework.Updater
{
    public class Release
    {
        public Release(Version releaseVersion, GitHubRelease.Asset asset, string name)
        {
            ReleaseVersion = releaseVersion;
            Asset = asset;
            Name = name;
        }

        public Version ReleaseVersion { get; private set; }
        public GitHubRelease.Asset Asset { get; private set; }
        public List<string> Changelog { get; } = new List<string>();
        public string Name { get; private set; }
    }
}