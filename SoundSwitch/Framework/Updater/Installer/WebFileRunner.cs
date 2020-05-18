using System;
using System.Diagnostics;

namespace SoundSwitch.Framework.Updater.Installer
{
    public class WebFileRunner
    {
        private readonly string _appPath;

        public WebFileRunner(string appPath)
        {
            _appPath = appPath;
        }

        private bool IsInProgramFile()
        {
            var x86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var x64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            return _appPath.IndexOf(x64, StringComparison.OrdinalIgnoreCase) >= 0 || _appPath.IndexOf(x86, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public Process RunFile(WebFile file, string args = "")
        {
            //args += $" /{(IsInProgramFile() ? "ALLUSERS" : "CURRENTUSER")}";
            return file.Start(args);
        }
    }
}