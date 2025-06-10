using System;

namespace SoundSwitch.Framework.Profile.Trigger;

public static class TriggerEnumExtensions
{
    /// <summary>
    /// Switch on the given enum
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void Switch(this TriggerFactory.Enum @enum, Action hotkey, Action window, Action process, Action steam, Action startup, Action uwpApp, Action trayMenu, Action changed)
    {
        Match(@enum, () => hotkey, () => window, () => process, () => steam, () => startup, () => uwpApp, () => trayMenu, () => changed)();
    }

    /// <summary>
    /// Match on the enum
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static T Match<T>(this TriggerFactory.Enum @enum, Func<T> hotkey, Func<T> window, Func<T> process, Func<T> steam, Func<T> startup, Func<T> uwpApp, Func<T> trayMenu, Func<T> changed)
    {
        return @enum switch
        {
            TriggerFactory.Enum.HotKey   => hotkey(),
            TriggerFactory.Enum.Window   => window(),
            TriggerFactory.Enum.Process  => process(),
            TriggerFactory.Enum.Steam    => steam(),
            TriggerFactory.Enum.Startup  => startup(),
            TriggerFactory.Enum.UwpApp   => uwpApp(),
            TriggerFactory.Enum.TrayMenu => trayMenu(),
            TriggerFactory.Enum.Changed  => changed(),
            _                            => throw new ArgumentOutOfRangeException(nameof(@enum), @enum, null)
        };
    }
}