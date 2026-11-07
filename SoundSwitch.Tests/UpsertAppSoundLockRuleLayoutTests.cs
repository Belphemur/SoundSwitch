using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using FluentAssertions;
using NUnit.Framework;

using SoundSwitch.UI.Forms;

namespace SoundSwitch.Tests;

/// <summary>
/// Scaling, resizing and accessibility invariants for the app-rules editor
/// (<see cref="UpsertAppSoundLockRule"/>, GitHub issue #2570).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="UpsertAppSoundLockRuleAppSettingsTestsFormConfig"/> builds the real form (the
/// constructor needs no <c>AppModel</c>) on an STA thread and asserts the designer contract.
/// </para>
/// <para>
/// <see cref="UpsertAppSoundLockRuleComputeModeLayoutTests"/> drives the pure
/// <c>ComputeModeLayout</c> helper the form's <c>ApplyModeLayout</c> consumes, over the designer
/// baseline (560×372 Automatic / 560×470 Hotkey, 96 DPI) at the scalings Windows offers. The
/// baseline record mirrors UpsertAppSoundLockRule.Designer.cs: the buttons bottom-right
/// (Cancel at 459, Save at 378, both 25 tall and 75 wide), the checkbox row above them
/// (110/210/340), the hotkey group at 12,168 with 12px side margins ending 84px above the
/// hotkey-mode client bottom.
/// </para>
/// </remarks>
[TestFixture]
public class UpsertAppSoundLockRuleLayoutTests
{
    private const double DESIGN_DPI = 96;

    private static readonly int[] ClientWidths = [560, 640, 760];

    public static double[] Scales() => [1.0, 1.25, 1.5, 1.75, 2.0];

    private static int Scale(double value, double scale) =>
        (int)Math.Round(value * scale, MidpointRounding.AwayFromZero);

    private static UpsertAppSoundLockRule.AppRuleDialogLayout Compute(bool hotkeyMode, double scale, int clientWidth) =>
        UpsertAppSoundLockRule.ComputeModeLayout(
            hotkeyMode,
            scale,
            clientWidth,
            hotkeyMode ? Scale(470, scale) : Scale(372, scale),
            Scale(75, scale),
            Scale(75, scale));

    [TestCaseSource(nameof(Scales))]
    public void ComputeModeLayoutNeverShrinksAUserEnlargedWindow(double scale)
    {
        var clientWidth = Scale(720, scale);
        var layout = Compute(hotkeyMode: false, scale, clientWidth);

        layout.ClientSize.Width.Should().Be(clientWidth);
        layout.ClientSize.Height.Should().Be(Scale(372, scale));
    }

    [TestCaseSource(nameof(Scales))]
    public void SaveAndCancelKeepTheDesignerMarginsAtEveryScale(double scale)
    {
        foreach (var hotkeyMode in new[] { false, true })
        foreach (var width in ClientWidths)
        {
            var layout = Compute(hotkeyMode, scale, Scale(width, scale));

            // Cancel: 26px (design) from the right edge; Save sits 6px left of Cancel.
            layout.CancelButtonLocation.X.Should().Be(
                layout.ClientSize.Width - Scale(26, scale) - Scale(75, scale));
            layout.SaveButtonLocation.X.Should().Be(
                layout.CancelButtonLocation.X - Scale(6, scale) - Scale(75, scale));
            layout.SaveButtonLocation.X.Should().BeGreaterThanOrEqualTo(Scale(110, scale));
        }
    }

    [TestCaseSource(nameof(Scales))]
    public void CheckboxRowSitsAboveTheButtonsWithoutOverlap(double scale)
    {
        foreach (var hotkeyMode in new[] { false, true })
        foreach (var width in ClientWidths)
        {
            var layout = Compute(hotkeyMode, scale, Scale(width, scale));

            layout.CheckboxY.Should().BeLessThan(layout.SaveButtonLocation.Y);
            // Checkbox row height is 19 design px; the buttons start right below it.
            (layout.CheckboxY + Scale(19, scale)).Should().BeLessThanOrEqualTo(layout.SaveButtonLocation.Y);
        }
    }

    [TestCaseSource(nameof(Scales))]
    public void CheckboxColumnsKeepTheDesignerSpread(double scale)
    {
        var layout = Compute(hotkeyMode: false, scale, Scale(560, scale));

        layout.CheckboxLeft.Should().Be(Scale(110, scale));
        layout.CheckboxMid.Should().Be(Scale(210, scale));
        layout.CheckboxRight.Should().Be(Scale(340, scale));
    }

    [TestCaseSource(nameof(Scales))]
    public void HotkeyGroupEndsAboveTheCheckboxRow(double scale)
    {
        var layout = Compute(hotkeyMode: true, scale, Scale(560, scale));

        // Design: 12px side margins, 84px between the group's bottom and the client bottom.
        layout.GrpHotkeyLocation.X.Should().Be(Scale(12, scale));
        layout.GrpHotkeySize.Width.Should().Be(layout.ClientSize.Width - Scale(24, scale));
        (layout.GrpHotkeyLocation.Y + layout.GrpHotkeySize.Height)
            .Should().Be(layout.ClientSize.Height - Scale(84, scale));
        layout.GrpHotkeyLocation.Y.Should().Be(Scale(168, scale));
    }

    [TestCaseSource(nameof(Scales))]
    public void ClientSizeNeverFallsBelowTheDesignSizeForTheMode(double scale)
    {
        Compute(hotkeyMode: false, scale, Scale(560, scale)).ClientSize
            .Should().Be(new Size(Scale(560, scale), Scale(372, scale)));
        Compute(hotkeyMode: true, scale, Scale(560, scale)).ClientSize
            .Should().Be(new Size(Scale(560, scale), Scale(470, scale)));
    }

    [Test]
    public void FormIsDpiScaledResizableAndWithinItsMinimum()
    {
        RunSta(() =>
        {
            using var form = CreateRuleDialog();

            form.AutoScaleMode.Should().Be(AutoScaleMode.Dpi);
            form.AutoScaleDimensions.Should().Be(new SizeF(96f, 96f));
            form.FormBorderStyle.Should().Be(FormBorderStyle.Sizable);
            form.MaximizeBox.Should().BeFalse();
            form.MinimizeBox.Should().BeFalse();
            form.MinimumSize.Width.Should().BeGreaterThanOrEqualTo(form.Width);
            form.MinimumSize.Height.Should().BeGreaterThanOrEqualTo(form.Height);
        });
    }

    [Test]
    public void ImageOnlyControlsHaveAccessibleNames()
    {
        RunSta(() =>
        {
            using var form = CreateRuleDialog();

            form.SelectProcessButton.AccessibleName.Should().NotBeNullOrEmpty();
            form.PlaybackResetButton.AccessibleName.Should().NotBeNullOrEmpty();
            form.RecordingResetButton.AccessibleName.Should().NotBeNullOrEmpty();
            form.HotKeyResetButton.AccessibleName.Should().NotBeNullOrEmpty();
        });
    }

    [Test]
    public void ModeSwitchAfterResizeKeepsTheUserWidthAndBottomAnchors()
    {
        RunSta(() =>
        {
            using var form = CreateRuleDialog(formSize: new Size(760, 372));

            form.ClientSize.Width.Should().Be(760);

            form.HotkeyModeRadio.Checked = true;
            form.ApplyModeLayout();

            form.ClientSize.Width.Should().Be(760);

            // Cancel stays 26 design px from the right edge; group spans the width.
            form.CancelButtonControl.Location.X.Should().Be(
                form.ClientSize.Width - 26 - form.CancelButtonControl.Width);
            form.HotkeyGroup.Width.Should().Be(form.ClientSize.Width - 24);
            (form.HotkeyGroup.Top + form.HotkeyGroup.Height)
                .Should().Be(form.ClientSize.Height - 84);

            form.AutomaticModeRadio.Checked = true;
            form.ApplyModeLayout();

            form.ClientSize.Width.Should().Be(760);
            form.CancelButtonControl.Location.X.Should().Be(
                form.ClientSize.Width - 26 - form.CancelButtonControl.Width);
        });
    }

    private static UpsertAppSoundLockRule CreateRuleDialog(Size? formSize = null)
    {
        var form = new UpsertAppSoundLockRule(
            new SoundSwitch.Model.AppSoundRule(),
            [], [],
            editing: false);
        if (formSize.HasValue)
        {
            form.ClientSize = formSize.Value;
            form.ApplyModeLayout();
        }
        return form;
    }

    private static void RunSta(Action action)
    {
        var thread = new Thread(action.Invoke) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
