using System;
using SoundSwitch.Forms;
using System.Windows.Forms;
using SoundSwitch.Util;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using SoundSwitch.Models;


namespace SoundSwitch
{
    public class Main
    {
        /// <summary>
        /// Instance of the main application class
        /// </summary>
        public static Main Instance { get; private set; }

        public AudioDeviceManager AudioDeviceManager { get; private set; } = new AudioDeviceManager("EndPointController.exe");

        readonly Util.TrayIcon _trayIcon;

        public Main()
        {
            Instance = this;

            _trayIcon = new Util.TrayIcon();
            PopulateTrayIconDeviceMenu();

            Hook = new KeyboardHook();
            if (!string.IsNullOrEmpty(Properties.Settings.Default.HotkeyKey) &&
                !string.IsNullOrEmpty(Properties.Settings.Default.HotkeyModifierKeys))
            {
                ReAttachKeyboardHook();
            }

            //Hook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt, Keys.F11);
            //Hook.KeyPressed += (sender, e) =>
            //{
            //    try
            //    {
            //        SoundConfig.ToggleDefaultDevice();
            //    }
            //    catch (ConfigurationException ex)
            //    {
            //        Settings.Instance.Show();
            //        MessageBox.Show(ex.Message, "Configuration needed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //    }
            //};
        }


        #region Hot keys
        /// <summary>
        /// Sets the hotkey combination, and <see cref="ReAttachKeyboardHook">re-attaches the keyboard hook</see>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modifierKeys"></param>
        public void SetHotkeyCombination(Keys key, ModifierKeys modifierKeys)
        {
            Properties.Settings.Default.HotkeyKey = Enum.Format(typeof(Keys), key, "g");
            Properties.Settings.Default.HotkeyModifierKeys = Enum.Format(typeof(ModifierKeys), modifierKeys, "g");
            Properties.Settings.Default.Save();

            ReAttachKeyboardHook();
        }

        private KeyboardHook Hook { get; set; }

        public void ReAttachKeyboardHook()
        {
            try
            {
                Hook.Dispose();
                Hook = new KeyboardHook();

                Keys hotkeyKey;
                ModifierKeys hotkeyModifierKeys;
                if (!String.IsNullOrEmpty(Properties.Settings.Default.HotkeyKey))
                {
                    hotkeyKey = (Keys)Enum.Parse(typeof(Keys), Properties.Settings.Default.HotkeyKey);
                    hotkeyModifierKeys = (ModifierKeys)Enum.Parse(typeof(ModifierKeys), Properties.Settings.Default.HotkeyModifierKeys);
                }
                else
                {
                    hotkeyKey = Keys.F11;
                    hotkeyModifierKeys = Util.ModifierKeys.Alt | Util.ModifierKeys.Control;
                }

                Hook.RegisterHotKey(hotkeyModifierKeys, hotkeyKey);
                Hook.KeyPressed += HandleHotkeyPress;
            }
            catch (Exception ex)
            {
                _trayIcon.ShowError(ex.Message);
            }
            
        }

        private void HandleHotkeyPress(object sender, KeyPressedEventArgs e)
        {
            try
            {
                CycleActiveDevice();
            }
            catch (InvalidOperationException)
            {
                _trayIcon.ShowNoDevices();
            }
            catch (Exception ex)
            {
                _trayIcon.ShowError(ex.Message);
            }
        }

        #endregion

        #region Misc settings
        /// <summary>
        /// If the application runs at windows startup
        /// </summary>
        public bool RunAtStartup
        {
            get
            {
                return Properties.Settings.Default.RunAtStartup;
            }
            set
            {
                SetAutoStart(value);
                Properties.Settings.Default.RunAtStartup = value;
                Properties.Settings.Default.Save();
            }
        }


        /// <summary>
        /// Set the Application as autoStarting using registry
        /// </summary>
        /// <param name="start"></param>
        private static void SetAutoStart(bool start)
        {
            var rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (start)
                rk?.SetValue(Application.ProductName, Application.ExecutablePath);
            else
                rk?.DeleteValue(Application.ProductName, false);

        }
        #endregion

        #region Tray icon 

        /// <summary>
        /// populates the tray icon with device names that are selected AND found on the system
        /// </summary>
        public void PopulateTrayIconDeviceMenu()
        {
            _trayIcon.SetDeviceList(GetAvailableDevices());
        }

        #endregion

        #region Selected devices

        private const string SelectedDevicesDelimiter = ";;;"; 

        /// <summary>
        /// Sets the devices that are enabled
        /// </summary>
        /// <param name="deviceNames"></param>
        public void SetSelectedDevices(string[] deviceNames)
        {
            Properties.Settings.Default.SelectedDevices = String.Join(SelectedDevicesDelimiter, deviceNames);
            Properties.Settings.Default.Save();

            PopulateTrayIconDeviceMenu();
        }

        /// <summary>
        /// Gets the names of the devices that are enabled to be used
        /// </summary>
        /// <returns></returns>
        public string[] GetSelectedDevices()
        {
            return Properties.Settings.Default.SelectedDevices.Split(new string[] { SelectedDevicesDelimiter }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Sets a particular device to be enabled or not
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="selected"></param>
        public void SetDeviceSelection(string deviceName, bool selected)
        {
            var current = new List<string>(GetSelectedDevices());
            if (selected && !current.Contains(deviceName))
            {
                current.Add(deviceName);
            }
            else if (current.Contains(deviceName))
            {
                current.Remove(deviceName);
            }
            SetSelectedDevices(current.ToArray());
        }

        #endregion

        #region Active device
        private string LastKnownActiveDevice
        {
            get
            {
                return Properties.Settings.Default.ActiveDevice;
            }
            set
            {
                Properties.Settings.Default.ActiveDevice = value;
            }
        }

        /// <summary>
        /// Attempts to set active device to the specified name 
        /// </summary>
        /// <param name="device"></param>
        public bool SetActiveDevice(AudioDevice device)
        {
            try
            {
                if (AudioDeviceManager.SetDeviceAsDefault(device))
                {
                    LastKnownActiveDevice = device.FriendlyName;
                    _trayIcon.ShowAudioChanged(device.FriendlyName);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _trayIcon.ShowError("Failed to change device: " + ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Cycles the active device to the next device. Returns true if succesfully switched (at least
        /// as far as we can tell), retursn false if could not successfully switch. Throws NoDevicesException
        /// if there are no devices configured.
        /// </summary>
        public bool CycleActiveDevice()
        {
            var list = GetAvailableDevices();
            if (list.Count == 0)
            {
                throw new NoDevicesException();
            }

            var defaultDev = list.IndexOf(list.First(device => device.IsDefault));

            // loop through effectively one less than list.count, since we only want to try to set every other item except the current
            for (var i = 1; i < list.Count; i++)
            {
                defaultDev = (defaultDev >= list.Count - 1) ? 0 : defaultDev + 1;
                if (SetActiveDevice(list[defaultDev]))
                {
                    return true;
                }
            }
            return false;
        }

        class NoDevicesException : InvalidOperationException
        {
            public NoDevicesException() : base("No devices to select") {}
        }
        #endregion

        /// <summary>
        /// Gets the list of available devices that have been selected. 
        /// </summary>
        /// <returns></returns>
        public List<AudioDevice> GetAvailableDevices()
        {
            var selected = new List<string>(GetSelectedDevices());
            return AudioDeviceManager.GetDevices().Where((device => selected.Contains(device.FriendlyName))).ToList();
        }
       
    }
}
