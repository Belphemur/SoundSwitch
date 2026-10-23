namespace SoundSwitch.Model
{
    /// <summary>
    /// How an <see cref="AppSoundRule"/> is applied.
    /// </summary>
    public enum AppRuleMode
    {
        /// <summary>
        /// Apply the first device pair automatically when a matching process starts
        /// or comes to the foreground.
        /// </summary>
        Automatic,

        /// <summary>
        /// Do nothing automatically. Pressing the rule's hotkey applies the next
        /// device pair to every matching running process.
        /// </summary>
        Hotkey
    }
}
