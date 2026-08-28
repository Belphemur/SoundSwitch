#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SoundSwitch.Audio.Manager.Interop.Com.Threading;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface;

namespace SoundSwitch.Audio.Manager.Interop.Client
{
    /// <summary>
    /// Owns the native <see cref="IMMDeviceEnumerator"/> coclass (activated directly via
    /// <see cref="ComGuid.AUDIO_IMMDEVICE_ENUMERATOR_OBJECT_IID"/>) — the in-house replacement for
    /// the legacy third-party MMDeviceEnumerator. ComThread-only: constructed and used on the ComThread.
    /// The swallow-and-return-null boundary is deliberate (issue #401 semantics).
    /// </summary>
    internal sealed class AudioDeviceEnumerator : IDisposable
    {
        [ComImport, Guid(ComGuid.AUDIO_IMMDEVICE_ENUMERATOR_OBJECT_IID)]
        private class MMDeviceEnumeratorComObject
        {
        }

        private readonly IMMDeviceEnumerator _enumerator;

        // Keeps registered notification clients rooted for as long as they are registered.
        private readonly HashSet<AudioDeviceNotificationClient> _notificationClients = new();

        public AudioDeviceEnumerator()
        {
            ComThread.Assert();
            _enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
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

            using var defaultDevice = GetDefaultEndpoint(flow, role);
            return deviceId == defaultDevice?.Id;
        }

        /// <summary>
        /// What is the default endpoint
        /// </summary>
        /// <param name="flow"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public AudioDevice? GetDefaultEndpoint(EDataFlow flow, ERole role)
        {
            ComThread.Assert();
            try
            {
                var hr = _enumerator.GetDefaultAudioEndpoint(flow, role, out var device);
                if (hr != HRESULT.S_OK || device == null)
                {
                    if (device != null) Marshal.ReleaseComObject(device);
                    return null;
                }

                return new AudioDevice(device);
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
        public AudioDevice? GetDevice(string deviceId)
        {
            ComThread.Assert();
            try
            {
                var hr = _enumerator.GetDevice(deviceId, out var device);
                if (hr != HRESULT.S_OK || device == null)
                {
                    if (device != null) Marshal.ReleaseComObject(device);
                    return null;
                }

                return new AudioDevice(device);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///  Get all the endpoints of specific dataflow and state
        /// </summary>
        /// <param name="dataFlow"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IReadOnlyList<AudioDevice> GetEndpoints(EDataFlow dataFlow, EDeviceState state)
        {
            ComThread.Assert();
            var hr = _enumerator.EnumAudioEndpoints(dataFlow, state, out var collection);
            if (hr != HRESULT.S_OK || collection == null)
                throw AudioDeviceException.FromHResult(hr, "IMMDeviceEnumerator.EnumAudioEndpoints");

            try
            {
                hr = collection.GetCount(out var count);
                if (hr != HRESULT.S_OK)
                    throw AudioDeviceException.FromHResult(hr, "IMMDeviceCollection.GetCount");

                var devices = new List<AudioDevice>((int)count);
                for (uint i = 0; i < count; i++)
                {
                    hr = collection.Item(i, out var device);
                    if (hr != HRESULT.S_OK || device == null) continue;
                    try
                    {
                        devices.Add(new AudioDevice(device));
                    }
                    catch
                    {
                        // Never leak the native device if the wrapper cannot be built.
                        Marshal.ReleaseComObject(device);
                    }
                }

                return devices;
            }
            finally
            {
                Marshal.ReleaseComObject(collection);
            }
        }

        /// <summary>
        /// Register a notification client. The client stays rooted until unregistration.
        /// </summary>
        public void RegisterNotificationClient(AudioDeviceNotificationClient client)
        {
            ComThread.Assert();
            var hr = _enumerator.RegisterEndpointNotificationCallback(client);
            if (hr != HRESULT.S_OK)
                throw AudioDeviceException.FromHResult(hr, "IMMDeviceEnumerator.RegisterEndpointNotificationCallback");

            _notificationClients.Add(client);
        }

        /// <summary>
        /// Unregister a previously registered notification client.
        /// </summary>
        public void UnregisterNotificationClient(AudioDeviceNotificationClient client)
        {
            ComThread.Assert();
            if (!_notificationClients.Remove(client))
                return;

            var hr = _enumerator.UnregisterEndpointNotificationCallback(client);
            if (hr != HRESULT.S_OK)
                throw AudioDeviceException.FromHResult(hr, "IMMDeviceEnumerator.UnregisterEndpointNotificationCallback");
        }

        public void Dispose()
        {
            ComThread.Assert();
            foreach (var client in _notificationClients)
            {
                try
                {
                    _enumerator.UnregisterEndpointNotificationCallback(client);
                }
                catch
                {
                    // Tearing down anyway — the enumerator is being released.
                }
            }

            _notificationClients.Clear();
            Marshal.ReleaseComObject(_enumerator);
        }
    }
}
