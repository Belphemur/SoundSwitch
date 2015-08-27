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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework;
using SoundSwitch.Properties;

namespace SoundSwitch.Forms
{
    public partial class Settings : Form
    {
        public const string DevicesDelimiter = ";;;";

        public Settings()
        {
            InitializeComponent();
            Icon = System.Drawing.Icon.FromHandle(Resources.Settings.GetHicon());
            var toolTip = new ToolTip();
            toolTip.SetToolTip(closeButton, "Changes are automatically saved");

            var toolTipComm = new ToolTip();
            toolTipComm.SetToolTip(communicationCheckbox, "When checked, switch also the default Communications device");

            txtHotkey.KeyDown += TxtHotkey_KeyDown;
            txtHotkey.Text = Main.Instance.HotKeysString;


            RunAtStartup.Checked = Main.Instance.RunAtStartup;
            communicationCheckbox.Checked = Main.Instance.ChangeCommunications;
            PopulateAudioList();
        }

        private void RunAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            var ras = RunAtStartup.Checked;
            try
            {
                Main.Instance.RunAtStartup = ras;
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error changing run at startup setting: " + ex.Message);
                RunAtStartup.Checked = Main.Instance.RunAtStartup;
            }
        }

        private void TxtHotkey_KeyDown(object sender, KeyEventArgs e)
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
                txtHotkey.ForeColor = Color.Crimson;
            }

            txtHotkey.Text = $"{displayString}{keyCode}";
            if (!string.IsNullOrEmpty(keyCode))
            {
                txtHotkey.ForeColor = Color.Green;
                Main.Instance.SetHotkeyCombination(new HotKeys(e.KeyCode, modifierKeys));
            }
        }

        private void PopulateAudioList()
        {
            try
            {
                var selected = Main.Instance.SelectedDevicesList;
                var audioDeviceWrappers = AudioController.getAllAudioDevices()
                    .Where(wrapper => !string.IsNullOrEmpty(wrapper.FriendlyName))
                    .OrderBy(s => s.FriendlyName);
                deviceListView.SmallImageList = new ImageList();
             
                deviceListView.Columns.Add("Device", -3, HorizontalAlignment.Center);
                foreach (var device in audioDeviceWrappers)
                {
                    deviceListView.SmallImageList.Images.Add(device.FriendlyName, AudioDeviceIconExtractor.ExtractIconFromAudioDevice(device, false));
                    if (selected.Contains(device.FriendlyName))
                    {
                        deviceListView.Items.Add(new ListViewItem
                        {
                            Text = device.FriendlyName,
                            Checked = true,
                            Group = deviceListView.Groups["selectedGroup"],
                            ImageKey = device.FriendlyName
                        });
                    }
                    else
                    {
                        deviceListView.Items.Add(new ListViewItem
                        {
                            Text = device.FriendlyName,
                            Checked = false,
                            Group = deviceListView.Groups["unSelectedGroup"],
                            ImageKey = device.FriendlyName
                        });
                    }
                }
           
            }
            catch (Exception e)
            {
                MessageBox.Show(@"Error: " + e.Message);
            }
            finally
            {
                deviceListView.ItemCheck += LstDevicesItemChecked;
            }
        }

        private void LstDevicesItemChecked(object sender, ItemCheckEventArgs e)
        {
            try
            {
                Main.Instance.AddRemoveDevice(deviceListView.Items[e.Index].ToString());
            }
            catch (Exception)
            {
                e.NewValue = e.CurrentValue;
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void communicationCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            var comm = communicationCheckbox.Checked;
            try
            {
                Main.Instance.ChangeCommunications = comm;
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error changing run at startup setting: " + ex.Message);
                communicationCheckbox.Checked = Main.Instance.ChangeCommunications;
            }
        }
    }
}