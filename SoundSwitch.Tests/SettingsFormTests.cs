using System.Collections.Generic;
using System.Drawing;

using FluentAssertions;
using NUnit.Framework;

using SoundSwitch.UI.Forms;

namespace SoundSwitch.Tests;

[TestFixture]
public class SettingsFormTests
{
    private static readonly string[] TwoDisplays = [@"\\.\DISPLAY1", @"\\.\DISPLAY2"];
    private static readonly string[] OneDisplay = [@"\\.\DISPLAY1"];

    // The banner monitor selector is [cursor, primary, ...connected displays]. Anything outside
    // that range throws ArgumentOutOfRangeException from the form's constructor.

    [Test]
    public void MonitorSelection_DefaultsToCursorWithNothingConfigured()
    {
        SettingsForm.ResolveMonitorSelectionIndex(OneDisplay, null, false).Should().Be(0);
    }

    [Test]
    public void MonitorSelection_HonoursThePrimaryScreenFlagWhenNothingIsPinned()
    {
        SettingsForm.ResolveMonitorSelectionIndex(OneDisplay, null, true).Should().Be(1);
    }

    [Test]
    public void MonitorSelection_SkipsPastThePseudoEntriesForAConnectedDisplay()
    {
        SettingsForm.ResolveMonitorSelectionIndex(TwoDisplays, @"\\.\DISPLAY1", false).Should().Be(2);
        SettingsForm.ResolveMonitorSelectionIndex(TwoDisplays, @"\\.\DISPLAY2", false).Should().Be(3);
    }

    [Test]
    public void MonitorSelection_MatchesThePinnedDisplayIgnoringCase()
    {
        SettingsForm.ResolveMonitorSelectionIndex(TwoDisplays, @"\\.\display2", false).Should().Be(3);
    }

    [Test]
    public void MonitorSelection_PinnedDisplayWinsOverThePrimaryScreenFlag()
    {
        SettingsForm.ResolveMonitorSelectionIndex(TwoDisplays, @"\\.\DISPLAY2", true).Should().Be(3);
    }

    [Test]
    public void MonitorSelection_FallsBackWhenThePinnedDisplayIsGone()
    {
        SettingsForm.ResolveMonitorSelectionIndex(TwoDisplays, @"\\.\DISPLAY9", false).Should().Be(0);
        SettingsForm.ResolveMonitorSelectionIndex(TwoDisplays, @"\\.\DISPLAY9", true).Should().Be(1);
    }

    [TestCaseSource(nameof(DeviceNameScenarios))]
    public void MonitorSelection_AlwaysLandsInsideTheList(
        string[] connectedDeviceNames, string savedDeviceName, bool usePrimaryScreen)
    {
        // Regression: the two-entry offset used to be applied to the 0/1 fallbacks as well, which
        // made a single-display setup with the primary flag on select index 3 of 3 and threw from
        // the Settings constructor, so the form could not be opened at all.
        var itemCount = connectedDeviceNames.Length + 2;

        SettingsForm.ResolveMonitorSelectionIndex(connectedDeviceNames, savedDeviceName, usePrimaryScreen)
            .Should().BeInRange(0, itemCount - 1);
    }

    private static IEnumerable<TestCaseData> DeviceNameScenarios()
    {
        var deviceNames = new[] { null, "", @"\\.\DISPLAY1", @"\\.\DISPLAY2", @"\\.\DISPLAY9" };
        var displayCounts = new[] { 0, 1, 2, 3 };

        foreach (var count in displayCounts)
        {
            var connected = new string[count];
            for (var i = 0; i < count; i++)
                connected[i] = $@"\\.\DISPLAY{i + 1}";

            foreach (var saved in deviceNames)
            foreach (var usePrimary in new[] { false, true })
                yield return new TestCaseData(connected, saved, usePrimary)
                    .SetName($"MonitorSelection_InRange_{count}Displays_{saved ?? "null"}_{usePrimary}");
        }
    }

    [TestCase(93, 96, 93)]
    [TestCase(93, 120, 116)]
    [TestCase(93, 144, 140)]
    [TestCase(3, 144, 5)]
    [TestCase(166, 144, 249)]
    public void ScaleNotificationGroupBoxHeight_ShouldScaleWithDeviceDpi(int logicalHeight, int deviceDpi, int expectedHeight)
    {
        Assert.That(SettingsForm.ScaleNotificationGroupBoxHeight(logicalHeight, deviceDpi), Is.EqualTo(expectedHeight));
    }

    // --- Banner settings row layout -------------------------------------------------
    // The rows appended by InitializeBannerAppearanceControls reuse the designer's own row pitch
    // and border gap, so they follow the DPI scaling InitializeComponent already applied. The
    // properties below are what stop a scaled row from colliding with the content above it.

    [Test]
    public void AppendedRowStartsAtTheGroupsExistingBottomMargin()
    {
        // Group box 167 tall, lowest child ends at 161 -> 6px gap, so the row starts at 167.
        SettingsForm.AppendedRowTop(167, 161).Should().Be(167);
        SettingsForm.BottomMargin(167, 161).Should().Be(6);
    }

    [Test]
    public void AppendedRowStaysBelowTheExistingContent()
    {
        for (var gap = 0; gap <= 20; gap++)
        {
            var contentBottom = 100 + gap * 3;
            var groupBoxHeight = contentBottom + gap;

            SettingsForm.AppendedRowTop(groupBoxHeight, contentBottom)
                .Should().BeGreaterThanOrEqualTo(contentBottom);
        }
    }

    [Test]
    public void HeightForRowKeepsTheSameGapBelowTheNewContent()
    {
        SettingsForm.HeightForRow(167, 161, 190).Should().Be(196);
        SettingsForm.BottomMargin(196, 190).Should().Be(6);
    }

    [Test]
    public void HeightForRowNeverShrinksTheGroupBox()
    {
        // Group box 200 tall, reference content ends at 190, so a row ending at 185 already fits
        // inside the existing padding and the height must stay put.
        SettingsForm.HeightForRow(200, 190, 185).Should().Be(200);
    }

    [Test]
    public void HeightForRowGrowsWhenTheRowGoesPastTheExistingContent()
    {
        // Same group box, but the row now ends below the reference content: it has to be pushed
        // down by the same border gap, which means growing the box.
        SettingsForm.HeightForRow(200, 190, 195).Should().Be(205);
        SettingsForm.BottomMargin(205, 195).Should().Be(10);
    }

    [Test]
    public void HeightForRowAlwaysContainsEveryChild()
    {
        for (var margin = 0; margin <= 12; margin++)
        for (var rowBottom = 150; rowBottom <= 400; rowBottom++)
        {
            var groupBoxHeight = 190 + margin;
            var referenceContentBottom = groupBoxHeight - margin;

            var height = SettingsForm.HeightForRow(groupBoxHeight, referenceContentBottom, rowBottom);

            height.Should().BeGreaterThanOrEqualTo(referenceContentBottom);
            height.Should().BeGreaterThanOrEqualTo(rowBottom);
            height.Should().BeGreaterThanOrEqualTo(groupBoxHeight);
        }
    }

    [Test]
    public void ThemeColors_ForDarkMode_ShouldUseReadableSurfaces()
    {
        Assert.That(SettingsForm.GetThemeTextColor(true), Is.EqualTo(Color.FromArgb(240, 240, 240)));
        Assert.That(SettingsForm.GetNotificationPanelColor(true), Is.EqualTo(Color.FromArgb(32, 32, 32)));
        Assert.That(SettingsForm.GetPreviewFillColor(true), Is.EqualTo(Color.FromArgb(45, 45, 45)));
    }

    [Test]
    public void ThemeColors_ForLightMode_ShouldPreserveExistingSurfaces()
    {
        Assert.That(SettingsForm.GetThemeTextColor(false), Is.EqualTo(SystemColors.ControlText));
        Assert.That(SettingsForm.GetNotificationPanelColor(false), Is.EqualTo(Color.White));
        Assert.That(SettingsForm.GetPreviewFillColor(false), Is.EqualTo(Color.AliceBlue));
    }
}
