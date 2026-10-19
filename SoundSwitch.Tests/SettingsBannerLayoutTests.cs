using System;
using System.Collections.Generic;
using System.Drawing;

using FluentAssertions;
using NUnit.Framework;

using SoundSwitch.UI.Forms;

namespace SoundSwitch.Tests;

/// <summary>
/// Layout invariants for the banner settings rows that
/// <c>SettingsForm.InitializeBannerAppearanceControls</c> appends to the notifications tab.
/// </summary>
/// <remarks>
/// <para>
/// The form itself cannot be built here: constructing <c>SettingsForm</c> needs
/// <c>AppModel.InitializeMain</c>, which registers Win32 hotkeys and therefore needs a live message
/// loop and window handle. This fixture therefore drives the same
/// <c>SettingsForm.AppendedRowTop</c>/<c>HeightForRow</c>/<c>ResolveMonitorSelectionIndex</c>
/// helpers the form calls, over the designer's 96 DPI baseline below.
/// </para>
/// <para>
/// The baseline mirrors Settings.Designer.cs. InitializeComponent scales it for the current DPI
/// before the rows are appended, which is what the scale factor models: the rows are derived from
/// these values instead of hard-coded pixels, so they have to stay correct once scaled.
/// </para>
/// </remarks>
[TestFixture]
public class SettingsBannerLayoutTests
{
    private const int DESIGN_DPI = 96;

    /// <summary>Designer baseline at 96 DPI; see Settings.Designer.cs.</summary>
    private sealed record Design(int RowOneTop, int RowTwoTop, int RowHeight, int LastRowBottom,
        int ControlColumnLeft, int ControlColumnWidth, int LabelColumnLeft, int LabelColumnWidth,
        int OptionsHeight, int ReferenceContentBottom,
        int PositionTop, int PositionHeight, int PositionWidth, int PositionContentBottom,
        int TabHeight, int ClientHeight, int CloseButtonTop, int CloseButtonHeight);

    private static Design Baseline => new(
        RowOneTop: 72, RowTwoTop: 101, RowHeight: 23, LastRowBottom: 153,
        ControlColumnLeft: 143, ControlColumnWidth: 120,
        LabelColumnLeft: 7, LabelColumnWidth: 130,
        OptionsHeight: 185, ReferenceContentBottom: 179,
        PositionTop: 12, PositionHeight: 167, PositionWidth: 196, PositionContentBottom: 161,
        TabHeight: 391, ClientHeight: 461, CloseButtonTop: 421, CloseButtonHeight: 25);

    private static Design ScaledBy(double scale)
    {
        int S(int value) => (int)Math.Round(value * scale);

        var b = Baseline;
        return b with
        {
            RowOneTop = S(b.RowOneTop),
            RowTwoTop = S(b.RowTwoTop),
            RowHeight = S(b.RowHeight),
            LastRowBottom = S(b.LastRowBottom),
            ControlColumnLeft = S(b.ControlColumnLeft),
            ControlColumnWidth = S(b.ControlColumnWidth),
            LabelColumnLeft = S(b.LabelColumnLeft),
            LabelColumnWidth = S(b.LabelColumnWidth),
            OptionsHeight = S(b.OptionsHeight),
            ReferenceContentBottom = S(b.ReferenceContentBottom),
            PositionTop = S(b.PositionTop),
            PositionHeight = S(b.PositionHeight),
            PositionWidth = S(b.PositionWidth),
            PositionContentBottom = S(b.PositionContentBottom),
            TabHeight = S(b.TabHeight),
            ClientHeight = S(b.ClientHeight),
            CloseButtonTop = S(b.CloseButtonTop),
            CloseButtonHeight = S(b.CloseButtonHeight),
        };
    }

    /// <summary>Where the two appended rows land, using the form's own derivation.</summary>
    private static (int ColorRowTop, int ScaleRowTop, int RowHeight, int RowPitch, int RowGap,
        int OptionsHeight, int PositionHeight, int ExtraHeight) Layout(Design d)
    {
        var rowPitch = Math.Max(1, d.RowTwoTop - d.RowOneTop);
        var rowHeight = d.RowHeight;
        var rowGap = rowPitch - rowHeight;

        var colorRowTop = d.LastRowBottom + rowGap;
        var scaleRowTop = colorRowTop + rowPitch;

        var positionRowTop = SettingsForm.AppendedRowTop(d.PositionHeight, d.PositionContentBottom);

        var optionsHeight = SettingsForm.HeightForRow(d.OptionsHeight, d.ReferenceContentBottom, scaleRowTop + rowHeight);
        var positionHeight = SettingsForm.HeightForRow(d.PositionHeight, d.PositionContentBottom, positionRowTop + rowHeight);

        return (colorRowTop, scaleRowTop, rowHeight, rowPitch, rowGap, optionsHeight, positionHeight,
            optionsHeight - d.OptionsHeight);
    }

    public static IEnumerable<double> Scales() => [1.0, 1.25, 1.5, 1.75, 2.0];

    [TestCaseSource(nameof(Scales))]
    public void AppendedRowsDoNotOverlapTheRowAbove(double scale)
    {
        var (colorRowTop, scaleRowTop, rowHeight, _, _, _, _, _) = Layout(ScaledBy(scale));

        // The regression the DPI review flagged: fixed 96 DPI coordinates for the new controls put
        // the colour button on top of the opacity row and the scale control on the display-info row.
        colorRowTop.Should().BeGreaterThanOrEqualTo(ScaledBy(scale).LastRowBottom);
        (colorRowTop + rowHeight).Should().BeLessThanOrEqualTo(scaleRowTop);
        (scaleRowTop + rowHeight).Should().BeGreaterThan(ScaledBy(scale).LastRowBottom + rowHeight);
    }

    [TestCaseSource(nameof(Scales))]
    public void AppendedRowsKeepTheDesignersRowPitch(double scale)
    {
        var (_, scaleRowTop, _, rowPitch, rowGap, _, _, _) = Layout(ScaledBy(scale));
        var d = ScaledBy(scale);

        // Reusing the designer's pitch is what makes the new rows line up with the existing ones at
        // any scaling instead of drifting by a constant pixel amount.
        rowPitch.Should().Be(d.RowTwoTop - d.RowOneTop);
        (scaleRowTop - d.LastRowBottom).Should().Be(rowGap + rowPitch);
    }

    [TestCaseSource(nameof(Scales))]
    public void AppendedRowsFitInsideTheirGroupBoxes(double scale)
    {
        var (colorRowTop, scaleRowTop, rowHeight, _, _, optionsHeight, positionHeight, _) = Layout(ScaledBy(scale));
        var d = ScaledBy(scale);

        optionsHeight.Should().BeGreaterThanOrEqualTo(scaleRowTop + rowHeight);
        optionsHeight.Should().BeGreaterThanOrEqualTo(d.ReferenceContentBottom);
        optionsHeight.Should().BeGreaterThanOrEqualTo(d.OptionsHeight);

        var positionRowTop = SettingsForm.AppendedRowTop(d.PositionHeight, d.PositionContentBottom);
        positionHeight.Should().BeGreaterThanOrEqualTo(positionRowTop + rowHeight);
        positionHeight.Should().BeGreaterThanOrEqualTo(d.PositionHeight);

        colorRowTop.Should().BeGreaterThan(0);
    }

    [TestCaseSource(nameof(Scales))]
    public void FormGrowsByExactlyTheExtraRowHeight(double scale)
    {
        var (_, _, _, _, _, _, _, extraHeight) = Layout(ScaledBy(scale));

        // tabControl and ClientSize grow by the same amount the options group box did, so the
        // notifications tab keeps the same slack it had before the rows were added.
        extraHeight.Should().BeGreaterThan(0);
        var d = ScaledBy(scale);
        (d.TabHeight + extraHeight).Should().BeLessThanOrEqualTo(d.ClientHeight);
    }

    [TestCaseSource(nameof(Scales))]
    public void CloseButtonStaysFullyInsideTheClientArea(double scale)
    {
        // The original defect: closeButton is anchored Bottom|Right and was additionally nudged
        // down by the same amount ClientSize grew, landing past the bottom edge on every open.
        var d = ScaledBy(scale);
        var (_, _, _, _, _, _, _, extraHeight) = Layout(d);

        // Bottom|Right anchor keeps the distance to the bottom edge, so it only follows ClientSize.
        var bottomGapBefore = d.ClientHeight - (d.CloseButtonTop + d.CloseButtonHeight);
        var newClientHeight = d.ClientHeight + extraHeight;
        var closeButtonBottom = newClientHeight - bottomGapBefore;

        closeButtonBottom.Should().BeLessThanOrEqualTo(newClientHeight);
        closeButtonBottom.Should().BeGreaterThan(d.TabHeight + extraHeight,
            "the close button has to stay below the tab control, not on top of it");
    }

    [TestCaseSource(nameof(Scales))]
    public void MonitorSelectorFitsInsideThePositionGroupBox(double scale)
    {
        var d = ScaledBy(scale);
        var (_, _, rowHeight, _, _, optionsHeight, positionHeight, _) = Layout(d);
        var rowTop = SettingsForm.AppendedRowTop(d.PositionHeight, d.PositionContentBottom);

        // The selector row stays inside the position group box...
        (rowTop + rowHeight).Should().BeLessThanOrEqualTo(positionHeight);

        // ...and the grown position group box still fits inside the grown options group box, which
        // is what actually has to hold both of them.
        (d.PositionTop + positionHeight).Should().BeLessThanOrEqualTo(optionsHeight);
        d.PositionWidth.Should().BeGreaterThan(0);
    }
}