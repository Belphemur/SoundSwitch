using System.Text.Json.Serialization;

namespace SoundSwitch.Framework.Updater.Releases;

[JsonSerializable(typeof(Models.Release[]))]
internal partial class GithubReleasesJsonContext : JsonSerializerContext;