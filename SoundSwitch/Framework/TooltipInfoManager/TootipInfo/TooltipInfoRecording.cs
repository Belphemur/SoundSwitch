using System.Linq;
using AudioEndPointControllerWrapper;
using SoundSwitch.Model;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.TooltipInfoManager.TootipInfo
{
    public class TooltipInfoRecording : ITooltipInfo
    {
        /// <summary>
        ///     The text to display for this tooltip
        /// </summary>
        /// <returns></returns>
        public string TextToDisplay()
        {
            var recordingDevice =
                AppModel.Instance.ActiveAudioDeviceLister.GetRecordingDevices()
                    .First(device => device.IsDefault(Role.Console));
            return string.Format(TooltipInfo.currentlyActive, recordingDevice);
        }

        /// <summary>
        ///     Type of the Tooltip info
        /// </summary>
        /// <returns></returns>
        public ToolTipInfoTypeEnum Type()
        {
            return ToolTipInfoTypeEnum.Recording;
        }

        public override string ToString()
        {
            return TooltipInfo.recording;
        }
    }
}