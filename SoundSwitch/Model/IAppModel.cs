/********************************************************************
 * Copyright (C) 2015 Jeroen Pelgrims
 * Copyright (C) 2015-2017 Antoine Aflalo
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
using NAudio.CoreAudioApi;
using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Banner.BannerPosition;
using SoundSwitch.Framework.Banner.BannerPosition.Position;
using SoundSwitch.Framework.Banner.MicrophoneMute;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.TrayIcon.IconDoubleClick;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Localization.Factory;
using SoundSwitch.UI.Component;

namespace SoundSwitch.Model;

public interface IAppModel : IDisposable
{
    #region Properties

    /// <summary>
    /// Devices selected for Switching
    /// </summary>
    DeviceCollection<DeviceInfo> SelectedDevices { get; }

    /// <summary>
    /// An union between the Active <see cref="IAudioDevice" /> of Windows and <see cref="SelectedPlaybackDevicesList" />
    /// </summary>
    IEnumerable<DeviceFullInfo> AvailablePlaybackDevices { get; }

    /// <summary>
    /// An union between the Active <see cref="IAudioDevice" /> of Windows and <see cref="SelectedRecordingDevicesList" />
    /// </summary>
    IEnumerable<DeviceFullInfo> AvailableRecordingDevices { get; }

    /// <summary>
    /// If the Playback device need also to be set for Communications.
    /// </summary>
    bool SetCommunications { get; set; }

    /// <summary>
    /// If the application runs at windows startup.
    /// </summary>
    bool RunAtStartup { get; set; }

    /// <summary>
    /// What did the user want as Notification of device changed
    /// </summary>
    NotificationTypeEnum NotificationSettings { get; set; }

    /// <summary>
    /// What did the user want as Banner Position of device changed
    /// </summary>
    BannerPositionEnum BannerPosition { get; set; }

    /// <summary>
    /// The tray icon of the application
    /// </summary>
    TrayIcon TrayIcon { get; }

    /// <summary>
    /// List the active audio devices
    /// </summary>
    IAudioDeviceLister AudioDeviceLister { get; }

    /// <summary>
    /// Beta or Stable channel.
    /// </summary>
    bool IncludeBetaVersions { get; set; }

    /// <summary>
    /// The sound to be played for a Custom notification.
    /// <exception cref="CachedSoundFileNotExistsException">Sound file doesn't exists or not set.</exception>
    /// </summary>
    CachedSound CustomNotificationSound { get; set; }

    /// <summary>
    /// Specifies how the application searches for updates and installs them.
    /// </summary>
    UpdateMode UpdateMode { get; set; }

    /// <summary>
    /// The language of the application.
    /// </summary>
    Language Language { get; set; }

    /// <summary>
    /// Switch also the foreground program
    /// </summary>
    bool SwitchForegroundProgram { get; set; }

    /// <summary>
    /// Always show banner on primary screen instead of active screen
    /// </summary>
    bool NotifyUsingPrimaryScreen { get; set; }

    /// <summary>
    /// Manage the profile in the application
    /// </summary>
    ProfileManager ProfileManager { get; }

    bool Telemetry { get; set; }
    bool QuickMenuEnabled { get; set; }
    bool KeepVolumeEnabled { get; set; }

    /// <summary>
    /// How many notification to show at the same time
    /// </summary>
    int MaxNumberNotification { get; set; }

    /// <summary>
    /// Is there only 1 concurrent notification enabled ? 
    /// </summary>
    bool IsSingleNotification { get; set; }

    TimeSpan BannerOnScreenTime { get; set; }
    int BannerOnScreenTimeSecs { get; set; }

    /// <summary>
    /// Current banner position implementation based on the BannerPosition setting
    /// </summary>
    IPosition BannerPositionImpl { get; }

    /// <summary>
    /// Show a banner when microphone is muted
    /// </summary>
    MicrophoneMuteEnum MicrophoneMuteNotification { get; set; }

    /// <summary>
    /// Select the action when double-clicking the tray icon
    /// </summary>
    IconDoubleClickEnum IconDoubleClick { get; set; }

    #endregion

    #region Events

    /// <summary>
    ///     When the selected list of device to switch from is changed (new device added or removed).
    /// </summary>
    event EventHandler<DeviceListChanged> SelectedDeviceChanged;

    /// <summary>
    ///     The Default Playback device has been changed.
    /// </summary>
    event EventHandler<DeviceDefaultChangedEvent> DefaultDeviceChanged;

    /// <summary>
    ///     If an exception happened in the <see cref="IAppModel" />
    /// </summary>
    event EventHandler<ExceptionEvent> ErrorTriggered;

    /// <summary>
    ///     The update checker found a newer release than the installed version.
    /// </summary>
    event EventHandler<NewReleaseAvailableEvent> NewVersionReleased;

    /// <summary>
    /// If the NotificationSettings has been modified
    /// </summary>
    event EventHandler<NotificationSettingsUpdatedEvent> NotificationSettingsChanged;

    /// <summary>
    /// If the BannerPosition has been modified
    /// </summary>
    event EventHandler<BannerDataChangedEvent> BannerSettingsChanged;

    /// <summary>
    /// When the custom sound is changed
    /// </summary>
    event EventHandler<CustomSoundChangedEvent> CustomSoundChanged;

    #endregion

    #region Methods

    /// <summary>
    ///     Initialize the Main class with Updater and Hotkeys
    /// </summary>
    /// <param name="active"></param>
    void InitializeMain(IAudioDeviceLister active);


    /// <summary>
    ///     Add a playback device into the Set.
    /// </summary>
    /// <param name="device"></param>
    /// <returns>
    ///     true if the element is added to the <see cref="T:System.Collections.Generic.HashSet`1" /> object; false if
    ///     the element is already present.
    /// </returns>
    bool SelectDevice(DeviceFullInfo device);

    /// <summary>
    ///     Remove a device from the Set.
    /// </summary>
    /// <param name="device"></param>
    /// <returns>
    ///     true if the element is successfully found and removed; otherwise, false.  This method returns false if
    ///     <paramref name="device" /> is not found in the <see cref="T:System.Collections.Generic.HashSet`1" /> object.
    /// </returns>
    bool UnselectDevice(DeviceFullInfo device);

    /// <summary>
    ///     Sets the hotkey combination
    /// </summary>
    /// <param name="hotKey"></param>
    /// <param name="action"></param>
    /// <param name="force"></param>
    /// <returns>if it's successfull</returns>
    public bool SetHotkeyCombination(HotKey hotKey, HotKeyAction action, bool force = false);

    /// <summary>
    ///     Attempts to set active device to the specified name
    /// </summary>
    /// <param name="device"></param>
    bool SetActiveDevice(DeviceInfo device);

    /// <summary>
    ///     Cycles the active device to the next device. Returns true if succesfully switched (at least
    ///     as far as we can tell), returns false if could not successfully switch. Throws NoDevicesException
    ///     if there are no devices configured.
    /// </summary>
    bool CycleActiveDevice(DataFlow type);

    /// <summary>
    /// Toggles the mute state of the default microphone
    /// </summary>
    /// <returns>Tuple with device name and mute state, null if no default microphone found or operation failed</returns>
    (string DeviceName, bool IsMuted)? ToggleMicrophoneMute();

    /// <summary>
    /// Sets the mute state of the default microphone
    /// </summary>
    /// <param name="muteState">The desired mute state</param>
    /// <returns>Tuple with device name and mute state, null if no default microphone found or operation failed</returns>
    (string DeviceName, bool IsMuted)? SetMicrophoneMuteState(bool muteState);

    #endregion

    /// <summary>
    /// Triggered when the update mode has been changed
    /// </summary>
    event EventHandler<UpdateMode> UpdateModeChanged;

    /// <summary>
    /// For the app to check for update
    /// </summary>
    void CheckForUpdate();

    /// <summary>
    /// Toggles the mute state of the microphone
    /// </summary>
    /// <returns>Tuple with device name and mute state, null if no default microphone found or operation failed</returns>
    (string DeviceName, bool IsMuted)? SetMicrophoneMuteState(string deviceId, bool muteState);
}