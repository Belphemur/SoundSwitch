using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface;

namespace SoundSwitch.Audio.Manager.Interop.Client
{
    internal class EnumeratorClient
    {
        private readonly MMDeviceEnumerator _enumerator;

        public EnumeratorClient()
        {
            _enumerator = new MMDeviceEnumerator();
        }

        ~EnumeratorClient()
        {
            _enumerator.Dispose();

        }

        public bool IsDefault(string deviceId, EDataFlow flow, ERole role)
        {
            if (role == ERole.ERole_enum_count)
            {
                var result = true;
                result &= IsDefault(deviceId, flow, ERole.eCommunications);
                result &= IsDefault(deviceId, flow, ERole.eConsole);
                result &= IsDefault(deviceId, flow, ERole.eMultimedia);

                return result;
            }

            var defaultDevice = _enumerator.GetDefaultAudioEndpoint((DataFlow)flow, (Role)role);
            return deviceId == defaultDevice.ID;
        } 

        [ComImport, Guid(ComGuid.AUDIO_IMMDEVICE_ENUMERATOR_OBJECT_IID)]
        private class _MMDeviceEnumerator
        {
            
        }
    }
}