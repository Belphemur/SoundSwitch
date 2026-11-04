using System;
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
    public void Validate_DynamicRule_AlwaysValid()
    {
        var rule = new AppSoundRule
        {
            Enforcement = AppRuleEnforcement.Dynamic,
            ProcessPath = ".*Spotify.*",
            WindowName = ".*YouTube.*"
        };

        AppRuleAssignmentValidator.Validate(rule).Should().Be(AppRuleAssignmentValidator.Error.None);
    }

    [Test]
    public void Validate_AssignmentWithConcretePath_NoWindowPattern_IsValid()
    {
        var rule = new AppSoundRule
        {
            Enforcement = AppRuleEnforcement.Assignment,
            ProcessPath = @"C:\Apps\Spotify\Spotify.exe"
        };

        AppRuleAssignmentValidator.Validate(rule).Should().Be(AppRuleAssignmentValidator.Error.None);
    }

    [Test]
    public void Validate_AssignmentWithRegexPattern_NeedsIdentity()
    {
        var rule = new AppSoundRule
        {
            Enforcement = AppRuleEnforcement.Assignment,
            ProcessPath = ".*Spotify.exe.*"
        };

        AppRuleAssignmentValidator.Validate(rule).Should().Be(AppRuleAssignmentValidator.Error.NeedsIdentity);
    }

    [Test]
    public void Validate_AssignmentWithGlobPattern_NeedsIdentity()
    {
        var rule = new AppSoundRule
        {
            Enforcement = AppRuleEnforcement.Assignment,
            ProcessPath = @"*\Spotify\*"
        };

        AppRuleAssignmentValidator.Validate(rule).Should().Be(AppRuleAssignmentValidator.Error.NeedsIdentity);
    }

    [Test]
    public void Validate_AssignmentWithEmptyProcess_NeedsIdentity()
    {
        var rule = new AppSoundRule { Enforcement = AppRuleEnforcement.Assignment, ProcessPath = "" };

        AppRuleAssignmentValidator.Validate(rule).Should().Be(AppRuleAssignmentValidator.Error.NeedsIdentity);
    }

    [Test]
    public void Validate_AssignmentWithWindowPattern_NoWindowTitle()
    {
        var rule = new AppSoundRule
        {
            Enforcement = AppRuleEnforcement.Assignment,
            ProcessPath = @"C:\Apps\Spotify\Spotify.exe",
            WindowName = ".*Chat.*"
        };

        AppRuleAssignmentValidator.Validate(rule).Should().Be(AppRuleAssignmentValidator.Error.NoWindowTitle);
    }

    [TestCase(@"C:\Apps\My App (1)\App.exe", false)]
    [TestCase(@"C:\Apps\My [a]\App.exe", true)]
    [TestCase(@".*.exe", true)]
    [TestCase(@"C:\Apps\Spotify\Spotify.exe", false)]
    public void HasPatternSyntax_DetectsRegexFragmentsOnly(string processPath, bool expected)
    {
        AppRuleAssignmentValidator.HasPatternSyntax(processPath).Should().Be(expected);
    }
}
