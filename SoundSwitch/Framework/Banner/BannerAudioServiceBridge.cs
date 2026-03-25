using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SoundSwitch.Banner;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Audio.Play;
using SoundSwitch.Framework.Threading;

namespace SoundSwitch.Framework.Banner;

public class BannerAudioServiceBridge : IBannerAudioService
{
    public async Task PlaySoundAsync(string soundPath, string? deviceId, CancellationToken cancellationToken)
    {
        if (!File.Exists(soundPath)) return;

        try
        {
            var sound = new CachedSound(soundPath);
            JobScheduler.Instance.ScheduleJob(new PlaySoundJob(deviceId, sound), cancellationToken);
            return;
        }
        catch (CachedSoundFileNotExistsException)
        {
            // Ignored
        }
    }
}
