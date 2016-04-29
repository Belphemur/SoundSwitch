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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.TooltipInfoManager;
using SoundSwitch.Framework.TooltipInfoManager.TootipInfo;
using SoundSwitch.Model;
using SoundSwitch.Properties;
using SoundSwitch.Util;

namespace SoundSwitch.UI.Forms
{
    public partial class Settings : Form
    {
        private readonly bool _loaded;

        public Settings()
        {
            InitializeComponent();
            Name = SettingsString.settings;
            Text = SettingsString.settings;
            if (AssemblyUtils.GetReleaseState() == AssemblyUtils.ReleaseState.Beta)
            {
                Name = SettingsString.settings + ' ' + AssemblyUtils.GetReleaseState();
                Text = SettingsString.settings + ' ' + AssemblyUtils.GetReleaseState();
            }
            Icon = Icon.FromHandle(Resources.Settings.GetHicon());
            var toolTip = new ToolTip();
            toolTip.SetToolTip(closeButton, SettingsString.closeTooltip);

            var toolTipComm = new ToolTip();
            toolTipComm.SetToolTip(communicationCheckbox, SettingsString.commTooltip);

            hotkeyTextBox.KeyDown += (sender, args) => SetHotkey(args);
            hotkeyTextBox.Text = AppConfigs.Configuration.PlaybackHotKeys.Display();
            hotkeyTextBox.Tag = new Tuple<AudioDeviceType, HotKeys>(AudioDeviceType.Playback,
                AppConfigs.Configuration.PlaybackHotKeys);
            hotkeyTextBox.Enabled = hotkeysCheckbox.Checked = AppConfigs.Configuration.PlaybackHotKeys.Enabled;
            var toolTipHotkeys = new ToolTip();
            toolTipHotkeys.SetToolTip(hotkeysCheckbox, SettingsString.hotkeyUncheckExplanation);

            RunAtStartup.Checked = AppModel.Instance.RunAtStartup;
            communicationCheckbox.Checked = AppModel.Instance.SetCommunications;

            var audioDeviceLister = new AudioDeviceLister(DeviceState.All);
            PopulateAudioList(playbackListView, AppModel.Instance.SelectedPlaybackDevicesList,
                audioDeviceLister.GetPlaybackDevices());
            PopulateAudioList(recordingListView, AppModel.Instance.SelectedRecordingDevicesList,
                audioDeviceLister.GetRecordingDevices());
            notifLabel.Text = SettingsString.notification;

            var toolTipNotification = new ToolTip();
            toolTipNotification.SetToolTip(notificationComboBox, Notifications.explanation);
            notificationComboBox.DisplayMember = "Label";
            notificationComboBox.ValueMember = "Type";
            notificationComboBox.DataSource = new NotificationFactory().AllImplementations.Values.Select(notification => notification.Displayer()).ToArray();
            notificationComboBox.SelectedValue = AppModel.Instance.NotificationSettings;

            betaVersionCheckbox.Checked = AppModel.Instance.SubscribedBetaVersions;
            var toolTipBeta = new ToolTip();
            toolTipBeta.SetToolTip(betaVersionCheckbox, SettingsString.betaExplanation);

            selectSoundFileDialog.Filter = SettingsString.supportedAudio + @" (*.wav;*.mp3)|*.wav;*.mp3;*.aiff";
            selectSoundFileDialog.FileOk += SelectSoundFileDialogOnFileOk;
            selectSoundFileDialog.CheckFileExists = true;

            var toolTipSoundButton = new ToolTip();
            toolTipSoundButton.SetToolTip(selectSoundButton, SettingsString.selectSoundButtonTooltip);
            selectSoundButton.Visible = AppModel.Instance.NotificationSettings ==
                                        NotificationTypeEnum.CustomNotification;

            stealthUpdateCheckbox.Checked = AppModel.Instance.StealthUpdate;
            var toolTipStealthUpdate = new ToolTip();
            toolTipStealthUpdate.SetToolTip(stealthUpdateCheckbox, SettingsString.stealthUpdateExplanation);

            tooltipInfoComboBox.DataSource = new TooltipInfoFactory().AllImplementations.Values.ToArray();

            _loaded = true;
        }

        private void SelectSoundFileDialogOnFileOk(object sender, CancelEventArgs cancelEventArgs)
        {
            AppModel.Instance.CustomNotificationSound = new CachedSound(selectSoundFileDialog.FileName);
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
                var tuple = (Tuple<AudioDeviceType, HotKeys>) hotkeyTextBox.Tag;
                var newTuple = new Tuple<AudioDeviceType, HotKeys>(tuple.Item1, new HotKeys(e.KeyCode, modifierKeys));
                hotkeyTextBox.Tag = newTuple;
                hotkeyTextBox.ForeColor = AppModel.Instance.SetHotkeyCombination(newTuple.Item2,
                    newTuple.Item1)
                    ? Color.Green
                    : Color.Red;
            }
            e.Handled = true;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tabControlSender = (TabControl) sender;
            if (tabControlSender.SelectedTab == playbackPage)
            {
                SetHotkeysFieldsVisibility(true);
                hotkeyTextBox.Text = AppConfigs.Configuration.PlaybackHotKeys.Display();
                hotkeyTextBox.Tag = new Tuple<AudioDeviceType, HotKeys>(AudioDeviceType.Playback,
                    AppConfigs.Configuration.PlaybackHotKeys);
                hotkeysCheckbox.Checked = AppConfigs.Configuration.PlaybackHotKeys.Enabled;
            }
            else if (tabControlSender.SelectedTab == recordingPage)
            {
                SetHotkeysFieldsVisibility(true);
                hotkeyTextBox.Text = AppConfigs.Configuration.RecordingHotKeys.Display();
                hotkeyTextBox.Tag = new Tuple<AudioDeviceType, HotKeys>(AudioDeviceType.Recording,
                    AppConfigs.Configuration.RecordingHotKeys);
                hotkeysCheckbox.Checked = AppConfigs.Configuration.RecordingHotKeys.Enabled;
            } else if (tabControlSender.SelectedTab == appSettingTabPage)
            {
                SetHotkeysFieldsVisibility(false);
            }
        }

        private void SetHotkeysFieldsVisibility(bool visibility)
        {
            hotkeysCheckbox.Visible = visibility;
            hotkeyTextBox.Visible = visibility;
            hotkeysLabel.Visible = visibility;
        }

        private void notificationComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            var value = ((ComboBox) sender).SelectedValue;

            if (value == null)
                return;

            if ((NotificationTypeEnum) value == AppModel.Instance.NotificationSettings)
                return;

            var isCustomNotification = (NotificationTypeEnum) value == NotificationTypeEnum.CustomNotification;
            selectSoundButton.Visible = isCustomNotification;

            if (isCustomNotification)
            {
                try
                {
                    var sound = AppModel.Instance.CustomNotificationSound;
                }
                catch (CachedSoundFileNotExistsException)
                {
                    selectSoundFileDialog.ShowDialog(this);
                }
            }

            AppModel.Instance.NotificationSettings = (NotificationTypeEnum) value;
        }


        private void tooltipInfoComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            var value = ((ComboBox)sender).SelectedValue;

            if (value == null)
                return;

            var tooltip = (ITooltipInfo) value;
            TooltipInfoManager.CurrentTooltipInfo = tooltip.Type();
        }

        private void selectSoundButton_Click(object sender, EventArgs e)
        {
            selectSoundFileDialog.ShowDialog(this);
        }

        private void hotkeysCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            var tuple = (Tuple<AudioDeviceType, HotKeys>) hotkeyTextBox.Tag;
            var currentState = tuple.Item2.Enabled;
            hotkeyTextBox.Enabled = tuple.Item2.Enabled = hotkeysCheckbox.Checked;
            if (currentState != tuple.Item2.Enabled)
                AppModel.Instance.SetHotkeyCombination(tuple.Item2, tuple.Item1);
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

        private void betaVersionCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            AppModel.Instance.SubscribedBetaVersions = betaVersionCheckbox.Checked;
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
                listView.SmallImageList = new ImageList
                {
                    ImageSize = new Size(32, 32),
                    ColorDepth = ColorDepth.Depth32Bit
                };

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

            if (selected.Contains(device.Id))
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
                        AppModel.Instance.SelectDevice((IAudioDevice) ((ListView) sender).Items[e.Index].Tag);
                        break;
                    case CheckState.Unchecked:
                        AppModel.Instance.UnselectDevice((IAudioDevice) ((ListView) sender).Items[e.Index].Tag);
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
            listView.Groups.Add(new ListViewGroup(DeviceState.Active.ToString(), SettingsString.connected));
            listView.Groups.Add(new ListViewGroup(DeviceState.NotPresent.ToString(), SettingsString.disconnected));
        }

        #endregion

        #endregion

        private void stealthUpdateCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            AppModel.Instance.StealthUpdate = stealthUpdateCheckbox.Checked;
        }

    }
}