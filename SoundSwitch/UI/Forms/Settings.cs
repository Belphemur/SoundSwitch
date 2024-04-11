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
using SoundSwitch.Framework.Banner;
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
                new Tuple<HotKeyAction, HotKey>(HotKeyAction.Playback, AppConfigs.Configuration.PlaybackHotKey);
            hotKeyControl.Enabled = hotkeysCheckBox.Checked = AppConfigs.Configuration.PlaybackHotKey.Enabled;

            muteHotKey.HotKey = AppConfigs.Configuration.MuteRecordingHotKey;
            muteHotKey.Tag = new Tuple<HotKeyAction, HotKey>(HotKeyAction.Mute, AppConfigs.Configuration.MuteRecordingHotKey);
            muteHotKey.Enabled = muteHotKeyCheckbox.Checked = AppConfigs.Configuration.MuteRecordingHotKey.Enabled;

            var hotkeysToolTip = new ToolTip();
            hotkeysToolTip.SetToolTip(hotkeysCheckBox, SettingsStrings.hotkeysTooltip);

            // Settings - Basic
            startWithWindowsCheckBox.Checked = AppModel.Instance.RunAtStartup;

            new IconChangerFactory().ConfigureListControl(iconChangeChoicesComboBox);
            iconChangeChoicesComboBox.SelectedValue = AppConfigs.Configuration.SwitchIcon;

            var iconChangeToolTip = new ToolTip();
            iconChangeToolTip.SetToolTip(iconChangeChoicesComboBox, SettingsStrings.iconChange_tooltip);

            // Settings - Audio
            switchCommunicationDeviceCheckBox.Checked = AppModel.Instance.SetCommunications;

            var switchCommunicationsDeviceToolTip = new ToolTip();
            switchCommunicationsDeviceToolTip.SetToolTip(switchCommunicationDeviceCheckBox,
                SettingsStrings.communicationsDeviceTooltip);

            foregroundAppCheckbox.Checked = AppModel.Instance.SwitchForegroundProgram;

            var foregroundAppToolTip = new ToolTip();
            foregroundAppToolTip.SetToolTip(foregroundAppCheckbox, SettingsStrings.foregroundAppTooltip);

            quickMenuCheckbox.DataBindings.Add(nameof(CheckBox.Checked), AppModel.Instance, nameof(AppModel.QuickMenuEnabled), false, DataSourceUpdateMode.OnPropertyChanged);
            var quickMenuCheckboxToolTip = new ToolTip();
            quickMenuCheckboxToolTip.SetToolTip(quickMenuCheckbox, SettingsStrings.quickMenu_desc);

            keepVolumeCheckbox.DataBindings.Add(nameof(CheckBox.Checked), AppModel.Instance, nameof(AppModel.KeepVolumeEnabled), false, DataSourceUpdateMode.OnPropertyChanged);
            var keepVolumeCheckboxToolTip = new ToolTip();
            keepVolumeCheckboxToolTip.SetToolTip(keepVolumeCheckbox, SettingsStrings.keepVolume_desc);

            new TooltipInfoFactory().ConfigureListControl(tooltipInfoComboBox);
            tooltipInfoComboBox.SelectedValue = TooltipInfoManager.CurrentTooltipInfo;
            
            new DeviceCyclerFactory().ConfigureListControl(cycleThroughComboBox);
            cycleThroughComboBox.SelectedValue = DeviceCyclerManager.CurrentCycler;
            var cycleThroughToolTip = new ToolTip();
            cycleThroughToolTip.SetToolTip(cycleThroughComboBox, SettingsStrings.cycleThroughTooltip);

            // Settings - Notification
            var notificationFactory = new NotificationFactory();
            notificationFactory.ConfigureListControl(notificationComboBox);
            notificationComboBox.SelectedValue = AppModel.Instance.NotificationSettings;
            var notificationToolTip = new ToolTip();
            notificationToolTip.SetToolTip(notificationComboBox, SettingsStrings.notificationTooltip);

            var bannerPositionFactory = new BannerPositionFactory();
            bannerPositionFactory.ConfigureListControl(positionComboBox);
            positionComboBox.SelectedValue = AppModel.Instance.BannerPosition;
            var positionToolTip = new ToolTip();
            positionToolTip.SetToolTip(positionComboBox, SettingsStrings.positionTooltip);

            var singleNotification = new ToolTip();
            singleNotification.SetToolTip(singleNotificationCheckbox, SettingsStrings.notification_single_tooltip);
            singleNotificationCheckbox.DataBindings.Add(nameof(CheckBox.Checked), AppModel.Instance, nameof(AppModel.IsSingleNotification), false, DataSourceUpdateMode.OnPropertyChanged);


            usePrimaryScreenCheckbox.Checked = AppModel.Instance.NotifyUsingPrimaryScreen;
            var usePrimaryScreenTooltip = new ToolTip();
            usePrimaryScreenTooltip.SetToolTip(usePrimaryScreenCheckbox, SettingsStrings.usePrimaryScreenTooltip);
            
            usePrimaryScreenCheckbox.Visible = positionLabel.Visible = positionComboBox.Visible = singleNotificationCheckbox.Visible =  AppModel.Instance.NotificationSettings == NotificationTypeEnum.BannerNotification;
            
            selectSoundFileDialog.Filter = SettingsStrings.audioFiles + @" (*.wav;*.mp3)|*.wav;*.mp3;*.aiff";
            selectSoundFileDialog.FileOk += SelectSoundFileDialogOnFileOk;
            selectSoundFileDialog.CheckFileExists = true;
            selectSoundFileDialog.CheckPathExists = true;

            var supportCustomSound = notificationFactory.Get(AppModel.Instance.NotificationSettings).SupportCustomSound();
            selectSoundButton.Visible = supportCustomSound;
            var selectSoundButtonToolTip = new ToolTip();
            selectSoundButtonToolTip.SetToolTip(selectSoundButton, SettingsStrings.selectSoundButtonTooltip);

            DeleteSoundButtonVisible(supportCustomSound);
            var removeCustomSoundToolTip = new ToolTip();
            removeCustomSoundToolTip.SetToolTip(deleteSoundButton, SettingsStrings.disableCustomSoundTooltip);

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
            new LanguageFactory().ConfigureListControl(languageComboBox);
            languageComboBox.SelectedValue = AppModel.Instance.Language;

            muteHotKey.Visible = false;
            muteHotKeyCheckbox.Visible = false;
            toggleMuteLabel.Visible = false;

            telemetryCheckbox.DataBindings.Add(nameof(CheckBox.Checked), AppModel.Instance, nameof(AppModel.Telemetry), false, DataSourceUpdateMode.OnPropertyChanged);
            var telemetryToolTip = new ToolTip();
            telemetryToolTip.SetToolTip(telemetryCheckbox, SettingsStrings.telemetry_desc);

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
                DeviceFullInfo recordingCommunication = null;

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
                    playback = _audioDeviceLister.GetDevices(DataFlow.Render, DeviceState.Active).FirstOrDefault(info =>
                        info.Equals(profile.Playback));
                }

                if (profile.Recording != null)
                {
                    recording = _audioDeviceLister.GetDevices(DataFlow.Capture, DeviceState.Active).FirstOrDefault(info =>
                        info.Equals(profile.Recording));
                }

                if (profile.Communication != null)
                {
                    communication = _audioDeviceLister.GetDevices(DataFlow.Render, DeviceState.Active).FirstOrDefault(info =>
                        info.Equals(profile.Communication));
                }

                if (profile.RecordingCommunication != null)
                {
                    recordingCommunication = _audioDeviceLister.GetDevices(DataFlow.Capture, DeviceState.Active).FirstOrDefault(info =>
                        info.Equals(profile.RecordingCommunication));
                }

                listViewItem.SubItems.AddRange(new[]
                {
                    new ListViewItem.ListViewSubItem(listViewItem, applicationTrigger?.ApplicationPath.Split('\\').Last() ?? "")
                        { Tag = appIcon },
                    new ListViewItem.ListViewSubItem(listViewItem, hotkeyTrigger?.HotKey.ToString() ?? ""),
                    new ListViewItem.ListViewSubItem(listViewItem, playback?.NameClean ?? profile.Playback?.ToString() ?? "")
                        { Tag = playback?.SmallIcon },
                    new ListViewItem.ListViewSubItem(listViewItem,
                        recording?.NameClean ?? profile.Recording?.ToString() ?? "") { Tag = recording?.SmallIcon },
                    new ListViewItem.ListViewSubItem(listViewItem,
                        communication?.NameClean ?? profile.Communication?.ToString() ?? "") { Tag = communication?.SmallIcon },
                    new ListViewItem.ListViewSubItem(listViewItem,
                        recordingCommunication?.NameClean ?? profile.RecordingCommunication?.ToString() ?? "") { Tag = recordingCommunication?.SmallIcon },
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
            var selectedDevices = AppModel.Instance.SelectedDevices;
            PopulateAudioList(playbackListView, selectedDevices, _audioDeviceLister.GetDevices(DataFlow.Render, DeviceState.Active | DeviceState.Unplugged));
            PopulateAudioList(recordingListView, selectedDevices, _audioDeviceLister.GetDevices(DataFlow.Capture, DeviceState.Active | DeviceState.Unplugged));
        }

        private void LocalizeForm()
        {
            RightToLeft = new LanguageFactory().Get(AppModel.Instance.Language).IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;
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
            tooltipOnHoverLabel.Text = SettingsStrings.tooltipOnHover;
            cycleThroughLabel.Text = SettingsStrings.cycleThrough;
            foregroundAppCheckbox.Text = SettingsStrings.foregroundApp;
            quickMenuCheckbox.Text = SettingsStrings.quickMenu;
            keepVolumeCheckbox.Text = SettingsStrings.keepVolume;

            // Settings - Update
            updateSettingsGroupBox.Text = SettingsStrings.updateSettings;
            updateSilentRadioButton.Text = SettingsStrings.updateInstallAutomatically;
            updateNotifyRadioButton.Text = SettingsStrings.updateNotify;
            updateNeverRadioButton.Text = SettingsStrings.updateNever;
            includeBetaVersionsCheckBox.Text = SettingsStrings.updateIncludeBetaVersions;
            telemetryCheckbox.Text = SettingsStrings.telemetry;

            // Settings - Language
            languageGroupBox.Text = SettingsStrings.language;

            // Settings - Notifcation
            notificationComboBox.Text = SettingsStrings.notification;
            usePrimaryScreenCheckbox.Text = SettingsStrings.usePrimaryScreen;
            positionLabel.Text = SettingsStrings.position;
            singleNotificationCheckbox.Text = SettingsStrings.notification_single;

            // Settings - Profile
            profileExplanationLabel.Text = SettingsStrings.profile_explanation;
            addProfileButton.Text = SettingsStrings.profile_addButton;
            deleteProfileButton.Text = SettingsStrings.profile_deleteButton;
            editProfileButton.Text = SettingsStrings.profile_button_edit;

            // Misc
            hotkeysCheckBox.Text = SettingsStrings.hotkeyEnabled;
            closeButton.Text = SettingsStrings.close;
            toggleMuteLabel.Text = SettingsStrings.mute_toggle_label;
            muteHotKeyCheckbox.Text = SettingsStrings.hotkeyEnabled;

            addProfileButton.Image = Resources.profile_menu_add;
            editProfileButton.Image = Resources.profile_menu_edit;
            deleteProfileButton.Image = Resources.profile_menu_delete;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tabControlSender = (TabControl)sender;
            if (tabControlSender.SelectedTab == playbackTabPage)
            {
                SetHotkeysFieldsVisibility(true);
                hotKeyControl.HotKey = AppConfigs.Configuration.PlaybackHotKey;
                hotKeyControl.Tag =
                    new Tuple<HotKeyAction, HotKey>(HotKeyAction.Playback, AppConfigs.Configuration.PlaybackHotKey);
                hotkeysCheckBox.Checked = AppConfigs.Configuration.PlaybackHotKey.Enabled;
                muteHotKey.Visible = false;
                muteHotKeyCheckbox.Visible = false;
                toggleMuteLabel.Visible = false;
            }
            else if (tabControlSender.SelectedTab == recordingTabPage)
            {
                SetHotkeysFieldsVisibility(true);
                hotKeyControl.HotKey = AppConfigs.Configuration.RecordingHotKey;
                hotKeyControl.Tag =
                    new Tuple<HotKeyAction, HotKey>(HotKeyAction.Recording, AppConfigs.Configuration.RecordingHotKey);
                hotkeysCheckBox.Checked = AppConfigs.Configuration.RecordingHotKey.Enabled;

                muteHotKey.Visible = true;
                muteHotKeyCheckbox.Visible = true;
                toggleMuteLabel.Visible = true;
            }
            else
            {
                muteHotKey.Visible = false;
                muteHotKeyCheckbox.Visible = false;
                toggleMuteLabel.Visible = false;
                SetHotkeysFieldsVisibility(false);
            }
        }

        private void SetHotkeysFieldsVisibility(bool visibility)
        {
            hotkeysCheckBox.Visible = visibility;
            hotKeyControl.Visible = visibility;
        }

        private void SelectSoundFileDialogOnFileOk(object sender, CancelEventArgs cancelEventArgs)
        {
            try
            {
                AppModel.Instance.CustomNotificationSound = new CachedSound(selectSoundFileDialog.FileName);
            }
            catch (Exception)
            {
                MessageBox.Show(@"Please select another file", @"Invalid Sound file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            deleteSoundButton.Visible = true;
        }


        private void DeleteSoundButtonVisible(bool supportCustomSound)
        {
            deleteSoundButton.Visible = supportCustomSound && AppModel.Instance.CustomNotificationSound != null;
        }

        private void HotkeysCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ForceSetHotkeys(sender, hotKeyControl);
        }

        private void MuteHotKeyCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ForceSetHotkeys(sender, muteHotKey);
        }

        private void ForceSetHotkeys(object sender, HotKeyTextBox hotKeyTextBox)
        {
            var control = (CheckBox)sender;
            if (hotKeyTextBox.Tag == null) return;

            var (action, hotKey) = (Tuple<HotKeyAction, HotKey>)hotKeyTextBox.Tag;
            var currentState = hotKey.Enabled;
            hotKeyTextBox.Enabled = hotKey.Enabled = control.Checked;
            if (currentState != hotKey.Enabled)
                AppModel.Instance.SetHotkeyCombination(hotKey, action, true);
        }

        private void HotKeyControl_HotKeyChanged(object sender, HotKeyTextBox.Event e)
        {
            var control = (HotKeyTextBox)sender;
            var tuple = (Tuple<HotKeyAction, HotKey>)control.Tag;
            if (tuple == null) return;

            var newTuple = new Tuple<HotKeyAction, HotKey>(tuple.Item1, control.HotKey);
            hotKeyControl.Tag = newTuple;

            AppModel.Instance.SetHotkeyCombination(newTuple.Item2, newTuple.Item1);
        }

        #region Device List Playback

        private void PopulateAudioList(ListView listView, IEnumerable<DeviceInfo> selectedDevices, IEnumerable<DeviceFullInfo> audioDevices)
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
                                })
                                .OrderBy(item => item.Text);
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
        private ListViewItem GenerateListViewItem(DeviceFullInfo device, IEnumerable<DeviceInfo> selected, ListView listView)
        {
            var listViewItem = new ListViewItem
            {
                Text = device.NameClean,
                ImageKey = device.IconPath,
                Tag = device
            };
            var isSelected = selected.Contains(device);
            if (device.State == DeviceState.Active && isSelected)
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

        #region Profiles

        private void AddProfileButton_Click(object sender, EventArgs e)
        {
            var form = new UpsertProfileExtended(new Profile(), _audioDeviceLister.GetDevices(DataFlow.Render, DeviceState.Active | DeviceState.Unplugged), _audioDeviceLister.GetDevices(DataFlow.Capture, DeviceState.Active | DeviceState.Unplugged), this);
            form.Show(this);
        }

        private void ProfilesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            editProfileButton.Enabled = profilesListView.SelectedIndices.Count == 1;
            deleteProfileButton.Enabled = profilesListView.SelectedIndices.Count > 0;
        }

        private void DeleteProfileButton_Click(object sender, EventArgs e)
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

        private void EditProfileButton_Click(object sender, EventArgs e)
        {
            if (profilesListView.SelectedItems.Count <= 0)
            {
                return;
            }

            var profile = (Profile)profilesListView.SelectedItems[0].Tag;
            var form = new UpsertProfileExtended(profile, _audioDeviceLister.GetDevices(DataFlow.Render, DeviceState.Active | DeviceState.Unplugged), _audioDeviceLister.GetDevices(DataFlow.Capture, DeviceState.Active | DeviceState.Unplugged), this, true);
            form.Show(this);
        }

        private void ProfilesListView_DoubleClick(object sender, EventArgs e)
        {
            EditProfileButton_Click(sender, e);
        }

        #endregion

        #region Basic Settings

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

        private void IconChangeChoicesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_loaded) return;
            var comboBox = (ComboBox)sender;
            if (comboBox == null) return;

            var item = (DisplayEnumObject<IconChangerEnum>)iconChangeChoicesComboBox.SelectedItem;
            AppConfigs.Configuration.SwitchIcon = item.Enum;
            AppConfigs.Configuration.Save();

            new IconChangerFactory().Get(item.Enum).ChangeIcon(AppModel.Instance.TrayIcon);
        }

        #endregion

        #region Audio Settings

        private void CommunicationCheckbox_CheckedChanged(object sender, EventArgs e)
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

        private void ForegroundAppCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            AppModel.Instance.SwitchForegroundProgram = foregroundAppCheckbox.Checked;
        }

        private void KeepVolumeCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            AppModel.Instance.KeepVolumeEnabled = keepVolumeCheckbox.Checked;
        }

        private void TooltipInfoComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!_loaded) return;
            var value = (DisplayEnumObject<TooltipInfoTypeEnum>)((ComboBox)sender).SelectedItem;
            if (value == null) return;

            TooltipInfoManager.CurrentTooltipInfo = value.Enum;
        }

        private void CyclerComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!_loaded) return;
            var value = (DisplayEnumObject<DeviceCyclerTypeEnum>)((ComboBox)sender).SelectedItem;
            if (value == null) return;

            DeviceCyclerManager.CurrentCycler = value.Enum;
        }

        #endregion

        #region Update Settings

        private void UpdateSilentRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (updateSilentRadioButton.Checked)
            {
                AppModel.Instance.UpdateMode = UpdateMode.Silent;
            }
        }

        private void UpdateNotifyRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (updateNotifyRadioButton.Checked)
            {
                AppModel.Instance.UpdateMode = UpdateMode.Notify;
            }
        }

        private void UpdateNeverRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (updateNeverRadioButton.Checked)
            {
                AppModel.Instance.UpdateMode = UpdateMode.Never;
            }
        }

        private void BetaVersionCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            AppModel.Instance.IncludeBetaVersions = includeBetaVersionsCheckBox.Checked;
        }

        #endregion

        #region Language

        private void LanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_loaded) return;
            var value = (DisplayEnumObject<Language>)((ComboBox)sender).SelectedItem;
            if (value == null) return;

            AppModel.Instance.Language = value.Enum;

            if (MessageBox.Show(SettingsStrings.languageRestartRequired,
                    SettingsStrings.languageRestartRequiredCaption,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Program.RestartApp();
            }
        }

        #endregion

        #region Notification

        private void NotificationComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!_loaded) return;
            var value = (DisplayEnumObject<NotificationTypeEnum>)((ComboBox)sender).SelectedItem;
            if (value == null) return;

            var notificationType = value.Enum;
            if (notificationType == AppModel.Instance.NotificationSettings) return;

            var supportCustomSound = new NotificationFactory().Get(notificationType).SupportCustomSound();
            selectSoundButton.Visible = supportCustomSound;
            DeleteSoundButtonVisible(supportCustomSound);

            usePrimaryScreenCheckbox.Visible = positionLabel.Visible = positionComboBox.Visible = singleNotificationCheckbox.Visible = notificationType == NotificationTypeEnum.BannerNotification;

            AppModel.Instance.NotificationSettings = notificationType;
        }

        private void PositionComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!_loaded) return;
            var value = (DisplayEnumObject<BannerPositionEnum>)((ComboBox)sender).SelectedItem;
            if (value == null) return;

            AppModel.Instance.BannerPosition = value.Enum;
        }

        private void SelectSoundButton_Click(object sender, EventArgs e)
        {
            selectSoundFileDialog.ShowDialog(this);
        }

        private void DeleteSoundButton_Click(object sender, EventArgs e)
        {
            AppModel.Instance.CustomNotificationSound = null;
            deleteSoundButton.Visible = false;
        }

        private void UsePrimaryScreenCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            AppModel.Instance.NotifyUsingPrimaryScreen = usePrimaryScreenCheckbox.Checked;
        }

        #endregion
    }
}