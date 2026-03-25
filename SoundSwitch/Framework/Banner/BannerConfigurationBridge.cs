using System;
using System.Drawing;
using SoundSwitch.Banner;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.Banner;

public class BannerConfigurationBridge : IBannerConfiguration
{
    public SoundSwitch.Banner.BannerPosition Position => AppModel.Instance.BannerPosition;
    public int Opacity => AppModel.Instance.BannerOpacityPercentage;
    public TimeSpan Ttl => AppModel.Instance.BannerOnScreenTime;
    public bool NotifyUsingPrimaryScreen => AppModel.Instance.NotifyUsingPrimaryScreen;
    public int MaxConcurrentBanners => AppModel.Instance.IsSingleNotification ? 1 : AppModel.Instance.MaxNumberNotification;
    public Point? CustomPosition => AppModel.Instance.CustomBannerPosition;
}
