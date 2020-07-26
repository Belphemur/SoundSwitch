using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Properties;

namespace SoundSwitch.UI.Forms.Profile
{
    public partial class AddProfileExtended : Form
    {
        private Trigger _selectedTrigger;

        public AddProfileExtended()
        {
            var triggerFactory = new TriggerFactory();
            InitializeComponent();
            Icon = Resources.profile;
            availableTriggerBox.Items.AddRange(triggerFactory.AllImplementations.Values.Cast<object>().ToArray());
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
        }

        private void setTriggerBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (setTriggerBox.SelectedItem == null)
            {
                return;
            }

            _selectedTrigger = (Trigger) setTriggerBox.SelectedItem;
        }
    }
}