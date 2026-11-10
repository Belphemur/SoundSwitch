using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.Banner.BannerPosition.Position;
using SoundSwitch.Framework.Banner.MicrophoneMute;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.NotificationManager.Notification;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.Properties;

namespace SoundSwitch.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class BannerStyleTests
{
    private static T Field<T>(object owner, string name) =>
        (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(owner)!;

    private static void AssertImageMatches(Image actual, Bitmap expected)
    {
        actual.Size.Should().Be(expected.Size);
        var bitmap = (Bitmap)actual;
        for (var y = 0; y < bitmap.Height; y++)
            for (var x = 0; x < bitmap.Width; x++)
                bitmap.GetPixel(x, y).Should().Be(expected.GetPixel(x, y));
    }

    [Test]
    public void MissingStyleDefaultsToNormal()
    {
        JsonConvert.DeserializeObject<SoundSwitchConfiguration>("{}")!.BannerStyle.Should().Be(BannerStyle.Normal);
    }

    [TestCase(BannerStyle.Normal)]
    [TestCase(BannerStyle.Compact)]
    public void StyleRoundTripsWithoutChangingMicrophoneModesOrTime(BannerStyle style)
    {
        var config = new SoundSwitchConfiguration
        {
            BannerStyle = style,
            MicrophoneMuteBanner = MicrophoneMute.Fading,
            MicrophoneUnmuteBanner = MicrophoneMute.Persistent,
            BannerOnScreenTime = TimeSpan.FromSeconds(7)
        };
        var restored = JsonConvert.DeserializeObject<SoundSwitchConfiguration>(JsonConvert.SerializeObject(config))!;
        restored.BannerStyle.Should().Be(style);
        restored.MicrophoneMuteBanner.Should().Be(MicrophoneMute.Fading);
        restored.MicrophoneUnmuteBanner.Should().Be(MicrophoneMute.Persistent);
        restored.BannerOnScreenTime.Should().Be(TimeSpan.FromSeconds(7));
    }

    [Test]
    public void InvalidStyleMigrationIsIdempotent()
    {
        var config = new SoundSwitchConfiguration { BannerStyle = (BannerStyle)99 };
        config.Migrate().Should().BeTrue();
        config.BannerStyle.Should().Be(BannerStyle.Normal);
        config.Migrate().Should().BeFalse();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void MicrophoneStylesRetainTheirOriginalIconsAndTextArrangement(bool isMuted)
    {
        var data = NotificationContentBuilder.BuildMicrophoneBanner("Test microphone", isMuted);
        var normal = data.GetContent(BannerStyle.Normal);
        normal.Title.Should().Be(string.Format(isMuted
            ? SettingsStrings.notification_microphone_muted : SettingsStrings.notification_microphone_unmuted, "Test microphone"));
        normal.Text.Should().BeNull();
        AssertImageMatches(normal.Image, isMuted ? Resources.microphone_muted : Resources.microphone_unmuted);

        var compact = data.GetContent(BannerStyle.Compact);
        compact.Title.Should().Be(isMuted ? SettingsStrings.microphone_off : SettingsStrings.microphone_on);
        compact.Text.Should().Be("Test microphone");
        AssertImageMatches(compact.Image, isMuted ? Resources.MicrophoneOff : Resources.MicrophoneOn);
    }

    [Test]
    public void MicrophoneBannerNameUsesFriendlyNameWithoutChangingTheCleanDeviceName()
    {
        var device = new DeviceInfo("Test microphone (USB Audio)", "mic", EDataFlow.eCapture, false, DateTime.UnixEpoch);

        AppModel.GetMicrophoneBannerName(device).Should().Be("Test microphone");
        device.NameClean.Should().Be("Test microphone (USB Audio)");
    }

    [Test]
    public void DeviceContentIsUnchangedBetweenStyles()
    {
        using var image = new Bitmap(32, 32);
        var data = new BannerData { Title = "Playback device", Text = "Test speaker", Image = image };
        foreach (var style in new[] { BannerStyle.Normal, BannerStyle.Compact })
        {
            var content = data.GetContent(style);
            content.Title.Should().Be(data.Title);
            content.Text.Should().Be(data.Text);
            content.Image.Should().BeSameAs(image);
        }
    }

    [Test]
    public void ReusedFormStopsPersistentTimerAndUpdatesFiniteInterval()
    {
        using var banner = new BannerForm();
        BannerData Data(TimeSpan ttl) => new()
        {
            Position = new PositionTopLeft(), Title = "Timer test", Ttl = ttl
        };

        banner.SetData(Data(TimeSpan.FromSeconds(2)));
        var timer = Field<System.Windows.Forms.Timer>(banner, "_timerHide");
        timer.Enabled.Should().BeTrue();
        timer.Interval.Should().Be(2000);
        banner.SetData(Data(TimeSpan.MaxValue));
        timer.Enabled.Should().BeFalse();
        banner.SetData(Data(TimeSpan.FromSeconds(7)));
        timer.Enabled.Should().BeTrue();
        timer.Interval.Should().Be(7000);
    }

    [Test]
    public void StyleRefreshPreservesTimerConfiguration()
    {
        WithBannerContext(() =>
        {
            AppConfigs.Configuration.BannerStyle = BannerStyle.Normal;
            using var banner = new BannerForm();
            banner.SetData(new BannerData
            {
                Position = new PositionTopLeft(), Title = "Style test", Text = "Device name",
                Ttl = TimeSpan.FromSeconds(7)
            });
            var timer = Field<System.Windows.Forms.Timer>(banner, "_timerHide");
            var originalFontSize = banner.Font.Size;
            AppConfigs.Configuration.BannerStyle = BannerStyle.Compact;
            banner.RefreshAppearance();
            banner.Font.Size.Should().BeApproximately(originalFontSize * 0.8f, 0.01f);
            Field<System.Windows.Forms.Timer>(banner, "_timerHide").Should().BeSameAs(timer);
            timer.Enabled.Should().BeTrue();
            timer.Interval.Should().Be(7000);
            Field<BannerData>(banner, "_currentData").Ttl.Should().Be(TimeSpan.FromSeconds(7));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void MicrophoneStyleRefreshUpdatesContentAndPreservesTimerAndAction(bool isMuted)
    {
        WithBannerContext(() =>
        {
            AppConfigs.Configuration.BannerStyle = BannerStyle.Normal;
            var data = NotificationContentBuilder.BuildMicrophoneBanner("Test microphone", isMuted);
            data.Position = new PositionTopLeft();
            data.Ttl = TimeSpan.FromSeconds(7);
            data.OnClick = (_, _) => { };
            var action = data.OnClick;
            using var banner = new BannerForm();
            banner.SetData(data);
            var timer = Field<System.Windows.Forms.Timer>(banner, "_timerHide");
            var top = Field<Label>(banner, "lblTop");
            var description = Field<Label>(banner, "lblTitle");
            var picture = Field<PictureBox>(banner, "pbxLogo");
            foreach (var style in new[] { BannerStyle.Compact, BannerStyle.Normal, BannerStyle.Compact })
            {
                AppConfigs.Configuration.BannerStyle = style;
                banner.RefreshAppearance();
                var content = data.GetContent(style);
                top.Text.Should().Be(content.Title);
                description.Text.Should().Be(content.Text ?? string.Empty);
                picture.Image.Should().BeSameAs(content.Image);
                Field<TableLayoutPanel>(banner, "tableLayoutPanel").GetRowSpan(picture).Should().Be(1,
                    "the original compact microphone layout is retained");
                picture.Size.Should().Be(BannerAppearance.ResolveImageSize(content.Image.Size,
                    new Size(16, 16), BannerAppearance.ImageScale(100, style == BannerStyle.Compact)));
                Field<BannerData>(banner, "_currentData").Should().BeSameAs(data);
                data.OnClick.Should().BeSameAs(action);
                data.Ttl.Should().Be(TimeSpan.FromSeconds(7));
                Field<System.Windows.Forms.Timer>(banner, "_timerHide").Should().BeSameAs(timer);
                timer.Enabled.Should().BeTrue();
                timer.Interval.Should().Be(7000);
            }
        });
    }

    [TestCase(16, 16, 100)]
    [TestCase(32, 32, 100)]
    [TestCase(256, 256, 100)]
    [TestCase(512, 128, 100)]
    [TestCase(256, 256, 50)]
    [TestCase(256, 256, 200)]
    public void CompactDeviceUsesTheMicrophoneLayoutRegardlessOfSourceIconSize(int width, int height, int scale)
    {
        WithBannerContext(() =>
        {
            AppConfigs.Configuration.BannerStyle = BannerStyle.Compact;
            AppConfigs.Configuration.BannerScalePercentage = scale;
            var microphoneData = NotificationContentBuilder.BuildMicrophoneBanner("Test microphone", true);
            microphoneData.Position = new PositionTopLeft();
            microphoneData.Ttl = TimeSpan.FromSeconds(7);
            using var microphone = new BannerForm();
            microphone.SetData(microphoneData);

            using var image = new Bitmap(width, height);
            using var banner = new BannerForm();
            banner.SetData(new BannerData
            {
                Position = new PositionTopLeft(), Title = "Playback device", Text = "Test speaker",
                Image = image, Ttl = TimeSpan.FromSeconds(7)
            });
            var table = Field<TableLayoutPanel>(banner, "tableLayoutPanel");
            var picture = Field<PictureBox>(banner, "pbxLogo");
            var top = Field<Label>(banner, "lblTop");
            var description = Field<Label>(banner, "lblTitle");
            var microphonePicture = Field<PictureBox>(microphone, "pbxLogo");
            var microphoneTop = Field<Label>(microphone, "lblTop");
            var microphoneDescription = Field<Label>(microphone, "lblTitle");
            picture.Size.Should().Be(microphonePicture.Size);
            picture.Size.Should().Be(BannerAppearance.CompactIconSize(scale));
            picture.SizeMode.Should().Be(PictureBoxSizeMode.Zoom);
            table.GetRow(picture).Should().Be(0);
            table.GetRowSpan(picture).Should().Be(1);
            picture.Anchor.Should().Be(AnchorStyles.Top | AnchorStyles.Left);
            picture.Top.Should().Be(microphonePicture.Top);
            description.Anchor.Should().Be(AnchorStyles.Left);
            (description.Top - top.Bottom).Should().Be(microphoneDescription.Top - microphoneTop.Bottom);

            AppConfigs.Configuration.BannerStyle = BannerStyle.Normal;
            banner.RefreshAppearance();
            table.GetRowSpan(picture).Should().Be(1);
            picture.Size.Should().Be(BannerAppearance.ResolveImageSize(image.Size,
                new Size(16, 16), BannerAppearance.ImageScale(scale, false)));
            description.Anchor.Should().Be(AnchorStyles.Left);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void NoneRemovesPreviousPersistentStateWithoutShowingAnotherBanner(bool isMuted)
    {
        WithBannerContext(() =>
        {
            var notification = CreateNotification();
            var manager = Field<MicrophoneMuteBannerManager>(notification, "_microphoneMuteBannerManager");
            var tracked = Field<Dictionary<string, BannerForm>>(manager, "_activeBanners");
            try
            {
                notification.NotifyMicrophoneMuteChanged("mic", "Test microphone", !isMuted);
                var previous = tracked["mic"];
                if (isMuted) notification.Configuration.MicrophoneMuteBanner = MicrophoneMute.None;
                else notification.Configuration.MicrophoneUnmuteBanner = MicrophoneMute.None;
                notification.NotifyMicrophoneMuteChanged("mic", "Test microphone", isMuted);
                tracked.Should().BeEmpty();
                previous.IsDisposed.Should().BeTrue();
                Field<BannerForm>(Field<BannerManager>(notification, "_bannerManager"), "_singleBanner").Should().BeNull();
            }
            finally { manager.ClearPersistentMuteBanners(); }
        });
    }

    [TestCase(BannerStyle.Normal, true)]
    [TestCase(BannerStyle.Normal, false)]
    [TestCase(BannerStyle.Compact, true)]
    [TestCase(BannerStyle.Compact, false)]
    public void BothPersistentStatesReuseTheSameWindowAndHaveNoTimer(BannerStyle style, bool isMuted)
    {
        WithBannerContext(() =>
        {
            AppConfigs.Configuration.BannerStyle = style;
            var notification = CreateNotification();
            var manager = Field<MicrophoneMuteBannerManager>(notification, "_microphoneMuteBannerManager");
            var tracked = Field<Dictionary<string, BannerForm>>(manager, "_activeBanners");
            try
            {
                notification.NotifyMicrophoneMuteChanged("mic", "Test microphone", isMuted);
                var banner = tracked["mic"];
                Field<BannerData>(banner, "_currentData").Ttl.Should().Be(TimeSpan.MaxValue);
                Field<System.Windows.Forms.Timer>(banner, "_timerHide").Should().BeNull();
                notification.NotifyMicrophoneMuteChanged("mic", "Test microphone", !isMuted);
                tracked["mic"].Should().BeSameAs(banner);
                var expected = Field<BannerData>(banner, "_currentData").GetContent(style);
                Field<Label>(banner, "lblTop").Text.Should().Be(expected.Title);
                Field<PictureBox>(banner, "pbxLogo").Image.Should().BeSameAs(expected.Image);
            }
            finally { manager.ClearPersistentMuteBanners(); }
        });
    }

    [TestCase(BannerStyle.Normal, true)]
    [TestCase(BannerStyle.Normal, false)]
    [TestCase(BannerStyle.Compact, true)]
    [TestCase(BannerStyle.Compact, false)]
    public void FadingUsesConfiguredTimeAndRemovesPreviousPersistentState(BannerStyle style, bool isMuted)
    {
        WithBannerContext(() =>
        {
            AppConfigs.Configuration.BannerStyle = style;
            var notification = CreateNotification();
            var manager = Field<MicrophoneMuteBannerManager>(notification, "_microphoneMuteBannerManager");
            var tracked = Field<Dictionary<string, BannerForm>>(manager, "_activeBanners");
            var regular = Field<BannerManager>(notification, "_bannerManager");
            try
            {
                notification.NotifyMicrophoneMuteChanged("mic", "Test microphone", !isMuted);
                var previous = tracked["mic"];
                if (isMuted) notification.Configuration.MicrophoneMuteBanner = MicrophoneMute.Fading;
                else notification.Configuration.MicrophoneUnmuteBanner = MicrophoneMute.Fading;
                notification.NotifyMicrophoneMuteChanged("mic", "Test microphone", isMuted);
                previous.IsDisposed.Should().BeTrue();
                tracked.Should().BeEmpty();
                var banner = Field<BannerForm>(regular, "_singleBanner");
                banner.Should().NotBeNull();
                Field<BannerData>(banner, "_currentData").Ttl.Should().Be(TimeSpan.FromSeconds(7));
                var data = Field<BannerData>(banner, "_currentData");
                data.CompactContent.Should().NotBeNull("fading must support both original microphone presentations");
                Field<System.Windows.Forms.Timer>(banner, "_timerHide").Interval.Should().Be(7000);
            }
            finally
            {
                manager.ClearPersistentMuteBanners();
                Field<BannerForm>(regular, "_singleBanner")?.Dispose();
            }
        });
    }

    [Test, Combinatorial]
    public void LeavingFadingImmediatelyRemovesThePreviousMicrophoneBanner(
        [Values(BannerStyle.Normal, BannerStyle.Compact)] BannerStyle style,
        [Values(true, false)] bool isMuted,
        [Values(1, 5)] int bannerCount,
        [Values(MicrophoneMute.Persistent, MicrophoneMute.None)] MicrophoneMute nextMode)
    {
        WithBannerContext(() =>
        {
            AppConfigs.Configuration.BannerStyle = style;
            AppConfigs.Configuration.MaxNumberNotification = bannerCount;
            var notification = CreateNotification();
            notification.Configuration.MicrophoneMuteBanner = MicrophoneMute.Fading;
            notification.Configuration.MicrophoneUnmuteBanner = MicrophoneMute.Fading;
            var regular = Field<BannerManager>(notification, "_bannerManager");
            var fading = Field<List<BannerForm>>(regular, "_bannerForms");
            var persistent = Field<MicrophoneMuteBannerManager>(notification, "_microphoneMuteBannerManager");
            var tracked = Field<Dictionary<string, BannerForm>>(persistent, "_activeBanners");
            try
            {
                notification.NotifyMicrophoneMuteChanged("mic", "Test microphone", !isMuted);
                var previous = bannerCount == 1 ? Field<BannerForm>(regular, "_singleBanner") : fading[0];
                if (isMuted) notification.Configuration.MicrophoneMuteBanner = nextMode;
                else notification.Configuration.MicrophoneUnmuteBanner = nextMode;

                notification.NotifyMicrophoneMuteChanged("mic", "Test microphone", isMuted);

                previous.IsDisposed.Should().BeTrue();
                Field<BannerForm>(regular, "_singleBanner").Should().BeNull();
                fading.Should().BeEmpty();
                if (nextMode == MicrophoneMute.Persistent)
                    Field<BannerData>(tracked["mic"], "_currentData").Ttl.Should().Be(TimeSpan.MaxValue);
                else
                    tracked.Should().BeEmpty();
            }
            finally
            {
                persistent.ClearPersistentMuteBanners();
                Field<BannerForm>(regular, "_singleBanner")?.Dispose();
                foreach (var banner in fading.ToArray()) banner.Dispose();
            }
        });
    }

    [TestCase(BannerStyle.Normal)]
    [TestCase(BannerStyle.Compact)]
    public void ReconcileToPersistentRemovesOnlyTheMatchingFadingMicrophone(BannerStyle style)
    {
        WithBannerContext(() =>
        {
            AppConfigs.Configuration.BannerStyle = style;
            AppConfigs.Configuration.MaxNumberNotification = 5;
            var notification = CreateNotification();
            notification.Configuration.MicrophoneMuteBanner = MicrophoneMute.Fading;
            notification.Configuration.MicrophoneUnmuteBanner = MicrophoneMute.Fading;
            var regular = Field<BannerManager>(notification, "_bannerManager");
            var fading = Field<List<BannerForm>>(regular, "_bannerForms");
            var persistent = Field<MicrophoneMuteBannerManager>(notification, "_microphoneMuteBannerManager");
            try
            {
                regular.ShowNotification(new BannerData
                {
                    Position = new PositionTopLeft(), Title = "Playback device", Text = "Test speaker",
                    Ttl = TimeSpan.FromSeconds(7)
                });
                var deviceBanner = fading[0];
                notification.NotifyMicrophoneMuteChanged("other", "Other microphone", false);
                var otherBanner = fading[1];
                notification.NotifyMicrophoneMuteChanged("mic", "Test microphone", false);
                var previous = fading[2];

                notification.Configuration.MicrophoneUnmuteBanner = MicrophoneMute.Persistent;
                notification.SyncMuteBanners(["mic"], [("mic", "Test microphone", false)]);

                previous.IsDisposed.Should().BeTrue();
                deviceBanner.IsDisposed.Should().BeFalse();
                otherBanner.IsDisposed.Should().BeFalse();
                fading.Should().BeEquivalentTo(new[] { deviceBanner, otherBanner });
                Field<Dictionary<string, BannerForm>>(persistent, "_activeBanners").Should().ContainKey("mic");
            }
            finally
            {
                persistent.ClearPersistentMuteBanners();
                foreach (var banner in fading.ToArray()) banner.Dispose();
            }
        });
    }

    [Test]
    public void LiveStyleChangeReflowsPersistentAndRegularStacks()
    {
        WithBannerContext(() =>
        {
            AppConfigs.Configuration.BannerStyle = BannerStyle.Normal;
            AppConfigs.Configuration.MaxNumberNotification = 5;
            var microphoneManager = new MicrophoneMuteBannerManager();
            var manager = new BannerManager();
            var persistent = Field<Dictionary<string, BannerForm>>(microphoneManager, "_activeBanners");
            var regular = Field<List<BannerForm>>(manager, "_bannerForms");
            try
            {
                microphoneManager.UpdateMicrophoneMuteState("first", "First microphone", false);
                microphoneManager.UpdateMicrophoneMuteState("second", "Second microphone", true);
                for (var index = 0; index < 2; index++)
                    manager.ShowNotification(new BannerData
                    {
                        Position = new PositionTopLeft(), Title = "Device", Text = $"Device {index}",
                        Ttl = TimeSpan.FromSeconds(7)
                    });
                AppConfigs.Configuration.BannerStyle = BannerStyle.Compact;
                BannerForm.RefreshVisibleBanners();
                Field<int>(persistent["second"], "_currentOffset").Should().Be(persistent["first"].Height + 10);
                var stacked = new List<BannerForm>(regular);
                Field<int>(stacked[0], "_currentOffset").Should().Be(stacked[1].Height + 10);
                Field<int>(stacked[1], "_currentOffset").Should().Be(0);
            }
            finally
            {
                microphoneManager.ClearPersistentMuteBanners();
                foreach (var banner in regular.ToArray()) banner.Dispose();
            }
        });
    }

    [Test]
    public void StackReflowKeepsNewestBannerFirstAfterRemovalAndReplacement()
    {
        WithBannerContext(() =>
        {
            AppConfigs.Configuration.BannerStyle = BannerStyle.Normal;
            AppConfigs.Configuration.MaxNumberNotification = 5;
            var manager = new BannerManager();
            var banners = Field<List<BannerForm>>(manager, "_bannerForms");
            void Show() => manager.ShowNotification(new BannerData
            {
                Position = new PositionTopLeft(), Title = "Device", Text = "Test speaker",
                Ttl = TimeSpan.FromSeconds(7)
            });
            try
            {
                Show();
                Show();
                var oldest = banners[0];
                var remaining = banners[1];
                oldest.Dispose();
                Show();
                var newest = banners[1];

                AppConfigs.Configuration.BannerStyle = BannerStyle.Compact;
                BannerForm.RefreshVisibleBanners();

                Field<int>(newest, "_currentOffset").Should().Be(0);
                Field<int>(remaining, "_currentOffset").Should().Be(newest.Height + 10);
            }
            finally
            {
                foreach (var banner in banners.ToArray()) banner.Dispose();
            }
        });
    }

    [Test]
    public void ReconcileIncludesPersistentOnAndRemovesOutOfScopeOrNonPersistentStates()
    {
        WithBannerContext(() =>
        {
            var notification = CreateNotification();
            var manager = Field<MicrophoneMuteBannerManager>(notification, "_microphoneMuteBannerManager");
            var tracked = Field<Dictionary<string, BannerForm>>(manager, "_activeBanners");
            try
            {
                notification.SyncMuteBanners(["on", "off"], [("on", "On microphone", false), ("off", "Off microphone", true)]);
                tracked.Keys.Should().BeEquivalentTo("on", "off");
                var onBanner = tracked["on"];
                notification.Configuration.MicrophoneMuteBanner = MicrophoneMute.Fading;
                notification.SyncMuteBanners(["on", "off"], [("on", "On microphone", false), ("off", "Off microphone", true)]);
                tracked.Keys.Should().BeEquivalentTo("on");
                tracked["on"].Should().BeSameAs(onBanner);
                notification.SyncMuteBanners([], []);
                tracked.Should().BeEmpty();
                onBanner.IsDisposed.Should().BeTrue();
            }
            finally { manager.ClearPersistentMuteBanners(); }
        });
    }

    private static NotificationBanner CreateNotification() => new()
    {
        Configuration = new NotificationConfiguration
        {
            BannerPosition = SoundSwitch.Framework.Banner.BannerPosition.BannerPosition.TopLeft,
            Ttl = TimeSpan.FromSeconds(7), Opacity = 100,
            MicrophoneMuteBanner = MicrophoneMute.Persistent,
            MicrophoneUnmuteBanner = MicrophoneMute.Persistent
        }
    };

    private static void WithBannerContext(Action test)
    {
        var previous = SynchronizationContext.Current;
        var style = AppConfigs.Configuration.BannerStyle;
        var count = AppConfigs.Configuration.MaxNumberNotification;
        var scale = AppConfigs.Configuration.BannerScalePercentage;
        var bannerContext = typeof(BannerManager).GetField("_syncContext", BindingFlags.Static | BindingFlags.NonPublic)!;
        var microphoneContext = typeof(MicrophoneMuteBannerManager).GetField("_syncContext", BindingFlags.Static | BindingFlags.NonPublic)!;
        var oldBannerContext = bannerContext.GetValue(null);
        var oldMicrophoneContext = microphoneContext.GetValue(null);
        var formContext = typeof(BannerForm).GetField("_uiContext", BindingFlags.Static | BindingFlags.NonPublic)!;
        var oldFormContext = formContext.GetValue(null);
        // WinForms owns the thread's shared marshalling control. Disposing a context here
        // destroys that control and breaks subsequent STA cases on the same test thread.
        var context = new WindowsFormsSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);
        formContext.SetValue(null, context);
        BannerManager.Setup();
        MicrophoneMuteBannerManager.Setup();
        AppConfigs.Configuration.MaxNumberNotification = 1;
        AppConfigs.Configuration.BannerScalePercentage = 100;
        try { test(); }
        finally
        {
            AppConfigs.Configuration.BannerStyle = style;
            AppConfigs.Configuration.MaxNumberNotification = count;
            AppConfigs.Configuration.BannerScalePercentage = scale;
            bannerContext.SetValue(null, oldBannerContext);
            microphoneContext.SetValue(null, oldMicrophoneContext);
            formContext.SetValue(null, oldFormContext);
            SynchronizationContext.SetSynchronizationContext(previous);
        }
    }
}
