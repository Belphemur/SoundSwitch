using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Factory;

namespace SoundSwitch.Framework.DeviceCyclerManager.DeviceCycler
{
    public interface IDeviceCycler : IEnumImpl<DeviceCyclerTypeEnum>
    {
        /// <summary>
        /// Cycle the audio device for the given type
        /// </summary>
        /// <param name="type"></param>
        bool CycleAudioDevice(AudioDeviceType type);

        /// <summary>
        ///     Attempts to set active device to the specified name
        /// </summary>
        /// <param name="device"></param>
        bool SetActiveDevice(IAudioDevice device);
    }
}