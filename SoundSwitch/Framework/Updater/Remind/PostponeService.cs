using System;
using Serilog;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.Updater.Releases;

namespace SoundSwitch.Framework.Updater.Remind;

public class PostponeService
{
    /// <summary>
    /// Set the release as postponed
    /// </summary>
    /// <param name="appRelease"></param>
    public void PostponeRelease(AppRelease appRelease)
    {
        var postponed = AppConfigs.Configuration.Postponed;
        try
        {
            if (postponed == null || postponed.Version < appRelease.ReleaseVersion)
            {
                AppConfigs.Configuration.Postponed = new ReleasePostponed(appRelease.ReleaseVersion, DateTime.UtcNow + 3 * TimeSpan.FromSeconds(AppConfigs.Configuration.UpdateCheckInterval), 1);
                return;
            }

            if (postponed.Version == appRelease.ReleaseVersion)
            {
                var postponedCount = postponed.Count + 1;
                if (postponedCount > 5)
                {
                    postponedCount = 5;
                }
                AppConfigs.Configuration.Postponed = postponed with
                {
                    Count = postponedCount, Until = DateTime.UtcNow + (3 + (postponedCount ^ 2)) * TimeSpan.FromSeconds(AppConfigs.Configuration.UpdateCheckInterval)
                };
            }
        }
        finally
        {
            Log.Information($"Release {appRelease} set as {AppConfigs.Configuration.Postponed}");
            AppConfigs.Configuration.Save();
        }
    }

    /// <summary>
    /// Return true if the release should be postponed, false if it can be done now.
    /// </summary>
    /// <param name="appRelease"></param>
    /// <returns></returns>
    public bool ShouldPostpone(AppRelease appRelease)
    {
        var postponed = AppConfigs.Configuration.Postponed;
        if (postponed == null)
        {
            return false;
        }

        if (appRelease.ReleaseVersion > postponed.Version)
        {
            return false;
        }

        return postponed.Until > DateTime.UtcNow;
    }
}