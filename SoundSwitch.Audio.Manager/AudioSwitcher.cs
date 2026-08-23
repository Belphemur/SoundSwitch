#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Serilog;

using SoundSwitch.Audio.Manager.Interop.Client;
using SoundSwitch.Audio.Manager.Interop.Com.Threading;
using SoundSwitch.Audio.Manager.Interop.Com.User;
using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager
{
    public class AudioSwitcher
    {
        private static AudioSwitcher? _instance;
        private PolicyClient? _policyClient;
        private AudioDeviceEnumerator? _enumerator;

        private ExtendedPolicyClient? _extendedPolicyClient;

        private AudioDeviceEnumerator EnumeratorClient
        {
            get
            {
                if (_enumerator != null)
                    return _enumerator;

                return _enumerator = ComThread.Invoke(() => new AudioDeviceEnumerator());
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

        /// <summary>
        /// Test seam: inject a pre-built <see cref="ExtendedPolicyClient"/> so callers
        /// can be exercised without relying on the COM-backed default instance.
        /// Pass <c>null</c> to clear the injected client and fall back to the default.
        /// </summary>
        internal void SetExtendedPolicyClientForTest(ExtendedPolicyClient? client) => _extendedPolicyClient = client;

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
        public bool SwitchProcessTo(string deviceId, ERole role, EDataFlow flow, uint processId)
        {
            var processName = "";
            try
            {
                var process = Process.GetProcessById((int)processId);
                processName = process.ProcessName;
            }
            catch (Exception e)
            {
                Trace.TraceInformation($"Attempt to switch [{processId}] but got exception: {e}");
                return false;
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

            return ComThread.Invoke((() =>
            {
                var allRolesAlreadyMatch = roles.All(eRole =>
                {
                    var current = ExtendPolicyClient.GetDefaultEndPoint(flow, eRole, processId);
                    return !string.IsNullOrEmpty(current) && deviceId.Equals(current);
                });
                if (allRolesAlreadyMatch)
                {
                    Trace.WriteLine($"Default endpoint for [{processId}:{processName}] already {deviceId}");
                    return false;
                }

                return ExtendPolicyClient.SetDefaultEndPoint(deviceId, flow, roles, processId);
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
        /// Set the same master volume level from the default audio device to the given device
        /// </summary>
        /// <param name="flow">Flow of the device receiving the volume</param>
        /// <param name="deviceId">Id of the device receiving the volume</param>
        public void SetVolumeFromDefaultDevice(EDataFlow flow, string deviceId)
        {
            using var currentDefault = GetDefaultAudioDevice(flow, ERole.eConsole);
            if (currentDefault == null)
                return;

            var audioInfo = InteractWithDevice(currentDefault, device =>
            {
                var defaultDeviceAudioEndpointVolume = device.EndpointVolume;
                return defaultDeviceAudioEndpointVolume == null ? default : (Volume: defaultDeviceAudioEndpointVolume.MasterVolumeLevelScalar, IsMuted: defaultDeviceAudioEndpointVolume.Mute);
            });

            if (audioInfo == default)
                return;

            using var nextDevice = GetDevice(deviceId);

            if (nextDevice == null)
                return;

            InteractWithDevice(nextDevice, device =>
            {
                if (device.State != EDeviceState.Active)
                    return device;

                var endpointVolume = device.EndpointVolume;
                if (endpointVolume == null)
                    return device;

                if (endpointVolume.ChannelCount == 2)
                {
                    endpointVolume.SetChannelVolumeLevelScalar(0, audioInfo.Volume);
                    endpointVolume.SetChannelVolumeLevelScalar(1, audioInfo.Volume);
                }
                else
                {
                    endpointVolume.MasterVolumeLevelScalar = audioInfo.Volume;
                }

                endpointVolume.Mute = audioInfo.IsMuted;
                return device;
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
        /// Get the device ID for a process based on active audio sessions.
        /// </summary>
        public string? GetSessionDeviceId(EDataFlow flow, uint processId)
        {
            return ComThread.Invoke(() =>
            {
                var devices = EnumeratorClient.GetEndpoints(flow, EDeviceState.Active);
                try
                {
                    foreach (var device in devices)
                    {
                        try
                        {
                            foreach (var session in device.GetSessions())
                            {
                                if (session.ProcessId == processId && session.State == AudioSessionState.Active)
                                {
                                    return device.Id;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Debug(ex, "Failed to check sessions for device {DeviceId}", device.Id);
                        }
                    }

                    return null;
                }
                finally
                {
                    foreach (var device in devices)
                        device.Dispose();
                }
            });
        }

        /// <summary>
        /// Builds a map of ProcessId -> DeviceId for all active sessions of a given flow.
        /// </summary>
        public Dictionary<uint, string> GetProcessDeviceMap(EDataFlow flow)
        {
            return ComThread.Invoke(() =>
            {
                var map = new Dictionary<uint, string>();
                var devices = EnumeratorClient.GetEndpoints(flow, EDeviceState.Active);
                try
                {
                    foreach (var device in devices)
                    {
                        try
                        {
                            foreach (var session in device.GetSessions())
                            {
                                if (session.ProcessId != 0 &&
                                    session.State == AudioSessionState.Active &&
                                    !map.ContainsKey(session.ProcessId))
                                {
                                    map[session.ProcessId] = device.Id;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Debug(ex, "Failed to build session map for device {DeviceId}", device.Id);
                        }
                    }

                    return map;
                }
                finally
                {
                    foreach (var device in devices)
                        device.Dispose();
                }
            });
        }

        /// <summary>
        /// Used to interact directly with an <see cref="AudioDevice"/> on the ComThread
        /// </summary>
        /// <param name="device"></param>
        /// <param name="interaction"></param>
        /// <typeparam name="T"></typeparam>
        public T InteractWithDevice<T>(AudioDevice device, Func<AudioDevice, T> interaction) => ComThread.Invoke(() => interaction(device));

        /// <summary>
        /// Get the current default endpoint
        /// </summary>
        /// <param name="flow"></param>
        /// <param name="role"></param>
        /// <returns>Null if no default device is defined. The caller owns the returned device.</returns>
        public AudioDevice? GetDefaultAudioDevice(EDataFlow flow, ERole role) => ComThread.Invoke(() => EnumeratorClient.GetDefaultEndpoint(flow, role));

        /// <summary>
        /// Get a device with the given id, returns null if not present
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns>The caller owns the returned device.</returns>
        public AudioDevice? GetDevice(string deviceId) => ComThread.Invoke(() => EnumeratorClient.GetDevice(deviceId));

        /// <summary>
        /// Get audio endpoints for the given flow and state
        /// </summary>
        /// <param name="flow"></param>
        /// <param name="state"></param>
        /// <returns>The devices, or null when the enumeration failed. The caller owns the returned devices.</returns>
        public IReadOnlyList<AudioDevice>? GetAudioDevices(EDataFlow flow, EDeviceState state) => ComThread.Invoke(() => EnumeratorClient.GetEndpoints(flow, state));

        /// <summary>
        /// Register a client for default-device / endpoint notifications. The registration is
        /// marshalled onto the ComThread; the client must stay rooted until unregistered.
        /// </summary>
        public void RegisterNotificationClient(AudioDeviceNotificationClient client) => ComThread.Invoke(() =>
        {
            try
            {
                EnumeratorClient.RegisterNotificationClient(client);
            }
            catch (Exception ex)
            {
                // The Windows audio service can be unavailable (stopped, sleep/resume, RDP
                // disconnect, fast-user-switch). Only that case is expected (Information);
                // everything else is an unexpected failure worth a Warning.
                if (AudioDeviceException.IsAudioServiceNotRunning(ex))
                {
                    Log.Information(ex, "Device notification registration skipped: Windows audio service not running.");
                }
                else
                {
                    Log.Warning(ex, "Device notification registration failed.");
                }
            }
        });

        /// <summary>
        /// Unregister a previously registered notification client. Marshalled onto the ComThread.
        /// </summary>
        public void UnregisterNotificationClient(AudioDeviceNotificationClient client) => ComThread.Invoke(() =>
        {
            try
            {
                EnumeratorClient.UnregisterNotificationClient(client);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Device notification unregistration failed.");
            }
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
