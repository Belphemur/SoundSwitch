#nullable enable

using System;

namespace SoundSwitch.Services
{
    /// <summary>
    /// Case-insensitive exe identity matching between the paths Windows writes into its per-app
    /// persisted endpoint store (NT device form <c>\Device\HarddiskVolume&lt;N&gt;\...</c>) and
    /// drive-letter process paths. Windows resolves a record's key by exact exe identity; two
    /// paths identify the same app when one is a <em>path-component-boundary</em> suffix of the
    /// other after both are reduced to their volume-relative form (the letter path of the same
    /// exe equals the NT path's remainder). Used by <see cref="AppSoundLockManager"/>, the
    /// <see cref="AppRuleAssignmentReconciler"/> and the owner query for the rule list.
    /// </summary>
    public static class AppRuleIdentityMatcher
    {
        /// <summary>
        /// True when both paths identify the same executable. Empty/null paths never match.
        /// </summary>
        public static bool SameApp(string? left, string? right)
        {
            // Two drive-letter spellings are two different apps when the volumes differ:
            // clearing C:\Apps\Player.exe must not touch a D:\Apps\Player.exe route (and vice
            // versa). NT store paths keep the volume-relative match below.
            if (left != null && right != null &&
                IsDriveLetterPath(left) && IsDriveLetterPath(right) &&
                char.ToUpperInvariant(left[0]) != char.ToUpperInvariant(right[0]))
                return false;

            var l = Normalize(left);
            var r = Normalize(right);
            return l.Length > 0 && r.Length > 0 &&
                   (EndsWithAtComponentBoundary(l, r) || EndsWithAtComponentBoundary(r, l));
        }

        /// <summary>
        /// Whether <paramref name="shorter"/> prefixes-end-matches <paramref name="longer"/> —
        /// a case-insensitive suffix match that additionally requires the tail to begin at a
        /// path-component boundary: a stored relative path may not match inside a longer
        /// directory name (<c>MyApps\Setup.exe</c> is not <c>Apps\Setup.exe</c>). Offset 0
        /// counts as a boundary (the whole path is the final component).
        /// </summary>
        public static bool EndsWithAtComponentBoundary(string longer, string shorter)
        {
            var index = longer.Length - shorter.Length;
            return index >= 0 &&
                   string.Compare(longer, index, shorter, 0, shorter.Length, StringComparison.OrdinalIgnoreCase) == 0 &&
                   (index == 0 || longer[index - 1] == '\\');
        }

        /// <summary>
        /// Reduce a path for identity comparison: NT volume paths to their remainder, drive
        /// letter paths without the drive colon. Anything else is kept as-is (case-insensitive
        /// suffix matching still applies).
        /// </summary>
        private static bool IsDriveLetterPath(string path) =>
            path.Length > 2 && path[1] == ':' && path[2] == '\\';

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
