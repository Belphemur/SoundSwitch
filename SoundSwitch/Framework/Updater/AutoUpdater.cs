/********************************************************************
* Copyright (C) 2015-2017 Antoine Aflalo
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

#nullable enable
using System;
using System.Threading;
using System.Windows.Forms;

using Serilog;

using SoundSwitch.Framework.Updater.Installer;
using SoundSwitch.Framework.Updater.Releases;
using SoundSwitch.Localization;
using SoundSwitch.Util;

namespace SoundSwitch.Framework.Updater;

/// <summary>
/// Take the update, download it and execute the installer with the wanted parameter
/// </summary>
public class AutoUpdater
{
    private readonly SynchronizationContext _context = SynchronizationContext.Current ?? new SynchronizationContext();
    public string InstallerParameters { get; }

    /// <summary>
    /// Constructor of the AutoUpdater
    /// </summary>
    /// <param name="installerParameters">Parameters given to the installer after downloading it</param>
    public AutoUpdater(string installerParameters)
    {
        InstallerParameters = installerParameters;
    }

    /// <summary>
    /// Download the update and start the installer with the configured parameters.
    /// </summary>
    /// <remarks>
    /// The caller must NOT exit the process after starting the installer: Setup closes the
    /// running instance through the Windows Restart Manager (CloseApplications=yes in
    /// setup.iss) — a close our WM_ENDSESSION handler already survives correctly — and
    /// relaunches the updated executable afterwards (RmRestart + RegisterApplicationRestart
    /// + /RESTARTAPPLICATIONS). Exiting ourselves up front defeats both: the Restart Manager
    /// can only relaunch a process it closed itself (#2580). If starting the installer
    /// fails, the app simply keeps running on the old version.
    /// </remarks>
    public void Update(AppRelease appRelease, Action<bool>? onCompleted = null)
    {
        var file = new WebFile(new Uri(appRelease.Asset.BrowserDownloadUrl));
        file.Downloaded += (sender, args) =>
        {
            Log.Information("Update downloaded: {File}", file);
#if NIGHTLY
            // Nightly artifacts carry a SHA-512 (unsigned builds); release-train
            // installers are Authenticode-signed and verified by signature.
            var checksumResult = appRelease.ExpectedSha512 == null
                ? SignatureChecker.IsValid(file.FilePath).UnwrapFailure()?.ToString()
                : UpdateVerifier.Verify(file.FilePath, appRelease.ExpectedSha512).UnwrapFailure();
            if (checksumResult != null)
            {
                Log.Error("The file failed its integrity check ({Kind}). Update cancelled. {checksumResult}",
                    appRelease.ExpectedSha512 == null ? "signature" : "sha512", checksumResult);
                onCompleted?.Invoke(false);
                var message = appRelease.ExpectedSha512 == null
                    ? string.Format(UpdateDownloadStrings.wrongSignature, "https://soundswitch.aaflalo.me")
                    : UpdateDownloadStrings.wrongChecksum;
                _context.Send(state => { MessageBox.Show(message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error); },
                    null);
                return;
            }
#else
            var signatureResult = SignatureChecker.IsValid(file.FilePath).UnwrapFailure();
            if (signatureResult != null)
            {
                Log.Error("The file has the wrong signature. Update cancelled. {signatureResult}", signatureResult);
                onCompleted?.Invoke(false);
                _context.Send(state => { MessageBox.Show(string.Format(UpdateDownloadStrings.wrongSignature, "https://soundswitch.aaflalo.me"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error); },
                    null);
                return;
            }
#endif

            try
            {
                new UpdateRunner().RunUpdate(file, InstallerParameters);
            }
            catch (Exception ex)
            {
                // The installer could not be started: stay on the current version and keep
                // running — do not exit, the Restart Manager never got the chance to take
                // over the shutdown (#2580).
                Log.Error(ex, "Failed to launch the update installer; SoundSwitch keeps running on the current version");
                onCompleted?.Invoke(false);
                return;
            }

            onCompleted?.Invoke(true);
        };
        file.DownloadFile();
    }
}
