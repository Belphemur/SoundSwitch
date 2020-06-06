#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using RailSharp;
using RailSharp.Internal.Result;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio.Lister;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using HotKey = SoundSwitch.Framework.WinApi.Keyboard.HotKey;
using WindowsAPIAdapter = SoundSwitch.Framework.WinApi.WindowsAPIAdapter;

namespace SoundSwitch.Framework.Profile
{
    public class ProfileManager
    {
        public delegate void ShowError(string errorMessage, string errorTitle);

        private readonly Dictionary<HotKey, ProfileSetting> _profileByHotkey;
        private readonly Dictionary<string, ProfileSetting> _profileByApplication;
        private readonly ForegroundProcess                  _foregroundProcess;
        private readonly AudioSwitcher                      _audioSwitcher;
        private readonly IAudioDeviceLister                 _activeDeviceLister;
        private readonly ShowError                          _showError;

        public IReadOnlyCollection<ProfileSetting> Profiles => AppConfigs.Configuration.ProfileSettings;

        public ProfileManager(ForegroundProcess  foregroundProcess,
                              AudioSwitcher      audioSwitcher,
                              IAudioDeviceLister activeDeviceLister,
                              ShowError          showError)
        {
            _foregroundProcess  = foregroundProcess;
            _audioSwitcher      = audioSwitcher;
            _activeDeviceLister = activeDeviceLister;
            _showError          = showError;
            _profileByApplication =
                AppConfigs.Configuration.ProfileSettings
                          .Where((setting) => setting.ApplicationPath != null)
                          .ToDictionary(setting => setting.ApplicationPath!.ToLower());
            _profileByHotkey =
                AppConfigs.Configuration.ProfileSettings
                          .Where((setting) => setting.HotKey != null)
                          .ToDictionary(setting => setting.HotKey)!;
        }

        /// <summary>
        /// Initialize the profile manager. Return the list of Profile that it couldn't register hotkeys for.
        /// </summary>
        /// <returns></returns>
        public Result<ProfileSetting[], VoidSuccess> Init()
        {
            RegisterEvents();

            var errors = AppConfigs.Configuration.ProfileSettings
                                   .Where(setting => setting.HotKey != null)
                                   .Where(profileSetting => !WindowsAPIAdapter.RegisterHotKey(profileSetting.HotKey))
                                   .ToArray();

            InitializeProfileExistingProcess();

            if (errors.Length > 0)
            {
                return errors;
            }

            return Result.Success();
        }

        private void RegisterEvents()
        {
            _foregroundProcess.Changed += (sender, @event) =>
            {
                if (!_profileByApplication.TryGetValue(@event.ProcessName.ToLower(), out var profile))
                    return;
                SwitchAudio(profile, @event.ProcessId);
            };

            WindowsAPIAdapter.HotKeyPressed += (sender, args) =>
            {
                if (!_profileByHotkey.TryGetValue(args.HotKey, out var profile))
                    return;
                SwitchAudio(profile);
            };
        }

        private DeviceInfo? CheckDeviceAvailable(DeviceInfo deviceInfo)
        {
            return deviceInfo.Type switch
            {
                DataFlow.Capture => _activeDeviceLister.RecordingDevices.FirstOrDefault(info => info.Equals(deviceInfo)),
                _                => _activeDeviceLister.PlaybackDevices.FirstOrDefault(info => info.Equals(deviceInfo))
            };
        }

        private void SwitchAudio(ProfileSetting profile, uint processId)
        {
            foreach (var device in profile.Devices)
            {
                var deviceToUse = CheckDeviceAvailable(device);
                if (deviceToUse == null)
                {
                    _showError.Invoke(string.Format(SettingsStrings.profile_error_device_not_found, device.Name), $"{SettingsStrings.profile_error_title}: {profile.ProfileName}");
                    continue;
                }
                _audioSwitcher.SwitchProcessTo(
                    deviceToUse.Id,
                    ERole.ERole_enum_count,
                    (EDataFlow) deviceToUse.Type,
                    processId);

                if (profile.AlsoSwitchDefaultDevice)
                {
                    _audioSwitcher.SwitchTo(deviceToUse.Id, ERole.ERole_enum_count);
                }
            }
        }

        private void SwitchAudio(ProfileSetting profile)
        {
            foreach (var device in profile.Devices)
            {
                var deviceToUse = CheckDeviceAvailable(device);
                if (deviceToUse == null)
                {
                    _showError.Invoke(string.Format(SettingsStrings.profile_error_device_not_found, device.Name), $"{SettingsStrings.profile_error_title}: {profile.ProfileName}");
                    continue;
                }
                _audioSwitcher.SwitchTo(deviceToUse.Id, ERole.ERole_enum_count);
            }
        }

        /// <summary>
        /// Add a profile to the system
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public Result<string, VoidSuccess> AddProfile(ProfileSetting profile)
        {
            return ValidateAddProfile(profile)
                .Map(success =>
                {
                    if (!string.IsNullOrEmpty(profile.ApplicationPath))
                        _profileByApplication.Add(profile.ApplicationPath!.ToLower(), profile);
                    if (profile.HotKey != null)
                        _profileByHotkey.Add(profile.HotKey, profile);

                    AppConfigs.Configuration.ProfileSettings.Add(profile);
                    AppConfigs.Configuration.Save();

                    return Result.Success();
                });
        }

        private Result<string, VoidSuccess> ValidateAddProfile(ProfileSetting profile)
        {
            if (string.IsNullOrEmpty(profile.ProfileName))
            {
                return SettingsStrings.profile_error_no_name;
            }

            if (string.IsNullOrEmpty(profile.ApplicationPath) && profile.HotKey == null)
            {
                return SettingsStrings.profile_error_needHKOrPath;
            }

            if (profile.Recording == null && profile.Playback == null)
            {
                return SettingsStrings.profile_error_needPlaybackOrRecording;
            }

            if (profile.HotKey != null && _profileByHotkey.ContainsKey(profile.HotKey))
            {
                return string.Format(SettingsStrings.profile_error_hotkey, profile.HotKey);
            }

            if (!string.IsNullOrEmpty(profile.ApplicationPath) && _profileByApplication.ContainsKey(profile.ApplicationPath.ToLower()))
            {
                return string.Format(SettingsStrings.profile_error_application, profile.ApplicationPath);
            }

            if (AppConfigs.Configuration.ProfileSettings.Contains(profile))
            {
                return string.Format(SettingsStrings.profile_error_name, profile.ProfileName);
            }

            if (profile.HotKey != null && !WindowsAPIAdapter.RegisterHotKey(profile.HotKey))
            {
                return string.Format(SettingsStrings.profile_error_hotkey, profile.HotKey);
            }

            return Result.Success();
        }

        /// <summary>
        /// Delete the given profiles.
        ///
        /// Result Failure contains the profile that couldn't be deleted because they don't exists.
        /// </summary>
        /// <param name="profiles"></param>
        /// <returns></returns>
        public Result<ProfileSetting[], VoidSuccess> DeleteProfiles(IEnumerable<ProfileSetting> profiles)
        {
            var errors            = new List<ProfileSetting>();
            var resetProcessAudio = false;
            foreach (var profile in profiles)
            {
                if (!AppConfigs.Configuration.ProfileSettings.Contains(profile))
                {
                    errors.Add(profile);
                    continue;
                }


                if (profile.ApplicationPath != null)
                {
                    resetProcessAudio = true;
                    _profileByApplication.Remove(profile.ApplicationPath.ToLower());
                }

                if (profile.HotKey != null)
                {
                    WindowsAPIAdapter.UnRegisterHotKey(profile.HotKey);
                    _profileByHotkey.Remove(profile.HotKey);
                }

                AppConfigs.Configuration.ProfileSettings.Remove(profile);
            }

            AppConfigs.Configuration.Save();
            if (errors.Count > 0)
            {
                return errors.ToArray();
            }

            if (resetProcessAudio)
            {
                _audioSwitcher.ResetProcessDeviceConfiguration();
                InitializeProfileExistingProcess();
            }

            return Result.Success();
        }

        private void InitializeProfileExistingProcess()
        {
            if (_profileByApplication.Count == 0)
            {
                return;
            }

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.HasExited)
                        continue;
                    if (!process.Responding)
                        continue;
                    var filePath = process.MainModule?.FileName.ToLower();
                    if (filePath == null)
                    {
                        continue;
                    }

                    if (_profileByApplication.TryGetValue(filePath, out var profile))
                    {
                        SwitchAudio(profile, (uint) process.Id);
                    }
                }
                catch (Win32Exception)
                {
                    //Happen when trying to access process MainModule belonging to windows like svchost
                }
                catch (InvalidOperationException)
                {
                    //Process in a weird state, can't get MainModule for it.
                }
                catch (Exception e)
                {
                    Log.Error(e, "Couldn't get information about the given process.");
                }
            }
        }
    }
}