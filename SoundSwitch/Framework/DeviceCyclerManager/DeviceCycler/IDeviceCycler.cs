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
        void CycleAudioDevice(AudioDeviceType type);
    }
}