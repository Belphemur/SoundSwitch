using System.Collections.Generic;
using System.Linq;
using AudioEndPointControllerWrapper;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.DeviceCyclerManager.DeviceCycler
{
    public abstract class ADeviceCycler : IDeviceCycler
    {
        private readonly IDictionary<AudioDeviceType, IAudioDevice> _lastDevices =
            new Dictionary<AudioDeviceType, IAudioDevice>();

        public abstract DeviceCyclerEnumType TypeEnum { get; }
        public abstract string Label { get; }

        /// <summary>
        ///     Cycle the audio device for the given type
        /// </summary>
        /// <param name="type"></param>
        public abstract void CycleAudioDevice(AudioDeviceType type);

        /// <summary>
        ///     Get the next device that need to be set as Default
        /// </summary>
        /// <param name="audioDevices"></param>
        /// <param name="type"></param>
        /// <param name="lastDevice"></param>
        /// <returns></returns>
        protected IAudioDevice GetNextDevice(ICollection<IAudioDevice> audioDevices, AudioDeviceType type)
        {
            IAudioDevice lastDevice;
            if (!_lastDevices.TryGetValue(type, out lastDevice))
            {
                return audioDevices.First();
            }

            var defaultDev = audioDevices.FirstOrDefault(device => device.Id == lastDevice.Id) ??
                             audioDevices.FirstOrDefault(device => device.IsDefault(Role.Console)) ??
                             audioDevices.Last();
            var next = audioDevices.SkipWhile((device, i) => !Equals(device, defaultDev)).Skip(1).FirstOrDefault() ??
                       audioDevices.ElementAt(0);
            return next;
        }

        /// <summary>
        ///     Attempts to set active device to the specified name
        /// </summary>
        /// <param name="device"></param>
        protected bool SetActiveDevice(IAudioDevice device)
        {
            using (AppLogger.Log.InfoCall())
            {
                AppLogger.Log.Info("Set Default device", device);
                if (!AppModel.Instance.SetCommunications)
                {
                    device.SetAsDefault(Role.Console);
                    device.SetAsDefault(Role.Multimedia);
                }
                else
                {
                    AppLogger.Log.Info("Set Default Communication device", device);
                    device.SetAsDefault(Role.All);
                }
                _lastDevices[device.Type] = device;
                return true;
            }
        }
    }
}