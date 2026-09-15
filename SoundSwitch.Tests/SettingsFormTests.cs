using System.Drawing;

using NUnit.Framework;

using SoundSwitch.UI.Forms;

namespace SoundSwitch.Tests;

[TestFixture]
public class SettingsFormTests
{
    [TestCase(93, 96, 93)]
    [TestCase(93, 120, 116)]
    [TestCase(93, 144, 140)]
    [TestCase(3, 144, 5)]
    [TestCase(166, 144, 249)]
    public void ScaleNotificationGroupBoxHeight_ShouldScaleWithDeviceDpi(int logicalHeight, int deviceDpi, int expectedHeight)
    {
        Assert.That(SettingsForm.ScaleNotificationGroupBoxHeight(logicalHeight, deviceDpi), Is.EqualTo(expectedHeight));
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
