/********************************************************************
* Copyright (C) 2015 Jeroen Pelgrims
* Copyright (C) 2015-2017 Antoine Aflalo
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
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.TooltipInfoManager;
using SoundSwitch.Framework.TooltipInfoManager.TootipInfo;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.Properties;
using SoundSwitch.Util;

namespace SoundSwitch.UI.Forms
{
    public sealed partial class SettingsForm : Form
    {
        private readonly bool _loaded;

        public SettingsForm()
        {
            // Form itself
            InitializeComponent();
            Icon = Resources.SettingsIcon;
            Text = AssemblyUtils.GetReleaseState() == AssemblyUtils.ReleaseState.Beta ?
                   $"{SettingsStrings.settings} {AssemblyUtils.GetReleaseState()}" :
                   SettingsStrings.settings;
            LocalizeForm();

            var closeToolTip = new ToolTip();
            closeToolTip.SetToolTip(closeButton, SettingsStrings.closeTooltip);

            hotkeysTextBox.Text = AppConfigs.Configuration.PlaybackHotKeys.Display();
            hotkeysTextBox.Tag = new Tuple<AudioDeviceType, HotKeys>(AudioDeviceType.Playback, AppConfigs.Configuration.PlaybackHotKeys);
            hotkeysTextBox.Enabled = hotkeysCheckBox.Checked = AppConfigs.Configuration.PlaybackHotKeys.Enabled;
            hotkeysTextBox.KeyDown += (sender, args) => SetHotkey(args);
            var hotkeysToolTip = new ToolTip();
            hotkeysToolTip.SetToolTip(hotkeysCheckBox, SettingsStrings.hotkeysTooltip);

            // Settings - Basic
            startWithWindowsCheckBox.Checked = AppModel.Instance.RunAtStartup;
            keepSystemTrayIconCheckBox.Checked = AppConfigs.Configuration.KeepSystrayIcon;

            var keepSystemTrayIconToolTip = new ToolTip();
            keepSystemTrayIconToolTip.SetToolTip(keepSystemTrayIconCheckBox, SettingsStrings.keepSystemTrayIconTooltip);

            // Settings - Audio
            switchCommunicationDeviceCheckBox.Checked = AppModel.Instance.SetCommunications;

            var switchCommunicationsDeviceToolTip = new ToolTip();
            switchCommunicationsDeviceToolTip.SetToolTip(switchCommunicationDeviceCheckBox, SettingsStrings.communicationsDeviceTooltip);

            var notificationToolTip = new ToolTip();
            notificationToolTip.SetToolTip(notificationComboBox, SettingsStrings.notificationTooltip);

            var notificationFactory = new NotificationFactory();
            notificationFactory.ConfigureListControl(notificationComboBox);
            notificationComboBox.SelectedValue = AppModel.Instance.NotificationSettings;

            selectSoundFileDialog.Filter = SettingsStrings.audioFiles + @" (*.wav;*.mp3)|*.wav;*.mp3;*.aiff";
            selectSoundFileDialog.FileOk += SelectSoundFileDialogOnFileOk;
            selectSoundFileDialog.CheckFileExists = true;
            selectSoundFileDialog.CheckPathExists = true;

            selectSoundButton.Visible = notificationFactory.Get(AppModel.Instance.NotificationSettings).SupportCustomSound() != NotificationCustomSoundEnum.NotSupported;
            var selectSoundButtonToolTip = new ToolTip();
            selectSoundButtonToolTip.SetToolTip(selectSoundButton, SettingsStrings.selectSoundButtonTooltip);

            new TooltipInfoFactory().ConfigureListControl(tooltipInfoComboBox);
            tooltipInfoComboBox.SelectedValue = TooltipInfoManager.CurrentTooltipInfo;

            new DeviceCyclerFactory().ConfigureListControl(cycleThroughComboBox);
            cycleThroughComboBox.SelectedValue = DeviceCyclerManager.CurrentCycler;

            var cycleThroughToolTip = new ToolTip();
            cycleThroughToolTip.SetToolTip(cycleThroughComboBox, SettingsStrings.cycleThroughTooltip);

            // Settings - Update
            includeBetaVersionsCheckBox.Checked = AppModel.Instance.IncludeBetaVersions;

            switch (AppModel.Instance.UpdateMode)
            {
                case UpdateMode.Silent:
                    updateSilentRadioButton.Checked = true;
                    break;
                case UpdateMode.Notify:
                    updateNotifyRadioButton.Checked = true;
                    break;
                case UpdateMode.Never:
                    updateNeverRadioButton.Checked = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var updateSilentToolTip = new ToolTip();
            updateSilentToolTip.SetToolTip(updateSilentRadioButton, SettingsStrings.updateInstallAutomaticallyTooltip);
            var updateNotifyToolTip = new ToolTip();
            updateNotifyToolTip.SetToolTip(updateNotifyRadioButton, SettingsStrings.updateNotifyTooltip);
            var updateNeverToolTip = new ToolTip();
            updateNeverToolTip.SetToolTip(updateNeverRadioButton, SettingsStrings.updateNeverTooltip);

            var includeBetaVersionsToolTip = new ToolTip();
            includeBetaVersionsToolTip.SetToolTip(includeBetaVersionsCheckBox, SettingsStrings.updateIncludeBetaVersionsTooltip);

            // Settings - Language
            languageComboBox.SelectedIndex = (int)AppConfigs.Configuration.Language;

            // Playback and Recording
            var audioDeviceLister = new AudioDeviceLister(DeviceState.All);
            PopulateAudioList(playbackListView, AppModel.Instance.SelectedPlaybackDevicesList, audioDeviceLister.GetPlaybackDevices());
            PopulateAudioList(recordingListView, AppModel.Instance.SelectedRecordingDevicesList, audioDeviceLister.GetRecordingDevices());

            _loaded = true;
        }

        private void LocalizeForm()
        {
            // TabPages
            playbackTabPage.Text = SettingsStrings.playback;
            playbackListView.Groups[0].Header = SettingsStrings.selected;

            recordingTabPage.Text = SettingsStrings.recording;
            recordingListView.Groups[0].Header = SettingsStrings.selected;

            appSettingTabPage.Text = SettingsStrings.settings;

            // Settings - Basic
            basicSettingsGroupBox.Text = SettingsStrings.basicSettings;
            startWithWindowsCheckBox.Text = SettingsStrings.startWithWindows;
            keepSystemTrayIconCheckBox.Text = SettingsStrings.keepSystemTrayIcon;

            // Settings - Audio
            audioSettingsGroupBox.Text = SettingsStrings.audioSettings;
            switchCommunicationDeviceCheckBox.Text = SettingsStrings.communicationsDevice;
            notificationLabel.Text = SettingsStrings.notification;
            tooltipOnHoverLabel.Text = SettingsStrings.tooltipOnHover;
            cycleThroughLabel.Text = SettingsStrings.cycleThrough;

            // Settings - Update
            updateSettingsGroupBox.Text = SettingsStrings.updateSettings;
            updateSilentRadioButton.Text = SettingsStrings.updateInstallAutomatically;
            updateNotifyRadioButton.Text = SettingsStrings.updateNotify;
            updateNeverRadioButton.Text = SettingsStrings.updateNever;
            includeBetaVersionsCheckBox.Text = SettingsStrings.updateIncludeBetaVersions;

            // Settings - Language
            languageGroupBox.Text = SettingsStrings.language;

            // Misc
            hotkeysLabel.Text = SettingsStrings.hotkeys;
            closeButton.Text = SettingsStrings.close;
        }

        private void SelectSoundFileDialogOnFileOk(object sender, CancelEventArgs cancelEventArgs)
        {
            AppModel.Instance.CustomNotificationSound = new CachedSound(selectSoundFileDialog.FileName);
        }

        private void SetHotkey(KeyEventArgs e)
        {
            HotKeys.ModifierKeys modifierKeys = 0;
            var displayString = "";
            foreach (var pressedModifier in KeyboardWindowsAPI.GetPressedModifiers())
            {
                if ((pressedModifier & Keys.Modifiers) == Keys.Control)
                {
                    modifierKeys |= HotKeys.ModifierKeys.Control;
                    displayString += "Ctrl+";
                }
                if ((pressedModifier & Keys.Modifiers) == Keys.Alt)
                {
                    modifierKeys |= HotKeys.ModifierKeys.Alt;
                    displayString += "Alt+";
                }
                if ((pressedModifier & Keys.Modifiers) == Keys.Shift)
                {
                    modifierKeys |= HotKeys.ModifierKeys.Shift;
                    displayString += "Shift+";
                }
                if (pressedModifier == Keys.LWin || pressedModifier == Keys.RWin)
                {
                    modifierKeys |= HotKeys.ModifierKeys.Win;
                    displayString += "Win+";
                }
            }

            var normalPressedKeys = KeyboardWindowsAPI.GetNormalPressedKeys();
            var key = normalPressedKeys.FirstOrDefault();


            if (key == Keys.None)
            {
                hotkeysTextBox.Text = displayString;
                hotkeysTextBox.ForeColor = Color.Crimson;
            }
            else
            {
                hotkeysTextBox.Text = displayString + key;
                var tuple = (Tuple<AudioDeviceType, HotKeys>)hotkeysTextBox.Tag;
                var newTuple = new Tuple<AudioDeviceType, HotKeys>(tuple.Item1, new HotKeys(e.KeyCode, modifierKeys));
                hotkeysTextBox.Tag = newTuple;
                hotkeysTextBox.ForeColor = AppModel.Instance.SetHotkeyCombination(newTuple.Item2, newTuple.Item1) ? Color.Green : Color.Red;
            }
            e.Handled = true;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tabControlSender = (TabControl)sender;
            if (tabControlSender.SelectedTab == playbackTabPage)
            {
                SetHotkeysFieldsVisibility(true);
                hotkeysTextBox.Text = AppConfigs.Configuration.PlaybackHotKeys.Display();
                hotkeysTextBox.Tag = new Tuple<AudioDeviceType, HotKeys>(AudioDeviceType.Playback, AppConfigs.Configuration.PlaybackHotKeys);
                hotkeysCheckBox.Checked = AppConfigs.Configuration.PlaybackHotKeys.Enabled;
            }
            else if (tabControlSender.SelectedTab == recordingTabPage)
            {
                SetHotkeysFieldsVisibility(true);
                hotkeysTextBox.Text = AppConfigs.Configuration.RecordingHotKeys.Display();
                hotkeysTextBox.Tag = new Tuple<AudioDeviceType, HotKeys>(AudioDeviceType.Recording, AppConfigs.Configuration.RecordingHotKeys);
                hotkeysCheckBox.Checked = AppConfigs.Configuration.RecordingHotKeys.Enabled;
            }
            else if (tabControlSender.SelectedTab == appSettingTabPage)
            {
                SetHotkeysFieldsVisibility(false);
            }
        }

        private void SetHotkeysFieldsVisibility(bool visibility)
        {
            hotkeysCheckBox.Visible = visibility;
            hotkeysTextBox.Visible = visibility;
            hotkeysLabel.Visible = visibility;
        }

        private void notificationComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            var value = ((ComboBox)sender).SelectedValue;

            if (value == null)
                return;

            NotificationTypeEnum notificationType = (NotificationTypeEnum)value;
            if (notificationType == AppModel.Instance.NotificationSettings)
                return;

            NotificationCustomSoundEnum supportCustomSound = new NotificationFactory().Get(notificationType).SupportCustomSound();
            selectSoundButton.Visible = supportCustomSound != NotificationCustomSoundEnum.NotSupported;

            if (supportCustomSound == NotificationCustomSoundEnum.Required)
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

            AppModel.Instance.NotificationSettings = notificationType;
        }

        private void tooltipInfoComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            var value = ((ComboBox)sender).SelectedValue;

            if (value == null)
                return;

            var tooltip = (TooltipInfoTypeEnum)value;
            TooltipInfoManager.CurrentTooltipInfo = tooltip;
        }

        private void cyclerComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            var value = ((ComboBox)sender).SelectedValue;

            if (value == null)
                return;

            var cycler = (DeviceCyclerTypeEnum)value;
            DeviceCyclerManager.CurrentCycler = cycler;
        }

        private void languageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            var comboBox = (ComboBox)sender;

            if (comboBox == null)
                return;

            AppModel.Instance.Language = (Language)languageComboBox.SelectedIndex;

            if (MessageBox.Show(SettingsStrings.languageRestartRequired,
                                SettingsStrings.languageRestartRequiredCaption,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Program.RestartApp();
            }
        }

        private void selectSoundButton_Click(object sender, EventArgs e)
        {
            var result = selectSoundFileDialog.ShowDialog(this);
            if (result == DialogResult.Cancel)
            {
                AppModel.Instance.CustomNotificationSound = null;
            }
        }

        private void hotkeysCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            var tuple = (Tuple<AudioDeviceType, HotKeys>)hotkeysTextBox.Tag;
            var currentState = tuple.Item2.Enabled;
            hotkeysTextBox.Enabled = tuple.Item2.Enabled = hotkeysCheckBox.Checked;
            if (currentState != tuple.Item2.Enabled)
                AppModel.Instance.SetHotkeyCombination(tuple.Item2, tuple.Item1);
        }

        #region Basic Settings (CheckBoxes)

        private void RunAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            var ras = startWithWindowsCheckBox.Checked;
            try
            {
                AppModel.Instance.RunAtStartup = ras;
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error changing run at startup setting: " + ex.Message);
                startWithWindowsCheckBox.Checked = AppModel.Instance.RunAtStartup;
            }
        }

        private void communicationCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            var comm = switchCommunicationDeviceCheckBox.Checked;
            try
            {
                AppModel.Instance.SetCommunications = comm;
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error changing run at startup setting: " + ex.Message);
                switchCommunicationDeviceCheckBox.Checked = AppModel.Instance.SetCommunications;
            }
        }

        private void betaVersionCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            AppModel.Instance.IncludeBetaVersions = includeBetaVersionsCheckBox.Checked;
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
        /// Using the information of the AudioDeviceWrapper, generate a ListViewItem
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
        /// Using the DeviceClassIconPath, get the Icon
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
                        AppModel.Instance.SelectDevice((IAudioDevice)((ListView)sender).Items[e.Index].Tag);
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
        /// Get the ListViewItem group in which the device belongs.
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
            listView.Groups.Add(new ListViewGroup(DeviceState.Active.ToString(), SettingsStrings.connected));
            listView.Groups.Add(new ListViewGroup(DeviceState.NotPresent.ToString(), SettingsStrings.disconnected));
        }

        #endregion

        #endregion

        private void checkboxSystrayIcon_CheckedChanged(object sender, EventArgs e)
        {
            AppConfigs.Configuration.KeepSystrayIcon = keepSystemTrayIconCheckBox.Checked;
            AppConfigs.Configuration.Save();
            AppModel.Instance.TrayIcon.UpdateIcon();
        }

        private void updateSilentRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (updateSilentRadioButton.Checked)
            {
                AppModel.Instance.UpdateMode = UpdateMode.Silent;
            }
        }

        private void updateNotifyRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (updateNotifyRadioButton.Checked)
            {
                AppModel.Instance.UpdateMode = UpdateMode.Notify;
            }
        }

        private void updateNeverRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (updateNeverRadioButton.Checked)
            {
                AppModel.Instance.UpdateMode = UpdateMode.Never;
            }
        }
    }
}