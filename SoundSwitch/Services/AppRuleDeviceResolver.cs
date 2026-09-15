#nullable enable

using System.Collections.Generic;
using System.Linq;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Model;

namespace SoundSwitch.Services
{
    /// <summary>
    /// Resolve a persisted <see cref="DeviceInfo"/> against the currently active devices.
    /// Matching follows the same rules as <c>ProfileManager.CheckDeviceAvailable</c>:
    /// the type must match, then the <see cref="DeviceInfo.Id"/>, falling back to <see cref="DeviceInfo.NameClean"/>.
    /// This is what lets rules and profiles survive device id changes (e.g. after a driver update).
    /// </summary>
    public static class AppRuleDeviceResolver
    {
        /// <summary>
        /// Resolve a stored device against the active devices of an <see cref="IAudioDeviceLister"/>.
        /// </summary>
        /// <param name="stored">The persisted device information</param>
        /// <param name="lister">The lister providing the currently active devices</param>
        /// <returns>The matching active device, or `null` when none matches.</returns>
        public static DeviceFullInfo? Resolve(DeviceInfo? stored, IAudioDeviceLister lister)
        {
            if (stored == null) return null;

            return Resolve(stored, lister.GetDevices(stored.Type, EDeviceState.Active));
        }

        /// <summary>
        /// Resolve a stored device against a set of candidate devices.
        /// </summary>
        /// <param name="stored">The persisted device information</param>
        /// <param name="candidates">The devices to match against</param>
        /// <returns>The matching device, or `null` when none matches.</returns>
        public static DeviceFullInfo? Resolve(DeviceInfo? stored, IEnumerable<DeviceFullInfo> candidates)
        {
            if (stored == null) return null;

            // Exact id match first: with duplicate cleaned names, an earlier same-name
            // candidate must not shadow the candidate actually carrying the stored id.
            return candidates.FirstOrDefault(info => info.Type == stored.Type && info.Id == stored.Id)
                   ?? candidates.FirstOrDefault(info => info.Equals(stored));
        }

        /// <summary>
        /// Resolve a stored device against the devices of an <see cref="IDeviceService"/>.
        /// </summary>
        /// <param name="stored">The persisted device information</param>
        /// <param name="deviceService">The service providing the available devices</param>
        /// <returns>The matching available device, or `null` when none matches.</returns>
        public static DeviceFullInfo? Resolve(DeviceInfo? stored, IDeviceService deviceService)
        {
            if (stored == null) return null;

            return stored.Type == EDataFlow.eCapture
                ? Resolve(stored, deviceService.AvailableRecordingDevices)
                : Resolve(stored, deviceService.AvailablePlaybackDevices);
        }
    }
}
