using System.Linq;
using AudioEndPointControllerWrapper;
using SoundSwitch.Model;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.TooltipInfoManager.TootipInfo
{
    public class TooltipInfoPlayback : ITooltipInfo
    {
        /// <summary>
        ///     The text to display for this tooltip
        /// </summary>
        /// <returns></returns>
        public string TextToDisplay()
        {
            var playbackDefaultDevice =
                AppModel.Instance.ActiveAudioDeviceLister.GetPlaybackDevices()
                    .FirstOrDefault(device => device.IsDefault(Role.Console));
            return playbackDefaultDevice == null ? null : string.Format(TooltipInfo.currentlyActive, playbackDefaultDevice);
        }

        /// <summary>
        ///     Type of the Tooltip info
        /// </summary>
        /// <returns></returns>
        public TooltipInfoTypeEnum Type()
        {
            return TooltipInfoTypeEnum.Playback;
        }

        public override string ToString()
        {
            return TooltipInfo.playback;
        }
    }
}