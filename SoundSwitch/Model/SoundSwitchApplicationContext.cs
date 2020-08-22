using System.Windows.Forms;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Audio.Lister;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.Updater;
using SoundSwitch.UI.Component;

namespace SoundSwitch.Model
{
    public class SoundSwitchApplicationContext : ApplicationContext
    {
        public SoundSwitchApplicationContext()
        {
            BannerManager.Setup();
            var deviceActiveLister          = new CachedAudioDeviceLister(DeviceState.Active);
            var deviceUnpluggedActiveLister = new CachedAudioDeviceLister(DeviceState.Active | DeviceState.Unplugged);
            deviceActiveLister.Refresh();
            deviceUnpluggedActiveLister.Refresh();
            
            AppModel.Instance.ActiveAudioDeviceLister    = deviceActiveLister;
            AppModel.Instance.ActiveUnpluggedAudioLister = deviceUnpluggedActiveLister;

            AppModel.Instance.TrayIcon = new TrayIcon();
            AppModel.Instance.InitializeMain();
            AppModel.Instance.NewVersionReleased += (sender, @event) =>
            {
                if (@event.UpdateMode == UpdateMode.Silent)
                {
                    new AutoUpdater("/VERYSILENT /NOCANCEL /NORESTART", ApplicationPath.Default).Update(
                        @event.Release, true);
                }
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