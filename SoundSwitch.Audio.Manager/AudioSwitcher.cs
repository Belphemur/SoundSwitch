using System.Runtime.InteropServices;
using SoundSwitch.Audio.Manager.Interop;

namespace SoundSwitch.Audio.Manager
{
    public class AudioSwitcher
    {
        private static AudioSwitcher _instance;
        private readonly PolicyClient _policyClient = new PolicyClient();

        private AudioSwitcher()
        {
        }

        public static AudioSwitcher Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                return _instance = new AudioSwitcher();
            }
        }

        public bool SwitchTo(string deviceId, ERole role)
        {
            if (role != ERole.ERole_enum_count)
            {
                return InternalSwitchTo(deviceId, role);
            }

            var result = true;
            result &= InternalSwitchTo(deviceId, ERole.eConsole);
            result &= InternalSwitchTo(deviceId, ERole.eMultimedia);
            result &= InternalSwitchTo(deviceId, ERole.eCommunications);
            return result;
        }

        private bool InternalSwitchTo(string deviceId, ERole role)
        {
            _policyClient.SetDefaultEndpoint(deviceId, role);
            return true;
        }
    }
}