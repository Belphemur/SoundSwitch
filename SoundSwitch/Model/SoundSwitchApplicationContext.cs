using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio.Lister;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Updater;
using SoundSwitch.IPC.Pipe;
using SoundSwitch.UI.Menu;

namespace SoundSwitch.Model
{
    public class SoundSwitchApplicationContext : ApplicationContext
    {
        private readonly Guid _messageHandlerId;

        public SoundSwitchApplicationContext()
        {
            BannerManager.Setup();
            QuickMenuManager<DeviceFullInfo>.Instance.Setup();
            QuickMenuManager<Profile>.Instance.Setup();

            var deviceActiveLister = new CachedAudioDeviceLister(DeviceState.Active | DeviceState.Unplugged);
            deviceActiveLister.Refresh();
            MMNotificationClient.Instance.Register();

            AppModel.Instance.InitializeMain(deviceActiveLister);

            AppModel.Instance.NewVersionReleased += (sender, @event) =>
            {
                if (@event.UpdateMode == UpdateMode.Silent)
                {
                    new AutoUpdater("/VERYSILENT").Update(
                        @event.AppRelease, true);
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
                case OpenSettingsRequest:
                    Log.Information("Asked by other instance to show settings");
                    AppModel.Instance.TrayIcon.ShowSettings();
                    return new OpenSettingsResponse { Success = true };

                case TriggerProfileRequest profileRequest:
                    Log.Information("Triggering profile: {ProfileName}", profileRequest.ProfileName);
                    var profile = AppModel.Instance.ProfileManager.Profiles.FirstOrDefault(p => p.Name == profileRequest.ProfileName);
                    if (profile != null)
                    {
                        AppModel.Instance.ProfileManager.SwitchAudio(profile);
                        return new TriggerProfileResponse { Success = true };
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

                default:
                    Log.Warning("Unknown message type received: {MessageType}", message.GetType());
                    throw new ArgumentException($"Unknown message type: {message.GetType()}");
            }
        }
    }
}