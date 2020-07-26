using System;
using System.Linq;
using System.Windows.Forms;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Localization;
using SoundSwitch.Properties;
using SoundSwitch.UI.Component;

namespace SoundSwitch.UI.Forms.Profile
{
    public partial class AddProfileExtended : Form
    {
        private readonly Framework.Profile.Profile _profile;
        private readonly TriggerFactory _triggerFactory;

        public AddProfileExtended(Framework.Profile.Profile profile)
        {
            _profile = profile;
            _triggerFactory = new TriggerFactory();
            InitializeComponent();

            HideTriggerComponents();

            hotKeyControl.Location = textInput.Location;

            LocalizeForm();
            Icon = Resources.profile;
            InitializeFromProfile();
        }

        private void LocalizeForm()
        {
            descriptionBox.Text = SettingsStrings.profile_desc;
            availableTriggersText.Text = SettingsStrings.profile_trigger_available;
            activeTriggerLabel.Text = SettingsStrings.profile_trigger_actives;
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

            var trigger = (Trigger) setTriggerBox.SelectedItem;
            descriptionLabel.Text = _triggerFactory.Get(trigger.Type).Description;
            descriptionLabel.Show();
            HideTriggerComponents();
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
                    textInput.DataBindings.Add(nameof(TextBox.Text), trigger, nameof(Trigger.WindowName), true,
                        DataSourceUpdateMode.OnPropertyChanged);
                    textInput.Show();
                    break;
                case TriggerFactory.Enum.Process:
                    textInput.DataBindings.Clear();
                    textInput.DataBindings.Add(nameof(TextBox.Text), trigger, nameof(Trigger.ApplicationPath), true,
                        DataSourceUpdateMode.OnPropertyChanged);
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
            } else if (setTriggerBox.Items.Count == 0)
            {
                HideTriggerComponents();
            }
        }

        private void selectProgramButton_Click(object sender, EventArgs e)
        {
            if (selectProgramDialog.ShowDialog(this) != DialogResult.OK)
                return;
            textInput.Text = selectProgramDialog.FileName;
        }
    }
}