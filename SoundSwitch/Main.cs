/********************************************************************
* Copyright (C) 2015 Jeroen Pelgrims
* Copyright (C) 2015 Antoine Aflalo
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
using SoundSwitch.Forms;
using System.Windows.Forms;
using SoundSwitch.Util;
using System.Collections.Generic;
using System.Linq;
using AudioEndPointControllerWrapper;
using Microsoft.Win32;
using Settings = SoundSwitch.Properties.Settings;


namespace SoundSwitch
{
    public class Main
    {
        /// <summary>
        /// Instance of the main application class
        /// </summary>
        public static Main Instance { get; private set; }

        public delegate void SelectedDeviceChangeHandler(object sender, List<string> newSelectedDevices );

        public event SelectedDeviceChangeHandler OnSelectedDeviceChange;

        public List<string> SelectedDevicesList
        {
            get
            {
                return
                    Properties.Settings.Default.SelectedDevices.Split(new[] {SelectedDevicesDelimiter},
                        StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            private set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                Properties.Settings.Default.SelectedDevices = string.Join(SelectedDevicesDelimiter, value);
                Properties.Settings.Default.Save();
                OnSelectedDeviceChange?.Invoke(this,value);
            }
        }

        public List<AudioDeviceWrapper> AvailableAudioDevices
        {
            get
            {
                return
                    AudioController.getAvailableAudioDevices()
                        .Where((device => SelectedDevicesList.Contains(device.FriendlyName)))
                        .ToList();
            }
        }

        readonly Util.TrayIcon _trayIcon;

        public static void InitMain()
        {
            Instance = new Main();
        }

        public Main()
        {
            _trayIcon = new Util.TrayIcon();
            OnSelectedDeviceChange += (sender, devices) =>
            {
                PopulateTrayIconDeviceMenu();
            };
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
            //Regsiter your application for restart
            if (RestartManagerWrapper.IsRestartManagerSupported)
            {
                string commandLine = null;

                RestartManagerWrapper.RegisterApplicationRestart(commandLine, ApplicationRestartFlags.NONE);

            }//End-if (RestartManagerWrapper.IsRestartManagerSupported)         

        }

        #region Tray icon 

        /// <summary>
        /// populates the tray icon with device names that are selected AND found on the system
        /// </summary>
        public void PopulateTrayIconDeviceMenu()
        {
            _trayIcon.SetDeviceList(AvailableAudioDevices);
        }

        #endregion
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

        #region Selected devices

        private const string SelectedDevicesDelimiter = ";;;"; 

        /// <summary>
        /// Sets a particular device to be enabled or not
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="selected"></param>
        public void SetDeviceSelection(string deviceName, bool selected)
        {
            var current = SelectedDevicesList;
            if (selected && !current.Contains(deviceName))
            {
                current.Add(deviceName);
            }
            else if (current.Contains(deviceName))
            {
                current.Remove(deviceName);
            }
            SelectedDevicesList = current;
        }

        #endregion

        #region Active device

        /// <summary>
        /// Attempts to set active device to the specified name 
        /// </summary>
        /// <param name="device"></param>
        public bool SetActiveDevice(AudioDeviceWrapper device)
        {
            try
            {
                device.SetAsDefault();
                _trayIcon.ShowAudioChanged(device.FriendlyName);
                Properties.Settings.Default.LastActiveAudioDevice = device.FriendlyName;
                Properties.Settings.Default.Save();
                return true;
            }
            catch (Exception ex)
            {
                _trayIcon.ShowError("Failed to change device: " + ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Cycles the active device to the next device. Returns true if succesfully switched (at least
        /// as far as we can tell), returns false if could not successfully switch. Throws NoDevicesException
        /// if there are no devices configured.
        /// </summary>
        public bool CycleActiveDevice()
        {
            var list = AvailableAudioDevices;
            if (list.Count == 0)
            {
                throw new NoDevicesException();
            }
            AudioDeviceWrapper defaultDev = null;
            try
            {
                defaultDev = list.First(device => device.IsDefault);
            }
            catch (Exception)
            {
                defaultDev =
                    list.FirstOrDefault(device => device.FriendlyName == Properties.Settings.Default.LastActiveAudioDevice) ??
                    list[0];
            }

            var next = list.SkipWhile((device, i) => device != defaultDev).Skip(1).FirstOrDefault() ?? list[0];
            return SetActiveDevice(next);
        }

        class NoDevicesException : InvalidOperationException
        {
            public NoDevicesException() : base("No devices to select") {}
        }
        #endregion
       
    }
}
