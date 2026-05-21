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
}
