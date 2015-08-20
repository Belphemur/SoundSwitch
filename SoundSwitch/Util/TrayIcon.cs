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
using SoundSwitch.Forms;
using SoundSwitch.Properties;

namespace SoundSwitch.Util
{
    public sealed class TrayIcon : IDisposable
    {
        private readonly Main _main;
        private readonly ContextMenuStrip _selectionMenu = new ContextMenuStrip();
        private readonly ContextMenuStrip _settingsMenu = new ContextMenuStrip();

        private readonly NotifyIcon _trayIcon = new NotifyIcon
        {
            Icon = Icon.FromHandle(Resources.SwitchIcon.GetHicon()),
            Visible = true,
            Text = Application.ProductName
        };

        private List<AudioDeviceWrapper> _availableAudioDeviceWrappers;
        private bool _deviceListChanged = true;

        public TrayIcon(Main main)
        {
            _trayIcon.ContextMenuStrip = _settingsMenu;
            _main = main;
            _availableAudioDeviceWrappers = _main.AvailableAudioDevices;

            _settingsMenu.Items.Add("Playback Devices", null, (sender, e) =>
            {
                Process.Start(new ProcessStartInfo("cmd", "/c " + "control mmsys.cpl sounds")
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            });
            // settingsMenu.Items.Add("Mixer", null, (sender, e) =>  ?? not sure how to display the mixer
            _settingsMenu.Items.Add("-");
            _settingsMenu.Items.Add("Settings", Resources.Settings, (sender, e) => ShowSettings());
            _settingsMenu.Items.Add("About", Resources.Help, (sender, e) => new About().Show());
            _settingsMenu.Items.Add("-");
            _settingsMenu.Items.Add("Exit", null, (sender, e) => Application.Exit());

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
            GC.SuppressFinalize(this);
        }

        private void SetEventHandlers()
        {
            _main.ErrorTriggered += (sender, exception) => ShowError(exception.Exception.Message);
            _main.AudioDeviceChanged +=
                (sender, audioDeviceWrapper) => ShowAudioChanged(audioDeviceWrapper.AudioDevice.FriendlyName);
            _main.SelectedDeviceChanged += (sender, changed) =>
            {
                UpdateAvailableDeviceList();
            };
            WindowsEventNotifier.EventTriggered += (sender, @event) =>
            {
                if (@event.Type != WindowsEventNotifier.EventType.DeviceChange)
                    return;
                UpdateAvailableDeviceList();
            };
        }

        private void UpdateAvailableDeviceList()
        {
            var audioDevices = _main.AvailableAudioDevices;
            _deviceListChanged = !_availableAudioDeviceWrappers.Equals(audioDevices);
            if (_deviceListChanged)
            {
                _availableAudioDeviceWrappers = audioDevices;
            }
        }

        public void ShowSettings()
        {
            new Settings(_main).Show();
        }

        /// <summary>
        ///     Sets the names of devices that show up in the menu
        /// </summary>
        public void UpdateDeviceSelectionList()
        {
            if (!_deviceListChanged)
            {
                return;
            }

            if (_availableAudioDeviceWrappers.Count < 0)
            {
                return;
            }

            _selectionMenu.Items.Clear();

            foreach (var item in _availableAudioDeviceWrappers)
            {
                _selectionMenu.Items.Add(new ToolStripDeviceItem(DeviceClicked, item));
            }
            _deviceListChanged = false;
        }

        private void DeviceClicked(object sender, EventArgs e)
        {
            try
            {
                var item = (ToolStripDeviceItem) sender;
                _main.SetActiveDevice(item.AudioDevice);
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