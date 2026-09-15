#nullable enable

using System.Collections.Generic;
using System.Linq;

using Serilog;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Model;

namespace SoundSwitch.Services
{
    /// <summary>
    /// Resolve a persisted <see cref="DeviceInfo"/> against the currently active devices.
    /// All matching knowledge lives in <see cref="DeviceReadOnlyCollection{T}.Match"/>:
    /// exact Id, then stable device-instance path (same type), then unique <see cref="DeviceInfo.NameClean"/>.
    /// Ambiguously named devices are never guessed — the rule is skipped and flagged.
    /// This is what lets rules survive device id changes (e.g. after a driver update).
    /// </summary>
    public static class AppRuleDeviceResolver
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(AppRuleDeviceResolver));

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
        /// Resolve a stored device against a set of candidate devices, with the full
        /// <see cref="MatchResult{T}"/> so callers can distinguish an ambiguous match from none.
        /// Logs a warning when the name is ambiguous.
        /// </summary>
        /// <param name="stored">The persisted device information</param>
        /// <param name="candidates">The devices to match against</param>
        public static MatchResult<DeviceFullInfo> ResolveResult(DeviceInfo? stored, IEnumerable<DeviceFullInfo> candidates)
        {
            if (stored == null) return MatchResult<DeviceFullInfo>.None();

            // Reuse the caller's collection when possible; otherwise index the candidates once.
            var collection = candidates as DeviceReadOnlyCollection<DeviceFullInfo>
                             ?? new DeviceReadOnlyCollection<DeviceFullInfo>(candidates, stored.Type);

            var result = collection.Match(stored);
            if (result.Kind == DeviceMatchKind.Ambiguous)
            {
                Logger.Warning("Ambiguous device match for {NameClean}: {Count} active devices share the name, skipping",
                    stored.NameClean, result.Candidates!.Count);
            }

            return result;
        }

        /// <summary>
        /// Resolve a stored device against a set of candidate devices.
        /// </summary>
        /// <param name="stored">The persisted device information</param>
        /// <param name="candidates">The devices to match against</param>
        /// <returns>The matching device, or `null` when none matches (including ambiguous names).</returns>
        public static DeviceFullInfo? Resolve(DeviceInfo? stored, IEnumerable<DeviceFullInfo> candidates)
        {
            var result = ResolveResult(stored, candidates);
            return result.IsResolved ? result.Device : null;
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
