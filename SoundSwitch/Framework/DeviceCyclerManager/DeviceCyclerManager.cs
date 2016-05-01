using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Configuration;

namespace SoundSwitch.Framework.DeviceCyclerManager
{
    public class DeviceCyclerManager
    {
        private readonly DeviceCyclerFactory _deviceCyclerFactory;

        public DeviceCyclerManager()
        {
            _deviceCyclerFactory = new DeviceCyclerFactory();
        }

        public static DeviceCyclerTypeEnum CurrentCycler
        {
            get { return AppConfigs.Configuration.CyclerType; }
            set
            {
                if(value == AppConfigs.Configuration.CyclerType)
                    return;
                AppConfigs.Configuration.CyclerType = value;
                AppConfigs.Configuration.Save();
            }
        }
        /// <summary>
        /// Cycle the audio device
        /// </summary>
        /// <param name="type"></param>
        public bool CycleDevice(AudioDeviceType type)
        {
            return _deviceCyclerFactory.Get(CurrentCycler).CycleAudioDevice(type);
        }
        /// <summary>
        /// Set the device as Default
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public bool SetAsDefault(IAudioDevice device)
        {
            return _deviceCyclerFactory.Get(CurrentCycler).SetActiveDevice(device);
        }
    }
}