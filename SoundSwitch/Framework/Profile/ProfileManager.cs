using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RailSharp;
using RailSharp.Internal.Result;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.WinApi;
using SoundSwitch.Common.WinApi.Keyboard;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.Profile
{
    public class ProfileManager
    {
        private readonly Dictionary<Hotkey, ProfileSetting> _profileByHotkey;
        private readonly Dictionary<string, ProfileSetting> _profileByApplication;
        private readonly ForegroundProcess _foregroundProcess;
        private readonly AudioSwitcher _audioSwitcher;

        public IReadOnlyCollection<ProfileSetting> Profiles => AppConfigs.Configuration.ProfileSettings;

        public ProfileManager(ForegroundProcess foregroundProcess, AudioSwitcher audioSwitcher)
        {
            _foregroundProcess = foregroundProcess;
            _audioSwitcher = audioSwitcher;
            _profileByApplication =
                AppConfigs.Configuration.ProfileSettings
                    .Where((setting) => setting.ApplicationPath != null)
                    .ToDictionary(setting => setting.ApplicationPath.ToLower());
            _profileByHotkey = AppConfigs.Configuration.ProfileSettings
                .Where((setting) => setting.Hotkey != null)
                .ToDictionary(setting => setting.Hotkey);
        }

        /// <summary>
        /// Initialize the profile manager. Return the list of Profile that it couldn't register hotkeys for.
        /// </summary>
        /// <returns></returns>
        public Result<ProfileSetting[], VoidSuccess> Init()
        {
            RegisterEvents();

            var errors = AppConfigs.Configuration.ProfileSettings
                .Where(setting => setting.Hotkey != null)
                .Where(profileSetting => !WindowsAPIAdapter.RegisterHotkey(profileSetting.Hotkey))
                .ToArray();

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
                _profileByApplication.TryGetValue(@event.ProcessName.ToLower(), out var profile);
                if (profile == null)
                    return;
                SwitchAudio(profile, @event.ProcessId);
            };

            WindowsAPIAdapter.HotkeyPressed += (sender, args) =>
            {
                _profileByHotkey.TryGetValue(args.Hotkey, out var profile);
                if (profile == null)
                    return;
                SwitchAudio(profile);
            };
        }

        private void SwitchAudio(ProfileSetting profile, uint processId = 0)
        {
            foreach (var device in new[] {profile.Playback, profile.Recording}.Where((info) => info != null))
            {
                if (processId != 0)
                {
                    _audioSwitcher.SwitchProcessTo(
                        device.Id,
                        ERole.ERole_enum_count,
                        (EDataFlow) device.Type,
                        processId);
                }

                //Always switch the default device.
                //Easy way to be sure a notification will be send.
                //And to be consistent with the default audio device.
                _audioSwitcher.SwitchTo(device.Id, ERole.ERole_enum_count);
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
                    if (profile.ApplicationPath != null)
                        _profileByApplication.Add(profile.ApplicationPath.ToLower(), profile);
                    if (profile.Hotkey != null)
                        _profileByHotkey.Add(profile.Hotkey, profile);

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

            if (profile.ApplicationPath == null && profile.Hotkey == null)
            {
                return SettingsStrings.profile_error_needHKOrPath;
            }

            if (profile.Recording == null && profile.Playback == null)
            {
                return SettingsStrings.profile_error_needPlaybackOrRecording;
            }

            if (profile.Hotkey != null && _profileByHotkey.ContainsKey(profile.Hotkey))
            {
                return string.Format(SettingsStrings.profile_error_hotkey, profile.Hotkey);
            }

            if (profile.ApplicationPath != null && _profileByApplication.ContainsKey(profile.ApplicationPath.ToLower()))
            {
                return string.Format(SettingsStrings.profile_error_application, profile.ApplicationPath);
            }

            if (AppConfigs.Configuration.ProfileSettings.Contains(profile))
            {
                return string.Format(SettingsStrings.profile_error_name, profile.ProfileName);
            }

            if (profile.Hotkey != null && !WindowsAPIAdapter.RegisterHotkey(profile.Hotkey))
            {
                return string.Format(SettingsStrings.profile_error_hotkey, profile.Hotkey);
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
            var errors = new List<ProfileSetting>();
            foreach (var profile in profiles)
            {
                if (!AppConfigs.Configuration.ProfileSettings.Contains(profile))
                {
                    errors.Add(profile);
                    continue;
                }


                if (profile.ApplicationPath != null)
                {
                    _profileByApplication.Remove(profile.ApplicationPath.ToLower());
                }

                if (profile.Hotkey != null)
                {
                    WindowsAPIAdapter.UnRegisterHotkey(profile.Hotkey);
                    _profileByHotkey.Remove(profile.Hotkey);
                }

                AppConfigs.Configuration.ProfileSettings.Remove(profile);
            }

            AppConfigs.Configuration.Save();
            if (errors.Count > 0)
            {
                return errors.ToArray();
            }

            return Result.Success();
        }
    }
}