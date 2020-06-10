using NAudio.CoreAudioApi;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Localization;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.TrayIcon.Icon.Changer
{
    public class AlwaysIconChanger : AbstractIconChanger
    {
        public override IconChangerFactory.ActionEnum TypeEnum => IconChangerFactory.ActionEnum.Always;
        public override string Label => TrayIconStrings.iconChanger_both;
        internal const int E_NOT_SET = unchecked((int)0x80070490);
        public override bool NeedsToChangeIcon(DeviceInfo deviceInfo)
        {
            return true;
        }

        public override void ChangeIcon(UI.Component.TrayIcon trayIcon)
        {
            using var enumerator = new MMDeviceEnumerator();
            try
            {
                using var defaultAudio = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                trayIcon.ReplaceIcon(new DeviceFullInfo(defaultAudio).SmallIcon);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                // Only handle "Element Not Found"
                if (e.ErrorCode == E_NOT_SET)
                {
                    // Set to app icon
                    trayIcon.ReplaceIcon(Resources.Switch_SoundWave);
                }
                else
                {
                    // Throw other ErrorCodes
                    throw e;
                }
            }
        }
    }
}