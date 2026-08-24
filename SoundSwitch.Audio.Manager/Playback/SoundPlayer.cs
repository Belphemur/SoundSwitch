#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoundSwitch.Audio.Manager.Playback
{
    /// <summary>
    /// Public playback facade — the only API the app's notification path touches (it replaces the
    /// device enumerator the job used to construct itself).
    /// </summary>
    public static class SoundPlayer
    {
        /// <summary>
        /// Render a buffer of samples on the given endpoint (or the default render/multimedia
        /// endpoint when <paramref name="deviceId"/> is null or empty). The returned task
        /// completes when playback has drained, the device is missing, or playback is cancelled;
        /// it faults on initialization failure. <paramref name="onCompleted"/> fires once on the
        /// stopped paths with the playback error, if any.
        /// </summary>
        public static Task PlayAsync(byte[] audioData, WaveFormat format, string? deviceId = null,
            CancellationToken cancellationToken = default, Action<Exception?>? onCompleted = null)
        {
            ArgumentNullException.ThrowIfNull(audioData);
            ArgumentNullException.ThrowIfNull(format);

            var player = new WavePlayer(audioData, format, deviceId, cancellationToken, onCompleted);
            player.Start();
            return player.Task;
        }
    }
}
