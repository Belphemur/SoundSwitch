using System.Runtime.InteropServices;
using SoundSwitch.Audio.Manager.Interop;
using SoundSwitch.Audio.Manager.Interop.Client;
using SoundSwitch.Audio.Manager.Interop.Com.Threading;
using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager
{
    public class AudioSwitcher
    {
        private static AudioSwitcher _instance;
        private readonly PolicyClient _policyClient = new PolicyClient();
        private readonly EnumeratorClient _enumerator = new EnumeratorClient();

        private ExtendedPolicyClient _extendedPolicyClient;

        private ExtendedPolicyClient ExtendPolicyClient
        {
            get
            {
                if (_extendedPolicyClient != null)
                {
                    return _extendedPolicyClient;
                }

                return _extendedPolicyClient = ComThread.Invoke(() => new ExtendedPolicyClient());
            }
        }

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

                return _instance = ComThread.Invoke(() => new AudioSwitcher());
            }
        }

        /// <summary>
        /// Switch the default audio device to the one given
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="role"></param>
        public void SwitchTo(string deviceId, ERole role)
        {
            if (role != ERole.ERole_enum_count)
            {
                ComThread.Invoke((() => _policyClient.SetDefaultEndpoint(deviceId, role)));

                return;
            }

            SwitchTo(deviceId, ERole.eConsole);
            SwitchTo(deviceId, ERole.eMultimedia);
            SwitchTo(deviceId, ERole.eCommunications);
        }

        public void SwitchTo(string deviceId, ERole role, EDataFlow flow, int processId)
        {

            var roles = new ERole[]
            {
                ERole.eConsole,
                ERole.eCommunications,
                ERole.eMultimedia
            };

            if (role != ERole.ERole_enum_count)
            {
                roles = new ERole[]
                {
                    role
                };
            }


            ComThread.Invoke((() => ExtendPolicyClient.SetDefaultEndPoint(deviceId, flow, roles, processId)));
        }

        public bool IsDefault(string deviceId, EDataFlow flow, ERole role)
        {
            return ComThread.Invoke(() => _enumerator.IsDefault(deviceId, flow, role));
        }
    }
}