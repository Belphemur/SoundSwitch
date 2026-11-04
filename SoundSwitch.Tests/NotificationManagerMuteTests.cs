using System;
using System.Collections.Generic;
using System.Drawing;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Banner.BannerDisplayInfo;
using SoundSwitch.Framework.Banner.BannerPosition;
using SoundSwitch.Framework.Banner.BannerPosition.Position;
using SoundSwitch.Framework.Banner.MicrophoneMute;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Model;
using SoundSwitch.UI.Component;

namespace SoundSwitch.Tests;

/// <summary>
/// Regression tests for SOUNDSWITCH-41T (issue #2555): microphone mute change notifications
/// used to crash with NullReferenceException when they arrived before the notification
/// manager (and the mute banner manager) had been initialized.
/// </summary>
[TestFixture]
public class NotificationManagerMuteTests
{
    private sealed class FakeNotificationSettings : INotificationSettings
    {
        public NotificationType SwitchDeviceNotification { get; set; }
        public NotificationType SwitchProfileNotification { get; set; }
        public NotificationType MicrophoneMuteNotification { get; set; }
        public bool NotificationAdvancedMode { get; set; }
        public CachedSound CustomNotificationSound { get; set; }
        public BannerPosition BannerPosition { get; set; }
        public Point CustomBannerPosition { get; set; }
        public bool HasCustomBannerPosition { get; }
        public TimeSpan BannerOnScreenTime { get; set; }
        public int BannerOnScreenTimeSecs { get; set; }
        public int BannerOpacityPercentage { get; set; }
        public IPosition BannerPositionImpl => null;
        public BannerDisplayInfo BannerDisplayInfo { get; set; }
        public MicrophoneMute MicrophoneMuteBanner { get; set; }
        public MicrophoneMute MicrophoneUnmuteBanner { get; set; }
        public int MaxNumberNotification { get; set; }
        public bool IsSingleNotification { get; set; }
        public bool NotifyUsingPrimaryScreen { get; set; }
        public string BannerMonitorDeviceName { get; set; }
        public Color BannerBackgroundColor { get; set; }
        public int BannerScalePercentage { get; set; }

#pragma warning disable CS0067
        public event EventHandler<NotificationSettingsUpdatedEvent> NotificationSettingsChanged;
        public event EventHandler<BannerDataChangedEvent> BannerSettingsChanged;
        public event EventHandler<CustomSoundChangedEvent> CustomSoundChanged;
#pragma warning restore CS0067

        public void SetBannerMonitor(string deviceName, bool usePrimaryScreen) { }
    }

    private sealed class FakeInfrastructure : IAppInfrastructure
    {
        public TrayIcon TrayIcon => null;
        public IAudioDeviceLister AudioDeviceLister => null;
        public global::SoundSwitch.Framework.Profile.ProfileManager ProfileManager => null;
        public global::SoundSwitch.Services.AppSoundLockManager AppSoundLockManager => null;

#pragma warning disable CS0067
        public event EventHandler<ExceptionEvent> ErrorTriggered;
#pragma warning restore CS0067

        public void InitializeMain(IAudioDeviceLister active, bool skipUpdate = false) { }
    }

    private sealed class FakeDeviceService : IDeviceService
    {
        public DeviceCollection<DeviceInfo> SelectedDevices => null;
        public IEnumerable<DeviceFullInfo> AvailablePlaybackDevices { get; } = Array.Empty<DeviceFullInfo>();
        public IEnumerable<DeviceFullInfo> AvailableRecordingDevices { get; } = Array.Empty<DeviceFullInfo>();

#pragma warning disable CS0067
        public event EventHandler<DeviceListChanged> SelectedDeviceChanged;
        public event EventHandler<DeviceDefaultChangedEvent> DefaultDeviceChanged;
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

    [Test]
    public void NotifyMuteChanged_BeforeInit_DoesNotThrow()
    {
        using var manager = new NotificationManager(new FakeNotificationSettings(), new FakeInfrastructure(), new FakeDeviceService());

        Action notify = () => manager.NotifyMuteChanged("{0.0.1.00000000}.{59e4df6f-d9a6-4ce0-9899-49dede000000}", "Test microphone", true);

        // Before Init() builds the notifications, mute events coming from the audio threads
        // must be skipped with a warning, not crash the process (SOUNDSWITCH-41T).
        notify.Should().NotThrow();
    }

    [Test]
    public void MicrophoneMuteNotificationIsBanner_BeforeInit_ReturnsFalse()
    {
        using var manager = new NotificationManager(new FakeNotificationSettings(), new FakeInfrastructure(), new FakeDeviceService());

        manager.MicrophoneMuteNotificationIsBanner.Should().BeFalse();
    }

    [Test]
    public void MicrophoneMuteBannerManager_BeforeSetup_DoesNotThrow()
    {
        var bannerManager = new MicrophoneMuteBannerManager();

        // Setup() captures the UI-thread synchronization context at application start;
        // mute events arriving on audio threads before that must be skipped, not crash.
        Action mute = () => bannerManager.UpdateMicrophoneMuteState("{0.0.1.00000000}.{59e4df6f-d9a6-4ce0-9899-49dede000000}", "Test microphone", true);
        Action unmute = () => bannerManager.UpdateMicrophoneMuteState("{0.0.1.00000000}.{59e4df6f-d9a6-4ce0-9899-49dede000000}", "Test microphone", false);
        Action remove = () => bannerManager.RemovePersistentMuteBanner("{0.0.1.00000000}.{59e4df6f-d9a6-4ce0-9899-49dede000000}");
        Action clear = () => bannerManager.ClearPersistentMuteBanners();

        mute.Should().NotThrow();
        unmute.Should().NotThrow();
        remove.Should().NotThrow();
        clear.Should().NotThrow();
    }
}
