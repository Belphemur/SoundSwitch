using System;
using System.Linq;
using System.Windows.Forms;
using SoundSwitch.Util;

namespace SoundSwitch.Forms
{
    public partial class Settings : Form
    {
        public const string DevicesDelimiter = ";;;";


        public static Settings Instance { get; } = new Settings();

        static Settings()
        {
            Instance.Closing += (sender, e) =>
            {
                e.Cancel = true;
                Instance.Hide();
            };
        }

        private Settings()
        {
            InitializeComponent();
            
            txtHotkey.KeyDown += TxtHotkey_KeyDown;
            txtHotkey.Text =
                $"{Properties.Settings.Default.HotkeyModifierKeys.Replace(", ", "+")}+{Properties.Settings.Default.HotkeyKey}";

            RunAtStartup.Checked = Main.Instance.RunAtStartup;
        }
        
        private void RunAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            var ras = RunAtStartup.Checked;
            try
            {
                Main.Instance.RunAtStartup = ras;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error changing run at startup setting: " + ex.Message);
                RunAtStartup.Checked = Main.Instance.RunAtStartup;
            }
        }

        private void TxtHotkey_KeyDown(object sender, KeyEventArgs e)
        {
            if (!new[] { 8, 17, 18, 46 }.Contains(e.KeyValue))
            {
                ModifierKeys modifierKeys = 0;
                var displayString = "";

                if (e.Control)
                {
                    modifierKeys |= Util.ModifierKeys.Control;
                    displayString += "Ctrl+";
                }
                if (e.Alt)
                {
                    modifierKeys |= Util.ModifierKeys.Alt;
                    displayString += "Alt+";
                }

                txtHotkey.Text = $"{displayString}{e.KeyCode}";
                Main.Instance.SetHotkeyCombination(e.KeyCode, modifierKeys);
            }
        }


        public new void Show()
        {
            lstDevices.Items.Clear();
            try
            {
                // disable click event 
                lstDevices.ItemCheck -= lstDevices_ItemCheck;

                var selected = Main.Instance.GetSelectedDevices();
                foreach (var item in Main.Instance.AudioDeviceManager.GetDevices())
                {
                    var idx = lstDevices.Items.Add(item.FriendlyName);
                    lstDevices.SetItemCheckState(idx, selected.Contains(item.FriendlyName) ? CheckState.Checked : CheckState.Unchecked);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: " + e.Message);
            }
            finally
            {
                // re-enable click event 
                lstDevices.ItemCheck += lstDevices_ItemCheck;
            }

            base.Show();

        }

        private void lstDevices_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                Main.Instance.SetDeviceSelection(lstDevices.Items[e.Index].ToString(), e.NewValue == CheckState.Checked);
            }
            catch (Exception)
            {
                e.NewValue = e.CurrentValue;
            }
        }

        private void Settings_VisibleChanged(object sender, EventArgs e)
        {
            // make sure the device list is in sync
            Main.Instance.PopulateTrayIconDeviceMenu();
        }

    }
}
