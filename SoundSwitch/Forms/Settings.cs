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


        public static Settings Instance { get; } = new Settings();

        static Settings()
        {
            Instance.Closing += (sender, e) =>
            {
                e.Cancel = true;
                Instance.Hide();
            };
        }

        private Settings()
        {
            InitializeComponent();
            var toolTip = new ToolTip();
            toolTip.SetToolTip(closeButton,"Changes are automatically saved");
            
            txtHotkey.KeyDown += TxtHotkey_KeyDown;
            if (!string.IsNullOrEmpty(Properties.Settings.Default.HotkeyModifierKeys) &&
                !string.IsNullOrEmpty(Properties.Settings.Default.HotkeyKey))
            {
                txtHotkey.Text =
                    $"{Properties.Settings.Default.HotkeyModifierKeys.Replace(", ", "+")}+{Properties.Settings.Default.HotkeyKey}";
            }

            RunAtStartup.Checked = Main.Instance.RunAtStartup;
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
            Main.Instance.SetHotkeyCombination(e.KeyCode, modifierKeys);
        }


        public new void Show()
        {
            lstDevices.Items.Clear();
            try
            {
                // disable click event 
                lstDevices.ItemCheck -= lstDevices_ItemCheck;

                var selected = Main.Instance.SelectedDevicesList;
                foreach (var item in AudioController.getAllAudioDevices().Where(wrapper => !string.IsNullOrEmpty(wrapper.FriendlyName)))
                {
                    var idx = lstDevices.Items.Add(item.FriendlyName);
                    lstDevices.SetItemCheckState(idx, selected.Contains(item.FriendlyName) ? CheckState.Checked : CheckState.Unchecked);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(@"Error: " + e.Message);
            }
            finally
            {
                // re-enable click event 
                lstDevices.ItemCheck += lstDevices_ItemCheck;
            }

            base.Show();

        }

        private void lstDevices_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                Main.Instance.SetDeviceSelection(lstDevices.Items[e.Index].ToString(), e.NewValue == CheckState.Checked);
            }
            catch (Exception)
            {
                e.NewValue = e.CurrentValue;
            }
        }

        private void Settings_VisibleChanged(object sender, EventArgs e)
        {
            // make sure the device list is in sync
            Main.Instance.PopulateTrayIconDeviceMenu();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
