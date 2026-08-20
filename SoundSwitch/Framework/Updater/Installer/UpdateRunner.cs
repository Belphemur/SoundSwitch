using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using SoundSwitch.Framework;
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
        // Only add /DIR if the caller hasn't already supplied it as a switch token
        // (e.g. "/DIR" or "/DIR=..."). Avoid a plain substring match so values like
        // "/DIRTY" or "/DIRECTORY" are not mistaken for the /DIR parameter.
        if (!Regex.IsMatch(args, @"(?i)(?:^|\s)/DIR(?:=|\s|$)"))
        {
            var installDir = ApplicationPath.InstallDirectory;
            if (!string.IsNullOrEmpty(installDir))
            {
                // Guard a trailing backslash (e.g. "D:\") that would otherwise let
                // Inno's command-line parser treat /DIR="D:\" as an escaped quote.
                if (installDir.EndsWith("\\"))
                {
                    installDir += "\\";
                }

                args += $" /DIR=\"{installDir}\"";
            }
        }

        return file.Start(args);
    }
}
