#nullable enable

using System;
using System.IO;

using SoundSwitch.Model;

namespace SoundSwitch.Services
{
    /// <summary>
    /// Validation of the constraints an <see cref="AppRuleEnforcement.Assignment"/> rule must
    /// satisfy before it can be saved: it needs a single concrete exe identity (no regex or
    /// glob — the persisted store routes one exe identity, and the editor's matching for it is
    /// exact) and must not carry a window-title pattern (the store keys on identities, not on
    /// windows). Mode and single-pair constraints are enforced by the editor itself, which only
    /// offers assignment in Automatic mode and builds exactly one pair there.
    /// </summary>
    public static class AppRuleAssignmentValidator
    {
        public enum Error
        {
            None,
            NeedsIdentity,
            NoWindowTitle
        }

        public static Error Validate(AppSoundRule rule)
        {
            if (rule.Enforcement != AppRuleEnforcement.Assignment)
                return Error.None;

            if (string.IsNullOrWhiteSpace(rule.ProcessPath) || HasPatternSyntax(rule.ProcessPath))
                return Error.NeedsIdentity;

            if (!string.IsNullOrWhiteSpace(rule.WindowName))
                return Error.NoWindowTitle;

            return Error.None;
        }

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
            if (processPath.IndexOf('\\') >= 0 &&
                !processPath.StartsWith(@"\\", StringComparison.Ordinal) &&
                File.Exists(processPath))
                return false;

            return processPath.IndexOfAny(new[] {'*', '?', '[', ']', '{', '}', '^', '$', '|', '+'}) >= 0;
        }
    }
}
