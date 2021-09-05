using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.UI.Menu;
using SoundSwitch.UI.Menu.Component;

namespace SoundSwitch.Framework.Profile.Hotkey
{
    public class ProfileHotkeyManager
    {
        private record HotKeysSelection(HashSet<Profile> Profiles)
        {
            [NotNull]
            internal Profile LastUsed { get; set; }

            internal bool HasOnlyOne => Profiles.Count == 1;

            public Profile NextOne() => Profiles.SkipWhile(profile => profile != LastUsed).Skip(1).FirstOrDefault() ?? Profiles.First();
        };

        private class HotKeyItem : IconMenuItem<Profile>.DataContainer
        {
            public HotKeyItem(bool selected, Profile payload) : base(payload.Icon, payload.Name, selected, payload.Name, payload)
            {
            }
        }

        private readonly Dictionary<HotKey, HotKeysSelection> _profiles = new();
        private readonly ProfileManager _profileManager;

        public ProfileHotkeyManager(ProfileManager profileManager)
        {
            _profileManager = profileManager;
            WindowsAPIAdapter.HotKeyPressed += WindowsAPIAdapterOnHotKeyPressed;
        }

        private void WindowsAPIAdapterOnHotKeyPressed(object? sender, WindowsAPIAdapter.KeyPressedEventArgs e)
        {
            if (!_profiles.ContainsKey(e.HotKey))
            {
                return;
            }

            var hotKeysSelection = _profiles[e.HotKey];
            if (hotKeysSelection.HasOnlyOne)
            {
                _profileManager.SwitchAudio(hotKeysSelection.LastUsed);
                return;
            }

            var nextProfile = hotKeysSelection.NextOne();
            _profileManager.SwitchAudio(nextProfile);
            hotKeysSelection.LastUsed = nextProfile;

            QuickMenuManager<Profile>.Instance.DisplayMenu(hotKeysSelection.Profiles.Select(profile => new HotKeyItem(nextProfile == profile, profile)), @event =>
            {
                hotKeysSelection.LastUsed = @event.Item.Payload;
                _profileManager.SwitchAudio(@event.Item.Payload);
            });
        }

        /// <summary>
        /// Check if it's a valid hotkey
        /// </summary>
        /// <param name="hotKey"></param>
        /// <returns></returns>
        public bool IsValidHotkey(HotKey hotKey)
        {
            if (_profiles.ContainsKey(hotKey))
                return true;

            return WindowsAPIAdapter.RegisterHotKey(hotKey) && WindowsAPIAdapter.UnRegisterHotKey(hotKey);
        }

        public bool Add(HotKey hotKey, Profile profile)
        {
            if (!_profiles.ContainsKey(hotKey))
            {
                if (!WindowsAPIAdapter.RegisterHotKey(hotKey))
                {
                    return false;
                }

                _profiles[hotKey] = new HotKeysSelection(new HashSet<Profile>(new[] { profile }))
                {
                    LastUsed = profile
                };
                return true;
            }

            _profiles[hotKey].Profiles.Add(profile);
            return true;
        }

        public bool Remove(HotKey hotKey, Profile profile)
        {
            if (!_profiles.ContainsKey(hotKey))
            {
                return false;
            }

            var profiles = _profiles[hotKey].Profiles;
            profiles.Remove(profile);
            if (profiles.Count == 0)
            {
                _profiles.Remove(hotKey);
                return WindowsAPIAdapter.UnRegisterHotKey(hotKey);
            }

            if (_profiles[hotKey].LastUsed == profile)
            {
                _profiles[hotKey].LastUsed = profiles.First();
            }

            return true;
        }
    }
}