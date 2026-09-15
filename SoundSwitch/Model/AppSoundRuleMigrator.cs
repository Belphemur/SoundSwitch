#nullable enable

using System;
using System.Collections.Generic;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Model
{
    /// <summary>
    /// Migrate <see cref="AppSoundRule"/>s that only persisted the raw device id strings
    /// (<see cref="AppSoundRule.PlaybackDeviceId"/>/<see cref="AppSoundRule.RecordingDeviceId"/>)
    /// to the new schema persisting full <see cref="DeviceInfo"/> objects
    /// (<see cref="AppSoundRule.PlaybackDevice"/>/<see cref="AppSoundRule.RecordingDevice"/>).
    /// </summary>
    /// <remarks>
    /// The device name was never persisted in the old schema, so the best we can do is use the
    /// raw id as the name. While the id still resolves, the rule keeps working and the runtime
    /// self-heal in <c>AppSoundLockManager</c> restores the real device name on the first match.
    /// </remarks>
    public static class AppSoundRuleMigrator
    {
        /// <summary>
        /// Migrate the given rules in place.
        /// </summary>
        /// <param name="rules">The rules to migrate</param>
        /// <returns>`true` if any rule was changed, `false` otherwise.</returns>
        public static bool Migrate(IEnumerable<AppSoundRule> rules)
        {
            var migrated = false;
#pragma warning disable CS0618 // Type or member is obsolete
            foreach (var rule in rules)
            {
                if (rule.PlaybackDevice == null && !string.IsNullOrEmpty(rule.PlaybackDeviceId))
                {
                    rule.PlaybackDevice = new DeviceInfo(rule.PlaybackDeviceId, rule.PlaybackDeviceId, EDataFlow.eRender, false, DateTime.UtcNow);
                    migrated = true;
                }

                if (rule.RecordingDevice == null && !string.IsNullOrEmpty(rule.RecordingDeviceId))
                {
                    rule.RecordingDevice = new DeviceInfo(rule.RecordingDeviceId, rule.RecordingDeviceId, EDataFlow.eCapture, false, DateTime.UtcNow);
                    migrated = true;
                }

                // Stop persisting meaningful data in the obsolete properties
                if (rule.PlaybackDeviceId != null)
                {
                    rule.PlaybackDeviceId = null;
                    migrated = true;
                }

                if (rule.RecordingDeviceId != null)
                {
                    rule.RecordingDeviceId = null;
                    migrated = true;
                }
            }
#pragma warning restore CS0618 // Type or member is obsolete

            return migrated;
        }
    }
}
