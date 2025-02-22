using System.Linq;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Common.Framework.Pipe;
using SoundSwitch.Framework.Audio.Lister;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Updater;
using SoundSwitch.UI.Menu;

namespace SoundSwitch.Model
{
    public class SoundSwitchApplicationContext : ApplicationContext
    {
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

            NamedPipe.OnMessageReceived += HandlePipeMessage;

            if (AppConfigs.Configuration.FirstRun)
            {
                AppModel.Instance.TrayIcon.ShowSettings();
                AppConfigs.Configuration.FirstRun = false;
                Log.Information("First run");
            }
        }

        void HandlePipeMessage(PipeMessage message)
        {
            switch (message.Type)
            {
                case PipeMessage.MessageType.OpenSettings:
                    AppModel.Instance.TrayIcon.ShowSettings();
                    Log.Information("Asked by other instance to show settings");
                    break;
                case PipeMessage.MessageType.TriggerProfile when message.Payload is TriggerProfilePayload profilePayload:
                    Log.Information("Triggering profile: {ProfileName}", profilePayload.ProfileName);
                    var profile = AppModel.Instance.ProfileManager.Profiles.FirstOrDefault(p => p.Name == profilePayload.ProfileName);
                    if (profile != null)
                    {
                        AppModel.Instance.ProfileManager.SwitchAudio(profile);
                    }
                    else
                    {
                        Log.Warning("Profile not found: {ProfileName}", profilePayload.ProfileName);
                    }
                    break;
                case PipeMessage.MessageType.TriggerSwitch when message.Payload is TriggerSwitchPayload switchPayload:
                    Log.Information("Triggering switch: {AudioType}", switchPayload.Type);
                    // Call appropriate switch method based on type
                    if (switchPayload.Type == AudioType.Recording)
                    {
                        AppModel.Instance.CycleActiveDevice(DataFlow.Capture);
                    }
                    else
                    {
                        AppModel.Instance.CycleActiveDevice(DataFlow.Render);
                    }
                    break;
            }
        }
    }
}