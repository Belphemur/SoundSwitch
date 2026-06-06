using System;
using System.IO;
using NUnit.Framework;
using FluentAssertions;
using NAudio.Wave;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;

namespace SoundSwitch.Tests;

[TestFixture]
public class NotificationSoundTests
{
    private class FakeNotificationConfiguration : INotificationConfiguration
    {
        public System.Windows.Forms.NotifyIcon Icon { get; set; }
        public Stream DefaultSound { get; set; }
        public CachedSound CustomSound { get; set; }
        public SoundSwitch.Framework.Banner.BannerPosition.BannerPosition BannerPosition { get; set; }
        public TimeSpan Ttl { get; set; }
        public int Opacity { get; set; }
        public SoundSwitch.Framework.Banner.MicrophoneMute.MicrophoneMute MicrophoneMuteBanner { get; set; }
        public SoundSwitch.Framework.Banner.MicrophoneMute.MicrophoneMute MicrophoneUnmuteBanner { get; set; }
    }

    [Test]
    public void NotifyMicrophoneMuteChanged_ShouldNotThrow_WhenCustomSoundIsNull()
    {
        // Arrange
        var notification = new NotificationSound();
        var defaultSoundBytes = new byte[] { 1, 2, 3, 4 };
        var defaultSoundStream = new MemoryStream(defaultSoundBytes);
        
        var config = new FakeNotificationConfiguration
        {
            DefaultSound = defaultSoundStream,
            CustomSound = null
        };
        notification.Configuration = config;

        // Act & Assert
        Assert.DoesNotThrow(() => notification.NotifyMicrophoneMuteChanged("device-id", "Microphone", true));
    }
}
