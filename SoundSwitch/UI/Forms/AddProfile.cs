using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.Properties;
using SoundSwitch.UI.UserControls;

namespace SoundSwitch.UI.Forms
{
    public partial class AddProfile : Form
    {
        private readonly ProfileSetting _profile;


        public AddProfile(IEnumerable<DeviceFullInfo> playbacks, IEnumerable<DeviceFullInfo> recordings)
        {
            InitializeComponent();
            Text = SettingsStrings.profile_feature_add;
            Icon = Resources.profile;
            selectProgramDialog.Filter = @"Executable (*.exe)|*.exe";

            _profile = new ProfileSetting();

            nameTextBox.DataBindings.Add(nameof(TextBox.Text), _profile, nameof(ProfileSetting.ProfileName), false, DataSourceUpdateMode.OnPropertyChanged);

            InitRecordingPlaybackComboBoxes(playbacks, recordings);
        }

        private void InitRecordingPlaybackComboBoxes(IEnumerable<DeviceFullInfo> playbacks, IEnumerable<DeviceFullInfo> recordings)
        {
            var recordingsItems = recordings
                .Select(info => new IconTextComboBox.DropDownItem
                    {
                        Icon = info.SmallIcon,
                        Tag = info,
                        Text = info.Name
                    }
                ).ToList();
            recordingsItems.Insert(0, IconTextComboBox.DropDownItem.Empty);
            recordingComboBox.DataSource = recordingsItems.ToArray();


            var recordingBinding = new Binding(nameof(ComboBox.SelectedValue),
                _profile,
                nameof(ProfileSetting.Recording),
                false,
                DataSourceUpdateMode.OnPropertyChanged)
            {
                DataSourceNullValue = new DeviceInfo(null, null, DataFlow.Capture)
            };

            recordingComboBox.DataBindings.Add(recordingBinding);


            var playbackItems = playbacks
                .Select(info => new IconTextComboBox.DropDownItem
                    {
                        Icon = info.SmallIcon,
                        Tag = info,
                        Text = info.Name
                    }
                ).ToList();

            playbackItems.Insert(0, IconTextComboBox.DropDownItem.Empty);
            playbackComboBox.DataSource = playbackItems
                .ToArray();

            var playbackBinding = new Binding(nameof(ComboBox.SelectedValue),
                _profile,
                nameof(ProfileSetting.Playback),
                false,
                DataSourceUpdateMode.OnPropertyChanged)
            {
                DataSourceNullValue = new DeviceInfo(null, null, DataFlow.Render)
            };
            playbackComboBox.DataBindings.Add(playbackBinding);
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void selectProgramButton_Click(object sender, EventArgs e)
        {

        }

        private void createButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(_profile.ToString());
        }
    }
}