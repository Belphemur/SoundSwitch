#if NIGHTLY
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SoundSwitch.Framework.Updater.Releases.Models;

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
