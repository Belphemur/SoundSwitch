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
using System.Linq;
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using SoundSwitch.Util;

namespace SoundSwitch.Forms
{
    public partial class Settings : Form
    {
        public const string DevicesDelimiter = ";;;";
        private readonly Main _main;

        public Settings(Main main)
        {
            _main = main;
            InitializeComponent();
            var toolTip = new ToolTip();
            toolTip.SetToolTip(closeButton, "Changes are automatically saved");

            txtHotkey.KeyDown += TxtHotkey_KeyDown;
            txtHotkey.Text = _main.HotKeysString;


            RunAtStartup.Checked = _main.RunAtStartup;
            PopulateAudioList();
        }

        private void RunAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            var ras = RunAtStartup.Checked;
            try
            {
                _main.RunAtStartup = ras;
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error changing run at startup setting: " + ex.Message);
                RunAtStartup.Checked = _main.RunAtStartup;
            }
        }

        private void TxtHotkey_KeyDown(object sender, KeyEventArgs e)
        {
            if (new[] {8, 17, 18, 46}.Contains(e.KeyValue)) return;

            ModifierKeys modifierKeys = 0;
            var displayString = "";

            if (e.Control)
            {
                modifierKeys |= Util.ModifierKeys.Control;
                displayString += "Ctrl+";
            }
            if (e.Alt)
            {
                modifierKeys |= Util.ModifierKeys.Alt;
                displayString += "Alt+";
            }

            txtHotkey.Text = $"{displayString}{e.KeyCode}";
            _main.SetHotkeyCombination(e.KeyCode, modifierKeys);
        }

        private void PopulateAudioList()
        {
            lstDevices.Items.Clear();
            try
            {
                var selected = _main.SelectedDevicesList;
                var audioDeviceWrappers = AudioController.getAllAudioDevices()
                    .Where(wrapper => !string.IsNullOrEmpty(wrapper.FriendlyName)).Select(wrapper => wrapper.FriendlyName).OrderBy(s => s);
                foreach (
                    var item in
                        audioDeviceWrappers.Where(name => selected.Contains(name)))
                {
                    lstDevices.Items.Add(item, true);
                }

                foreach (
                    var item in
                        audioDeviceWrappers.Where(name => !selected.Contains(name)))
                {
                    lstDevices.Items.Add(item, false);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(@"Error: " + e.Message);
            }
            finally
            {
                lstDevices.ItemCheck += LstDevicesItemChecked;
            }
        }

        private void LstDevicesItemChecked(object sender, ItemCheckEventArgs e)
        {
            try
            {
                _main.SetDeviceSelection(lstDevices.Items[e.Index].ToString(), e.NewValue == CheckState.Checked);
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
    }
}