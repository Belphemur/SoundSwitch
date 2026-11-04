#nullable enable

using System;

namespace SoundSwitch.Services
{
    /// <summary>
    /// Case-insensitive exe identity matching between the paths Windows writes into its per-app
    /// persisted endpoint store (NT device form <c>\Device\HarddiskVolume&lt;N&gt;\...</c>) and
    /// drive-letter process paths. Windows resolves a record's key by exact exe identity; two
    /// paths identify the same app when one is a suffix of the other after both are reduced to
    /// their volume-relative form (the letter path of the same exe equals the NT path's
    /// remainder). Used by <see cref="AppSoundLockManager"/>, the
    /// <see cref="AppRuleAssignmentReconciler"/> and the owner query for the rule list.
    /// </summary>
    public static class AppRuleIdentityMatcher
    {
        /// <summary>
        /// True when both paths identify the same executable. Empty/null paths never match.
        /// </summary>
        public static bool SameApp(string? left, string? right)
        {
            var l = Normalize(left);
            var r = Normalize(right);
            return l.Length > 0 && r.Length > 0 &&
                   (l.EndsWith(r, StringComparison.OrdinalIgnoreCase) ||
                    r.EndsWith(l, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Reduce a path for identity comparison: NT volume paths to their remainder, drive
        /// letter paths without the drive colon. Anything else is kept as-is (case-insensitive
        /// suffix matching still applies).
        /// </summary>
        private static string Normalize(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            var relative = SoundSwitch.Audio.Manager.PersistedDefaultEndpointStore.GetVolumeRelativePath(path);
            if (relative.Length > 0)
                return relative;

            // Drive letter form "C:\Users\...\app.exe" -> "\Users\...\app.exe"
            return path.Length > 2 && path[1] == ':' && path[2] == '\\'
                ? path.Substring(2)
                : path;
        }
    }
}
