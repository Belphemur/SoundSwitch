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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.ApplicationServices;
using SoundSwitch.Framework;
using SoundSwitch.Util;

namespace SoundSwitch
{
    public class Main
    {
        public delegate void AudioChangeHandler(object sender, AudioChangeEvent e);

        public delegate void ErrorHandler(object sender, ExceptionEvent e);

        public delegate void SelectedDeviceChangeHandler(object sender, DeviceListChanged e);

        private readonly SoundSwitchConfiguration _configuration;

        public Main(SoundSwitchConfiguration configuration)
        {
            _configuration = configuration;
            Hook = new KeyboardHook();

            ReAttachKeyboardHook();

            RegisterForRestart();
            RegisterRecovery();
        }

        public List<string> SelectedDevicesList
        {
            get
            {
                return
                    _configuration.SelectedDeviceList;
            }

            private set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _configuration.SelectedDeviceList = value;
                _configuration.Save();
                SelectedDeviceChanged?.Invoke(this, new DeviceListChanged(value));
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

        public string HotKeysString
        {
            get
            {
                var key = Enum.Format(typeof (Keys), _configuration.HotKeys, "g");
                var modKeys = Enum.Format(typeof (ModifierKeys), _configuration.HotModifierKeys, "g");
                return $"{modKeys.Replace(", ", "+")}+{key}";
            }
        }

        public event SelectedDeviceChangeHandler SelectedDeviceChanged;
        public event ErrorHandler ErrorTriggered;
        public event AudioChangeHandler AudioDeviceChanged;

        private void RegisterRecovery()
        {
            var settings = new RecoverySettings(new RecoveryData(SaveState, _configuration), 0);
            ApplicationRestartRecoveryManager.RegisterForApplicationRecovery(settings);
            Trace.WriteLine("Recovery Registered");
        }

        private void RegisterForRestart()
        {
            var settings = new RestartSettings("/restart", RestartRestrictions.None);
            ApplicationRestartRecoveryManager.RegisterForApplicationRestart(settings);
            Trace.WriteLine("Restart Registered");
        }

        private int SaveState(object state)
        {
            Trace.WriteLine("Saving State");
            var settings = (SoundSwitchConfiguration) state;
            var cancelled = ApplicationRestartRecoveryManager.ApplicationRecoveryInProgress();
            if (cancelled)
            {
                Trace.Write("Recovery Cancelled");
                ApplicationRestartRecoveryManager.ApplicationRecoveryFinished(false);
                return 0;
            }
            settings.Save();
            ApplicationRestartRecoveryManager.ApplicationRecoveryFinished(true);
            Trace.WriteLine("Recovery Success");
            return 0;
        }

        #region Events

        public class AudioChangeEvent : EventArgs
        {
            public AudioChangeEvent(AudioDeviceWrapper audioDevice)
            {
                AudioDevice = audioDevice;
            }

            public AudioDeviceWrapper AudioDevice { get; }
        }

        public class ExceptionEvent : EventArgs
        {
            public ExceptionEvent(Exception exception)
            {
                Exception = exception;
            }

            public Exception Exception { get; private set; }
        }

        public class DeviceListChanged : EventArgs
        {
            public DeviceListChanged(List<string> seletedDevicesList)
            {
                SeletedDevicesList = seletedDevicesList;
            }

            public List<string> SeletedDevicesList { get; private set; }
        }

        #endregion

        #region Hot keys

        /// <summary>
        ///     Sets the hotkey combination, and <see cref="ReAttachKeyboardHook">re-attaches the keyboard hook</see>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modifierKeys"></param>
        public void SetHotkeyCombination(Keys key, ModifierKeys modifierKeys)
        {
            _configuration.HotKeys = key;
            _configuration.HotModifierKeys = modifierKeys;
            _configuration.Save();

            ReAttachKeyboardHook();
        }

        private KeyboardHook Hook { get; set; }

        public void ReAttachKeyboardHook()
        {
            try
            {
                Hook.Dispose();
                Hook = new KeyboardHook();

                Hook.RegisterHotKey(_configuration.HotModifierKeys, _configuration.HotKeys);
                Hook.KeyPressed += HandleHotkeyPress;
            }
            catch (Exception ex)
            {
                ErrorTriggered?.Invoke(this, new ExceptionEvent(ex));
            }
        }

        private void HandleHotkeyPress(object sender, KeyPressedEventArgs e)
        {
            try
            {
                CycleActiveDevice();
            }
            catch (Exception ex)
            {
                ErrorTriggered?.Invoke(this, new ExceptionEvent(ex));
            }
        }

        #endregion

        #region Misc settings

        /// <summary>
        ///     If the application runs at windows startup
        /// </summary>
        public bool RunAtStartup
        {
            get { return _configuration.RunOnStartup; }
            set
            {
                SetAutoStart(value);
                _configuration.RunOnStartup = value;
                _configuration.Save();
            }
        }


        /// <summary>
        ///     Set the Application as autoStarting using registry
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
        ///     Sets a particular device to be enabled or not
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
        ///     Attempts to set active device to the specified name
        /// </summary>
        /// <param name="device"></param>
        public bool SetActiveDevice(AudioDeviceWrapper device)
        {
            try
            {
                device.SetAsDefault();
                AudioDeviceChanged?.Invoke(this, new AudioChangeEvent(device));
                _configuration.LastActiveDevice = device.FriendlyName;
                _configuration.Save();
                return true;
            }
            catch (Exception ex)
            {
                ErrorTriggered?.Invoke(this, new ExceptionEvent(ex));
            }
            return false;
        }

        /// <summary>
        ///     Cycles the active device to the next device. Returns true if succesfully switched (at least
        ///     as far as we can tell), returns false if could not successfully switch. Throws NoDevicesException
        ///     if there are no devices configured.
        /// </summary>
        public bool CycleActiveDevice()
        {
            var list = AvailableAudioDevices;
            switch (list.Count)
            {
                case 0:
                    throw new NoDevicesException();
                case 1:
                    return false;
            }
            AudioDeviceWrapper defaultDev = null;
            try
            {
                defaultDev = list.First(device => device.IsDefault);
            }
            catch (Exception)
            {
                defaultDev =
                    list.FirstOrDefault(device => device.FriendlyName == _configuration.LastActiveDevice) ??
                    list[0];
            }

            var next = list.SkipWhile((device, i) => device != defaultDev).Skip(1).FirstOrDefault() ?? list[0];
            return SetActiveDevice(next);
        }

        [Serializable]
        private class NoDevicesException : InvalidOperationException
        {
            public NoDevicesException() : base("No devices to select")
            {
            }
        }

        #endregion
    }
}