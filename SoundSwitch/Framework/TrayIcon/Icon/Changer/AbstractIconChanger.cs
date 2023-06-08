using CoreAudio;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Framework.TrayIcon.Icon.Changer
{
    public abstract class AbstractIconChanger : IIconChanger
    {
        private readonly ILogger _log;

        protected AbstractIconChanger()
        {
            _log = Log.ForContext("IconChanger", TypeEnum);
        }

        public abstract IconChangerFactory.ActionEnum TypeEnum { get; }
        public abstract string Label { get; }

        protected abstract DataFlow Flow { get; }

        protected virtual bool NeedsToChangeIcon(DeviceInfo deviceInfo)
        {
            return deviceInfo.Type == Flow;
        }

        public void ChangeIcon(UI.Component.TrayIcon trayIcon)
        {
            using var audio = AudioSwitcher.Instance.GetDefaultAudioEndpoint((EDataFlow)Flow, ERole.eConsole);
            ChangeIcon(trayIcon, audio, ERole.eConsole);
        }

        public void ChangeIcon(UI.Component.TrayIcon trayIcon, DeviceFullInfo deviceInfo, ERole role)
        {
            var log = _log.ForContext("Device", deviceInfo);
            log.Information("Changing icon");
            if (deviceInfo == null)
            {
                return;
            }

            //Don't change icon for communication device
            if (role == ERole.eCommunications)
            {
                return;
            }

            if (!NeedsToChangeIcon(deviceInfo))
            {
                log.Information("No need to change icon");
                return;
            }


            trayIcon.ReplaceIcon(deviceInfo.SmallIcon);
            log.Information("Icon replaced");
        }
    }
}