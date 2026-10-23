using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using FluentAssertions;

using Newtonsoft.Json;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Model;

namespace SoundSwitch.Tests;

/// <summary>
/// Tests the mode-based <see cref="AppSoundRule"/> model (issue #2496): mode,
/// optional hotkey, ordered device pairs, edit-time copy and configuration round-trip.
/// </summary>
[TestFixture]
public class AppSoundRuleHotKeyTests
{
    private static readonly HotKey RuleHotKey = new(Keys.F1, HotKey.ModifierKeys.Control | HotKey.ModifierKeys.Shift);
    private const string PlaybackId = "{0.0.0.00000000}.{11111111-1111-1111-1111-111111111111}";
    private const string RecordingId = "{0.0.1.00000000}.{22222222-2222-2222-2222-222222222222}";

    private static DeviceInfo Playback(string name = "Speakers (Realtek(R) Audio)") =>
        new(name, PlaybackId, EDataFlow.eRender, true, DateTime.UtcNow);

    private static DeviceInfo Recording() =>
        new("Microphone (Realtek(R) Audio)", RecordingId, EDataFlow.eCapture, false, DateTime.UtcNow);

    [Test]
    public void Copy_ShouldPreserveHotKeyAndMode()
    {
        var rule = new AppSoundRule
        {
            Mode = AppRuleMode.Hotkey,
            HotKey = RuleHotKey
        };

        var copy = rule.Copy();

        copy.HotKey.Should().Be(RuleHotKey);
        copy.Mode.Should().Be(AppRuleMode.Hotkey);
        copy.Id.Should().Be(rule.Id);
    }

    [Test]
    public void Copy_WithoutHotKey_ShouldStayNull()
    {
        var copy = new AppSoundRule().Copy();

        copy.HotKey.Should().BeNull();
        copy.Mode.Should().Be(AppRuleMode.Automatic);
    }

    [Test]
    public void Copy_ShouldDeepCopyDevicePairs()
    {
        var rule = new AppSoundRule
        {
            DevicePairs =
            {
                new AppSoundRuleDevicePair { Playback = Playback(), Recording = Recording() },
                new AppSoundRuleDevicePair { Playback = Playback("Headset (USB Audio)") }
            }
        };

        var copy = rule.Copy();

        copy.DevicePairs.Should().HaveCount(2);
        copy.DevicePairs[0].Should().NotBeSameAs(rule.DevicePairs[0]);
        copy.DevicePairs[1].Should().NotBeSameAs(rule.DevicePairs[1]);
        copy.DevicePairs[0].Playback!.Id.Should().Be(PlaybackId);

        copy.DevicePairs[0].Playback = null;
        rule.DevicePairs[0].Playback.Should().NotBeNull("mutating the copy must not mutate the original rule");
    }

    [Test]
    public void DevicePair_HasAnyDevice()
    {
        new AppSoundRuleDevicePair().HasAnyDevice.Should().BeFalse();
        new AppSoundRuleDevicePair { Playback = Playback() }.HasAnyDevice.Should().BeTrue();
        new AppSoundRuleDevicePair { Recording = Recording() }.HasAnyDevice.Should().BeTrue();
        new AppSoundRuleDevicePair { Playback = Playback(), Recording = Recording() }.HasAnyDevice.Should().BeTrue();
    }

    [Test]
    public void DevicePair_Copy_IsIndependent()
    {
        var pair = new AppSoundRuleDevicePair { Playback = Playback(), Recording = Recording() };

        var copy = pair.Copy();
        copy.Playback = null;

        pair.Playback.Should().NotBeNull();
        copy.Recording.Should().BeSameAs(pair.Recording);
    }

    [Test]
    public void Serialize_RoundTrip_PreservesModeHotKeyAndPairs()
    {
        var serializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };
        var writer = new StringWriter();
        serializer.Serialize(writer, new AppSoundRule
        {
            Mode = AppRuleMode.Hotkey,
            HotKey = RuleHotKey,
            DevicePairs =
            {
                new AppSoundRuleDevicePair { Playback = Playback(), Recording = Recording() },
                new AppSoundRuleDevicePair { Playback = Playback("Headset (USB Audio)") }
            }
        });

        var restored = JsonConvert.DeserializeObject<AppSoundRule>(writer.ToString());

        restored.Should().NotBeNull();
        restored!.Mode.Should().Be(AppRuleMode.Hotkey);
        restored.HotKey.Should().Be(RuleHotKey);
        restored.DevicePairs.Should().HaveCount(2);
        restored.DevicePairs[0].Playback!.Id.Should().Be(PlaybackId);
        restored.DevicePairs[0].Recording!.Id.Should().Be(RecordingId);
        restored.DevicePairs[1].Playback!.NameClean.Should().Be("Headset (USB Audio)");
        restored.DevicePairs[1].Recording.Should().BeNull();
    }

    [Test]
    public void Deserialize_LegacyRuleWithoutHotKey_LeavesItNull()
    {
        var rule = JsonConvert.DeserializeObject<AppSoundRule>(@"{""ProcessPath"":""app.exe""}");

        rule.Should().NotBeNull();
        rule!.HotKey.Should().BeNull();
        rule.Mode.Should().Be(AppRuleMode.Automatic);
        rule.DevicePairs.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void Deserialize_NullDevicePairs_DoesNotThrow()
    {
        var rule = JsonConvert.DeserializeObject<AppSoundRule>(@"{""ProcessPath"":""app.exe"",""DevicePairs"":null}");

        rule.Should().NotBeNull();
        rule!.DevicePairs.Should().BeNull();
    }
}
