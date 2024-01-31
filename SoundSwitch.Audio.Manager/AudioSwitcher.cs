#nullable enable
using System;
using System.Diagnostics;
using System.Linq;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Audio.Manager.Interop.Client;
using SoundSwitch.Audio.Manager.Interop.Com.Threading;
using SoundSwitch.Audio.Manager.Interop.Com.User;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Audio.Manager
{
    public class AudioSwitcher
    {
        private static AudioSwitcher? _instance;
        private PolicyClient? _policyClient;
        private EnumeratorClient? _enumerator;

        private ExtendedPolicyClient? _extendedPolicyClient;

        private EnumeratorClient EnumeratorClient
        {
            get
            {
                if (_enumerator != null)
                    return _enumerator;

                return _enumerator = ComThread.Invoke(() => new EnumeratorClient());
            }
        }

        private PolicyClient PolicyClient
        {
            get
            {
                if (_policyClient != null)
                    return _policyClient;

                return _policyClient = ComThread.Invoke(() => new PolicyClient());
            }
        }

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
                ComThread.Invoke(() =>
                {
                    if (EnumeratorClient.IsDefault(deviceId, EDataFlow.eRender, role) || EnumeratorClient.IsDefault(deviceId, EDataFlow.eCapture, role))
                    {
                        Trace.WriteLine($"Default endpoint already {deviceId}");
                        return;
                    }

                    PolicyClient.SetDefaultEndpoint(deviceId, role);
                });

                return;
            }

            SwitchTo(deviceId, ERole.eConsole);
            SwitchTo(deviceId, ERole.eMultimedia);
            SwitchTo(deviceId, ERole.eCommunications);
        }

        /// <summary>
        /// Switch the audio endpoint of the given process
        /// </summary>
        /// <param name="deviceId">Id of the device</param>
        /// <param name="role">Which role to switch</param>
        /// <param name="flow">Which flow to switch</param>
        /// <param name="processId">ProcessID of the process</param>
        public void SwitchProcessTo(string deviceId, ERole role, EDataFlow flow, uint processId)
        {
            var processName = "";
            try
            {
                var process = Process.GetProcessById((int)processId);
                processName = process.ProcessName;
            }
            catch (InvalidOperationException e)
            {
                Trace.TraceInformation($"Attempt to switch [{processId}] but got exception: {e}");
                return;
            }
            catch (ArgumentException e)
            {
                Trace.TraceInformation($"Attempt to switch [{processId}] but got exception: {e}");
                return;
            }
            catch (Exception e)
            {
                Trace.TraceError($"Can't get process info: {e}");
            }

            Trace.TraceInformation($"Attempt to switch [{processId}:{processName}] to {deviceId}");
            var roles = new[]
            {
                ERole.eConsole,
                ERole.eCommunications,
                ERole.eMultimedia
            };

            if (role != ERole.ERole_enum_count)
            {
                roles = new[]
                {
                    role
                };
            }

            ComThread.Invoke((() =>
            {
                var currentEndpoint = roles.Select(eRole => ExtendPolicyClient.GetDefaultEndPoint(flow, eRole, processId)).FirstOrDefault(endpoint => !string.IsNullOrEmpty(endpoint));
                if (deviceId.Equals(currentEndpoint))
                {
                    Trace.WriteLine($"Default endpoint for [{processId}:{processName}] already {deviceId}");
                    return;
                }

                ExtendPolicyClient.SetDefaultEndPoint(deviceId, flow, roles, processId);
            }));
        }

        /// <summary>
        /// Switch the audio device of the Foreground Process
        /// </summary>
        /// <param name="deviceId">Id of the device</param>
        /// <param name="role">Which role to switch</param>
        /// <param name="flow">Which flow to switch</param>
        public void SwitchForegroundProcessTo(string deviceId, ERole role, EDataFlow flow)
        {
            var processId = ComThread.Invoke(() => User32.ForegroundProcessId);
            if (processId == Environment.ProcessId)
            {
                Log.Warning("Tried to switch audio device of the app");
                return;
            }

            SwitchProcessTo(deviceId, role, flow, processId);
        }

        /// <summary>
        /// Set the same master volume level from the default audio device to the next device
        /// </summary>
        /// <param name="deviceId">Id of the device</param>
        /// <param name="role">Which role to switch</param>
        /// <param name="flow">Which flow to switch</param>
        /// <summary>
        /// Get the current default endpoint
        /// </summary>
        /// <param name="flow"></param>
        /// <param name="role"></param>
        /// <returns>Null if no default device is defined</returns>
        private MMDevice? GetMmDefaultAudioEndpoint(EDataFlow flow, ERole role) => ComThread.Invoke(() => EnumeratorClient.GetDefaultEndpoint(flow, role));

        /// <summary>
        /// Set the same master volume level from the default audio device to the next device
        /// </summary>
        /// <param name="device"></param>
        public void SetVolumeFromDefaultDevice(DeviceInfo device)
        {
            var currentDefault = GetMmDefaultAudioEndpoint((EDataFlow)device.Type, ERole.eConsole);
            if (currentDefault == null)
                return;

            var audioInfo = InteractWithMmDevice(currentDefault, mmDevice =>
            {
                var defaultDeviceAudioEndpointVolume = mmDevice.AudioEndpointVolume;
                return defaultDeviceAudioEndpointVolume == null ? default : (Volume: defaultDeviceAudioEndpointVolume.MasterVolumeLevelScalar, IsMuted: defaultDeviceAudioEndpointVolume.Mute);
            });

            if (audioInfo == default)
                return;

            var nextDevice = GetDevice(device.Id);
            
            if(nextDevice == null)
                return;
            
            InteractWithMmDevice(nextDevice, mmDevice =>
            {
                if (mmDevice is not { State: DeviceState.Active })
                    return nextDevice;
                
                if (mmDevice.AudioEndpointVolume == null)
                    return nextDevice;

                if (mmDevice.AudioEndpointVolume.Channels.Count == 2)
                {
                    mmDevice.AudioEndpointVolume.Channels[0].VolumeLevelScalar = audioInfo.Volume;
                    mmDevice.AudioEndpointVolume.Channels[1].VolumeLevelScalar = audioInfo.Volume;
                }
                else
                { 
                    mmDevice.AudioEndpointVolume.MasterVolumeLevelScalar = audioInfo.Volume;
                }
                mmDevice.AudioEndpointVolume.Mute = audioInfo.IsMuted;
                return mmDevice;
            });
        }


        /// <summary>
        /// Is the given deviceId the default audio device in the system
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="flow"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool IsDefault(string deviceId, EDataFlow flow, ERole role)
        {
            return ComThread.Invoke(() => EnumeratorClient.IsDefault(deviceId, flow, role));
        }

        /// <summary>
        /// Get the device used by the given process
        /// </summary>
        /// <param name="flow"></param>
        /// <param name="role"></param>
        /// <param name="processId"></param>
        /// <returns></returns>
        public string? GetUsedDevice(EDataFlow flow, ERole role, uint processId)
        {
            return ComThread.Invoke(() => ExtendPolicyClient.GetDefaultEndPoint(flow, role, processId));
        }

        /// <summary>
        /// Get the current default endpoint
        /// </summary>
        /// <param name="flow"></param>
        /// <param name="role"></param>
        /// <returns>Null if no default device is defined</returns>
        public DeviceFullInfo? GetDefaultAudioEndpoint(EDataFlow flow, ERole role) => ComThread.Invoke(() =>
        {
            var defaultEndpoint = EnumeratorClient.GetDefaultEndpoint(flow, role);
            return defaultEndpoint == null ? null : new DeviceFullInfo(defaultEndpoint);
        });

        /// <summary>
        /// Used to interact directly with a <see cref="MMDevice"/>
        /// </summary>
        /// <param name="device"></param>
        /// <param name="interaction"></param>
        /// <typeparam name="T"></typeparam>
        public T InteractWithMmDevice<T>(MMDevice device, Func<MMDevice, T> interaction) => ComThread.Invoke(() => interaction(device));

        /// <summary>
        /// Get the current default endpoint
        /// </summary>
        /// <param name="flow"></param>
        /// <param name="role"></param>
        /// <returns>Null if no default device is defined</returns>
        public MMDevice? GetDefaultMmDevice(EDataFlow flow, ERole role) => ComThread.Invoke(() => EnumeratorClient.GetDefaultEndpoint(flow, role));

        /// <summary>
        /// Get a device with the given id, returns null if not present
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public MMDevice? GetDevice(string deviceId) => ComThread.Invoke(() => EnumeratorClient.GetDevice(deviceId));

        /// <summary>
        /// Get a device with the given id, returns null if not present
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public DeviceFullInfo? GetAudioEndpoint(string deviceId) => ComThread.Invoke(() =>
        {
            var device = EnumeratorClient.GetDevice(deviceId);
            return device == null ? null : new DeviceFullInfo(device);
        });

        /// <summary>
        /// Reset Windows configuration for the process that had their audio device changed
        /// </summary>
        public void ResetProcessDeviceConfiguration()
        {
            ComThread.Invoke(() => ExtendPolicyClient.ResetAllSetEndpoint());
        }
    }
}