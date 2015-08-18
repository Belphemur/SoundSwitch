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
using Settings = SoundSwitch.Forms.Settings;

namespace SoundSwitch.Util
{
    public class TrayIcon : IDisposable
    {
        private readonly NotifyIcon _trayIcon = new NotifyIcon
        {
            Icon = Icon.FromHandle(Resources.SwitchIcon.GetHicon()),
            Visible = true,
            Text = Application.ProductName
        };
        private readonly ContextMenuStrip _settingsMenu = new ContextMenuStrip();

        private readonly ContextMenuStrip _selectionMenu = new ContextMenuStrip();
        private readonly Main _main;


        public TrayIcon(Main main)
        {
            _trayIcon.ContextMenuStrip = _settingsMenu;
            _main = main;

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
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    _trayIcon.ContextMenuStrip = _selectionMenu;
                    MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                    mi.Invoke(_trayIcon, null);

                    _trayIcon.ContextMenuStrip = _settingsMenu;
                }
            };
            SetDeviceList(_main.AvailableAudioDevices);
            SetEventHandlers();

        }

        private void SetEventHandlers()
        {
            _main.ErrorTriggered += (sender, exception) => ShowError(exception.Message);
            _main.AudioDeviceChanged += (sender, audioDeviceWrapper) => ShowAudioChanged(audioDeviceWrapper.FriendlyName);
            _main.SelectedDeviceChanged += (sender, devices) => SetDeviceList(_main.AvailableAudioDevices);
        }

        public void ShowSettings()
        {
            new Settings(_main).Show();
        }

        /// <summary>
        /// Sets the names of devices that show up in the menu
        /// </summary>
        /// <param name="deviceNames"></param>
        public void SetDeviceList(List<AudioDeviceWrapper> deviceNames) 
        {
            if (deviceNames.Count < 0)
                return;

            _selectionMenu.Items.Clear();
            
            foreach (var item in deviceNames)
            {
                _selectionMenu.Items.Add(new ToolStripDeviceItem(deviceClicked, item));
            }
        }

        void deviceClicked(object sender, EventArgs e)
        {
            try
            {
                var item = (ToolStripDeviceItem)sender;
                _main.SetActiveDevice(item.AudioDevice);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Notification that audio has changed
        /// </summary>
        /// <param name="deviceName"></param>
        public void ShowAudioChanged(string deviceName)
        {
            _trayIcon.ShowBalloonTip(500, "SoundSwitch: Audio output changed", deviceName, ToolTipIcon.Info);
        }

        /// <summary>
        /// Notification for when there are no devices configured
        /// </summary>
        public void ShowNoDevices()
        {
            _trayIcon.ShowBalloonTip(3000, "SoundSwitch: Configuration needed", "No devices available to switch to. Open configuration by right-clicking on the SoundSwitch icon. ", ToolTipIcon.Warning);
        }

        /// <summary>
        /// shows an error messasge
        /// </summary>
        /// <param name="errorMessage"></param>
        public void ShowError(string errorMessage, string errorTitle = "Error")
        {
            _trayIcon.ShowBalloonTip(3000, "SoundSwitch: " + errorTitle, errorMessage, ToolTipIcon.Error);
        }

        public void Dispose()
        {
           _trayIcon.Dispose();
        }

    }
}
