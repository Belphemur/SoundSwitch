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

    public void Update(AppRelease appRelease, bool closeApp, Action<bool>? onCompleted = null)
    {
        var file = new WebFile(new Uri(appRelease.Asset.BrowserDownloadUrl));
        file.Downloaded += (sender, args) =>
        {
            Log.Information("Update downloaded: {File}", file);
#if NIGHTLY
            // Nightly artifacts carry a SHA-512 (unsigned builds); release-train
            // installers are Authenticode-signed and verified by signature.
            var checksumResult = string.IsNullOrEmpty(appRelease.ExpectedSha512)
                ? SignatureChecker.IsValid(file.FilePath).UnwrapFailure()
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

            new UpdateRunner().RunUpdate(file, InstallerParameters);
            onCompleted?.Invoke(true);
            if (closeApp)
            {
                _context.Send(s => { Application.Exit(); }, null);
            }
        };
        file.DownloadFile();
    }
}
