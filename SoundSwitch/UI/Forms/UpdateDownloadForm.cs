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

using System;
using System.Windows.Forms;
using Serilog;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.Updater.Installer;
using SoundSwitch.Framework.Updater.Remind;
using SoundSwitch.Localization;
using SoundSwitch.Properties;
using SoundSwitch.UI.Component;

namespace SoundSwitch.UI.Forms
{
    public sealed partial class UpdateDownloadForm : Form
    {
        private WebFile _releaseFile;
        private Release _releaseInfo;
        private readonly PostponeService _postponeService = new();

        public UpdateDownloadForm()
        {
            InitializeComponent();
            Icon = Resources.UpdateIcon;

            LocalizeForm();
            downloadProgress.DisplayStyle = TextProgressBar.ProgressBarDisplayText.Both;
            downloadProgress.Visible = false;
            TopMost = true;
        }

        public void DownloadRelease(Release release)
        {
            Text = release.Name;
            _releaseInfo = release;
            installButton.Enabled = true;
            changeLog.SetChangelog(release.Changelog);
            Name = release.Name;
            downloadProgress.CustomText = release.Asset.name;
            downloadProgress.Value = 0;

            _releaseFile = new WebFile(new Uri(release.Asset.browser_download_url));

            _releaseFile.DownloadProgress += (sender, progress) =>
            {
                if (downloadProgress.IsDisposed)
                {
                    return;
                }

                if (downloadProgress.InvokeRequired)
                {
                    downloadProgress.BeginInvoke(new Action(() => { downloadProgress.Value = (int) Math.Ceiling(progress.Percentage); }));
                }
                else
                {
                    downloadProgress.Value = (int) Math.Ceiling(progress.Percentage);
                }
            };
            _releaseFile.DownloadFailed += (sender, @event) =>
            {
                Log.Error(@event.Exception, "Couldn't download the Release ");
                MessageBox.Show(@event.Exception.Message,
                    UpdateDownloadStrings.downloadFailed,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            _releaseFile.Downloaded += (sender, args) =>
            {
                if (!SignatureChecker.IsValid(_releaseFile.FilePath))
                {
                    Log.Error("Wrong signature for the release");
                    MessageBox.Show(UpdateDownloadStrings.notSigned,
                        UpdateDownloadStrings.notSignedTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                new UpdateRunner().RunUpdate(_releaseFile, "/SILENT");
                if (InvokeRequired)
                {
                    BeginInvoke((Action) Close);
                }
                else
                {
                    Close();
                }
            };
            ShowDialog();
        }

        private void LocalizeForm()
        {
            // Misc
            changeLogGroup.Text = UpdateDownloadStrings.changelog;
            cancelButton.Text = UpdateDownloadStrings.remindMe;
            installButton.Text = UpdateDownloadStrings.install;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (_releaseFile.DownloadStarted)
            {
                _releaseFile.CancelDownload();
            }
            else
            {
                _postponeService.PostponeRelease(_releaseInfo);
            }
            Close();
        }

        private void installButton_Click(object sender, EventArgs e)
        {
            downloadProgress.Enabled = true;
            downloadProgress.Visible = true;
            _releaseFile.DownloadFile();
            installButton.Enabled = false;
            cancelButton.Text = UpdateDownloadStrings.cancel;
        }

        private void UpdateDownloadForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _releaseFile.CancelDownload();
        }
    }
}