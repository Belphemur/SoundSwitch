using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RailSharp;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.Properties;
using SoundSwitch.UI.Component;

namespace SoundSwitch.UI.Forms.Profile
{
    public partial class AddProfileExtended : Form
    {
        private readonly Framework.Profile.Profile _profile;
        private readonly SettingsForm _settingsForm;
        private readonly TriggerFactory _triggerFactory;

        public AddProfileExtended(Framework.Profile.Profile profile, IEnumerable<DeviceFullInfo> playbacks, IEnumerable<DeviceFullInfo> recordings, SettingsForm settingsForm)
        {
            _profile = profile;
            _settingsForm = settingsForm;
            _triggerFactory = new TriggerFactory();
            InitializeComponent();

            HideTriggerComponents();

            hotKeyControl.Location = textInput.Location;

            LocalizeForm();
            Icon = Resources.profile;
            InitializeFromProfile();

            try
            {
                //Only let user on Windows 10 change the switch also default device
                //Since the feature isn't available on Windows 7
                if (Environment.OSVersion.Version.Major < 10)
                {
                    profile.AlsoSwitchDefaultDevice = true;
                    switchDefaultCheckBox.Hide();
                }
            }
            catch (Exception)
            {
                // ignored
            }
            descriptionLabel.Hide();
            triggerLabel.Hide();
            InitRecordingPlaybackComboBoxes(playbacks, recordings);
            switchDefaultCheckBox.DataBindings.Add(nameof(CheckBox.Checked), _profile, nameof(Framework.Profile.Profile.AlsoSwitchDefaultDevice), false, DataSourceUpdateMode.OnPropertyChanged);
            nameTextBox.DataBindings.Add(nameof(TextBox.Text), _profile, nameof(Framework.Profile.Profile.Name), false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void InitRecordingPlaybackComboBoxes(IEnumerable<DeviceFullInfo> playbacks,
            IEnumerable<DeviceFullInfo> recordings)
        {
            recordingComboBox.DataSource =
                recordings
                    .OrderBy(info => info.State)
                    .ThenBy(info => info.NameClean)
                    .Select(info => new IconTextComboBox.DropDownItem
                        {
                            Icon = info.SmallIcon,
                            Tag = info,
                            Text = info.NameClean
                        }
                    ).ToArray();


            var playbackItems = playbacks
                .OrderBy(info => info.State)
                .ThenBy(info => info.NameClean)
                .Select(info => new IconTextComboBox.DropDownItem
                    {
                        Icon = info.SmallIcon,
                        Tag = info,
                        Text = info.NameClean
                    }
                ).ToArray();
            communicationComboBox.DataSource = playbackItems;
            playbackComboBox.DataSource = playbackItems.ToArray();

            communicationComboBox.DataBindings.Add(nameof(ComboBox.SelectedValue), _profile, nameof(Framework.Profile.Profile.Communication), false, DataSourceUpdateMode.OnPropertyChanged);
            recordingComboBox.DataBindings.Add(nameof(ComboBox.SelectedValue), _profile, nameof(Framework.Profile.Profile.Recording), false, DataSourceUpdateMode.OnPropertyChanged);
            playbackComboBox.DataBindings.Add(nameof(ComboBox.SelectedValue), _profile, nameof(Framework.Profile.Profile.Playback), false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void LocalizeForm()
        {
            descriptionBox.Text = SettingsStrings.profile_desc;
            availableTriggersText.Text = SettingsStrings.profile_trigger_available;
            activeTriggerLabel.Text = SettingsStrings.profile_trigger_actives;

            recordingLabel.Text = SettingsStrings.recording;
            playbackLabel.Text = SettingsStrings.playback;
            communicationLabel.Text = SettingsStrings.communication;
            Text = SettingsStrings.profile_feature_add;
            selectProgramDialog.Filter = $@"{SettingsStrings.profile_feature_executable}|*.exe";
            nameLabel.Text = SettingsStrings.profile_name;
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
            var availableTriggers = _triggerFactory.AllImplementations
                .Where(pair =>
                {
                    if (countByTrigger.TryGetValue(pair.Key, out var trigger))
                    {
                        return pair.Value.MaxOccurence == -1 || trigger.Count() < pair.Value.MaxOccurence;
                    }

                    return true;
                })
                .Select(pair => pair.Value)
                .Cast<object>()
                .ToArray();

            if (availableTriggerBox.Items.Count != availableTriggers.Length)
            {
                availableTriggerBox.Items.Clear();
                availableTriggerBox.Items.AddRange(availableTriggers);
                availableTriggerBox.SelectedIndex = 0;
            }
        }

        private void addTriggerButton_Click(object sender, EventArgs e)
        {
            if (availableTriggerBox.SelectedItem == null)
            {
                return;
            }

            var trigger = new Trigger(((ITriggerDefinition) availableTriggerBox.SelectedItem).TypeEnum);
            setTriggerBox.Items.Add(trigger);
            setTriggerBox.SelectedItem = trigger;
            _profile.Triggers.Add(trigger);
            InitializeAvailableTriggers();
        }

        private void setTriggerBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (setTriggerBox.SelectedItem == null)
            {
                return;
            }

            HideTriggerComponents();

            var trigger = (Trigger) setTriggerBox.SelectedItem;
            var triggerDefinition = _triggerFactory.Get(trigger.Type);
            descriptionLabel.Text = triggerDefinition.Description;
            triggerLabel.Text = triggerDefinition.Label;
            descriptionLabel.Show();
            triggerLabel.Show();

            switch (trigger.Type)
            {
                case TriggerFactory.Enum.HotKey:
                    hotKeyControl.HotKey = trigger.HotKey;
                    hotKeyControl.CleanHotKeyChangedHandler();
                    hotKeyControl.HotKeyChanged += (o, @event) => trigger.HotKey = hotKeyControl.HotKey;
                    hotKeyControl.Show();
                    break;
                case TriggerFactory.Enum.Window:
                    textInput.DataBindings.Clear();
                    textInput.DataBindings.Add(nameof(TextBox.Text), trigger, nameof(Trigger.WindowName), true, DataSourceUpdateMode.OnPropertyChanged);
                    textInput.Show();
                    break;
                case TriggerFactory.Enum.Process:
                    textInput.DataBindings.Clear();
                    textInput.DataBindings.Add(nameof(TextBox.Text), trigger, nameof(Trigger.ApplicationPath), true, DataSourceUpdateMode.OnPropertyChanged);
                    textInput.Show();
                    selectProgramButton.Show();
                    break;
                case TriggerFactory.Enum.Steam:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HideTriggerComponents()
        {
            selectProgramButton.Hide();
            textInput.Hide();
            hotKeyControl.Hide();
            selectProgramButton.Hide();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (setTriggerBox.SelectedItem == null)
            {
                return;
            }

            //Remove first from the profile, else the SelectedItem will be null
            var trigger = (Trigger) setTriggerBox.SelectedItem;
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

        private void selectProgramButton_Click(object sender, EventArgs e)
        {
            if (selectProgramDialog.ShowDialog(this) != DialogResult.OK)
                return;
            textInput.Text = selectProgramDialog.FileName;
        }

        private void playbackComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (playbackComboBox.SelectedIndex == -1)
            {
                playbackRemoveButton.Visible = false;
                return;
            }

            playbackRemoveButton.Visible = true;
        }

        private void recordingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (recordingComboBox.SelectedIndex == -1)
            {
                recordingRemoveButton.Visible = false;
                return;
            }

            recordingRemoveButton.Visible = true;
        }

        private void communicationComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (communicationComboBox.SelectedIndex == -1)
            {
                communicationRemoveButton.Visible = false;
                return;
            }

            communicationRemoveButton.Visible = true;
        }

        private void playbackRemoveButton_Click(object sender, EventArgs e)
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

        private void recordingRemoveButton_Click(object sender, EventArgs e)
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

        private void communicationRemoveButton_Click(object sender, EventArgs e)
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

        private void saveButton_Click(object sender, EventArgs e)
        {
            var result = AppModel.Instance.ProfileManager.AddProfile(_profile);
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
    }
}