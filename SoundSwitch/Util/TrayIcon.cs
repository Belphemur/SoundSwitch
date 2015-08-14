using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using SoundSwitch.Forms;

namespace SoundSwitch.Util
{
    public class TrayIcon
    {
        private readonly NotifyIcon trayIcon = new NotifyIcon
        {
            Icon = Icon.FromHandle(Properties.Resources.SwitchIcon.GetHicon()),
            Visible = true,
            Text = Application.ProductName
        };
        private readonly ContextMenuStrip _settingsMenu = new ContextMenuStrip();

        private readonly ContextMenuStrip _selectionMenu = new ContextMenuStrip();
        
        public TrayIcon()
        {
            trayIcon.ContextMenuStrip = _settingsMenu;

            _settingsMenu.Items.Add("Playback Devices", null, (sender, e) =>
            {
                Process.Start(new ProcessStartInfo("cmd", "/c " + "control mmsys.cpl sounds")
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            });
            // settingsMenu.Items.Add("Mixer", null, (sender, e) =>  ?? not sure how to display the mixer
            _settingsMenu.Items.Add("-");
            _settingsMenu.Items.Add("Settings", null, (sender, e) => Settings.Instance.Show());
            _settingsMenu.Items.Add("About", null, (sender, e) => About.Instance.Show());
            _settingsMenu.Items.Add("-");
            _settingsMenu.Items.Add("Exit", null, (sender, e) => Application.Exit());

            _selectionMenu.Items.Add("No devices selected", null, (sender, e) => Settings.Instance.Show());

            trayIcon.MouseClick += (sender, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    trayIcon.ContextMenuStrip = _selectionMenu;
                    MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                    mi.Invoke(trayIcon, null);

                    trayIcon.ContextMenuStrip = _settingsMenu;
                }
            };
        }

        /// <summary>
        /// Sets the names of devices that show up in the menu
        /// </summary>
        /// <param name="deviceNames"></param>
        public void SetDeviceList(List<AudioDeviceWrapper> deviceNames) 
        {
            _selectionMenu.Items.Clear();
            foreach (var item in deviceNames)
            {
                _selectionMenu.Items.Add(new ToolStripDeviceItem(deviceClicked, item));
            }
        }

        void deviceClicked(object sender, EventArgs e)
        {
            try
            {
                var item = (ToolStripDeviceItem)sender;
                Main.Instance.SetActiveDevice(item.AudioDevice);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Notification that audio has changed
        /// </summary>
        /// <param name="deviceName"></param>
        public void ShowAudioChanged(string deviceName)
        {
            trayIcon.ShowBalloonTip(500, "SoundSwitch: Audio output changed", deviceName, ToolTipIcon.Info);
        }

        /// <summary>
        /// Notification for when there are no devices configured
        /// </summary>
        public void ShowNoDevices()
        {
            trayIcon.ShowBalloonTip(3000, "SoundSwitch: Configuration needed", "No devices available to switch to. Open configuration by right-clicking on the SoundSwitch icon. ", ToolTipIcon.Warning);
        }

        /// <summary>
        /// shows an error messasge
        /// </summary>
        /// <param name="errorMessage"></param>
        public void ShowError(string errorMessage, string errorTitle = "Error")
        {
            trayIcon.ShowBalloonTip(3000, "SoundSwitch: " + errorTitle, errorMessage, ToolTipIcon.Error);
        }
    }
}
