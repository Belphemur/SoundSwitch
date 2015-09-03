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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Model;
using SoundSwitch.Properties;
using SoundSwitch.Util;

namespace SoundSwitch.UI.Forms
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            Icon = Icon.FromHandle(Resources.Settings.GetHicon());
            var toolTip = new ToolTip();
            toolTip.SetToolTip(closeButton, "Changes are automatically saved");

            var toolTipComm = new ToolTip();
            toolTipComm.SetToolTip(communicationCheckbox, "When checked, switch also the default Communications device");

            hotkeyTextBox.KeyDown += (sender, args) => SetHotkey(args);
            hotkeyTextBox.Text = AppConfigs.Configuration.PlaybackHotKeys.ToString();
            hotkeyTextBox.Tag = AudioDeviceType.Playback;

            RunAtStartup.Checked = AppModel.Instance.RunAtStartup;
            communicationCheckbox.Checked = AppModel.Instance.SetCommunications;
            var audioDeviceLister = new AudioDeviceLister(DeviceState.All);
            PopulateAudioList(playbackListView, AppModel.Instance.SelectedPlaybackDevicesList,
                audioDeviceLister.GetPlaybackDevices());
            PopulateAudioList(recordingListView, AppModel.Instance.SelectedRecordingDevicesList,
                audioDeviceLister.GetRecordingDevices());
        }

        private void SetHotkey(KeyEventArgs e)
        {
            HotKeys.ModifierKeys modifierKeys = 0;
            var displayString = "";

            if (e.Control)
            {
                modifierKeys |= HotKeys.ModifierKeys.Control;
                displayString += "Ctrl+";
            }
            if (e.Alt)
            {
                modifierKeys |= HotKeys.ModifierKeys.Alt;
                displayString += "Alt+";
            }
            if (e.Shift)
            {
                modifierKeys |= HotKeys.ModifierKeys.Shift;
                displayString += "Shift+";
            }
            var keyCode = e.KeyCode.ToString();
            if (new[] {8, 16, 17, 18, 46}.Contains(e.KeyValue))
            {
                keyCode = "";
                hotkeyTextBox.ForeColor = Color.Crimson;
            }

            hotkeyTextBox.Text = $"{displayString}{keyCode}";
            if (!string.IsNullOrEmpty(keyCode))
            {
                hotkeyTextBox.ForeColor = AppModel.Instance.SetHotkeyCombination(new HotKeys(e.KeyCode, modifierKeys),
                    (AudioDeviceType) hotkeyTextBox.Tag)
                    ? Color.Green
                    : Color.Red;
            }
            e.Handled = true;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        #region Basic Settings (CheckBoxes)

        private void RunAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            var ras = RunAtStartup.Checked;
            try
            {
                AppModel.Instance.RunAtStartup = ras;
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error changing run at startup setting: " + ex.Message);
                RunAtStartup.Checked = AppModel.Instance.RunAtStartup;
            }
        }

        private void communicationCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            var comm = communicationCheckbox.Checked;
            try
            {
                AppModel.Instance.SetCommunications = comm;
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error changing run at startup setting: " + ex.Message);
                communicationCheckbox.Checked = AppModel.Instance.SetCommunications;
            }
        }

        #endregion

        #region Device List Playback

        private void PopulateAudioList(ListView listView, ICollection<string> selectedDevices,
            ICollection<IAudioDevice> audioDevices)
        {
            try
            {
                PopulateDeviceTypeGroups(listView);

                var audioDeviceWrappers = audioDevices
                    .Where(wrapper => !string.IsNullOrEmpty(wrapper.FriendlyName))
                    .OrderBy(s => s.FriendlyName);
                listView.SmallImageList = new ImageList {ImageSize = new Size(32,32), ColorDepth = ColorDepth.Depth32Bit};

                listView.Columns.Add("Device", -3, HorizontalAlignment.Center);
                foreach (var device in audioDeviceWrappers)
                {
                    AddDeviceIconSmallImage(device, listView);

                    var listViewItem = GenerateListViewItem(device, selectedDevices, listView);

                    listView.Items.Add(listViewItem);
                }
            }
            finally
            {
                listView.ItemCheck += ListViewItemChecked;
            }
        }

        /// <summary>
        ///     Using the information of the AudioDeviceWrapper, generate a ListViewItem
        /// </summary>
        /// <param name="device"></param>
        /// <param name="selected"></param>
        /// <param name="listView"></param>
        /// <returns></returns>
        private ListViewItem GenerateListViewItem(IAudioDevice device, ICollection<string> selected, ListView listView)
        {
            var listViewItem = new ListViewItem
            {
                Text = device.FriendlyName,
                ImageKey = device.DeviceClassIconPath,
                Tag = device
            };

            if (selected.Contains(device.FriendlyName))
            {
                listViewItem.Checked = true;
                listViewItem.Group = listView.Groups["selectedGroup"];
            }
            else
            {
                listViewItem.Checked = false;
                listViewItem.Group = GetGroup(device.DeviceState, listView);
            }
            return listViewItem;
        }

        /// <summary>
        ///     Using the DeviceClassIconPath, get the Icon
        /// </summary>
        /// <param name="device"></param>
        /// <param name="listView"></param>
        private void AddDeviceIconSmallImage(IAudioDevice device, ListView listView)
        {
            if (!listView.SmallImageList.Images.ContainsKey(device.DeviceClassIconPath))
            {
                listView.SmallImageList.Images.Add(device.DeviceClassIconPath,
                    AudioDeviceIconExtractor.ExtractIconFromAudioDevice(device, true)); 
            }
        }

        private void ListViewItemChecked(object sender, ItemCheckEventArgs e)
        {
            try
            {
                switch (e.NewValue)
                {
                    case CheckState.Checked:
                        AppModel.Instance.SelectDevice((IAudioDevice) ((ListView)sender).Items[e.Index].Tag);
                        break;
                    case CheckState.Unchecked:
                        AppModel.Instance.UnselectDevice((IAudioDevice)((ListView)sender).Items[e.Index].Tag);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception)
            {
                e.NewValue = e.CurrentValue;
            }
        }

        #region Groups

        /// <summary>
        ///     Get the ListViewItem group in which the device belongs.
        /// </summary>
        /// <param name="deviceState"></param>
        /// <param name="listView"></param>
        /// <returns></returns>
        private ListViewGroup GetGroup(DeviceState deviceState, ListView listView)
        {
            switch (deviceState)
            {
                case DeviceState.Active:
                    return listView.Groups[DeviceState.Active.ToString()];
                default:
                    return listView.Groups[DeviceState.NotPresent.ToString()];
            }
        }

        private void PopulateDeviceTypeGroups(ListView listView)
        {
            listView.Groups.Add(new ListViewGroup(DeviceState.Active.ToString(), "Connected"));
            listView.Groups.Add(new ListViewGroup(DeviceState.NotPresent.ToString(), "Disconnected"));
        }

        #endregion

        #endregion

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tabControlSender = (TabControl) sender;
            if (tabControlSender.SelectedTab == playbackPage)
            {
                hotkeyTextBox.Text = AppConfigs.Configuration.PlaybackHotKeys.ToString();
                hotkeyTextBox.Tag = AudioDeviceType.Playback;
            } else if (tabControlSender.SelectedTab == recordingPage)
            {
                hotkeyTextBox.Text = AppConfigs.Configuration.RecordingHotKeys.ToString();
                hotkeyTextBox.Tag = AudioDeviceType.Recording;
            }
        }
    }
}