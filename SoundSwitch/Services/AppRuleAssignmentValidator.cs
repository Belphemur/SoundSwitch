#nullable enable

using System;
using System.IO;

using SoundSwitch.Model;

namespace SoundSwitch.Services
{
    /// <summary>
    /// Whether a rule can also be registered as a Windows app assignment (its route written into
    /// the per-app store so the engine keeps applying it, even while SoundSwitch is closed).
    ///
    /// There is no user-facing choice: a rule that <em>can</em> be an assignment always is one,
    /// and it keeps its dynamic enforcement on top as the fallback (process start, foreground,
    /// hotkey). The identity route only needs a single concrete exe path — the store routes one
    /// exe identity, so a regex/glob or a window-title condition cannot be expressed there and
    /// those rules stay dynamic-only.
    /// </summary>
    public static class AppRuleAssignmentValidator
    {
        /// <summary>
        /// Whether the rule is eligible to also write an identity assignment: Automatic mode,
        /// one device pair, a concrete exe path and no window-title condition.
        /// </summary>
        public static bool IsEligible(AppSoundRule rule)
        {
            if (rule.Mode != AppRuleMode.Automatic)
                return false;

            if (rule.DevicePairs == null || rule.DevicePairs.Count != 1)
                return false;

            if (string.IsNullOrWhiteSpace(rule.ProcessPath) || HasPatternSyntax(rule.ProcessPath))
                return false;

            return string.IsNullOrWhiteSpace(rule.WindowName);
        }

        /// <summary>
        /// Keep <see cref="AppSoundRule.Enforcement"/> in sync with eligibility. Called on save
        /// and on migration: the flag is derived state, never a user preference (the editor no
        /// longer offers a choice).
        /// </summary>
        /// <returns>The value the flag should hold.</returns>
        public static AppRuleEnforcement EnforcementFor(AppSoundRule rule) =>
            IsEligible(rule) ? AppRuleEnforcement.Assignment : AppRuleEnforcement.Dynamic;

        /// <summary>
        /// Whether the process field carries regex or glob syntax instead of a concrete exe
        /// path. Parentheses are legal in Windows paths ("Program Files (x86)"), so only
        /// operators with no directory-name meaning flag a pattern. Characters such as
        /// <c>+</c>, <c>[</c> or <c>^</c> are legal in file names too (<c>C:\Apps\C++\app.exe</c>),
        /// so a path that names an existing file is trusted as concrete. The probe is skipped for
        /// UNC paths: this runs on the UI thread per keystroke, and an SMB timeout must not block
        /// the editor.
        /// </summary>
        internal static bool HasPatternSyntax(string processPath)
        {
            // Cheap check first: a path without any operator cannot be a pattern, so the common
            // case never touches the filesystem (this runs per keystroke on the UI thread).
            if (processPath.IndexOfAny(PatternCharacters) < 0)
                return false;

            // A path that names an existing local file is concrete even when its name carries an
            // operator ("C:\...\C++\app.exe"). The probe is skipped for UNC paths: this runs on
            // the UI thread per keystroke, and an SMB timeout must not block the editor.
            if (processPath.IndexOf('\\') >= 0 &&
                !processPath.StartsWith(@"\\", StringComparison.Ordinal) &&
                File.Exists(processPath))
                return false;

            return true;
        }

        private static readonly char[] PatternCharacters = {'*', '?', '[', ']', '{', '}', '^', '$', '|', '+'};
    }
}
