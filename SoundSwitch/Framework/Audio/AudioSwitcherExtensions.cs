#nullable enable
using System;
using System.Collections.Generic;

using Serilog;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Framework.Audio
{
    /// <summary>
    /// App-side construction of the <see cref="DeviceInfo"/>/<see cref="DeviceFullInfo"/> DTOs from
    /// the library's <see cref="AudioDevice"/>. This lives in the app (not in
    /// SoundSwitch.Audio.Manager) because the DTO types are owned by SoundSwitch.Common and the
    /// library must not reference them.
    /// </summary>
    public static class AudioSwitcherExtensions
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(AudioSwitcherExtensions));

        /// <summary>
        /// Get the current default endpoint
        /// </summary>
        /// <param name="switcher"></param>
        /// <param name="flow"></param>
        /// <param name="role"></param>
        /// <returns>Null if no default device is defined</returns>
        public static DeviceFullInfo? GetDefaultAudioEndpoint(this AudioSwitcher switcher, EDataFlow flow, ERole role)
        {
            var device = switcher.GetDefaultAudioDevice(flow, role);
            if (device == null)
                return null;

            try
            {
                return new DeviceFullInfo(device);
            }
            catch (Exception e)
            {
                Logger.Warning(e, "Couldn't get default device info [{Flow}|{Role}]", flow, role);
                device.Dispose();
                return null;
            }
        }

        /// <summary>
        /// Get a device with the given id, returns null if not present
        /// </summary>
        /// <param name="switcher"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static DeviceFullInfo? GetAudioEndpoint(this AudioSwitcher switcher, string deviceId)
        {
            var device = switcher.GetDevice(deviceId);
            if (device == null)
                return null;

            try
            {
                return new DeviceFullInfo(device);
            }
            catch (Exception e)
            {
                Logger.Warning(e, "Couldn't get device info [{DeviceId}]", deviceId);
                device.Dispose();
                return null;
            }
        }

        /// <summary>
        /// Get audio endpoints for the given flow and state. Devices whose cleaned name is empty
        /// are filtered out (previously enforced inside the library; re-applied app-side now that
        /// the library returns <see cref="AudioDevice"/>).
        /// </summary>
        /// <param name="switcher"></param>
        /// <param name="flow"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IEnumerable<DeviceFullInfo> GetAudioEndpoints(this AudioSwitcher switcher, EDataFlow flow, EDeviceState state)
        {
            var devices = switcher.GetAudioDevices(flow, state) ?? throw new InvalidOperationException("Audio endpoint enumeration failed.");
            var endpoints = new List<DeviceFullInfo>(devices.Count);
            foreach (var device in devices)
            {
                DeviceFullInfo deviceInfo;
                try
                {
                    deviceInfo = new DeviceFullInfo(device);
                }
                catch (Exception e)
                {
                    Logger.Warning(e, "Couldn't get device info [{DeviceId}]", device.Id);
                    device.Dispose();
                    continue;
                }

                // Devices whose cleaned name is empty are filtered out; dispose them so the
                // AudioDevice they own (a COM reference) doesn't leak.
                if (string.IsNullOrEmpty(deviceInfo.NameClean))
                {
                    deviceInfo.Dispose();
                    continue;
                }

                endpoints.Add(deviceInfo);
            }

            return endpoints.ToArray();
        }
    }
}
