using FluentAssertions;
using NUnit.Framework;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.TrayIcon;

namespace SoundSwitch.Tests;

[TestFixture]
public class DeviceFormFactorDetectorTests
{
    private static DeviceFullInfo Device(string name, EDataFlow type, string iconPath = "")
    {
        return new DeviceFullInfo(name, "{00000000-0000-0000-0000-000000000000}", type, iconPath, EDeviceState.Active, false);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(@"C:\Windows\System32\mmres.dll,-5004")]
    public void RenderDevice_WithSpeakerSignals_IsSpeaker(string iconPath)
    {
        DeviceFormFactorDetector.From(Device("Speakers", EDataFlow.eRender, iconPath))
            .Should().Be(IconKind.Speaker);
    }

    [TestCase(@"C:\Windows\System32\mmres.dll,-5005")]
    [TestCase("mmres.dll,-5005")]
    public void RenderDevice_WithHeadphoneIconPath_IsHeadphone(string iconPath)
    {
        DeviceFormFactorDetector.From(Device("Audio Device", EDataFlow.eRender, iconPath))
            .Should().Be(IconKind.Headphone);
    }

    [TestCase(@"C:\Windows\System32\mmres.dll,-5051")]
    [TestCase(@"C:\Windows\System32\mmres.dll,-5044")]
    [TestCase(@"C:\Windows\System32\mmres.dll,-5052")]
    public void RenderDevice_WithHeadsetIconPath_IsHeadset(string iconPath)
    {
        DeviceFormFactorDetector.From(Device("Audio Device", EDataFlow.eRender, iconPath))
            .Should().Be(IconKind.Headset);
    }

    [Test]
    public void RenderDevice_WithUnmappedIconPath_FallsBackToNameClean()
    {
        DeviceFormFactorDetector.From(Device("Speakers", EDataFlow.eRender, @"C:\Windows\System32\mmres.dll,-6000"))
            .Should().Be(IconKind.Speaker);
    }

    [Test]
    public void CaptureDevice_IsMicrophone()
    {
        DeviceFormFactorDetector.From(Device("Microphone", EDataFlow.eCapture, @"C:\Windows\System32\mmres.dll,-5004"))
            .Should().Be(IconKind.Microphone);
    }

    [TestCase("WH-1000XM4")]
    [TestCase("AirPods Pro")]
    [TestCase("QC35")]
    [TestCase("Earbuds")]
    public void RenderDevice_WithHeadphoneNameClean_IsHeadphone(string name)
    {
        DeviceFormFactorDetector.From(Device(name, EDataFlow.eRender))
            .Should().Be(IconKind.Headphone);
    }

    [Test]
    public void RenderDevice_WithHeadsetNameClean_IsHeadset()
    {
        DeviceFormFactorDetector.From(Device("Bose Headset 700", EDataFlow.eRender))
            .Should().Be(IconKind.Headset);
    }

    [Test]
    public void RenderDevice_WithUnmatchedNameClean_IsSpeaker()
    {
        DeviceFormFactorDetector.From(Device("Realtek HD Audio", EDataFlow.eRender))
            .Should().Be(IconKind.Speaker);
    }

    [Test]
    public void NullDevice_IsSpeaker()
    {
        DeviceFormFactorDetector.From(null)
            .Should().Be(IconKind.Speaker);
    }
}
