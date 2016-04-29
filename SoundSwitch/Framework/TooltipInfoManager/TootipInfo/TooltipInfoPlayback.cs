using System.Linq;
using AudioEndPointControllerWrapper;
using SoundSwitch.Model;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.TooltipInfoManager.TootipInfo
{
    public class TooltipInfoPlayback : ITooltipInfo
    {
        public TooltipInfoTypeEnum TypeEnum { get; } = TooltipInfoTypeEnum.Playback;
        public string Label { get; } = TooltipInfo.playback;

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

        public override string ToString()
        {
            return Label;
        }
    }
}