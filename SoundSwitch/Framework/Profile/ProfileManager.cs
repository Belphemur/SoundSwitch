#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using NAudio.CoreAudioApi;
using RailSharp;
using RailSharp.Internal.Result;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Com.User;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.Profile.Hotkey;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.Util;

namespace SoundSwitch.Framework.Profile
{
    public class ProfileManager
    {
        public delegate void ShowError(string errorMessage, string errorTitle);

        private readonly WindowMonitor _windowMonitor;
        private readonly AudioSwitcher _audioSwitcher;
        private readonly IAudioDeviceLister _activeDeviceLister;
        private readonly ShowError _showError;
        private readonly TriggerFactory _triggerFactory;
        private readonly NotificationManager.NotificationManager _notificationManager;

        private Profile? _steamProfile;

        private readonly Dictionary<User32.NativeMethods.HWND, Profile> _activeWindowsTrigger = new();

        private readonly Dictionary<string, (Profile Profile, Trigger.Trigger Trigger)> _profileByApplication = new();
        private readonly Dictionary<string, (Profile Profile, Trigger.Trigger Trigger)> _profilesByWindowName = new();
        private readonly Dictionary<string, (Profile Profile, Trigger.Trigger Trigger)> _profilesByUwpApp = new();

        private readonly ProfileHotkeyManager _profileHotkeyManager;


        public IReadOnlyCollection<Profile> Profiles => AppConfigs.Configuration.Profiles;

        public ProfileManager(WindowMonitor windowMonitor,
                              AudioSwitcher audioSwitcher,
                              IAudioDeviceLister activeDeviceLister,
                              ShowError showError,
                              TriggerFactory triggerFactory,
                              NotificationManager.NotificationManager notificationManager)
        {
            _windowMonitor = windowMonitor;
            _audioSwitcher = audioSwitcher;
            _activeDeviceLister = activeDeviceLister;
            _showError = showError;
            _triggerFactory = triggerFactory;
            _notificationManager = notificationManager;
            _profileHotkeyManager = new(this);
        }

        private bool RegisterTriggers(Profile profile, bool onInit = false)
        {
            var success = true;
            foreach (var trigger in profile.Triggers)
            {
                success &= trigger.Type.Match(() => _profileHotkeyManager.Add(trigger.HotKey, profile),
                    () =>
                    {
                        _profilesByWindowName.Add(trigger.WindowName.ToLower(), (profile, trigger));
                        return true;
                    },
                    () =>
                    {
                        _profileByApplication.Add(trigger.ApplicationPath.ToLower(), (profile, trigger));
                        return true;
                    },
                    () =>
                    {
                        _steamProfile = profile;
                        return true;
                    },
                    () =>
                    {
                        if (!onInit)
                        {
                            return true;
                        }

                        SwitchAudio(profile);
                        return true;
                    }, () =>
                    {
                        _profilesByUwpApp.Add(trigger.WindowName.ToLower(), (profile, trigger));
                        return true;
                    },
                    () => true);
               
            }

            return success;
        }

        private void UnRegisterTriggers(Profile profile)
        {
            foreach (var trigger in profile.Triggers)
            {
                trigger.Type.Switch(() => { _profileHotkeyManager.Remove(trigger.HotKey, profile); },
                    () => { _profilesByWindowName.Remove(trigger.WindowName.ToLower()); },
                    () => { _profileByApplication.Remove(trigger.ApplicationPath.ToLower()); },
                    () => { _steamProfile = null; }, () => { },
                    () => { _profilesByUwpApp.Remove(trigger.WindowName.ToLower()); },
                    () => { });
            }
        }

        /// <summary>
        /// Initialize the profile manager. Return the list of Profile that it couldn't register hotkeys for.
        /// </summary>
        /// <returns></returns>
        public Result<Profile[], VoidSuccess> Init()
        {
            var errors = AppConfigs.Configuration.Profiles.Where(profile => !RegisterTriggers(profile, true)).ToArray();

            try
            {
                RegisterEvents();

                InitializeProfileExistingProcess();


                if (errors.Length > 0)
                {
                    Log.Warning("Couldn't initiate all profiles: {profiles}", errors.Select(profile => profile.Name));
                    return errors;
                }

                return Result.Success();
            }
            finally
            {
                Log.Information("Profile manager initiated {profiles} with {errors} errors", AppConfigs.Configuration.Profiles.Count, errors.Length);
            }
        }

        private void RegisterEvents()
        {
            _windowMonitor.ForegroundChanged += (sender, @event) =>
            {
                if (HandleSteamBigPicture(@event)) return;

                if (HandleApplication(@event)) return;

                if (HandleUwpApp(@event)) return;

                if (HandleWindowName(@event)) return;
            };

            WindowsAPIAdapter.WindowDestroyed += (sender, @event) => { RestoreState(@event.Hwnd); };
        }

        private bool HandleUwpApp(WindowMonitor.Event @event)
        {
            (Profile Profile, Trigger.Trigger Trigger) profileTuple;

            var windowNameLowerCase = @event.WindowName.ToLower();

            profileTuple = _profilesByUwpApp.FirstOrDefault(pair => windowNameLowerCase.Contains(pair.Key)).Value;
            if (profileTuple != default && @event.WindowClass == "ApplicationFrameWindow")
            {
                SaveCurrentState(@event.Hwnd, profileTuple.Profile, profileTuple.Trigger);
                SwitchAudio(profileTuple.Profile);
                return true;
            }

            return false;
        }

        private bool HandleWindowName(WindowMonitor.Event @event)
        {
            var windowNameLower = @event.WindowName.ToLower();

            var profileTuple = _profilesByWindowName.FirstOrDefault(pair => windowNameLower.Contains(pair.Key)).Value;
            if (profileTuple != default)
            {
                SaveCurrentState(@event.Hwnd, profileTuple.Profile, profileTuple.Trigger);
                SwitchAudio(profileTuple.Profile, @event.ProcessId);
                return true;
            }

            return false;
        }

        private bool HandleApplication(WindowMonitor.Event @event)
        {
            (Profile Profile, Trigger.Trigger Trigger) profileTuple;
            if (_profileByApplication.TryGetValue(@event.ProcessName.ToLower(), out profileTuple))
            {
                SaveCurrentState(@event.Hwnd, profileTuple.Profile, profileTuple.Trigger);
                SwitchAudio(profileTuple.Profile, @event.ProcessId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Save the current state of the system if it wasn't saved already
        /// </summary>
        private bool SaveCurrentState(User32.NativeMethods.HWND windowHandle, Profile profile, Trigger.Trigger trigger)
        {
            var triggerDefinition = _triggerFactory.Get(trigger.Type);
            if (!(triggerDefinition.AlwaysDefaultAndRestoreDevice || (profile.RestoreDevices && triggerDefinition.CanRestoreDevices)))
            {
                return false;
            }

            if (_activeWindowsTrigger.ContainsKey(windowHandle))
            {
                return false;
            }

            var communication = _audioSwitcher.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eCommunications);
            var playback = _audioSwitcher.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            var recording = _audioSwitcher.GetDefaultAudioEndpoint(EDataFlow.eCapture, ERole.eMultimedia);

            var currentState = new Profile
            {
                AlsoSwitchDefaultDevice = true,
                Name = SettingsStrings.profile_trigger_restoreDevices_title,
                Communication = communication,
                Playback = playback,
                Recording = recording,
                NotifyOnActivation = profile.NotifyOnActivation
            };
            _activeWindowsTrigger.Add(windowHandle, currentState);
            return true;
        }

        /// <summary>
        /// Restore the old state
        /// </summary>
        /// <param name="windowHandle"></param>
        /// <returns></returns>
        private bool RestoreState(User32.NativeMethods.HWND windowHandle)
        {
            if (!_activeWindowsTrigger.TryGetValue(windowHandle, out var oldState))
            {
                return false;
            }

            SwitchAudio(oldState);
            _activeWindowsTrigger.Remove(windowHandle);
            return true;
        }

        /// <summary>
        /// Handle steam big picture
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        private bool HandleSteamBigPicture(WindowMonitor.Event @event)
        {
            if (_steamProfile == null || @event.WindowName != "Steam" || @event.WindowClass != "CUIEngineWin32")
                return false;

            SaveCurrentState(@event.Hwnd, _steamProfile, _steamProfile.Triggers.First(trigger => trigger.Type == TriggerFactory.Enum.Steam));

            SwitchAudio(_steamProfile);
            return true;
        }

        private DeviceInfo? CheckDeviceAvailable(DeviceInfo deviceInfo)
        {
            return deviceInfo.Type switch
            {
                DataFlow.Capture => _activeDeviceLister.RecordingDevices.FirstOrDefault(info => info.Equals(deviceInfo)),
                _                => _activeDeviceLister.PlaybackDevices.FirstOrDefault(info => info.Equals(deviceInfo))
            };
        }

        private void SwitchAudio(Profile profile, uint processId)
        {
            _notificationManager.NotifyProfileChanged(profile, processId);
            foreach (var device in profile.Devices)
            {
                var deviceToUse = CheckDeviceAvailable(device.DeviceInfo);
                if (deviceToUse == null)
                {
                    _showError.Invoke(string.Format(SettingsStrings.profile_error_device_not_found, device.DeviceInfo.NameClean), $"{SettingsStrings.profile_error_title}: {profile.Name}");
                    continue;
                }

                _audioSwitcher.SwitchProcessTo(
                    deviceToUse.Id,
                    device.Role,
                    (EDataFlow)deviceToUse.Type,
                    processId);

                if (profile.AlsoSwitchDefaultDevice)
                {
                    _audioSwitcher.SwitchTo(deviceToUse.Id, device.Role);
                }
            }
        }


        public void SwitchAudio(Profile profile)
        {
            _notificationManager.NotifyProfileChanged(profile, null);
            foreach (var device in profile.Devices)
            {
                var deviceToUse = CheckDeviceAvailable(device.DeviceInfo);
                if (deviceToUse == null)
                {
                    _showError.Invoke(string.Format(SettingsStrings.profile_error_device_not_found, device.DeviceInfo.NameClean), $"{SettingsStrings.profile_error_title}: {profile.Name}");
                    continue;
                }

                _audioSwitcher.SwitchTo(deviceToUse.Id, device.Role);
            }
        }

        /// <summary>
        /// Return the globally available triggers
        /// Remove the one that aren't accessible anymore
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITriggerDefinition> AvailableTriggers()
        {
            var triggers = Profiles.SelectMany(profile => profile.Triggers).GroupBy(trigger => trigger.Type).ToDictionary(grouping => grouping.Key, grouping => grouping.Count());
            var triggerFactory = new TriggerFactory();
            return triggerFactory.AllImplementations
                                 .Where(pair =>
                                 {
                                     if (triggers.TryGetValue(pair.Key, out var count))
                                     {
                                         return pair.Value.MaxGlobalOccurence < count;
                                     }

                                     return true;
                                 })
                                 .Select(pair => pair.Value);
        }

        /// <summary>
        /// Add a profile to the system
        /// </summary>
        public Result<string, VoidSuccess> AddProfile(Profile profile)
        {
            return ValidateProfile(profile)
                .Map(success =>
                {
                    RegisterTriggers(profile);
                    AppConfigs.Configuration.Profiles.Add(profile);
                    AppConfigs.Configuration.Save();

                    return success;
                });
        }

        /// <summary>
        /// Update a profile
        /// </summary>
        public Result<string, VoidSuccess> UpdateProfile(Profile oldProfile, Profile newProfile)
        {
            DeleteProfiles(new[] { oldProfile });
            return ValidateProfile(newProfile)
                   .Map(success =>
                   {
                       RegisterTriggers(newProfile);
                       AppConfigs.Configuration.Profiles.Add(newProfile);
                       AppConfigs.Configuration.Save();
                       return success;
                   }).Catch(s =>
                   {
                       AddProfile(oldProfile);
                       return s;
                   });
        }

        private Result<string, VoidSuccess> ValidateProfile(Profile profile)
        {
            if (string.IsNullOrEmpty(profile.Name))
            {
                return SettingsStrings.profile_error_no_name;
            }

            if (profile.Triggers.Count == 0)
            {
                return SettingsStrings.profile_error_triggers_min;
            }

            if (profile.Recording == null && profile.Playback == null)
            {
                return SettingsStrings.profile_error_needPlaybackOrRecording;
            }

            if (AppConfigs.Configuration.Profiles.Contains(profile))
            {
                return string.Format(SettingsStrings.profile_error_name, profile.Name);
            }

            foreach (var trigger in profile.Triggers)
            {
                var error = trigger.Type.Match(() =>
                    {
                        if (trigger.HotKey == null || !_profileHotkeyManager.IsValidHotkey(trigger.HotKey))
                        {
                            return string.Format(SettingsStrings.profile_error_hotkey, trigger.HotKey);
                        }

                        return null;
                    }, () =>
                    {
                        if (string.IsNullOrEmpty(trigger.WindowName) || _profilesByWindowName.ContainsKey(trigger.WindowName.ToLower()))
                        {
                            return string.Format(SettingsStrings.profile_error_window, trigger.WindowName);
                        }

                        return null;
                    }, () =>
                    {
                        if (string.IsNullOrEmpty(trigger.ApplicationPath) || _profileByApplication.ContainsKey(trigger.ApplicationPath.ToLower()))
                        {
                            return string.Format(SettingsStrings.profile_error_application, trigger.ApplicationPath);
                        }

                        return null;
                    },
                    () => _steamProfile != null ? SettingsStrings.profile_error_steam : null,
                    () => null,
                    () =>
                    {
                        if (string.IsNullOrEmpty(trigger.WindowName) || _profilesByUwpApp.ContainsKey(trigger.WindowName.ToLower()))
                        {
                            return string.Format(SettingsStrings.profile_error_window, trigger.WindowName);
                        }

                        return null;
                    }, () => null);
                if (error != null)
                {
                    return error;
                }
            }

            return Result.Success();
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

            if (profile.HotKey != null && !_profileHotkeyManager.IsValidHotkey(profile.HotKey))
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
        public Result<Profile[], VoidSuccess> DeleteProfiles(IEnumerable<Profile> profilesToDelete)
        {
            var errors = new List<Profile>();
            var profiles = profilesToDelete.ToArray();
            var resetProcessAudio = profiles.Any(profile => profile.Triggers.Any(trigger => trigger.Type == TriggerFactory.Enum.Process || trigger.Type == TriggerFactory.Enum.Window));
            foreach (var profile in profiles)
            {
                if (!AppConfigs.Configuration.Profiles.Contains(profile))
                {
                    errors.Add(profile);
                    continue;
                }

                UnRegisterTriggers(profile);

                AppConfigs.Configuration.Profiles.Remove(profile);
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
                        var handle = User32.NativeMethods.HWND.Cast(process.Handle);
                        SaveCurrentState(handle, profile.Profile, profile.Trigger);
                        SwitchAudio(profile.Profile, (uint)process.Id);
                    }
                }
                catch (Win32Exception)
                {
                    //Happen when trying to access process MainModule belonging to windows like svchost
                }
                catch (InvalidOperationException)
                {
                    //ApplicationPath in a weird state, can't get MainModule for it.
                }
                catch (Exception e)
                {
                    Log.Error(e, "Couldn't get information about the given process.");
                }
            }
        }
    }
}