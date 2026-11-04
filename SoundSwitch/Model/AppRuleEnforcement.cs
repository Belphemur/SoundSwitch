namespace SoundSwitch.Model
{
    /// <summary>
    /// How an <see cref="AppSoundRule"/>'s routing is enforced.
    /// </summary>
    public enum AppRuleEnforcement
    {
        /// <summary>
        /// Today's behaviour: the rule is re-applied by SoundSwitch whenever a matching
        /// process appears, comes to the foreground, or on hotkey.
        /// </summary>
        Dynamic,

        /// <summary>
        /// The route is written into Windows' per-app persisted store once and is then
        /// re-applied by the audio engine itself (even while SoundSwitch is closed).
        /// </summary>
        Assignment
    }
}
