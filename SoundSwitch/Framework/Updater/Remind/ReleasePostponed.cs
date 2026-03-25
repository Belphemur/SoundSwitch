using NuGet.Versioning;
using System;

namespace SoundSwitch.Framework.Updater.Remind;

public record ReleasePostponed(SemanticVersion Version, DateTime Until, uint Count);
