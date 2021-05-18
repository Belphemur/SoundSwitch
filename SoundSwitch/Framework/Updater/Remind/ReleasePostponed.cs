using System;

namespace SoundSwitch.Framework.Updater.Remind
{
    public record ReleasePostponed(Version Version, DateTime Until)
    {
        /// <summary>
        /// Return true if the release should be postponed, false if it can be done now.
        /// </summary>
        /// <param name="release"></param>
        /// <returns></returns>
        public bool ShouldPostpone(Release release)
        {
            if (release.ReleaseVersion > Version)
            {
                return false;
            }

            return Until > DateTime.UtcNow;
        }
    }
}