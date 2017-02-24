namespace SoundSwitch.Framework.Updater
{
    public enum UpdateMode
    {
        /// <summary>
        /// Updates are installed in the background automatically without asking the user anything
        /// </summary>
        Silent,

        /// <summary>
        /// If an update exists, the user will be notified
        /// </summary>
        Notify,

        /// <summary>
        /// Update mechanism is disabled
        /// </summary>
        Never
    }
}