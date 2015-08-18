/*
 * Gautham Prabhu K 2014
 * gautham.prabhu.se@gmail.com
 * No warranty of any kind implied or otherwise.
 * 
 */

using System;
using System.Runtime.InteropServices;

namespace SoundSwitch.Util
{
    public class RestartManagerWrapper
    {
        /// <summary>
        ///     Windows XP version
        /// </summary>
        private const int WINDOWSXPVERSION = 5;

        /// <summary>
        /// </summary>
        public static bool IsRestartManagerSupported => Environment.OSVersion.Version.Major > WINDOWSXPVERSION;

        /// <summary>
        ///     Register for restart can be done only if OS version is Vista or higher
        /// </summary>
        /// <param name="pszCommandline"></param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern uint RegisterApplicationRestart(string pszCommandline, int dwFlags);
    }

    /// <summary>
    ///     Windows messages
    /// </summary>
    public class WindowsMessages
    {
        /// <summary>
        ///     Windows message query end session
        /// </summary>
        public const int WM_QUERYENDSESSION = 17;

        /// <summary>
        ///     Windows message end session
        /// </summary>
        public const int WM_ENDSESSION = 22;
    }

    /// <summary>
    ///     LParam constants
    /// </summary>
    public class LParameter
    {
        /// <summary>
        ///     End session close application
        /// </summary>
        public const int ENDSESSION_CLOSEAPP = 1;
    }

    /// <summary>
    ///     Register application dw Flags
    /// </summary>
    public class ApplicationRestartFlags
    {
        /// <summary>
        ///     No flag
        /// </summary>
        public const int NONE = 0;

        /// <summary>
        ///     Do not restart the process if it terminates due to an unhandled exception
        /// </summary>
        public const int RESTART_NO_CRASH = 1;

        /// <summary>
        ///     Do not restart the process if it terminates due to the application not responding.
        /// </summary>
        public const int RESTART_NO_HANG = 2;

        /// <summary>
        ///     Do not restart the process if it terminates due to the installation of an update.
        /// </summary>
        public const int RESTART_NO_PATCH = 4;

        /// <summary>
        ///     Do not restart the process if the computer is restarted as the result of an update.
        /// </summary>
        public const int RESTART_NO_REBOOT = 8;
    }
}