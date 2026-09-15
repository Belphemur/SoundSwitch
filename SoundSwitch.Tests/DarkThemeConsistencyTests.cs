using System.Drawing;
using System.Windows.Forms;

using NUnit.Framework;

using SoundSwitch.UI.Component;
using SoundSwitch.UI.Forms;

namespace SoundSwitch.Tests;

[TestFixture]
public class DarkThemeConsistencyTests
{
    [Test]
    public void HotKeyTextBox_ValidColor_LightKeepsGreen()
    {
        Assert.That(HotKeyTextBox.ValidColor(false), Is.EqualTo(Color.Green));
    }

    [Test]
    public void HotKeyTextBox_ValidColor_DarkIsLegibleLightGreen()
    {
        Assert.That(HotKeyTextBox.ValidColor(true), Is.EqualTo(Color.FromArgb(102, 187, 106)));
    }

    [Test]
    public void HotKeyTextBox_InvalidColor_LightKeepsCrimson()
    {
        Assert.That(HotKeyTextBox.InvalidColor(false), Is.EqualTo(Color.Crimson));
    }

    [Test]
    public void HotKeyTextBox_InvalidColor_DarkIsLegibleSalmon()
    {
        Assert.That(HotKeyTextBox.InvalidColor(true), Is.EqualTo(Color.FromArgb(255, 107, 107)));
    }

    [Test]
    public void ChangelogWebViewer_LightStyleSheet_KeepsOriginalLightColors()
    {
        var css = ChangelogWebViewer.GetStyleSheet(false);
        Assert.That(css, Does.Contain("background: #fff"));
        Assert.That(css, Does.Contain("#eaecef"));
        Assert.That(css, Does.Not.Contain("color:"));
    }

    [Test]
    public void ChangelogWebViewer_DarkStyleSheet_UsesDarkPalette()
    {
        var css = ChangelogWebViewer.GetStyleSheet(true);
        Assert.That(css, Does.Contain("background: #202020"));
        Assert.That(css, Does.Contain("color: #e0e0e0"));
        Assert.That(css, Does.Contain("#3f3f3f"));
        Assert.That(css, Does.Contain("#6cb2f5"));
        Assert.That(css, Does.Not.Contain("#eaecef"));
    }

    [Test]
    public void ProcessSelectionForm_ApplyTheme_DarkAppliesDarkGridPalette()
    {
        using var dgv = new DataGridView();

        ProcessSelectionForm.ApplyTheme(dgv, true);

        Assert.That(dgv.BackgroundColor, Is.EqualTo(ProcessSelectionForm.DarkGridBackgroundColor));
        Assert.That(dgv.GridColor, Is.EqualTo(ProcessSelectionForm.DarkGridLineColor));
        Assert.That(dgv.DefaultCellStyle.BackColor, Is.EqualTo(ProcessSelectionForm.DarkGridBackgroundColor));
        Assert.That(dgv.DefaultCellStyle.ForeColor, Is.EqualTo(ProcessSelectionForm.DarkGridForegroundColor));
        Assert.That(dgv.ColumnHeadersDefaultCellStyle.BackColor, Is.EqualTo(ProcessSelectionForm.DarkGridHeaderColor));
        Assert.That(dgv.EnableHeadersVisualStyles, Is.False);
    }

    [Test]
    public void ProcessSelectionForm_ApplyTheme_LightRestoresStandardPalette()
    {
        using var dgv = new DataGridView();

        ProcessSelectionForm.ApplyTheme(dgv, false);

        Assert.That(dgv.EnableHeadersVisualStyles, Is.True);
        Assert.That(dgv.DefaultCellStyle.BackColor, Is.EqualTo(SystemColors.Window));
        Assert.That(dgv.DefaultCellStyle.ForeColor, Is.EqualTo(SystemColors.ControlText));
    }
}
