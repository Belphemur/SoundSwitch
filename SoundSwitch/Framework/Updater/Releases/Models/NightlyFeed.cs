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

#if NIGHTLY
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SoundSwitch.Framework.Updater.Releases.Models;

/// <summary>
/// One nightly installer entry in the R2 <c>version.json</c> feed.
/// </summary>
public class NightlyArtifact
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("published")]
    public string Published { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("sha512")]
    public string Sha512 { get; set; }

    [JsonPropertyName("commit")]
    public string Commit { get; set; }

    [JsonPropertyName("changelog")]
    public List<string> Changelog { get; set; }
}

/// <summary>
/// Shape of the R2 <c>version.json</c> nightly feed.
/// </summary>
public class NightlyFeed
{
    [JsonPropertyName("latest")]
    public string Latest { get; set; }

    [JsonPropertyName("published")]
    public string Published { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("artifacts")]
    public List<NightlyArtifact> Artifacts { get; set; }
}
#endif
