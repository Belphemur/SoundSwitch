using System;
using System.Collections.Generic;
using AudioEndPointControllerWrapper;
using SoundSwitch.Model;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.DeviceCyclerManager.DeviceCycler
{
    public class DeviceCyclerAvailable : ADeviceCycler
    {
        public override DeviceCyclerTypeEnum TypeEnum { get; } = DeviceCyclerTypeEnum.All;
        public override string Label { get; } = AudioCycler.all;

        /// <summary>
        ///     Cycle the audio device for the given type
        /// </summary>
        /// <param name="type"></param>
        public override bool CycleAudioDevice(AudioDeviceType type)
        {
            ICollection<IAudioDevice> audioDevices;
            switch (type)
            {
                case AudioDeviceType.Playback:
                    audioDevices = AppModel.Instance.AvailablePlaybackDevices;
                    break;
                case AudioDeviceType.Recording:
                    audioDevices = AppModel.Instance.AvailableRecordingDevices;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            switch (audioDevices.Count)
            {
                case 0:
                    throw new AppModel.NoDevicesException();
                case 1:
                    return false;
            }

            return SetActiveDevice(GetNextDevice(audioDevices, type));
        }
    }
}