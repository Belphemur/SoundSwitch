/********************************************************************
* Copyright (C) 2015 Jeroen Pelgrims
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Model;
using SoundSwitch.Properties;
using SoundSwitch.UI.Forms;

namespace SoundSwitch.Util
{
    public sealed class TrayIcon : IDisposable
    {
        private readonly ContextMenuStrip _selectionMenu = new ContextMenuStrip();
        private readonly ContextMenuStrip _settingsMenu = new ContextMenuStrip();
        private readonly SynchronizationContext _context = SynchronizationContext.Current ?? new SynchronizationContext();
        private volatile bool _needToUpdateList = true;
        public NotifyIcon NotifyIcon { get; }    = new NotifyIcon
        {
            Icon = Icon.FromHandle(Resources.SoundSwitch16.GetHicon()),
            Visible = true,
            Text = Application.ProductName
        };

        private readonly ToolStripMenuItem _updateMenuItem;

        public TrayIcon()
        {
            _updateMenuItem = new ToolStripMenuItem(TrayIconStrings.NoUpdate, Resources.Update, OnUpdateClick)
            {
                Enabled = false
            };
            NotifyIcon.ContextMenuStrip = _settingsMenu;

            PopulateSettingsMenu();

            _selectionMenu.Items.Add(TrayIconStrings.NoDevSel, Resources.Settings, (sender, e) => ShowSettings());

            NotifyIcon.MouseClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    UpdateDeviceSelectionList();
                    NotifyIcon.ContextMenuStrip = _selectionMenu;
                    var mi = typeof (NotifyIcon).GetMethod("ShowContextMenu",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    mi.Invoke(NotifyIcon, null);

                    NotifyIcon.ContextMenuStrip = _settingsMenu;
                }
            };
            SetEventHandlers();
        }

        public void Dispose()
        {
            _selectionMenu.Dispose();
            _settingsMenu.Dispose();
            NotifyIcon.Dispose();
            _updateMenuItem.Dispose();
        }

        private void PopulateSettingsMenu()
        {
            _settingsMenu.Items.Add(TrayIconStrings.playbackDev, Resources.PlaybackDevices,
                (sender, e) => { Process.Start(new ProcessStartInfo("control", "mmsys.cpl sounds")); });
            _settingsMenu.Items.Add(TrayIconStrings.mixer, Resources.Mixer,
                (sender, e) => { Process.Start(new ProcessStartInfo("sndvol.exe")); });
            _settingsMenu.Items.Add("-");
            _settingsMenu.Items.Add(TrayIconStrings.settings, Resources.Settings, (sender, e) => ShowSettings());
            _settingsMenu.Items.Add(TrayIconStrings.about, Resources.Help, (sender, e) => new About().Show());
            _settingsMenu.Items.Add(_updateMenuItem);
            _settingsMenu.Items.Add("-");
            _settingsMenu.Items.Add(TrayIconStrings.exit, Resources.exit, (sender, e) => Application.Exit());
        }

        private void OnUpdateClick(object sender, EventArgs eventArgs)
        {
            if (_updateMenuItem.Tag == null)
                return;

            new UpdateDownloadForm((Release) _updateMenuItem.Tag).ShowDialog();
            NotifyIcon.BalloonTipClicked -= OnUpdateClick;
        }

        private void SetEventHandlers()
        {
            AppModel.Instance.ErrorTriggered += (sender, @event) =>
            {
                using (AppLogger.Log.ErrorCall())
                {
                    if (@event.Exception is AppModel.NoDevicesException)
                    {
                        ShowNoDevices();
                    }
                    else
                    {
                        AppLogger.Log.Error("Exception managed", @event.Exception);
                        ShowError(@event.Exception.Message, @event.Exception.GetType().Name);
                    }
                }
            };
            AppModel.Instance.DefaultDeviceChanged +=
                (sender, audioChangeEvent) =>
                {
                    if (audioChangeEvent.role != Role.Console)
                    {
                        return;
                    }
                    _needToUpdateList = true;
                };
            AppModel.Instance.SelectedDeviceChanged +=
                 (sender, @event) => { _needToUpdateList = true; };
            AppModel.Instance.NewVersionReleased += (sender, @event) =>
            {
                _context.Send(s => { NewReleaseAvailable(sender, @event); }, null);
            };

            AppModel.Instance.DeviceRemoved +=
                (sender, @event) => { _needToUpdateList = true; };

            AppModel.Instance.DeviceAdded +=
                 (sender, @event) => { _needToUpdateList = true; };

            AppModel.Instance.DeviceStateChanged +=
                 (sender, @event) => { _needToUpdateList = true; };
        }

        private void NewReleaseAvailable(object sender, UpdateChecker.NewReleaseEvent newReleaseEvent)
        {
            _updateMenuItem.Tag = newReleaseEvent.Release;
            _updateMenuItem.Text = string.Format(TrayIconStrings.updateAvailable, newReleaseEvent.Release.ReleaseVersion);
            _updateMenuItem.Enabled = true;
            NotifyIcon.BalloonTipClicked += OnUpdateClick;
            NotifyIcon.ShowBalloonTip(3000,
                string.Format(TrayIconStrings.versionAvailable, Application.ProductName,
                    newReleaseEvent.Release.ReleaseVersion),
                newReleaseEvent.Release.Name + '\n' + TrayIconStrings.howDownloadUpdate, ToolTipIcon.Info);
        }


        public void ShowSettings()
        {
            new Settings().Show();
        }

        /// <summary>
        ///     Sets the names of devices that show up in the menu
        /// </summary>
        public void UpdateDeviceSelectionList()
        {
            using (AppLogger.Log.InfoCall())
            {
                if (!_needToUpdateList)
                {
                    AppLogger.Log.Info("Device list doesn't need update");
                    return;
                }
                if (AppModel.Instance.AvailablePlaybackDevices.Count < 0 &&
                    AppModel.Instance.AvailableRecordingDevices.Count < 0)
                {
                    AppLogger.Log.Info("Device list empty");
                    return;
                }

                _selectionMenu.Items.Clear();
                AppLogger.Log.Info("Set tray icon menu devices");
                foreach (var item in AppModel.Instance.AvailablePlaybackDevices)
                {
                    _selectionMenu.Items.Add(new ToolStripDeviceItem(DeviceClicked, item));
                }

                if (AppModel.Instance.AvailableRecordingDevices.Count > 0)
                {
                    _selectionMenu.Items.Add("-");
                    foreach (var item in AppModel.Instance.AvailableRecordingDevices)
                    {
                        _selectionMenu.Items.Add(new ToolStripDeviceItem(DeviceClicked, item));
                    }
                }
                _needToUpdateList = false;
            }
        }

        private void DeviceClicked(object sender, EventArgs e)
        {
            try
            {
                var item = (ToolStripDeviceItem) sender;
                AppModel.Instance.SetActiveDevice(item.AudioDevice);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///     Notification for when there are no devices configured
        /// </summary>
        public void ShowNoDevices()
        {
            AppLogger.Log.Info("No devices available");
            NotifyIcon.ShowBalloonTip(3000, string.Format(TrayIconStrings.confNeeded, Application.ProductName),
                TrayIconStrings.confNeededExp, ToolTipIcon.Warning);
        }

        /// <summary>
        ///     shows an error messasge
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="errorTitle"></param>
        public void ShowError(string errorMessage, string errorTitle)
        {
            NotifyIcon.ShowBalloonTip(3000, $"{Application.ProductName}: {errorTitle}", errorMessage, ToolTipIcon.Error);
        }
    }
}