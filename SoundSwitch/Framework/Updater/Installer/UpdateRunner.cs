using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using SoundSwitch.Framework.Configuration;

namespace SoundSwitch.Framework.Updater.Installer;

public class UpdateRunner
{
  
    public Process RunUpdate(WebFile file, string args = "")
    {
        args += " /NOCANCEL /NORESTART /CLOSEAPPLICATIONS";
        if (DateTime.UtcNow - AppConfigs.Configuration.LastDonationNagTime < AppConfigs.Configuration.TimeBetweenDonateNag)
        {
            args += " /NODONATE";
        }
        else
        {
            AppConfigs.Configuration.LastDonationNagTime = DateTime.UtcNow;
            AppConfigs.Configuration.Save();
        }

        // Force the update into the directory this app is currently running from,
        // so auto-updates respect a non-default install location (issue #2353).
        if (!args.Contains("/DIR", StringComparison.OrdinalIgnoreCase))
        {
            var installDir = Path.GetDirectoryName(Application.ExecutablePath);
            if (!string.IsNullOrEmpty(installDir))
            {
                args += $" /DIR=\"{installDir}\"";
            }
        }

        return file.Start(args);
    }
}
