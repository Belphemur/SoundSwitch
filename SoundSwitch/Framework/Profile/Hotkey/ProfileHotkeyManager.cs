using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SoundSwitch.Common.Framework.Icon;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.UI.Menu;
using SoundSwitch.UI.Menu.Component;

namespace SoundSwitch.Framework.Profile.Hotkey;

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
        public HotKeyItem(bool selected, Profile payload)
            : this(selected, payload, payload.Icon) { }

        private HotKeyItem(bool selected, Profile payload, IconHandle iconHandle)
            : base(iconHandle.Icon, payload.Name, selected, payload.Name, payload)
        {
            // DataContainer clones the icon immediately; release our reference.
            iconHandle.Dispose();
        }
    }

    private readonly Dictionary<HotKey, HotKeysSelection> _profiles = new();
    private readonly ProfileManager _profileManager;
    private readonly IAppSettings _appSettings;
    private readonly HotkeyRegistry _hotkeyRegistry;

    public ProfileHotkeyManager(ProfileManager profileManager, IAppSettings appSettings, HotkeyRegistry hotkeyRegistry)
    {
        _profileManager = profileManager;
        _appSettings = appSettings;
        _hotkeyRegistry = hotkeyRegistry;
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

        if (_appSettings.QuickMenuEnabled)
        {
            QuickMenuManager<Profile>.Instance.DisplayMenu(hotKeysSelection.Profiles.Select(profile => new HotKeyItem(nextProfile == profile, profile)), @event =>
            {
                hotKeysSelection.LastUsed = @event.Item.Payload;
                _profileManager.SwitchAudio(@event.Item.Payload);
            });
        }
    }

    /// <summary>
    /// Check if it's a valid hotkey
    /// </summary>
    /// <param name="hotKey"></param>
    /// <returns></returns>
    public bool IsValidHotkey(HotKey hotKey)
    {
        return GetHotkeyConflict(hotKey) == null;
    }

    /// <summary>
    /// Localized reason a profile can't claim the hotkey, or null when it can.
    /// The registry names the SoundSwitch system that already holds the combination
    /// (ADR docs/design/architecture/2026-11-21-shared-mechanism-consolidation.md §D3),
    /// so a cross-system conflict is reported instead of silently stealing the
    /// other owner's registration through the raw OS probe.
    /// </summary>
    public string? GetHotkeyConflict(HotKey hotKey)
    {
        if (_profiles.ContainsKey(hotKey))
            return null;

        var conflict = _hotkeyRegistry.DescribeConflict(hotKey);
        if (conflict != null)
            return conflict;

        return WindowsAPIAdapter.RegisterHotKey(hotKey) && WindowsAPIAdapter.UnRegisterHotKey(hotKey)
            ? null
            : string.Format(SettingsStrings.hotkey_conflict_external, hotKey);
    }

    public bool Add(HotKey hotKey, Profile profile)
    {
        if (!_profiles.ContainsKey(hotKey))
        {
            if (_hotkeyRegistry.TryRegister(hotKey, new HotkeyOwner(HotkeyOwnerKind.Profile, profile.Name)) != null)
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
            // Owner-scoped: with the promotion above, the recorded owner is exactly
            // this profile when the last claimant leaves, so only what the profile
            // feature actually owns is unbound.
            return _hotkeyRegistry.Unregister(hotKey, new HotkeyOwner(HotkeyOwnerKind.Profile, profile.Name));
        }

        // The recorded owner must always be a profile the user can still find:
        // when other claimants remain after this removal, promote one of them so
        // conflict messages never name a deleted profile.
        _hotkeyRegistry.UpdateOwner(hotKey, new HotkeyOwner(HotkeyOwnerKind.Profile, profiles.First().Name));

        if (_profiles[hotKey].LastUsed == profile)
        {
            _profiles[hotKey].LastUsed = profiles.First();
        }

        return true;
    }
}
