using System;

namespace SoundSwitch.Framework.Updater.Remind
{
    public record ReleasePostponed(Version Version, DateTime Until, uint Count);
}