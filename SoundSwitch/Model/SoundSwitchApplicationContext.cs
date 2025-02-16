﻿using System.Windows.Forms;
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

            NamedPipe.OnMessageReceived += @enum =>
            {
                if (@enum != NamedPipe.MessageEnum.OpenSettings)
                    return;
                AppModel.Instance.TrayIcon.ShowSettings();
                Log.Information("Asked by other instance to show settings");
            };


            if (AppConfigs.Configuration.FirstRun)
            {
                AppModel.Instance.TrayIcon.ShowSettings();
                AppConfigs.Configuration.FirstRun = false;
                Log.Information("First run");
            }
        }
    }
}