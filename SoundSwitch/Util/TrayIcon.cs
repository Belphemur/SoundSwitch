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
            _updateMenuItem = new ToolStripMenuItem(TrayIconStrings.NoUpdate, Resources.Update, OnUpdateClick) {Enabled = false};
            _trayIcon.ContextMenuStrip = _settingsMenu;

            _availablePlaybackDevices = AppModel.Instance.AvailablePlaybackDevices;
            _availableRecordingDevices = AppModel.Instance.AvailableRecordingDevices;

            PopulateSettingsMenu();

            _selectionMenu.Items.Add(TrayIconStrings.NoDevSel, Resources.Settings, (sender, e) => ShowSettings());

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
                    var audioDeviceType = audioChangeEvent.device.Type;
                    switch (audioDeviceType)
                    {
                        case AudioDeviceType.Playback:
                            ShowPlaybackChanged(audioChangeEvent.device.FriendlyName);
                            break;
                        case AudioDeviceType.Recording:
                            ShowRecordingChanged(audioChangeEvent.device.FriendlyName);
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


            AppModel.Instance.DeviceRemoved += (sender, @event) =>
            {
                RemoveDeviceFromContextMenu(@event);
            };

            AppModel.Instance.DeviceAdded += (sender, @event) =>
            {
                AddDeviceToContextMenu(@event);
            };

            AppModel.Instance.DeviceStateChanged += (sender, @event) =>
            {
                if (@event.newState == DeviceState.Active)
                {
                    AddDeviceToContextMenu(@event);
                }
                else
                {
                    RemoveDeviceFromContextMenu(@event);
                }
            };

            AppModel.Instance.InitializeMain();
        }

        private void AddDeviceToContextMenu(DeviceEvent @event)
        {
            var result = false;
            switch (@event.device.Type)
            {
                case AudioDeviceType.Playback:
                    if (AppModel.Instance.SelectedPlaybackDevicesList.Contains(@event.device.FriendlyName))
                    {
                        _availablePlaybackDevices.Add(@event.device);
                        result = true;
                    }
                    break;
                case AudioDeviceType.Recording:
                    if (AppModel.Instance.SelectedRecordingDevicesList.Contains(@event.device.FriendlyName))
                    {
                        _availableRecordingDevices.Add(@event.device);
                        result = true;
                    }
                    break;
            }
            if (result)
            {
                _deviceListChanged = true;
                AppLogger.Log.Info("Added device in list: ", @event.device);
            }
        }

        private void RemoveDeviceFromContextMenu(DeviceEvent @event)
        {
            var result = false;
            switch (@event.device.Type)
            {
                case AudioDeviceType.Playback:
                    result = _availablePlaybackDevices.Remove(@event.device);
                    break;
                case AudioDeviceType.Recording:
                    result = _availableRecordingDevices.Remove(@event.device);
                    break;
            }
            if (result)
            {
                _deviceListChanged = true;
                AppLogger.Log.Info("Removed device from list: ", @event.device);
            }
        }

        private void UpdateImageContextMenu(AudioDeviceType audioDeviceType, DeviceDefaultChangedEvent audioChangeEvent)
        {
            if (audioChangeEvent.role != Role.Console)
                return;

            foreach (
                var toolStripDevItem in
                    _selectionMenu.Items.OfType<ToolStripDeviceItem>().Where(item => item.AudioDevice.Type == audioDeviceType))
            {
                toolStripDevItem.Image = toolStripDevItem.AudioDevice.FriendlyName == audioChangeEvent.device.FriendlyName
                    ? Resources.Check
                    : null;
            }
        }

        private void NewReleaseAvailable(object sender, UpdateChecker.NewReleaseEvent newReleaseEvent)
        {
            _updateMenuItem.Tag = newReleaseEvent.Release;
            _updateMenuItem.Text = string.Format(TrayIconStrings.updateAvailable, newReleaseEvent.Release.ReleaseVersion);
            _updateMenuItem.Enabled = true;
            _trayIcon.ShowBalloonTip(3000, string.Format(TrayIconStrings.versionAvailable, newReleaseEvent.Release.ReleaseVersion), TrayIconStrings.howDownloadUpdate, ToolTipIcon.Info);
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
            _trayIcon.ShowBalloonTip(500, string.Format(TrayIconStrings.playbackChanged, Application.ProductName), deviceName, ToolTipIcon.Info);
        }

        private void ShowRecordingChanged(string deviceName)
        {
            _trayIcon.ShowBalloonTip(500, string.Format(TrayIconStrings.recordingChanged, Application.ProductName), deviceName, ToolTipIcon.Info);
        }

        /// <summary>
        ///     Notification for when there are no devices configured
        /// </summary>
        public void ShowNoDevices()
        {
            AppLogger.Log.Info("No devices available");
            _trayIcon.ShowBalloonTip(3000, string.Format(TrayIconStrings.confNeeded, Application.ProductName), TrayIconStrings.confNeededExp, ToolTipIcon.Warning);
        }

        /// <summary>
        ///     shows an error messasge
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="errorTitle"></param>
        public void ShowError(string errorMessage, string errorTitle)
        {
            _trayIcon.ShowBalloonTip(3000, $"{Application.ProductName}: {errorTitle}", errorMessage, ToolTipIcon.Error);
        }
    }
}