using System;
using NuGet.Versioning;

namespace SoundSwitch.Framework.Updater.Remind
{
    public record ReleasePostponed(SemanticVersion Version, DateTime Until, uint Count);
}