﻿/********************************************************************
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

using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Serilog;
using SoundSwitch.Framework.Updater.Installer;
using SoundSwitch.Framework.Updater.Releases;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.Updater
{
    /// <summary>
    /// Take the update, download it and execute the installer with the wanted parameter
    /// </summary>
    public class AutoUpdater
    {
        private readonly SynchronizationContext _context = SynchronizationContext.Current ?? new SynchronizationContext();
        public string InstallerParameters { get; }
        public string InstallerFilePath { get; }

        /// <summary>
        /// Constructor of the AutoUpdater
        /// </summary>
        /// <param name="installerParameters">Parameters given to the installer after downloading it</param>
        /// <param name="filePath">Where to download the Installer without the filename</param>
        public AutoUpdater(string installerParameters, string filePath)
        {
            InstallerParameters = installerParameters;
            InstallerFilePath = Path.Combine(filePath, "SoundSwitch_Update.exe");
        }

        public void Update(AppRelease appRelease, bool closeApp)
        {
          
                var file = new WebFile(new Uri(appRelease.Asset.BrowserDownloadUrl), InstallerFilePath);
                file.Downloaded += (sender, args) =>
                {
                    Log.Information("Update downloaded: {File}", file);
                    if (!SignatureChecker.IsValid(file.FilePath))
                    {
                        Log.Error("The file has the wrong signature. Update cancelled.");
                        _context.Send(state =>
                        {
                            MessageBox.Show(string.Format(UpdateDownloadStrings.wrongSignature, "https://soundswitch.aaflalo.me"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }, null);
                        return;
                    }

                    new UpdateRunner().RunUpdate(file, InstallerParameters);
                    if (closeApp)
                    {
                        _context.Send(s => { Application.Exit(); }, null);
                    }
                };
                file.DownloadFile();
            
        }
    }
}
