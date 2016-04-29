using System;
using System.Collections.Generic;
using AudioEndPointControllerWrapper;
using SoundSwitch.Model;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.DeviceCyclerManager.DeviceCycler
{
    public class DeviceCyclerAll : ADeviceCycler
    {
        public override DeviceCyclerEnumType TypeEnum { get; } = DeviceCyclerEnumType.All;
        public override string Label { get; } = AudioCycler.all;

        /// <summary>
        ///     Cycle the audio device for the given type
        /// </summary>
        /// <param name="type"></param>
        public override void CycleAudioDevice(AudioDeviceType type)
        {
            ICollection<IAudioDevice> audioDevices;
            switch (type)
            {
                case AudioDeviceType.Playback:
                    audioDevices = AppModel.Instance.ActiveAudioDeviceLister.GetPlaybackDevices();
                    break;
                case AudioDeviceType.Recording:
                    audioDevices = AppModel.Instance.ActiveAudioDeviceLister.GetRecordingDevices();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            SetActiveDevice(GetNextDevice(audioDevices, type));
        }
    }
}