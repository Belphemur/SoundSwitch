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
using System.Text.Json.Serialization;

namespace SoundSwitch.Framework.Updater.Releases;

/// <summary>
/// Source-generated JSON serialization context for the R2 nightly feed.
/// </summary>
[JsonSerializable(typeof(Models.NightlyFeed))]
internal partial class NightlyFeedJsonContext : JsonSerializerContext;
#endif
