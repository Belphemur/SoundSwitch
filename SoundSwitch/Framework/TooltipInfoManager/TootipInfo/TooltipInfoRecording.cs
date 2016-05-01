using System.Linq;
using AudioEndPointControllerWrapper;
using SoundSwitch.Model;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.TooltipInfoManager.TootipInfo
{
    public class TooltipInfoRecording : ITooltipInfo
    {
        public TooltipInfoTypeEnum TypeEnum { get; } = TooltipInfoTypeEnum.Recording;
        public string Label { get; } = TooltipInfo.recording;

        /// <summary>
        ///     The text to display for this tooltip
        /// </summary>
        /// <returns></returns>
        public string TextToDisplay()
        {
            var recordingDevice =
                AppModel.Instance.ActiveAudioDeviceLister.GetRecordingDevices()
                    .FirstOrDefault(device => device.IsDefault(Role.Console));
            return recordingDevice == null ? null : string.Format(TooltipInfo.recordingActive, recordingDevice);
        }

        public override string ToString()
        {
            return Label;
        }
    }
}