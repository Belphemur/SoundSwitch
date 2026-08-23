#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using NAudio.CoreAudioApi;

using Serilog;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio.Lister;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.Banner.MicrophoneMute;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Telemetry;
using SoundSwitch.Framework.Updater;
using SoundSwitch.IPC.Pipe;
using SoundSwitch.IPC.Pipe.Messages;
using SoundSwitch.IPC.Pipe.Messages.GetActiveDevices;
using SoundSwitch.IPC.Pipe.Messages.GetProfileList;
using SoundSwitch.IPC.Pipe.Messages.GetSwitchableDevices;
using SoundSwitch.IPC.Pipe.Messages.Microphone;
using SoundSwitch.IPC.Pipe.Messages.Models;
using SoundSwitch.IPC.Pipe.Messages.Mute;
using SoundSwitch.IPC.Pipe.Messages.OpenSettings;
using SoundSwitch.IPC.Pipe.Messages.TriggerProfile;
using SoundSwitch.IPC.Pipe.Messages.TriggerSwitch;
using SoundSwitch.UI.Menu;

namespace SoundSwitch.Model;

public class SoundSwitchApplicationContext : ApplicationContext
{
    private readonly Guid _messageHandlerId;

    public SoundSwitchApplicationContext()
    {
        BannerManager.Setup();
        MicrophoneMuteBannerManager.Setup();
        QuickMenuManager<DeviceFullInfo>.Instance.Setup();
        QuickMenuManager<Profile>.Instance.Setup();

        var deviceActiveLister = new CachedAudioDeviceLister(DeviceState.Active | DeviceState.Unplugged);
        deviceActiveLister.Refresh();
        MMNotificationClient.Instance.Register();

        AppModel.Instance.InitializeMain(deviceActiveLister, Program.SkipUpdate);

        TelemetryService.TrackUpdateMode(SoundSwitch.Framework.Configuration.AppConfigs.Configuration.UpdateMode);

        var cfg = AppConfigs.Configuration;
        TelemetryService.TrackSetting("include_beta_versions", cfg.IncludeBetaVersions);
        TelemetryService.TrackSetting("language", cfg.Language.ToString());
        TelemetryService.TrackSetting("quick_menu_enabled", cfg.QuickMenuEnabled);
        TelemetryService.TrackSetting("keep_volume_enabled", cfg.KeepVolumeEnabled);
        TelemetryService.TrackSetting("switch_foreground_program", cfg.SwitchForegroundProgram);
        TelemetryService.TrackSetting("notification_advanced_mode", cfg.NotificationAdvancedMode);
        TelemetryService.TrackSetting("auto_add_new_connected_devices", cfg.AutoAddNewConnectedDevices);
        TelemetryService.TrackSetting("switch_icon", cfg.SwitchIcon.ToString());
        TelemetryService.TrackProfileCount(AppModel.Instance.ProfileManager.Profiles.Count());
        TelemetryService.TrackAppRuleCount(cfg.AppSoundRules.Count);

        AppModel.Instance.NewVersionReleased += (sender, @event) =>
        {
            if (@event.UpdateMode == UpdateMode.Silent)
            {
                TelemetryService.TrackUpdateAvailable(UpdateMode.Silent);
                new AutoUpdater("/VERYSILENT").Update(
                    @event.AppRelease, true,
                    onCompleted: ok => TelemetryService.TrackUpdateInstalled(UpdateMode.Silent, ok ? "success" : "failed"));
            }
        };

        _messageHandlerId = NamedPipe.RegisterMessageHandler(HandlePipeMessageAsync);

        if (AppConfigs.Configuration.FirstRun)
        {
            AppModel.Instance.TrayIcon.ShowSettings();
            AppConfigs.Configuration.FirstRun = false;
            Log.Information("First run");
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            NamedPipe.UnregisterMessageHandler(_messageHandlerId);
        }

        base.Dispose(disposing);
    }

    async Task<IPipeMessage> HandlePipeMessageAsync(IPipeMessage message, System.Threading.CancellationToken token)
    {
        switch (message)
        {
            case MicrophoneStateRequest:
                try
                {
                    var stateResponse = await Task.Run(() =>
                    {
                        var defaultDevice = AudioSwitcher.Instance.GetDefaultMmDevice(
                            Audio.Manager.Interop.Enum.EDataFlow.eCapture,
                            Audio.Manager.Interop.Enum.ERole.eCommunications
                        );
                        if (defaultDevice == null)
                        {
                            return null;
                        }

                        return AudioSwitcher.Instance.InteractWithDevice(defaultDevice, device => new MicrophoneStateResponse
                        {
                            Success = true,
                            IsMuted = device.AudioEndpointVolume.Mute,
                            DeviceName = device.FriendlyName
                        });
                    }, token);

                    if (stateResponse == null)
                    {
                        Log.Warning("No default capture device found");
                        return new MicrophoneStateResponse { Success = false, IsMuted = false, DeviceName = "" };
                    }

                    return stateResponse;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to get microphone state");
                    return new MicrophoneStateResponse { Success = false, IsMuted = false, DeviceName = "" };
                }

            case MuteRequest muteRequest:
                try
                {
                    Log.Information("Setting microphone mute state to: {Mute}", muteRequest.Mute);
                    var result = AppModel.Instance.SetMicrophoneMuteState(muteRequest.Mute);

                    if (result == null)
                    {
                        Log.Warning("No default capture device found");
                        return new MicrophoneStateResponse { Success = false, IsMuted = false, DeviceName = "" };
                    }

                    return new MicrophoneStateResponse
                    {
                        Success = true,
                        IsMuted = result.Value.IsMuted,
                        DeviceName = result.Value.DeviceName
                    };
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to set microphone state");
                    return new MicrophoneStateResponse { Success = false, IsMuted = false, DeviceName = "" };
                }

            case OpenSettingsRequest:
                Log.Information("Asked by other instance to show settings");
                try
                {
                    AppModel.Instance.TrayIcon.ShowSettings();
                    return new OpenSettingsResponse { Success = true };
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to show settings");
                    return new OpenSettingsResponse { Success = false };
                }

            case TriggerProfileRequest profileRequest:
                Log.Information("Triggering profile: {ProfileName}", profileRequest.ProfileName);
                var profile = AppModel.Instance.ProfileManager.Profiles.FirstOrDefault(p => p.Name == profileRequest.ProfileName);
                if (profile != null)
                {
                    try
                    {
                        AppModel.Instance.ProfileManager.SwitchAudio(profile);
                        return new TriggerProfileResponse { Success = true };
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to switch audio for profile {ProfileName}", profileRequest.ProfileName);
                        return new TriggerProfileResponse { Success = false, Error = ex.Message };
                    }
                }

                Log.Warning("Profile not found: {ProfileName}", profileRequest.ProfileName);
                return new TriggerProfileResponse
                {
                    Success = false,
                    Error = $"Profile {profileRequest.ProfileName} not found"
                };

            case TriggerSwitchRequest switchRequest:
                Log.Information("Triggering switch: {AudioType}", switchRequest.Type);
                try
                {
                    switch (switchRequest.Type)
                    {
                        case AudioType.Recording:
                            AppModel.Instance.CycleActiveDevice(DataFlow.Capture);
                            break;
                        case AudioType.Playback:
                        default:
                            AppModel.Instance.CycleActiveDevice(DataFlow.Render);
                            break;
                    }

                    return new TriggerSwitchResponse { Success = true };
                }
                catch
                {
                    return new TriggerSwitchResponse { Success = false };
                }

            case GetProfileListRequest:
                var profiles = AppModel.Instance.ProfileManager.Profiles
                    .Select(p => new ProfileInfo
                    {
                        Name = p.Name,
                        PlaybackDevice = p.Playback?.NameClean ?? "Not set",
                        RecordingDevice = p.Recording?.NameClean ?? "Not set",
                        PlaybackCommunicationDevice = p.Communication?.NameClean ?? "Not set",
                        RecordingCommunicationDevice = p.RecordingCommunication?.NameClean ?? "Not set"
                    })
                    .ToArray();
                return new GetProfileListResponse { Profiles = profiles };

            case GetActiveDevicesRequest:
                try
                {
                    var playback = await Task.Run(() =>
                    {
                        using var device = AudioSwitcher.Instance.GetDefaultAudioEndpoint(
                            Audio.Manager.Interop.Enum.EDataFlow.eRender,
                            Audio.Manager.Interop.Enum.ERole.eMultimedia);
                        return device?.NameClean ?? "";
                    }, token);
                    var playbackComm = await Task.Run(() =>
                    {
                        using var device = AudioSwitcher.Instance.GetDefaultAudioEndpoint(
                            Audio.Manager.Interop.Enum.EDataFlow.eRender,
                            Audio.Manager.Interop.Enum.ERole.eCommunications);
                        return device?.NameClean ?? "";
                    }, token);
                    var recording = await Task.Run(() =>
                    {
                        using var device = AudioSwitcher.Instance.GetDefaultAudioEndpoint(
                            Audio.Manager.Interop.Enum.EDataFlow.eCapture,
                            Audio.Manager.Interop.Enum.ERole.eMultimedia);
                        return device?.NameClean ?? "";
                    }, token);
                    var recordingComm = await Task.Run(() =>
                    {
                        using var device = AudioSwitcher.Instance.GetDefaultAudioEndpoint(
                            Audio.Manager.Interop.Enum.EDataFlow.eCapture,
                            Audio.Manager.Interop.Enum.ERole.eCommunications);
                        return device?.NameClean ?? "";
                    }, token);

                    return new GetActiveDevicesResponse
                    {
                        Success = true,
                        ActiveProfile = AppModel.Instance.ProfileManager.LastTriggeredProfile,
                        PlaybackDevice = playback,
                        RecordingDevice = recording,
                        PlaybackCommunicationDevice = playbackComm,
                        RecordingCommunicationDevice = recordingComm
                    };
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to get active devices");
                    return new GetActiveDevicesResponse
                    {
                        Success = false,
                        Error = ex.Message,
                        ActiveProfile = null,
                        PlaybackDevice = "",
                        RecordingDevice = "",
                        PlaybackCommunicationDevice = "",
                        RecordingCommunicationDevice = ""
                    };
                }

            case GetSwitchableDevicesRequest:
                try
                {
                    var playback = await Task.Run(() =>
                    {
                        var names = new List<string>();
                        foreach (var device in AppModel.Instance.AvailablePlaybackDevices)
                        {
                            names.Add(device.NameClean);
                        }
                        return names.ToArray();
                    }, token);
                    var recording = await Task.Run(() =>
                    {
                        var names = new List<string>();
                        foreach (var device in AppModel.Instance.AvailableRecordingDevices)
                        {
                            names.Add(device.NameClean);
                        }
                        return names.ToArray();
                    }, token);

                    return new GetSwitchableDevicesResponse
                    {
                        Success = true,
                        PlaybackDevices = playback,
                        RecordingDevices = recording
                    };
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to get switchable devices");
                    return new GetSwitchableDevicesResponse
                    {
                        Success = false,
                        Error = ex.Message
                    };
                }

            default:
                Log.Warning("Unknown message type received: {MessageType}", message.GetType());
                throw new ArgumentException($"Unknown message type: {message.GetType()}");
        }
    }
}
