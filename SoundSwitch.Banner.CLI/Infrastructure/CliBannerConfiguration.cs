using SoundSwitch.Banner;
using System.Drawing;

namespace SoundSwitch.Banner.CLI.Infrastructure;

public class CliBannerConfiguration : IBannerConfiguration
{
    public int Opacity { get; set; } = 100;
    public TimeSpan Ttl { get; set; } = TimeSpan.FromSeconds(3);
    public int MaxConcurrentBanners => 5;
    public bool NotifyUsingPrimaryScreen => true;
    public Point? CustomPosition => null;
}
