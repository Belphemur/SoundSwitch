using System.IO;
using System.Windows.Forms;

using FluentAssertions;

using Newtonsoft.Json;

using NUnit.Framework;

using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Model;

namespace SoundSwitch.Tests;

/// <summary>
/// Tests the optional hotkey added to <see cref="AppSoundRule"/> (issue #2496):
/// it must survive the edit-time copy and a configuration round-trip, yet stay
/// optional for rules persisted before the feature existed.
/// </summary>
[TestFixture]
public class AppSoundRuleHotKeyTests
{
    private static readonly HotKey RuleHotKey = new(Keys.F1, HotKey.ModifierKeys.Control | HotKey.ModifierKeys.Shift);

    [Test]
    public void Copy_ShouldPreserveHotKey()
    {
        var rule = new AppSoundRule { HotKey = RuleHotKey };

        var copy = rule.Copy();

        copy.HotKey.Should().Be(RuleHotKey);
        copy.Id.Should().Be(rule.Id);
    }

    [Test]
    public void Copy_WithoutHotKey_ShouldStayNull()
    {
        var copy = new AppSoundRule().Copy();

        copy.HotKey.Should().BeNull();
    }

    [Test]
    public void Serialize_RoundTrip_PreservesHotKey()
    {
        var serializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };
        var writer = new StringWriter();
        serializer.Serialize(writer, new AppSoundRule { HotKey = RuleHotKey });

        var restored = JsonConvert.DeserializeObject<AppSoundRule>(writer.ToString());

        restored.Should().NotBeNull();
        restored!.HotKey.Should().Be(RuleHotKey);
    }

    [Test]
    public void Deserialize_LegacyRuleWithoutHotKey_LeavesItNull()
    {
        var rule = JsonConvert.DeserializeObject<AppSoundRule>(@"{""ProcessPath"":""app.exe""}");

        rule.Should().NotBeNull();
        rule!.HotKey.Should().BeNull();
    }
}
