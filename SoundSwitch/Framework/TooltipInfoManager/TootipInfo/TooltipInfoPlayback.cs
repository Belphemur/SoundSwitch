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
                    .First(device => device.IsDefault(Role.Console));
            return string.Format(TooltipInfo.currentlyActive, playbackDefaultDevice);
        }

        /// <summary>
        ///     Type of the Tooltip info
        /// </summary>
        /// <returns></returns>
        public ToolTipInfoTypeEnum Type()
        {
            return ToolTipInfoTypeEnum.Playback;
        }

        public override string ToString()
        {
            return TooltipInfo.playback;
        }
    }
}