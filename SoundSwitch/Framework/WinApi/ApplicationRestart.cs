/********************************************************************
 * Copyright (C) 2015 Jeroen Pelgrims
 * Copyright (C) 2015-2024 Antoine Aflalo
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 ********************************************************************/

using System;
using System.Runtime.InteropServices;

using Serilog;

namespace SoundSwitch.Framework.WinApi;

/// <summary>
/// Best-effort registration with Windows' Application Restart machinery.
/// </summary>
/// <remarks>
/// The Restart Manager's <c>RmRestart</c> — what Inno Setup's <c>/RESTARTAPPLICATIONS</c>
/// invokes after an update — "can only restart applications that have been registered for
/// restart" via <c>RegisterApplicationRestart</c> (Microsoft, "Guidelines for Applications",
/// Restart Manager). Without this registration the updated SoundSwitch stays dead until the
/// next logon (#2580).
/// </remarks>
internal static class ApplicationRestart
{
    /// <summary>
    /// Do not restart the process when the computer is restarted as the result of an update
    /// (e.g. <c>ExitWindowsEx(EWX_RESTARTAPPS)</c> after a Windows Update reboot).
    /// </summary>
    private const uint RESTART_NO_REBOOT = 0x8;

    /// <summary>
    /// Marker argument used as the registered restart command line. It must be non-empty:
    /// a <c>null</c> or empty command line REMOVES the registration. The executable name is
    /// added by the OS; the marker itself is only read by startup classification
    /// (Program.OldMain: a process started with it was relaunched by the Restart Manager).
    /// </summary>
    internal const string RestartCommandLine = "--restarted";

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int RegisterApplicationRestart(string? commandLine, uint flags);

    /// <summary>
    /// Registers this process so the Restart Manager can relaunch it after an installer
    /// closed it. Flag choice:
    /// <list type="bullet">
    /// <item><see cref="RESTART_NO_REBOOT"/> is set: without it, a reboot-triggering update
    /// would relaunch SoundSwitch through the OS restart machinery on top of the autostart
    /// Run key (setup.iss:111) at the next logon — a double start the user mutex
    /// (Program.cs OldMain, user-mutex conflict path) would have to arbitrate.</item>
    /// <item><c>RESTART_NO_PATCH</c> is deliberately NOT set: its documented meaning is
    /// "do not restart the process if it terminates due to the installation of an update" —
    /// precisely the installer relaunch this registration exists for; setting it could make
    /// <c>RmRestart</c> a no-op again.</item>
    /// <item><c>RESTART_NO_CRASH</c>/<c>RESTART_NO_HANG</c> are not set: crash/hang recovery
    /// stays available, and it is consent-based — Windows offers the restart to the user
    /// instead of auto-restarting.</item>
    /// </list>
    /// </summary>
    public static void TryRegister()
    {
        try
        {
            var hr = RegisterApplicationRestart(RestartCommandLine, RESTART_NO_REBOOT);
            if (hr == 0)
            {
                Log.Information("Registered with the Windows restart manager: installers closing SoundSwitch can relaunch it");
                return;
            }

            Log.Warning("Couldn't register with the Windows restart manager (HRESULT {HResult:x8}); installers won't relaunch SoundSwitch", hr);
        }
        catch (Exception ex)
        {
            // Best-effort: a missing registration only means no automatic relaunch after an
            // installer close — never a reason to block startup.
            Log.Warning(ex, "Couldn't register with the Windows restart manager; installers won't relaunch SoundSwitch");
        }
    }
}
