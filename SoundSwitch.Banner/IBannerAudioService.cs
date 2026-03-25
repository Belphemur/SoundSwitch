using System.Threading;
using System.Threading.Tasks;

namespace SoundSwitch.Banner;

/// <summary>
/// Service providing audio playback capabilities for the banner.
/// </summary>
public interface IBannerAudioService
{
    /// <summary>
    /// Plays a sound file on the specified device.
    /// </summary>
    /// <param name="soundPath">The path to the sound file.</param>
    /// <param name="deviceId">The ID of the device to play on (null for default).</param>
    /// <param name="cancellationToken">Cancellation token to stop playback.</param>
    /// <returns>A task representing the playback operation.</returns>
    Task PlaySoundAsync(string soundPath, string? deviceId, CancellationToken cancellationToken);
}
