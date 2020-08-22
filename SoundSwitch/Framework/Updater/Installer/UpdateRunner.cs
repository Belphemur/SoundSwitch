using System;
using System.Diagnostics;
using SoundSwitch.Framework.Configuration;

namespace SoundSwitch.Framework.Updater.Installer
{
    public class UpdateRunner
    {
  
        public Process RunUpdate(WebFile file, string args = "")
        {
            args += " /NOCANCEL /NORESTART /CLOSEAPPLICATIONS";
            if (DateTime.UtcNow - AppConfigs.Configuration.LastTimeUpdate < AppConfigs.Configuration.TimeBetweenDonateNag)
            {
                args += " /NODONATE";
            }
            AppConfigs.Configuration.LastTimeUpdate = DateTime.UtcNow;
            AppConfigs.Configuration.Save();
            return file.Start(args);
        }
    }
}