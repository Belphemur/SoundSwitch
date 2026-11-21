using System;
using System.Collections.Generic;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Audio.Lister;
using SoundSwitch.Model;
using SoundSwitch.Services;
using SoundSwitch.UI.Component;

namespace SoundSwitch.Tests;

/// <summary>
///     Shared <see cref="IDeviceService" /> fake for tests that need a device service
///     collaborator without audio infrastructure (the ADR's fake seam: interfaces stay
///     the standard test double, now composed with the real service classes too).
/// </summary>
public class FakeDeviceService : IDeviceService
{
    public DeviceCollection<DeviceInfo> SelectedDevices => null;
    public IEnumerable<DeviceFullInfo> AvailablePlaybackDevices { get; } = Array.Empty<DeviceFullInfo>();
    public IEnumerable<DeviceFullInfo> AvailableRecordingDevices { get; } = Array.Empty<DeviceFullInfo>();

#pragma warning disable CS0067
    public event EventHandler<DeviceListChanged> SelectedDeviceChanged;
    public event EventHandler<DeviceDefaultChangedEvent> DefaultDeviceChanged;
    public event EventHandler<MicrophoneMuteChangedEventArgs> DefaultMicMuteChanged;
    public event EventHandler MuteTargetChanged;
#pragma warning restore CS0067

    public bool SelectDevice(DeviceFullInfo device) => false;
    public bool UnselectDevice(DeviceFullInfo device) => false;
    public bool SetActiveDevice(DeviceInfo device) => false;
    public bool CycleActiveDevice(EDataFlow type) => false;
    public (string DeviceName, bool IsMuted)? ToggleMicrophoneMute() => null;
    public (string DeviceName, bool IsMuted)? GetMicrophoneMuteState() => null;
    public (string DeviceName, bool IsMuted)? SetMicrophoneMuteState(bool muteState) => null;
    public (string DeviceName, bool IsMuted)? SetMicrophoneMuteState(string deviceId, bool muteState) => null;
}

/// <summary>
///     Shared <see cref="IAppInfrastructure" /> fake for tests that compose the real
///     services with fakes over the interfaces (the ADR's tested seam).
/// </summary>
public class FakeInfrastructure : IAppInfrastructure
{
    public TrayIcon TrayIcon => null;
    public IAudioDeviceLister AudioDeviceLister => null;
    public ProfileManager ProfileManager => null;
    public AppSoundLockManager AppSoundLockManager => null;

#pragma warning disable CS0067
    public event EventHandler<ExceptionEvent> ErrorTriggered;
#pragma warning restore CS0067
}

