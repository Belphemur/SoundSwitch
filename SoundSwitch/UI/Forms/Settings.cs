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
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Common.Framework.Icon;
using SoundSwitch.Common.WinApi.Keyboard;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Audio.Lister;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.Factory;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.TrayIcon.Icon;
using SoundSwitch.Framework.TrayIcon.TooltipInfoManager;
using SoundSwitch.Framework.TrayIcon.TooltipInfoManager.TootipInfo;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Localization;
using SoundSwitch.Localization.Factory;
using SoundSwitch.Model;
using SoundSwitch.Properties;
using SoundSwitch.Util;

namespace SoundSwitch.UI.Forms
{
    public sealed partial class SettingsForm : Form
    {
        private static readonly Icon RessourceSettingsIcon = Resources.SettingsIcon;

        private bool _loaded;

        public SettingsForm()
        {
            // Form itself
            InitializeComponent();
            Icon = RessourceSettingsIcon;
            Text = AssemblyUtils.GetReleaseState() == AssemblyUtils.ReleaseState.Beta
                ? $"{SettingsStrings.settings} {AssemblyUtils.GetReleaseState()}"
                : SettingsStrings.settings;
            LocalizeForm();

            var closeToolTip = new ToolTip();
            closeToolTip.SetToolTip(closeButton, SettingsStrings.closeTooltip);

            hotkeysTextBox.Text = AppConfigs.Configuration.PlaybackHotKeys.Display();
            hotkeysTextBox.Tag =
                new Tuple<DataFlow, HotKeys>(DataFlow.Render, AppConfigs.Configuration.PlaybackHotKeys);
            hotkeysTextBox.Enabled = hotkeysCheckBox.Checked = AppConfigs.Configuration.PlaybackHotKeys.Enabled;
            hotkeysTextBox.KeyDown += (sender, args) => SetHotkey(args);
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

            foregroundAppCheckbox.Checked = AppModel.Instance.SwitchForegroundProgram;

            var foregroundAppToolTip = new ToolTip();
            foregroundAppToolTip.SetToolTip(foregroundAppCheckbox, SettingsStrings.foregroundAppTooltip);

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
        }

        private void PopulateProfiles(IAudioDeviceLister deviceLister)
        {
            ListViewItem ProfileToListViewItem(ProfileSetting profile)
            {
                var listViewItem = new ListViewItem(profile.ProfileName) {Tag = profile};
                Icon appIcon = null;
                DeviceFullInfo recording = null;
                DeviceFullInfo playback = null;
                if (!string.IsNullOrEmpty(profile.ApplicationPath))
                {
                    try
                    {
                        appIcon = IconExtractor.Extract(profile.ApplicationPath, 0, false);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                if (profile.Playback != null)
                {
                    playback = deviceLister.PlaybackDevices.FirstOrDefault(info => info.Id == profile.Playback.Id);
                }

                if (profile.Recording != null)
                {
                    recording = deviceLister.RecordingDevices.FirstOrDefault(info => info.Id == profile.Recording.Id);
                }

                listViewItem.SubItems.AddRange(new[]
                {
                    new ListViewItem.ListViewSubItem(listViewItem, profile.ApplicationPath ?? "") {Tag = appIcon},
                    new ListViewItem.ListViewSubItem(listViewItem, profile.HotKeys?.ToString() ?? ""),
                    new ListViewItem.ListViewSubItem(listViewItem, playback?.Name ?? profile.Playback?.ToString() ?? "") {Tag = playback?.SmallIcon},
                    new ListViewItem.ListViewSubItem(listViewItem, recording?.Name ?? profile.Recording?.ToString() ?? "") {Tag = recording?.SmallIcon},
                });
                return listViewItem;
            }

            addProfileButton.Image = Resources.profile_add;

            profilesListView.View = View.Details;
            profilesListView.FullRowSelect = true;

            profilesListView.Columns.Add(SettingsStrings.profile_name, 50, HorizontalAlignment.Left);
            profilesListView.Columns.Add(SettingsStrings.profile_program, -2, HorizontalAlignment.Left);
            profilesListView.Columns.Add(SettingsStrings.hotkeys, -2, HorizontalAlignment.Left);
            profilesListView.Columns.Add(SettingsStrings.playback, -2, HorizontalAlignment.Left);
            profilesListView.Columns.Add(SettingsStrings.recording, -2, HorizontalAlignment.Left);

            profilesListView.OwnerDraw = true;
            profilesListView.DrawColumnHeader += (sender, args) => args.DrawDefault = true;
            profilesListView.DrawSubItem += (sender, args) =>
            {
                if (string.IsNullOrEmpty(args.SubItem.Text) || args.SubItem.Tag == null)
                {
                    args.DrawDefault = true;
                    return;
                }


                var icon = (Icon) args.SubItem.Tag;
                args.DrawBackground();
                var splitPathIfPresent = args.SubItem.Text.Split('\\').Last();

                if (args.Item.Selected)
                {
                    var r = new Rectangle(args.Bounds.Left, args.Bounds.Top, args.Bounds.Right, args.Bounds.Height);
                    args.Graphics.FillRectangle(SystemBrushes.Highlight, r);
                    args.SubItem.ForeColor = SystemColors.HighlightText;
                }
                else
                {
                    args.SubItem.ForeColor = SystemColors.WindowText;
                }

                var imageRect = new Rectangle(args.Bounds.X, args.Bounds.Y, args.Bounds.Height, args.Bounds.Height);
                args.Graphics.DrawIcon(icon, imageRect);

                args.Graphics.DrawString(splitPathIfPresent,
                    args.SubItem.Font,
                    new SolidBrush(args.SubItem.ForeColor),
                    (args.SubItem.Bounds.Location.X + icon.Width + 5),
                    args.SubItem.Bounds.Location.Y);
            };

            foreach (var profile in AppModel.Instance.ProfileManager.Profiles)
            {
                var listViewItem = ProfileToListViewItem(profile);
                profilesListView.Items.Add(listViewItem);
            }
        }

        public async Task AsyncInit()
        {
            // Playback and Recording
            using var audioDeviceLister = new CachedAudioDeviceLister(DeviceState.All);
            await audioDeviceLister.Refresh();
            PopulateAudioDevices(audioDeviceLister);

            // Profiles
            PopulateProfiles(audioDeviceLister);

            _loaded = true;
        }

        private void PopulateAudioDevices(CachedAudioDeviceLister audioDeviceLister)
        {
            PopulateAudioList(playbackListView, AppModel.Instance.SelectedDevices,
                audioDeviceLister.PlaybackDevices);
            PopulateAudioList(recordingListView, AppModel.Instance.SelectedDevices,
                audioDeviceLister.RecordingDevices);
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
            addProfileButton.Text = SettingsStrings.profile_addButton;

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

            // Misc
            hotkeysLabel.Text = SettingsStrings.hotkeys;
            closeButton.Text = SettingsStrings.close;
        }


        private void SelectSoundFileDialogOnFileOk(object sender, CancelEventArgs cancelEventArgs)
        {
            AppModel.Instance.CustomNotificationSound = new CachedSound(selectSoundFileDialog.FileName);
            deleteSoundButton.Visible = true;
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
                var tuple = (Tuple<DataFlow, HotKeys>) hotkeysTextBox.Tag;
                var newTuple = new Tuple<DataFlow, HotKeys>(tuple.Item1, new HotKeys(e.KeyCode, modifierKeys));
                hotkeysTextBox.Tag = newTuple;
                hotkeysTextBox.ForeColor = AppModel.Instance.SetHotkeyCombination(newTuple.Item2, newTuple.Item1)
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
            if (tabControlSender.SelectedTab == playbackTabPage)
            {
                SetHotkeysFieldsVisibility(true);
                hotkeysTextBox.Text = AppConfigs.Configuration.PlaybackHotKeys.Display();
                hotkeysTextBox.Tag =
                    new Tuple<DataFlow, HotKeys>(DataFlow.Render, AppConfigs.Configuration.PlaybackHotKeys);
                hotkeysCheckBox.Checked = AppConfigs.Configuration.PlaybackHotKeys.Enabled;
            }
            else if (tabControlSender.SelectedTab == recordingTabPage)
            {
                SetHotkeysFieldsVisibility(true);
                hotkeysTextBox.Text = AppConfigs.Configuration.RecordingHotKeys.Display();
                hotkeysTextBox.Tag =
                    new Tuple<DataFlow, HotKeys>(DataFlow.Capture, AppConfigs.Configuration.RecordingHotKeys);
                hotkeysCheckBox.Checked = AppConfigs.Configuration.RecordingHotKeys.Enabled;
            }
            else
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
            var value = (DisplayEnumObject<NotificationTypeEnum>) ((ComboBox) sender).SelectedItem;

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

            AppModel.Instance.NotificationSettings = notificationType;
        }

        private void tooltipInfoComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            var value = (DisplayEnumObject<TooltipInfoTypeEnum>) ((ComboBox) sender).SelectedItem;

            if (value == null)
                return;


            TooltipInfoManager.CurrentTooltipInfo = value.Enum;
        }

        private void cyclerComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            var value = (DisplayEnumObject<DeviceCyclerTypeEnum>) ((ComboBox) sender).SelectedItem;

            if (value == null)
                return;

            DeviceCyclerManager.CurrentCycler = value.Enum;
        }

        private void languageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;

            var value = (DisplayEnumObject<Language>) ((ComboBox) sender).SelectedItem;

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
            var tuple = (Tuple<DataFlow, HotKeys>) hotkeysTextBox.Tag;
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
                foreach (var device in audioDevices)
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
        private ListViewItem GenerateListViewItem(DeviceFullInfo device, IEnumerable<DeviceInfo> selected,
            ListView listView)
        {
            var listViewItem = new ListViewItem
            {
                Text = device.Name,
                ImageKey = device.IconPath,
                Tag = device
            };
            var selectedDevice = device;
            if (selected.Contains(selectedDevice))
            {
                listViewItem.Checked = true;
                listViewItem.Group = listView.Groups["selectedGroup"];
            }
            else
            {
                listViewItem.Checked = false;
                listViewItem.Group = GetGroup(device.State, listView);
            }

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
                        AppModel.Instance.SelectDevice((DeviceFullInfo) ((ListView) sender).Items[e.Index].Tag);
                        break;
                    case CheckState.Unchecked:
                        AppModel.Instance.UnselectDevice((DeviceFullInfo) ((ListView) sender).Items[e.Index].Tag);
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

        private void iconChangeChoicesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            var comboBox = (ComboBox) sender;

            if (comboBox == null)
                return;

            var item = (DisplayEnumObject<IconChangerFactory.ActionEnum>) iconChangeChoicesComboBox.SelectedItem;
            AppConfigs.Configuration.SwitchIcon = item.Enum;
            AppConfigs.Configuration.Save();

            new IconChangerFactory().Get(item.Enum).ChangeIcon(AppModel.Instance.TrayIcon);
        }

        private async void addProfileButton_Click(object sender, EventArgs e)
        {
            await ShowAddProfile();
        }

        private async Task ShowAddProfile()
        {
            using var audioLister = new CachedAudioDeviceLister(DeviceState.All);
            await audioLister.Refresh();
            new AddProfile(audioLister.PlaybackDevices, audioLister.RecordingDevices).Show(Owner);
        }
    }
}