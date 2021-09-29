using NAudio.CoreAudioApi;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Framework.TrayIcon.Icon.Changer
{
    public abstract class AbstractIconChanger : IIconChanger
    {
        public abstract IconChangerFactory.ActionEnum TypeEnum { get; }
        public abstract string Label { get; }

        protected abstract DataFlow Flow { get; }

        protected virtual bool NeedsToChangeIcon(DeviceInfo deviceInfo)
        {
            return deviceInfo.Type == Flow;
        }

        public void ChangeIcon(UI.Component.TrayIcon trayIcon)
        {
            var audio = AudioSwitcher.Instance.GetDefaultAudioEndpoint((EDataFlow)Flow, ERole.eConsole);
            ChangeIcon(trayIcon, audio);
        }

        public void ChangeIcon(UI.Component.TrayIcon trayIcon, DeviceFullInfo deviceInfo)
        {
            if (deviceInfo == null)
            {
                return;
            }

            if (!NeedsToChangeIcon(deviceInfo))
            {
                return;
            }


            trayIcon.ReplaceIcon(deviceInfo.SmallIcon);
        }
    }
}