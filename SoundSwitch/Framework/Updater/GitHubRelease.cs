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

namespace SoundSwitch.Framework.Updater
{
    public class GitHubRelease
    {
        public string tag_name { get; set; }
        public string body { get; set; }
        public string name { get; set; }
        public bool prerelease { get; set; }
        public List<Asset> assets { get; set; }

        public class Asset
        {
            public string name { get; set; }
            public string browser_download_url { get; set; }
        }

        public override string ToString()
        {
            return $"(tag_name: {tag_name}, name: {name}, prerelease: {prerelease})";
        }
    }
}