using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SoundSwitch.Framework.Audio.Device;
using SoundSwitch.Model;
using SoundSwitch.Properties;
using SoundSwitch.UI.UserControls;

namespace SoundSwitch.UI.Forms
{
    public partial class AddProfile : Form
    {
      

        public AddProfile(IEnumerable<DeviceFullInfo> playbacks, IEnumerable<DeviceFullInfo> recordings)
        {
            InitializeComponent();
            Text = @"Add Profile";
            Icon = Resources.profile;
            selectProgramDialog.Filter = @"Executable (*.exe)|*.exe";

            recordingComboBox.DataSource = recordings
                .Select(info => new IconTextComboBox.DropDownItem
                    {
                        Icon = info.SmallIcon,
                        Tag = info,
                        Text = info.Name
                    }
                ).ToArray();

            playbackComboBox.DataSource = playbacks
                .Select(info => new IconTextComboBox.DropDownItem
                    {
                        Icon = info.SmallIcon,
                        Tag = info,
                        Text = info.Name
                    }
                ).ToArray();
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void selectProgramButton_Click(object sender, EventArgs e)
        {

        }
    }
}