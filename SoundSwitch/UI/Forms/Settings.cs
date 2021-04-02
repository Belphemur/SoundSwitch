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
using NAudio.CoreAudioApi;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Common.Framework.Icon;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.Factory;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Framework.TrayIcon.Icon;
using SoundSwitch.Framework.TrayIcon.TooltipInfoManager;
using SoundSwitch.Framework.TrayIcon.TooltipInfoManager.TootipInfo;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Localization;
using SoundSwitch.Localization.Factory;
using SoundSwitch.Model;
using SoundSwitch.Properties;
using SoundSwitch.UI.Component;
using SoundSwitch.UI.Component.ListView;
using SoundSwitch.Util;

namespace SoundSwitch.UI.Forms
{
    public sealed partial class SettingsForm : Form
    {
        private static readonly Icon RessourceSettingsIcon = Resources.SettingsIcon;

        private bool _loaded;
        private IAudioDeviceLister _audioDeviceLister;

        public SettingsForm(IAudioDeviceLister audioDeviceLister)
        {
            _audioDeviceLister = audioDeviceLister;
            // Form itself
            InitializeComponent();
            Icon = RessourceSettingsIcon;
            Text = AssemblyUtils.GetReleaseState() == AssemblyUtils.ReleaseState.Beta
                ? $"{SettingsStrings.settings} {AssemblyUtils.GetReleaseState()}"
                : SettingsStrings.settings;
            LocalizeForm();

            var closeToolTip = new ToolTip();
            closeToolTip.SetToolTip(closeButton, SettingsStrings.closeTooltip);

            hotKeyControl.HotKey = AppConfigs.Configuration.PlaybackHotKey;
            hotKeyControl.Tag =
                new Tuple<DataFlow, HotKey>(DataFlow.Render, AppConfigs.Configuration.PlaybackHotKey);
            hotKeyControl.Enabled = hotkeysCheckBox.Checked = AppConfigs.Configuration.PlaybackHotKey.Enabled;

            var hotkeysToolTip = new ToolTip();
            hotkeysToolTip.SetToolTip(hotkeysCheckBox, SettingsStrings.hotkeysTooltip);

            // Settings - Basic
            startWithWindowsCheckBox.Checked = AppModel.Instance.RunAtStartup;

            new IconChangerFactory().ConfigureListControl(iconChangeChoicesComboBox);
            iconChangeChoicesComboBox.SelectedValue = AppConfigs.Configuration.SwitchIcon;

            var iconChangeToolTip = new ToolTip();
            iconChangeToolTip.SetToolTip(iconChangeLabel, SettingsStrings.iconChange_tooltip);

            // Settings - Audio
            switchCommunicationDeviceCheckBox.Checked = AppModel.Instance.SetCommunications;

            var switchCommunicationsDeviceToolTip = new ToolTip();
            switchCommunicationsDeviceToolTip.SetToolTip(switchCommunicationDeviceCheckBox,
                SettingsStrings.communicationsDeviceTooltip);

            foregroundAppCheckbox.Checked = AppModel.Instance.SwitchForegroundProgram;

            var foregroundAppToolTip = new ToolTip();
            foregroundAppToolTip.SetToolTip(foregroundAppCheckbox, SettingsStrings.foregroundAppTooltip);

            usePrimaryScreenCheckbox.Checked = AppModel.Instance.NotifyUsingPrimaryScreen;
            usePrimaryScreenCheckbox.Enabled = AppModel.Instance.NotificationSettings == NotificationTypeEnum.BannerNotification;

            var usePrimaryScreenTooltip = new ToolTip();
            usePrimaryScreenTooltip.SetToolTip(usePrimaryScreenCheckbox, SettingsStrings.usePrimaryScreenTooltip);

            var notificationToolTip = new ToolTip();
            notificationToolTip.SetToolTip(notificationComboBox, SettingsStrings.notificationTooltip);

            var notificationFactory = new NotificationFactory();
            notificationFactory.ConfigureListControl(notificationComboBox);
            notificationComboBox.SelectedValue = AppModel.Instance.NotificationSettings;

            selectSoundFileDialog.Filter = SettingsStrings.audioFiles + @" (*.wav;*.mp3)|*.wav;*.mp3;*.aiff";
            selectSoundFileDialog.FileOk += SelectSoundFileDialogOnFileOk;
            selectSoundFileDialog.CheckFileExists = true;
            selectSoundFileDialog.CheckPathExists = true;

            var soundSupported = notificationFactory.Get(AppModel.Instance.NotificationSettings).SupportCustomSound() !=
                                 NotificationCustomSoundEnum.NotSupported;
            selectSoundButton.Visible = soundSupported;

            var removeCustomSoundToolTip = new ToolTip();
            removeCustomSoundToolTip.SetToolTip(deleteSoundButton, SettingsStrings.disableCustomSoundTooltip);
            try
            {
                deleteSoundButton.Visible = soundSupported && AppModel.Instance.CustomNotificationSound != null;
            }
            catch (CachedSoundFileNotExistsException)
            {
            }

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
            includeBetaVersionsToolTip.SetToolTip(includeBetaVersionsCheckBox,
                SettingsStrings.updateIncludeBetaVersionsTooltip);

            // Settings - Language
            new LanguageFactory().ConfigureListControl(languageComboBox);
            languageComboBox.SelectedValue = AppModel.Instance.Language;

            PopulateSettings();

            _loaded = true;
        }

        private void PopulateSettings()
        {
            PopulateAudioDevices();
            playbackListView.SetGroupsState(ListViewGroupState.Collapsible);
            recordingListView.SetGroupsState(ListViewGroupState.Collapsible);
            // Profiles
            PopulateProfiles();
        }

        private void PopulateProfiles()
        {
            profilesListView.Columns.Add(SettingsStrings.profile_name, 50, HorizontalAlignment.Left);
            profilesListView.Columns.Add(SettingsStrings.profile_program, 100, HorizontalAlignment.Left);
            profilesListView.Columns.Add(SettingsStrings.hotkeys, 150, HorizontalAlignment.Left);
            profilesListView.Columns.Add(SettingsStrings.playback, 150, HorizontalAlignment.Left);
            profilesListView.Columns.Add(SettingsStrings.recording, 150, HorizontalAlignment.Left);
            profilesListView.Columns.Add(SettingsStrings.communication, 150, HorizontalAlignment.Left);

            RefreshProfiles();
        }

        public void RefreshProfiles()
        {
            ListViewItem ProfileToListViewItem(Profile profile)
            {
                var listViewItem = new ListViewItem(profile.Name) { Tag = profile };
                Icon appIcon = null;
                DeviceFullInfo recording = null;
                DeviceFullInfo playback = null;
                DeviceFullInfo communication = null;

                var applicationTrigger = profile.Triggers.FirstOrDefault(trig => trig.Type == TriggerFactory.Enum.Process);
                var hotkeyTrigger = profile.Triggers.FirstOrDefault(trig => trig.Type == TriggerFactory.Enum.HotKey);

                if (applicationTrigger != null)
                {
                    try
                    {
                        appIcon = IconExtractor.Extract(applicationTrigger.ApplicationPath, 0, false);
                    }
                    catch
                    {
                        appIcon = Resources.program;
                    }
                }

                if (profile.Playback != null)
                {
                    playback = _audioDeviceLister.PlaybackDevices.FirstOrDefault(info =>
                        info.Equals(profile.Playback));
                }

                if (profile.Recording != null)
                {
                    recording = _audioDeviceLister.RecordingDevices.FirstOrDefault(info =>
                        info.Equals(profile.Recording));
                }

                if (profile.Communication != null)
                {
                    communication = _audioDeviceLister.PlaybackDevices.FirstOrDefault(info =>
                        info.Equals(profile.Communication));
                }

                listViewItem.SubItems.AddRange(new[]
                {
                    new ListViewItem.ListViewSubItem(listViewItem, applicationTrigger?.ApplicationPath.Split('\\').Last() ?? "")
                        {Tag = appIcon},
                    new ListViewItem.ListViewSubItem(listViewItem, hotkeyTrigger?.HotKey.ToString() ?? ""),
                    new ListViewItem.ListViewSubItem(listViewItem, playback?.NameClean ?? profile.Playback?.ToString() ?? "")
                        {Tag = playback?.SmallIcon},
                    new ListViewItem.ListViewSubItem(listViewItem,
                        recording?.NameClean ?? profile.Recording?.ToString() ?? "") {Tag = recording?.SmallIcon},
                    new ListViewItem.ListViewSubItem(listViewItem,
                        communication?.NameClean ?? profile.Communication?.ToString() ?? "") {Tag = communication?.SmallIcon},
                });

                return listViewItem;
            }

            profilesListView.Items.Clear();

            foreach (var profile in AppModel.Instance.ProfileManager.Profiles)
            {
                var listViewItem = ProfileToListViewItem(profile);
                profilesListView.Items.Add(listViewItem);
            }

            if (AppModel.Instance.ProfileManager.Profiles.Count > 0)
            {
                foreach (ColumnHeader column in profilesListView.Columns)
                {
                    column.Width = -2;
                }
            }
        }

        private void PopulateAudioDevices()
        {
            var selectedDevices = AppModel.Instance.SelectedDevices.ToArray();
            PopulateAudioList(playbackListView, selectedDevices,
                _audioDeviceLister.PlaybackDevices);
            PopulateAudioList(recordingListView, selectedDevices,
                _audioDeviceLister.RecordingDevices);
        }

        private void LocalizeForm()
        {
            // TabPages
            playbackTabPage.Text = SettingsStrings.playback;
            playbackListView.Groups[0].Header = SettingsStrings.selected;

            recordingTabPage.Text = SettingsStrings.recording;
            recordingListView.Groups[0].Header = SettingsStrings.selected;

            appSettingTabPage.Text = SettingsStrings.settings;
            tabProfile.Text = SettingsStrings.profile_tab;

            // Settings - Basic
            basicSettingsGroupBox.Text = SettingsStrings.basicSettings;
            startWithWindowsCheckBox.Text = SettingsStrings.startWithWindows;
            iconChangeLabel.Text = SettingsStrings.iconChange;

            // Settings - Audio
            audioSettingsGroupBox.Text = SettingsStrings.audioSettings;
            switchCommunicationDeviceCheckBox.Text = SettingsStrings.communicationsDevice;
            notificationLabel.Text = SettingsStrings.notification;
            tooltipOnHoverLabel.Text = SettingsStrings.tooltipOnHover;
            cycleThroughLabel.Text = SettingsStrings.cycleThrough;
            foregroundAppCheckbox.Text = SettingsStrings.foregroundApp;
            usePrimaryScreenCheckbox.Text = SettingsStrings.usePrimaryScreen;

            // Settings - Update
            updateSettingsGroupBox.Text = SettingsStrings.updateSettings;
            updateSilentRadioButton.Text = SettingsStrings.updateInstallAutomatically;
            updateNotifyRadioButton.Text = SettingsStrings.updateNotify;
            updateNeverRadioButton.Text = SettingsStrings.updateNever;
            includeBetaVersionsCheckBox.Text = SettingsStrings.updateIncludeBetaVersions;

            // Settings - Language
            languageGroupBox.Text = SettingsStrings.language;

            // Settings - Profile
            profileExplanationLabel.Text = SettingsStrings.profile_explanation;
            addProfileButton.Text = SettingsStrings.profile_addButton;
            deleteProfileButton.Text = SettingsStrings.profile_deleteButton;
            editProfileButton.Text = SettingsStrings.profile_button_edit;


            // Misc
            hotkeysCheckBox.Text = SettingsStrings.hotkeyEnabled;
            closeButton.Text = SettingsStrings.close;
        }


        private void SelectSoundFileDialogOnFileOk(object sender, CancelEventArgs cancelEventArgs)
        {
            AppModel.Instance.CustomNotificationSound = new CachedSound(selectSoundFileDialog.FileName);
            deleteSoundButton.Visible = true;
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
                hotKeyControl.HotKey = AppConfigs.Configuration.PlaybackHotKey;
                hotKeyControl.Tag =
                    new Tuple<HotKeyAction, HotKey>(HotKeyAction.Playback, AppConfigs.Configuration.PlaybackHotKey);
                hotkeysCheckBox.Checked = AppConfigs.Configuration.PlaybackHotKey.Enabled;
            }
            else if (tabControlSender.SelectedTab == recordingTabPage)
            {
                SetHotkeysFieldsVisibility(true);
                hotKeyControl.HotKey = AppConfigs.Configuration.RecordingHotKey;
                hotKeyControl.Tag =
                    new Tuple<HotKeyAction, HotKey>(HotKeyAction.Recording, AppConfigs.Configuration.RecordingHotKey);
                hotkeysCheckBox.Checked = AppConfigs.Configuration.RecordingHotKey.Enabled;
            }
            else
            {
                SetHotkeysFieldsVisibility(false);
            }
        }

        private void SetHotkeysFieldsVisibility(bool visibility)
        {
            hotkeysCheckBox.Visible = visibility;
            hotKeyControl.Visible = visibility;
        }

        private void notificationComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            var value = (DisplayEnumObject<NotificationTypeEnum>)((ComboBox)sender).SelectedItem;

            if (value == null)
                return;

            var notificationType = value.Enum;
            if (notificationType == AppModel.Instance.NotificationSettings)
                return;

            var supportCustomSound =
                new NotificationFactory().Get(notificationType).SupportCustomSound();
            selectSoundButton.Visible = supportCustomSound != NotificationCustomSoundEnum.NotSupported;

            if (supportCustomSound == NotificationCustomSoundEnum.Required)
            {
                try
                {
                    var sound = AppModel.Instance.CustomNotificationSound;
                    deleteSoundButton.Visible = true;
                }
                catch (CachedSoundFileNotExistsException)
                {
                    selectSoundFileDialog.ShowDialog(this);
                }
            }

            usePrimaryScreenCheckbox.Enabled = notificationType == NotificationTypeEnum.BannerNotification;

            AppModel.Instance.NotificationSettings = notificationType;
        }

        private void tooltipInfoComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            var value = (DisplayEnumObject<TooltipInfoTypeEnum>)((ComboBox)sender).SelectedItem;

            if (value == null)
                return;


            TooltipInfoManager.CurrentTooltipInfo = value.Enum;
        }

        private void cyclerComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            var value = (DisplayEnumObject<DeviceCyclerTypeEnum>)((ComboBox)sender).SelectedItem;

            if (value == null)
                return;

            DeviceCyclerManager.CurrentCycler = value.Enum;
        }

        private void languageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;

            var value = (DisplayEnumObject<Language>)((ComboBox)sender).SelectedItem;

            if (value == null)
                return;

            AppModel.Instance.Language = value.Enum;

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
            var tuple = (Tuple<HotKeyAction, HotKey>)hotKeyControl.Tag;
            var currentState = tuple.Item2.Enabled;
            hotKeyControl.Enabled = tuple.Item2.Enabled = hotkeysCheckBox.Checked;
            if (currentState != tuple.Item2.Enabled)
                AppModel.Instance.SetHotkeyCombination(tuple.Item2, tuple.Item1, true);
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

        private void PopulateAudioList(ListView listView, IEnumerable<DeviceInfo> selectedDevices,
            IEnumerable<DeviceFullInfo> audioDevices)
        {
            try
            {
                PopulateDeviceTypeGroups(listView);

                listView.SmallImageList = new ImageList
                {
                    ImageSize = new Size(32, 32),
                    ColorDepth = ColorDepth.Depth32Bit
                };

                listView.Columns.Add("Device", -3, HorizontalAlignment.Center);
                var items =
                    audioDevices.Select(device =>
                    {
                        AddDeviceIconSmallImage(device, listView);

                        return GenerateListViewItem(device, selectedDevices, listView);
                    }).OrderBy(item => item.Text);
                listView.Items.AddRange(items.ToArray());
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
        private ListViewItem GenerateListViewItem(DeviceFullInfo device, IEnumerable<DeviceInfo> selected,
            ListView listView)
        {
            var listViewItem = new ListViewItem
            {
                Text = device.NameClean,
                ImageKey = device.IconPath,
                Tag = device
            };
            var selectedDevice = device;
            var isSelected = selected.Contains(selectedDevice);
            if (selectedDevice.State == DeviceState.Active && isSelected)
            {
                listViewItem.Group = listView.Groups["selectedGroup"];
            }
            else
            {
                listViewItem.Group = GetGroup(device.State, listView);
            }

            listViewItem.Checked = isSelected;

            return listViewItem;
        }

        /// <summary>
        /// Using the DeviceClassIconPath, get the Icon
        /// </summary>
        /// <param name="device"></param>
        /// <param name="listView"></param>
        private void AddDeviceIconSmallImage(DeviceFullInfo device, ListView listView)
        {
            if (!listView.SmallImageList.Images.ContainsKey(device.IconPath))
            {
                listView.SmallImageList.Images.Add(device.IconPath,
                    device.LargeIcon);
            }
        }

        private void ListViewItemChecked(object sender, ItemCheckEventArgs e)
        {
            try
            {
                switch (e.NewValue)
                {
                    case CheckState.Checked:
                        AppModel.Instance.SelectDevice((DeviceFullInfo)((ListView)sender).Items[e.Index].Tag);
                        break;
                    case CheckState.Unchecked:
                        AppModel.Instance.UnselectDevice((DeviceFullInfo)((ListView)sender).Items[e.Index].Tag);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception exception)
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

        private void deleteSoundButton_Click(object sender, EventArgs e)
        {
            AppModel.Instance.CustomNotificationSound = null;
            deleteSoundButton.Visible = false;
        }

        private void ForegroundAppCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            AppModel.Instance.SwitchForegroundProgram = foregroundAppCheckbox.Checked;
        }

        private void usePrimaryScreenCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            AppModel.Instance.NotifyUsingPrimaryScreen = usePrimaryScreenCheckbox.Checked;
        }

        private void iconChangeChoicesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            var comboBox = (ComboBox)sender;

            if (comboBox == null)
                return;

            var item = (DisplayEnumObject<IconChangerFactory.ActionEnum>)iconChangeChoicesComboBox.SelectedItem;
            AppConfigs.Configuration.SwitchIcon = item.Enum;
            AppConfigs.Configuration.Save();

            new IconChangerFactory().Get(item.Enum).ChangeIcon(AppModel.Instance.TrayIcon);
        }

        private void addProfileButton_Click(object sender, EventArgs e)
        {
            var form = new UpsertProfileExtended(new Profile(), _audioDeviceLister.PlaybackDevices, _audioDeviceLister.RecordingDevices, this);
            form.Show(this);
        }

        private void profilesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            editProfileButton.Enabled = profilesListView.SelectedIndices.Count == 1;
            deleteProfileButton.Enabled = profilesListView.SelectedIndices.Count > 0;
        }

        private void deleteProfileButton_Click(object sender, EventArgs e)
        {
            if (profilesListView.SelectedItems.Count <= 0)
            {
                return;
            }

            var profiles = profilesListView.SelectedItems.Cast<ListViewItem>()
                .Select(item => (Profile)item.Tag);
            AppModel.Instance.ProfileManager.DeleteProfiles(profiles);
            deleteProfileButton.Enabled = false;
            editProfileButton.Enabled = false;
            RefreshProfiles();
        }

        private void hotKeyControl_HotKeyChanged(object sender, HotKeyTextBox.Event e)
        {
            var tuple = (Tuple<HotKeyAction, HotKey>)hotKeyControl.Tag;
            if (tuple == null)
                return;

            var newTuple = new Tuple<HotKeyAction, HotKey>(tuple.Item1, hotKeyControl.HotKey);
            hotKeyControl.Tag = newTuple;

            AppModel.Instance.SetHotkeyCombination(newTuple.Item2, newTuple.Item1);
        }

        private void editProfileButton_Click(object sender, EventArgs e)
        {
            if (profilesListView.SelectedItems.Count <= 0)
            {
                return;
            }

            var profile = (Profile)profilesListView.SelectedItems[0].Tag;
            var form = new UpsertProfileExtended(profile, _audioDeviceLister.PlaybackDevices, _audioDeviceLister.RecordingDevices, this, true);
            form.Show(this);
        }

        private void profilesListView_DoubleClick(object sender, EventArgs e)
        {
            editProfileButton_Click(sender, e);
        }
    }
}