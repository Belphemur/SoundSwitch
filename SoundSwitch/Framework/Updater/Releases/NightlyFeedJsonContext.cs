#if NIGHTLY
using System.Text.Json.Serialization;

namespace SoundSwitch.Framework.Updater.Releases;

[JsonSerializable(typeof(Models.NightlyFeed))]
internal partial class NightlyFeedJsonContext : JsonSerializerContext;
#endif
