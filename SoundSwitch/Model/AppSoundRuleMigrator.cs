#nullable enable

using System;
using System.Collections.Generic;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Model
{
    /// <summary>
    /// Migrate <see cref="AppSoundRule"/>s through the schema changes:
    /// <list type="number">
    /// <item>raw device ids (<see cref="AppSoundRule.PlaybackDeviceId"/>/<see cref="AppSoundRule.RecordingDeviceId"/>)
    /// to full <see cref="DeviceInfo"/> objects;</item>
    /// <item>the single <see cref="AppSoundRule.PlaybackDevice"/>/<see cref="AppSoundRule.RecordingDevice"/>
    /// pair to the ordered <see cref="AppSoundRule.DevicePairs"/> list;</item>
    /// <item>rules created by the short-lived hotkey schema (a <see cref="AppSoundRule.HotKey"/>
    /// without a mode) to <see cref="AppRuleMode.Hotkey"/>.</item>
    /// </list>
    /// The migration is idempotent: after the first run the legacy properties are
    /// cleared, so a second run returns <c>false</c> and leaves the pairs untouched.
    /// </summary>
    /// <remarks>
    /// The device name was never persisted in the oldest schema, so the best we can do is use
    /// the raw id as the name. While the id still resolves, the rule keeps working and the runtime
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
                // 1. Raw device id -> DeviceInfo (schema before 7.4).
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

                // 2. Single device pair -> ordered pair list (schema before modes).
                if (rule.DevicePairs == null)
                {
                    rule.DevicePairs = new List<AppSoundRuleDevicePair>();
                    migrated = true;
                }

                // Drop malformed entries so later code can assume every pair is usable.
                // A null entry can only come from hand-edited JSON; an empty pair cannot
                // be produced by the editor because it requires at least one device.
                if (rule.DevicePairs.RemoveAll(pair => pair == null || !pair.HasAnyDevice) > 0)
                {
                    migrated = true;
                }

                // Whether this rule already used the new schema. Only rules coming from
                // the old single-device schema are eligible for the hotkey-mode inference.
                var hadPairs = rule.DevicePairs.Count > 0;

                if (!hadPairs && (rule.PlaybackDevice != null || rule.RecordingDevice != null))
                {
                    rule.DevicePairs.Add(new AppSoundRuleDevicePair
                    {
                        Playback = rule.PlaybackDevice,
                        Recording = rule.RecordingDevice
                    });
                    migrated = true;
                }

                // 3. A rule created by PR #2522 has a hotkey but no mode. New-schema
                // rules always have a pair list, so they are not affected.
                if (!hadPairs && rule.HotKey != null && rule.Mode == AppRuleMode.Automatic)
                {
                    rule.Mode = AppRuleMode.Hotkey;
                    migrated = true;
                }

                if (!Enum.IsDefined(typeof(AppRuleMode), rule.Mode))
                {
                    rule.Mode = AppRuleMode.Automatic;
                    migrated = true;
                }

                // Stop persisting meaningful data in the legacy properties.
                if (rule.PlaybackDevice != null)
                {
                    rule.PlaybackDevice = null;
                    migrated = true;
                }

                if (rule.RecordingDevice != null)
                {
                    rule.RecordingDevice = null;
                    migrated = true;
                }

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
