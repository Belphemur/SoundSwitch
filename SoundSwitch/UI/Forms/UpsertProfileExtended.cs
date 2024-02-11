/********************************************************************
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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RailSharp;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Localization;
using SoundSwitch.Localization.Factory;
using SoundSwitch.Model;
using SoundSwitch.Properties;
using SoundSwitch.UI.Component;

namespace SoundSwitch.UI.Forms
{
    public partial class UpsertProfileExtended : Form
    {
        private readonly Profile _profile;
        private readonly Profile _oldProfile;
        private readonly SettingsForm _settingsForm;
        private readonly bool _editing;
        private readonly TriggerFactory _triggerFactory;
        private ToolTip _restoreDeviceToolTip;

        public UpsertProfileExtended(Profile profile, IEnumerable<DeviceFullInfo> playbacks, IEnumerable<DeviceFullInfo> recordings, SettingsForm settingsForm, bool editing = false)
        {
            RightToLeft = new LanguageFactory().Get(AppModel.Instance.Language).IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;

            _oldProfile = editing ? profile : null;
            _profile = editing ? profile.Copy() : profile;
            _settingsForm = settingsForm;
            _editing = editing;
            _triggerFactory = new TriggerFactory();
            InitializeComponent();

            HideTriggerComponents();

            hotKeyControl.Location = textInput.Location;
            hotKeyControl.ListenToHotkey = true;

            LocalizeForm();
            using var iconBitmap = new Bitmap(Resources.profile_menu_icon);
            Icon = Icon.FromHandle(iconBitmap.GetHicon());
            InitializeFromProfile();

            try
            {
                //Only let user on Windows 10 change the switch also default device
                //Since the feature isn't available on Windows 7
                if (Environment.OSVersion.Version.Major < 10)
                {
                    profile.AlsoSwitchDefaultDevice = true;
                    profile.SwitchForegroundApp = false;
                    switchDefaultCheckBox.Hide();
                    switchForegroundCheckbox.Hide();
                }
            }
            catch (Exception)
            {
                // ignored
            }

            descriptionLabel.Hide();
            triggerLabel.Hide();
            InitRecordingPlaybackComboBoxes(playbacks, recordings);
            switchDefaultCheckBox.DataBindings.Add(nameof(CheckBox.Checked), _profile, nameof(Profile.AlsoSwitchDefaultDevice), false, DataSourceUpdateMode.OnPropertyChanged);
            nameTextBox.DataBindings.Add(nameof(TextBox.Text), _profile, nameof(Profile.Name), false, DataSourceUpdateMode.OnPropertyChanged);
            notifyCheckbox.DataBindings.Add(nameof(CheckBox.Checked), _profile, nameof(Profile.NotifyOnActivation), false, DataSourceUpdateMode.OnPropertyChanged);
            restoreDevicesCheckBox.DataBindings.Add(nameof(CheckBox.Checked), _profile, nameof(Profile.RestoreDevices), false, DataSourceUpdateMode.OnPropertyChanged);
            switchForegroundCheckbox.DataBindings.Add(nameof(CheckBox.Checked), _profile, nameof(Profile.SwitchForegroundApp), false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void InitRecordingPlaybackComboBoxes(IEnumerable<DeviceFullInfo> playbacks,
                                                     IEnumerable<DeviceFullInfo> recordings)
        {
            var recordingItems = recordings
                                 .OrderBy(info => info.State)
                                 .ThenBy(info => info.NameClean)
                                 .Select(info => new IconTextComboBox.DropDownItem
                                     {
                                         Icon = info.SmallIcon,
                                         Tag = info,
                                         Text = info.NameClean
                                     }
                                 )
                                 .ToArray();

            var playbackItems = playbacks
                                .OrderBy(info => info.State)
                                .ThenBy(info => info.NameClean)
                                .Select(info => new IconTextComboBox.DropDownItem
                                    {
                                        Icon = info.SmallIcon,
                                        Tag = info,
                                        Text = info.NameClean
                                    }
                                )
                                .ToArray();

            communicationComboBox.DataSource = playbackItems;
            playbackComboBox.DataSource = playbackItems.ToArray();

            recordingComboBox.DataSource = recordingItems;
            communicationRecordingComboBox.DataSource = recordingItems.ToArray();

            communicationComboBox.DataBindings.Add(nameof(ComboBox.SelectedValue), _profile, nameof(Profile.Communication), true, DataSourceUpdateMode.OnPropertyChanged);
            recordingComboBox.DataBindings.Add(nameof(ComboBox.SelectedValue), _profile, nameof(Profile.Recording), true, DataSourceUpdateMode.OnPropertyChanged);
            playbackComboBox.DataBindings.Add(nameof(ComboBox.SelectedValue), _profile, nameof(Profile.Playback), true, DataSourceUpdateMode.OnPropertyChanged);
            communicationRecordingComboBox.DataBindings.Add(nameof(ComboBox.SelectedValue), _profile, nameof(Profile.RecordingCommunication), true, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void LocalizeForm()
        {
            descriptionBox.Text = SettingsStrings.profile_desc;
            availableTriggersText.Text = SettingsStrings.profile_trigger_available;
            activeTriggerLabel.Text = SettingsStrings.profile_trigger_actives;

            recordingLabel.Text = SettingsStrings.recording;
            playbackLabel.Text = SettingsStrings.playback;
            communicationLabel.Text = SettingsStrings.playback;
            communicationRecordingLabel.Text = SettingsStrings.recording;
            communicationBox.Text = SettingsStrings.communication;
            Text = SettingsStrings.profile_feature_add;
            selectProgramDialog.Filter = $@"{SettingsStrings.profile_feature_executable}|*.exe";
            nameLabel.Text = SettingsStrings.profile_name;
            notifyCheckbox.Text = SettingsStrings.profile_notify_on_activation;
            switchDefaultCheckBox.Text = SettingsStrings.profile_defaultDevice_checkbox;
            saveButton.Text = SettingsStrings.profile_button_save;
            restoreDevicesCheckBox.Text = SettingsStrings.profile_trigger_restoreDevices;
            switchForegroundCheckbox.Text = SettingsStrings.foregroundApp;

            new ToolTip().SetToolTip(switchDefaultCheckBox, SettingsStrings.profile_defaultDevice_checkbox_tooltip);
            new ToolTip().SetToolTip(switchForegroundCheckbox, SettingsStrings.foregroundAppTooltip);


            _restoreDeviceToolTip = new ToolTip();
            _restoreDeviceToolTip.SetToolTip(
                restoreDevicesCheckBox,
                string.Format(SettingsStrings.profile_trigger_restoreDevices_desc, SettingsStrings.profile_defaultDevice_checkbox)
            );
        }

        private void InitializeFromProfile()
        {
            InitializeAvailableTriggers();
            setTriggerBox.Items.AddRange(_profile.Triggers.Cast<object>().ToArray());
        }

        private void InitializeAvailableTriggers()
        {
            var countByTrigger = _profile.Triggers.GroupBy(trigger => trigger.Type)
                                         .ToDictionary(triggers => triggers.Key);
            var availableTriggers = AppModel.Instance.ProfileManager.AvailableTriggers()
                                            .Where(pair =>
                                            {
                                                if (countByTrigger.TryGetValue(pair.TypeEnum, out var trigger))
                                                {
                                                    return pair.MaxOccurence == -1 || trigger.Count() < pair.MaxOccurence;
                                                }

                                                return true;
                                            })
                                            .Cast<object>()
                                            .ToArray();

            if (availableTriggerBox.Items.Count != availableTriggers.Length)
            {
                availableTriggerBox.Items.Clear();
                availableTriggerBox.Items.AddRange(availableTriggers);
                availableTriggerBox.SelectedIndex = 0;
            }
        }

        private void AddTriggerButton_Click(object sender, EventArgs e)
        {
            if (availableTriggerBox.SelectedItem == null)
            {
                return;
            }

            var trigger = new Trigger(((ITriggerDefinition)availableTriggerBox.SelectedItem).TypeEnum);
            setTriggerBox.Items.Add(trigger);
            setTriggerBox.SelectedItem = trigger;
            _profile.Triggers.Add(trigger);
            InitializeAvailableTriggers();
        }

        private void SetTriggerBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (setTriggerBox.SelectedItem == null)
            {
                return;
            }

            HideTriggerComponents();

            var trigger = (Trigger)setTriggerBox.SelectedItem;
            var triggerDefinition = _triggerFactory.Get(trigger.Type);
            descriptionLabel.Text = $@"{triggerDefinition.Description} (Max: {(triggerDefinition.MaxGlobalOccurence == -1 ? "∞" : triggerDefinition.MaxGlobalOccurence.ToString())})";
            triggerLabel.Text = triggerDefinition.Label;
            descriptionLabel.Show();
            triggerLabel.Show();

            trigger.Type.Switch(() =>
                {
                    hotKeyControl.HotKey = trigger.HotKey;
                    hotKeyControl.CleanHotKeyChangedHandler();
                    hotKeyControl.HotKeyChanged += (o, @event) => trigger.HotKey = hotKeyControl.HotKey;
                    hotKeyControl.Show();
                },
                () =>
                {
                    textInput.DataBindings.Clear();
                    textInput.DataBindings.Add(nameof(TextBox.Text), trigger, nameof(Trigger.WindowName), true, DataSourceUpdateMode.OnPropertyChanged);
                    textInput.Show();
                },
                () =>
                {
                    textInput.DataBindings.Clear();
                    textInput.DataBindings.Add(nameof(TextBox.Text), trigger, nameof(Trigger.ApplicationPath), true, DataSourceUpdateMode.OnPropertyChanged);
                    textInput.Show();
                    selectProgramButton.Show();
                },
                () => { },
                () => { },
                () =>
                {
                    textInput.DataBindings.Clear();
                    textInput.DataBindings.Add(nameof(TextBox.Text), trigger, nameof(Trigger.WindowName), true, DataSourceUpdateMode.OnPropertyChanged);
                    textInput.Show();
                },
                () => { },
                () => { });
        }

        private void HideTriggerComponents()
        {
            selectProgramButton.Hide();
            textInput.Hide();
            hotKeyControl.Hide();
            selectProgramButton.Hide();
            if (!_profile.AlsoSwitchDefaultDevice)
            {
                restoreDevicesCheckBox.Enabled = false;
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (setTriggerBox.SelectedItem == null)
            {
                return;
            }

            //Remove first from the profile, else the SelectedItem will be null
            var trigger = (Trigger)setTriggerBox.SelectedItem;
            _profile.Triggers.Remove(trigger);
            setTriggerBox.Items.Remove(trigger);
            InitializeAvailableTriggers();
            if (setTriggerBox.Items.Count > 0)
            {
                setTriggerBox.SelectedIndex = 0;
            }
            else if (setTriggerBox.Items.Count == 0)
            {
                HideTriggerComponents();
                triggerLabel.Hide();
                descriptionLabel.Hide();
            }
        }

        private void SelectProgramButton_Click(object sender, EventArgs e)
        {
            if (selectProgramDialog.ShowDialog(this) != DialogResult.OK)
                return;
            textInput.Text = selectProgramDialog.FileName;
        }

        private void PlaybackComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (playbackComboBox.SelectedIndex == -1)
            {
                playbackRemoveButton.Visible = false;
                return;
            }

            playbackRemoveButton.Visible = true;
        }

        private void CommunicationRecordingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (communicationRecordingComboBox.SelectedIndex == -1)
            {
                communicationRecordingRemoveButton.Visible = false;
                return;
            }

            communicationRecordingRemoveButton.Visible = true;
        }

        private void RecordingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (recordingComboBox.SelectedIndex == -1)
            {
                recordingRemoveButton.Visible = false;
                return;
            }

            recordingRemoveButton.Visible = true;
        }

        private void CommunicationComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (communicationComboBox.SelectedIndex == -1)
            {
                communicationRemoveButton.Visible = false;
                return;
            }

            communicationRemoveButton.Visible = true;
        }

        private void PlaybackRemoveButton_Click(object sender, EventArgs e)
        {
            _profile.Playback = null;
            try
            {
                playbackComboBox.SelectedIndex = -1;
            }
            catch (ArgumentException)
            {
                //Happens because I receive a System.DBNull when there isn't a selection.
            }

            playbackRemoveButton.Visible = false;
        }

        private void RecordingRemoveButton_Click(object sender, EventArgs e)
        {
            _profile.Recording = null;
            try
            {
                recordingComboBox.SelectedIndex = -1;
            }
            catch (ArgumentException)
            {
                //Happens because I receive a System.DBNull when there isn't a selection.
            }

            recordingRemoveButton.Visible = false;
        }

        private void CommunicationRemoveButton_Click(object sender, EventArgs e)
        {
            _profile.Communication = null;
            try
            {
                communicationComboBox.SelectedIndex = -1;
            }
            catch (ArgumentException)
            {
                //Happens because I receive a System.DBNull when there isn't a selection.
            }

            communicationRemoveButton.Visible = false;
        }

        private void CommunicationRecordingRemoveButton_Click(object sender, EventArgs e)
        {
            _profile.RecordingCommunication = null;
            try
            {
                communicationRecordingComboBox.SelectedIndex = -1;
            }
            catch (ArgumentException)
            {
                //Happens because I receive a System.DBNull when there isn't a selection.
            }

            communicationRecordingRemoveButton.Visible = false;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var result = _editing ? AppModel.Instance.ProfileManager.UpdateProfile(_oldProfile, _profile) : AppModel.Instance.ProfileManager.AddProfile(_profile);

            result.Map(success =>
                  {
                      _settingsForm.RefreshProfiles();
                      _settingsForm.Focus();
                      Close();
                      return success;
                  })
                  .Catch<string>(s =>
                  {
                      MessageBox.Show(s, SettingsStrings.profile_error_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                      return Result.Success();
                  });
        }

        private void RestoreDevicesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_profile.AlsoSwitchDefaultDevice || !restoreDevicesCheckBox.Checked) return;

            restoreDevicesCheckBox.Checked = false;
            _restoreDeviceToolTip.Show(_restoreDeviceToolTip.GetToolTip(restoreDevicesCheckBox), restoreDevicesCheckBox, restoreDevicesCheckBox.PointToClient(Cursor.Position), 3000);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _restoreDeviceToolTip.Hide(restoreDevicesCheckBox);
            base.OnFormClosing(e);
        }

        private void SwitchDefaultCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (switchDefaultCheckBox.Checked)
            {
                restoreDevicesCheckBox.Enabled = true;
                return;
            }

            restoreDevicesCheckBox.Enabled = false;
        }
    }
}