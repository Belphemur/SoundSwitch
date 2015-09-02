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
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework;
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

        private readonly NotifyIcon _trayIcon = new NotifyIcon
        {
            Icon = Icon.FromHandle(Resources.SoundSwitch16.GetHicon()),
            Visible = true,
            Text = Application.ProductName
        };

        private readonly ToolStripMenuItem _updateMenuItem;
        private ICollection<IAudioDevice> _availablePlaybackDevices;
        private ICollection<IAudioDevice> _availableRecordingDevices;
        private bool _deviceListChanged = true;

        public TrayIcon()
        {
            _updateMenuItem = new ToolStripMenuItem("No Update", Resources.Update, OnUpdateClick) {Enabled = false};
            _trayIcon.ContextMenuStrip = _settingsMenu;

            _availablePlaybackDevices = AppModel.Instance.AvailablePlaybackDevices;
            _availableRecordingDevices = AppModel.Instance.AvailableRecordingDevices;

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
            GC.SuppressFinalize(_availablePlaybackDevices);
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
                    var audioDeviceType = audioChangeEvent.AudioDevice.Type;
                    switch (audioDeviceType)
                    {
                        case AudioDeviceType.Playback:
                            ShowPlaybackChanged(audioChangeEvent.AudioDevice.FriendlyName);
                            break;
                        case AudioDeviceType.Recording:
                            ShowRecordingChanged(audioChangeEvent.AudioDevice.FriendlyName);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    UpdateImageContextMenu(audioDeviceType, audioChangeEvent);
                };
            AppModel.Instance.SelectedDeviceChanged += (sender, deviceListChanged) => { UpdateAvailableDeviceList(); };
            AppModel.Instance.NewVersionReleased += (sender, @event) =>
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
            AppModel.Instance.InitializeMain();
        }

        private void UpdateImageContextMenu(AudioDeviceType audioDeviceType, AudioChangeEvent audioChangeEvent)
        {
            foreach (
                var toolStripDevItem in
                    _selectionMenu.Items.OfType<ToolStripDeviceItem>().Where(item => item.AudioDevice.Type == audioDeviceType))
            {
                toolStripDevItem.Image = toolStripDevItem.AudioDevice.FriendlyName == audioChangeEvent.AudioDevice.FriendlyName
                    ? Resources.Check
                    : null;
            }
        }

        private void NewReleaseAvailable(object sender, UpdateChecker.NewReleaseEvent newReleaseEvent)
        {
            _updateMenuItem.Tag = newReleaseEvent.Release;
            _updateMenuItem.Text = $"Update Available ({newReleaseEvent.Release.ReleaseVersion})";
            _updateMenuItem.Enabled = true;
            _trayIcon.ShowBalloonTip(3000, $"Version {newReleaseEvent.Release.ReleaseVersion} is available", "Right click on the tray icon to download.", ToolTipIcon.Info);
        }

        private void UpdateAvailableDeviceList()
        {
            var audioDevices = AppModel.Instance.AvailablePlaybackDevices;
            _deviceListChanged = !_availablePlaybackDevices.Equals(audioDevices);
            if (_deviceListChanged)
            {
                _availablePlaybackDevices = audioDevices;
            }
            audioDevices = AppModel.Instance.AvailableRecordingDevices;
            _deviceListChanged = !_availableRecordingDevices.Equals(audioDevices);
            if (_deviceListChanged)
            {
                _availableRecordingDevices = audioDevices;
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

                if (_availablePlaybackDevices.Count < 0 && _availableRecordingDevices.Count < 0)
                {
                    AppLogger.Log.Info("Device list empty");
                    return;
                }

                _selectionMenu.Items.Clear();
                AppLogger.Log.Info("Set tray icon menu devices");
                foreach (var item in _availablePlaybackDevices)
                {
                    _selectionMenu.Items.Add(new ToolStripDeviceItem(DeviceClicked, item));
                }
                if (_availableRecordingDevices.Count > 0)
                {
                    _selectionMenu.Items.Add("-");
                    foreach (var item in _availableRecordingDevices)
                    {
                         _selectionMenu.Items.Add(new ToolStripDeviceItem(DeviceClicked, item));
                    }
                }
                _deviceListChanged = false;
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

        private void ShowPlaybackChanged(string deviceName)
        {
            _trayIcon.ShowBalloonTip(500, "SoundSwitch: Playback device changed", deviceName, ToolTipIcon.Info);
        }

        private void ShowRecordingChanged(string deviceName)
        {
            _trayIcon.ShowBalloonTip(500, "SoundSwitch: Recording device changed", deviceName, ToolTipIcon.Info);
        }

        /// <summary>
        ///     Notification for when there are no devices configured
        /// </summary>
        public void ShowNoDevices()
        {
            AppLogger.Log.Error("No devices available");
            _trayIcon.ShowBalloonTip(3000, "SoundSwitch: Configuration needed", "No devices available to switch to. Open configuration by right-clicking on the SoundSwitch icon. ", ToolTipIcon.Warning);
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