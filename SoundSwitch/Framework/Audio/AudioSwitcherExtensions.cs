#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
                Trace.TraceWarning("Couldn't get default device info [{0}|{1}]: {2}", flow, role, e);
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
                Trace.TraceWarning("Couldn't get device info [{0}]: {1}", deviceId, e);
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
            return devices.Select(device =>
                {
                    try
                    {
                        return new DeviceFullInfo(device);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceWarning("Couldn't get device info [{0}]: {1}", device.Id, e);
                        device.Dispose();
                        return null;
                    }
                })
                .Where(device => !string.IsNullOrEmpty(device?.NameClean))
                .Cast<DeviceFullInfo>().ToArray();
        }
    }
}
