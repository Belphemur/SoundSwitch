#nullable enable
using System;
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

            var defaultDevice = GetDefaultEndpoint(flow, role);
            return deviceId == defaultDevice?.ID;
        }

        /// <summary>
        /// What is the default endpoint
        /// </summary>
        /// <param name="flow"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public MMDevice? GetDefaultEndpoint(EDataFlow flow, ERole role)
        {
            try
            {
                var defaultDevice = _enumerator.GetDefaultAudioEndpoint((DataFlow)flow, (Role)role);
                return defaultDevice;
            }
            catch (Exception)
            {
                //Happens if there is no default device for the given Data Flow and/or role
                // See issue #401
                return null;
            }
        }

        /// <summary>
        /// Get device with the given id
        /// </summary>
        public MMDevice? GetDevice(string deviceId)
        {
            try
            {
                return _enumerator.GetDevice(deviceId);
            }
            catch
            {
                return null;
            }
        }

        [ComImport, Guid(ComGuid.AUDIO_IMMDEVICE_ENUMERATOR_OBJECT_IID)]
        private class _MMDeviceEnumerator
        {
        }
    }
}