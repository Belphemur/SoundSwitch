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
using System.Reflection;
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Properties;
using SoundSwitch.UI.Forms;

namespace SoundSwitch.Util
{
    public sealed class TrayIcon : IDisposable
    {
        private readonly ContextMenuStrip _selectionMenu = new ContextMenuStrip();
        private readonly ContextMenuStrip _settingsMenu = new ContextMenuStrip();

        private readonly NotifyIcon _trayIcon = new NotifyIcon
        {
            Icon = Icon.FromHandle(Resources.SoundSwitch16.GetHicon()),
            Visible = true,
            Text = Application.ProductName
        };

        private readonly ToolStripMenuItem _updateMenuItem;
        private List<AudioDeviceWrapper> _availableAudioDeviceWrappers;
        private bool _deviceListChanged = true;

        public TrayIcon()
        {
            _updateMenuItem = new ToolStripMenuItem("No Update", Resources.Update, OnUpdateClick) {Enabled = false};
            _trayIcon.ContextMenuStrip = _settingsMenu;

            _availableAudioDeviceWrappers = Main.Instance.AvailablePlaybackDevices;

            PopulateSettingsMenu();

            _selectionMenu.Items.Add("No devices selected", Resources.Settings, (sender, e) => ShowSettings());

            _trayIcon.MouseClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    UpdateDeviceSelectionList();
                    _trayIcon.ContextMenuStrip = _selectionMenu;
                    var mi = typeof (NotifyIcon).GetMethod("ShowContextMenu",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    mi.Invoke(_trayIcon, null);

                    _trayIcon.ContextMenuStrip = _settingsMenu;
                }
            };
            UpdateDeviceSelectionList();
            SetEventHandlers();
        }

        public void Dispose()
        {
            _selectionMenu.Dispose();
            _settingsMenu.Dispose();
            _trayIcon.Dispose();
            _updateMenuItem.Dispose();
            GC.SuppressFinalize(_availableAudioDeviceWrappers);
            GC.SuppressFinalize(this);
        }

        private void PopulateSettingsMenu()
        {
            _settingsMenu.Items.Add("Playback Devices", Resources.PlaybackDevices,
                (sender, e) => { Process.Start(new ProcessStartInfo("control", "mmsys.cpl sounds")); });
            _settingsMenu.Items.Add("Mixer", Resources.Mixer,
                (sender, e) => { Process.Start(new ProcessStartInfo("sndvol.exe")); });
            _settingsMenu.Items.Add("-");
            _settingsMenu.Items.Add("Settings", Resources.Settings, (sender, e) => ShowSettings());
            _settingsMenu.Items.Add("About", Resources.Help, (sender, e) => new About().Show());
            _settingsMenu.Items.Add(_updateMenuItem);
            _settingsMenu.Items.Add("-");
            _settingsMenu.Items.Add("Exit", Resources.exit, (sender, e) => Application.Exit());
        }

        private void OnUpdateClick(object sender, EventArgs eventArgs)
        {
            new UpdateDownloadForm((Release) _updateMenuItem.Tag).ShowDialog();
        }

        private void SetEventHandlers()
        {
            Main.Instance.ErrorTriggered += (sender, @event) =>
            {
                using (AppLogger.Log.ErrorCall())
                {
                    if (@event.Exception is Main.NoDevicesException)
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
            Main.Instance.AudioDeviceChanged +=
                (sender, audioChangeEvent) =>
                {
                    ShowAudioChanged(audioChangeEvent.AudioDevice.FriendlyName);
                    foreach (ToolStripDeviceItem item in _selectionMenu.Items)
                    {
                        item.Image = item.AudioDevice.FriendlyName == audioChangeEvent.AudioDevice.FriendlyName
                            ? Resources.Check
                            : null;
                    }
                };
            Main.Instance.SelectedDeviceChanged += (sender, deviceListChanged) => { UpdateAvailableDeviceList(); };
            Main.Instance.NewVersionReleased += (sender, @event) =>
            {
                if (_settingsMenu.IsHandleCreated)
                {
                    _settingsMenu.Invoke(new Action(() => { NewReleaseAvailable(sender, @event); }));
                }
                else
                {
                    NewReleaseAvailable(sender, @event);
                }
            };


            WindowsAPIAdapter.DeviceChanged += (sender, deviceChangeEvent) => { UpdateAvailableDeviceList(); };
            Main.Instance.InitializeMain();
        }

        private void NewReleaseAvailable(object sender, UpdateChecker.NewReleaseEvent newReleaseEvent)
        {
            _updateMenuItem.Tag = newReleaseEvent.Release;
            _updateMenuItem.Text = $"Update Available ({newReleaseEvent.Release.ReleaseVersion})";
            _updateMenuItem.Enabled = true;
            _trayIcon.ShowBalloonTip(3000, $"Version {newReleaseEvent.Release.ReleaseVersion} is available",
                "Right click on the tray icon to download.", ToolTipIcon.Info);
        }

        private void UpdateAvailableDeviceList()
        {
            var audioDevices = Main.Instance.AvailablePlaybackDevices;
            _deviceListChanged = !_availableAudioDeviceWrappers.Equals(audioDevices);
            if (_deviceListChanged)
            {
                _availableAudioDeviceWrappers = audioDevices;
            }
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
                if (!_deviceListChanged)
                {
                    AppLogger.Log.Info("Device list unchanged");
                    return;
                }

                if (_availableAudioDeviceWrappers.Count < 0)
                {
                    AppLogger.Log.Info("Device list empty");
                    return;
                }

                _selectionMenu.Items.Clear();
                AppLogger.Log.Info("Set tray icon menu devices");
                foreach (var item in _availableAudioDeviceWrappers)
                {
                    _selectionMenu.Items.Add(new ToolStripDeviceItem(DeviceClicked, item));
                }
                _deviceListChanged = false;
            }
        }

        private void DeviceClicked(object sender, EventArgs e)
        {
            try
            {
                var item = (ToolStripDeviceItem) sender;
                Main.Instance.SetActiveDevice(item.AudioDevice);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///     Notification that audio has changed
        /// </summary>
        /// <param name="deviceName"></param>
        public void ShowAudioChanged(string deviceName)
        {
            _trayIcon.ShowBalloonTip(500, "SoundSwitch: Audio output changed", deviceName, ToolTipIcon.Info);
        }

        /// <summary>
        ///     Notification for when there are no devices configured
        /// </summary>
        public void ShowNoDevices()
        {
            AppLogger.Log.Error("No devices available");
            _trayIcon.ShowBalloonTip(3000, "SoundSwitch: Configuration needed",
                "No devices available to switch to. Open configuration by right-clicking on the SoundSwitch icon. ",
                ToolTipIcon.Warning);
        }

        /// <summary>
        ///     shows an error messasge
        /// </summary>
        /// <param name="errorMessage"></param>
        public void ShowError(string errorMessage, string errorTitle = "Error")
        {
            _trayIcon.ShowBalloonTip(3000, "SoundSwitch: " + errorTitle, errorMessage, ToolTipIcon.Error);
        }
    }
}