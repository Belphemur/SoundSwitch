/********************************************************************
* Copyright (C) 2015 Antoine Aflalo
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
using System.Drawing;
using System.Windows.Forms;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Properties;
using SoundSwitch.UI.Controls;

namespace SoundSwitch.UI.Forms
{
    public partial class UpdateDownloadForm : Form
    {
        private readonly WebFile _releaseFile;

        public UpdateDownloadForm(Release release)
        {
            InitializeComponent();
            Icon = Icon.FromHandle(Resources.Update.GetHicon());
            Text = release.Name;
            changeLog.SetChangelog(release.Changelog);
            downloadProgress.DisplayStyle = TextProgressBar.ProgressBarDisplayText.Both;
            downloadProgress.CustomText = release.Asset.name;
            _releaseFile = new WebFile(new Uri(release.Asset.browser_download_url));
            _releaseFile.DownloadProgressChanged +=
                (sender, args) =>
                {
                    downloadProgress.Invoke(new Action(() => { downloadProgress.Value = args.ProgressPercentage; }));
                };
            _releaseFile.DownloadFailed += (sender, @event) =>
            {
                AppLogger.Log.Error("Couldn't download the Release ", @event.Exception);
                MessageBox.Show(@event.Exception.Message, UpdateFormStrings.downloadFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            _releaseFile.Downloaded += (sender, args) =>
            {
                installButton.Invoke(new Action(() =>
                {
                    installButton.Enabled = true;
                    downloadProgress.Enabled = false;
                }));
            };
            _releaseFile.DownloadFile();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void installButton_Click(object sender, EventArgs e)
        {
            _releaseFile.Start();
            Close();
        }

        private void UpdateDownloadForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _releaseFile.CancelDownload();
        }
    }
}