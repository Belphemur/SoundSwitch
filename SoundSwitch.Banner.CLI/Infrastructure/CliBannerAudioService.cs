using NAudio.Wave;
using SoundSwitch.Banner;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace SoundSwitch.Banner.CLI.Infrastructure;

public class CliBannerAudioService : IBannerAudioService
{
    public async Task PlaySoundAsync(string soundPath, string? deviceId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(soundPath) || !File.Exists(soundPath)) return;
        
        await Task.Run(() =>
        {
            try {
                using var audioFile = new AudioFileReader(soundPath);
                using var outputDevice = new WasapiOut();
                outputDevice.Init(audioFile);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing && !cancellationToken.IsCancellationRequested)
                {
                    Thread.Sleep(100);
                }
            } catch { /* Ignored */ }
        }, cancellationToken);
    }
}
