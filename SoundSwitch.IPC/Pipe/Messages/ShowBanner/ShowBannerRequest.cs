#nullable enable
using MessagePack;
using SoundSwitch.Banner;

namespace SoundSwitch.IPC.Pipe.Messages.ShowBanner;

[MessagePackObject(keyAsPropertyName: true)]
public class ShowBannerRequest : IPipeMessage
{
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? ImageSource { get; set; }
    public TimeSpan? Ttl { get; set; }
    public ShowOnScreen? Screen { get; set; }
}
