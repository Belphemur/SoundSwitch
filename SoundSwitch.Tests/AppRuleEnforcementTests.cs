using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NUnit.Framework;

using SoundSwitch.Model;
using SoundSwitch.Services;

namespace SoundSwitch.Tests;

/// <summary>
/// <see cref="AppRuleEnforcement"/> round-trips, migration normalization and the assignment
/// validator constraints of the app-rules identity ADR (§6.1, §8).
/// </summary>
[TestFixture]
public class AppRuleEnforcementTests
{
    [Test]
    public void NewRule_DefaultsToDynamic()
    {
        var rule = new AppSoundRule();

        rule.Enforcement.Should().Be(AppRuleEnforcement.Dynamic);
        rule.AssignmentExePath.Should().BeNull();
    }

    [Test]
    public void Serialization_EnforcementSurvivesRoundTrip_ThroughConfigurationSettings()
    {
        // Same serializer settings as ConfigurationManager (NullValueHandling.Ignore).
        var serializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };
        var rule = new AppSoundRule
        {
            Enforcement = AppRuleEnforcement.Assignment,
            AssignmentExePath = @"C:\Apps\Spotify\Spotify.exe"
        };

        var writer = new System.IO.StringWriter();
        serializer.Serialize(writer, rule);
        var roundTripped = serializer.Deserialize<JObject>(new JsonTextReader(new System.IO.StringReader(writer.ToString())));

        roundTripped.Should().NotBeNull();
        roundTripped!["Enforcement"]!.Value<int>().Should().Be((int)AppRuleEnforcement.Assignment);
        roundTripped[nameof(AppSoundRule.AssignmentExePath)].Should().NotBeNull();
    }

    [Test]
    public void Migrate_UnknownEnforcementValue_FallsBackToDynamic()
    {
        var rule = new AppSoundRule { Enforcement = (AppRuleEnforcement)99 };

        var migrated = AppSoundRuleMigrator.Migrate(new[] { rule });

        migrated.Should().BeTrue();
        rule.Enforcement.Should().Be(AppRuleEnforcement.Dynamic);
    }

    [Test]
    public void Migrate_AssignmentWithWindowPattern_FallsBackToDynamic()
    {
        var rule = new AppSoundRule
        {
            Enforcement = AppRuleEnforcement.Assignment,
            ProcessPath = @"C:\Apps\Spotify\Spotify.exe",
            WindowName = ".*YouTube.*"
        };

        var migrated = AppSoundRuleMigrator.Migrate(new[] { rule });

        migrated.Should().BeTrue();
        rule.Enforcement.Should().Be(AppRuleEnforcement.Dynamic);
    }

    [Test]
    public void Migrate_AssignmentWithHotkeyMode_FallsBackToDynamic()
    {
        var rule = new AppSoundRule
        {
            Enforcement = AppRuleEnforcement.Assignment,
            ProcessPath = @"C:\Apps\Spotify\Spotify.exe",
            Mode = AppRuleMode.Hotkey
        };

        var migrated = AppSoundRuleMigrator.Migrate(new[] { rule });

        migrated.Should().BeTrue();
        rule.Enforcement.Should().Be(AppRuleEnforcement.Dynamic);
    }

    [Test]
    public void Migrate_ValidAssignment_IsKeptAndIdempotent()
    {
        var rule = new AppSoundRule
        {
            Enforcement = AppRuleEnforcement.Assignment,
            ProcessPath = @"C:\Apps\Spotify\Spotify.exe"
        };

        AppSoundRuleMigrator.Migrate(new[] { rule }).Should().BeFalse();
        rule.Enforcement.Should().Be(AppRuleEnforcement.Assignment);

        AppSoundRuleMigrator.Migrate(new[] { rule }).Should().BeFalse();
    }

    [Test]
    public void Migrate_MissingEnforcementProperty_DeserializesToDynamic()
    {
        var rule = JsonConvert.DeserializeObject<AppSoundRule>(@"{""ProcessPath"":""app.exe""}");

        rule.Should().NotBeNull();
        rule!.Enforcement.Should().Be(AppRuleEnforcement.Dynamic);
    }

    [Test]
    public void Copy_CarriesEnforcementFields()
    {
        var rule = new AppSoundRule
        {
            Enforcement = AppRuleEnforcement.Assignment,
            AssignmentExePath = @"C:\Apps\Spotify\Spotify.exe"
        };

        var copy = rule.Copy();

        copy.Enforcement.Should().Be(AppRuleEnforcement.Assignment);
        copy.AssignmentExePath.Should().Be(rule.AssignmentExePath);
    }

    // ── Validator ──────────────────────────────────────────────────

    [Test]
    public void EnforcementFor_ConcretePathWithoutWindowPattern_IsAssignment()
    {
        var rule = new AppSoundRule
        {
            Mode = AppRuleMode.Automatic,
            ProcessPath = @"C:\Apps\Spotify\Spotify.exe",
            DevicePairs = new List<AppSoundRuleDevicePair> { new() { Playback = null, Recording = null } }
        };

        AppRuleAssignmentValidator.IsEligible(rule).Should().BeTrue();
        AppRuleAssignmentValidator.EnforcementFor(rule).Should().Be(AppRuleEnforcement.Assignment);
    }

    [TestCase(".*Spotify.exe.*")]
    [TestCase(@"*\Spotify\*")]
    [TestCase("")]
    public void EnforcementFor_PatternOrEmptyPath_StaysDynamic(string processPath)
    {
        var rule = new AppSoundRule
        {
            Mode = AppRuleMode.Automatic,
            ProcessPath = processPath,
            DevicePairs = new List<AppSoundRuleDevicePair> { new() }
        };

        AppRuleAssignmentValidator.EnforcementFor(rule).Should().Be(AppRuleEnforcement.Dynamic);
    }

    [Test]
    public void EnforcementFor_WindowTitleCondition_StaysDynamic()
    {
        var rule = new AppSoundRule
        {
            Mode = AppRuleMode.Automatic,
            ProcessPath = @"C:\Apps\Spotify\Spotify.exe",
            WindowName = ".*Chat.*",
            DevicePairs = new List<AppSoundRuleDevicePair> { new() }
        };

        AppRuleAssignmentValidator.EnforcementFor(rule).Should().Be(AppRuleEnforcement.Dynamic);
    }

    [Test]
    public void EnforcementFor_HotkeyRule_StaysDynamic()
    {
        var rule = new AppSoundRule
        {
            Mode = AppRuleMode.Hotkey,
            ProcessPath = @"C:\Apps\Spotify\Spotify.exe",
            DevicePairs = new List<AppSoundRuleDevicePair> { new(), new() }
        };

        AppRuleAssignmentValidator.EnforcementFor(rule).Should().Be(AppRuleEnforcement.Dynamic);
    }
    [TestCase(@"C:\Apps\My App (1)\App.exe", false)]
    [TestCase(@"C:\Apps\My [a]\App.exe", true)]
    [TestCase(@".*.exe", true)]
    [TestCase(@"C:\Apps\Spotify\Spotify.exe", false)]
    public void HasPatternSyntax_DetectsRegexFragmentsOnly(string processPath, bool expected)
    {
        AppRuleAssignmentValidator.HasPatternSyntax(processPath).Should().Be(expected);
    }

    [Test]
    public void HasPatternSyntax_AcceptsAnExistingPathWithARegexCharacter()
    {
        // "+" is legal in Windows file/directory names: a real C:\...\C++\app.exe is concrete.
        // A UNC temp directory would make HasPatternSyntax skip its file probe by design
        // (an SMB timeout must not block the editor), so the premise of this test does not hold
        // there: skip instead of failing on a machine whose TEMP is a network share.
        Assume.That(System.IO.Path.GetTempPath().StartsWith(@"\\") == false,
            "UNC temp directories are excluded by the local-path probe this test covers");

        var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ss-app-rule-" + Guid.NewGuid().ToString("N"));
        var path = System.IO.Path.Combine(directory, "C++", "app.exe");
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        System.IO.File.WriteAllText(path, string.Empty);
        try
        {
            AppRuleAssignmentValidator.HasPatternSyntax(path).Should().BeFalse(
                "a path naming an existing file is concrete even when its name carries a regex operator");
        }
        finally
        {
            System.IO.Directory.Delete(directory, true);
        }
    }
}
