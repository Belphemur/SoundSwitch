using System;
using System.Runtime.InteropServices;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Tests;

/// <summary>
/// Pins the HRESULT surface of the playback path: the numeric contract of the HRESULT enum and
/// how <see cref="AudioDeviceException"/> carries a status code — including the "Windows audio
/// service not running" detection shared with raw COM failures.
/// </summary>
[TestFixture]
public sealed class AudioDeviceExceptionTests
{
    [Test]
    public void HresultConstants_KeepNativeValues()
    {
        ((uint)HRESULT.S_OK).Should().Be(0x0u);
        ((uint)HRESULT.S_FALSE).Should().Be(0x1u);
        ((uint)HRESULT.AUDCLNT_E_DEVICE_INVALIDATED).Should().Be(0x88890004u);
        ((uint)HRESULT.AUDCLNT_E_SERVICE_NOT_RUNNING).Should().Be(0x88890010u);
        ((uint)HRESULT.AUDCLNT_S_NO_SINGLE_PROCESS).Should().Be(0x889000du);
        ((uint)HRESULT.ERROR_NOT_FOUND).Should().Be(0x80070490u);
        ((uint)HRESULT.PROCESS_NO_AUDIO).Should().Be(0x80070057u);
    }

    [Test]
    public void Constructor_CarriesStatusInPropertyAndHResult()
    {
        var exception = new AudioDeviceException(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED, "Initialize failed");

        exception.Status.Should().Be(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED);
        exception.HResult.Should().Be(unchecked((int)0x88890004));
        exception.Message.Should().Be("Initialize failed (HRESULT 0x88890004)");
    }

    [Test]
    public void Constructor_PreservesInnerException()
    {
        var inner = new InvalidOperationException("boom");

        var exception = new AudioDeviceException(HRESULT.PROCESS_NO_AUDIO, "Play", inner);

        exception.InnerException.Should().Be(inner);
    }

    [Test]
    public void FromHResult_DescribesOperationAndStatus()
    {
        var exception = AudioDeviceException.FromHResult(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED, "Initialize");

        exception.Status.Should().Be(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED);
        exception.Message.Should().Be("Initialize failed (HRESULT 0x88890004)");
    }

    [Test]
    public void ServiceNotRunning_UsesAudclntServiceNotRunningStatus()
    {
        var exception = AudioDeviceException.ServiceNotRunning("Initialize");

        exception.Status.Should().Be(HRESULT.AUDCLNT_E_SERVICE_NOT_RUNNING);
        exception.HResult.Should().Be(unchecked((int)0x88890010));
        exception.Message.Should().Contain("the Windows audio service is not running");
    }

    [Test]
    public void IsAudioServiceNotRunning_TrueForMatchingAudioDeviceException()
    {
        AudioDeviceException.IsAudioServiceNotRunning(AudioDeviceException.ServiceNotRunning("Initialize")).Should().BeTrue();
    }

    [Test]
    public void IsAudioServiceNotRunning_TrueForRawComExceptionWithSameHResult()
    {
        // A raw COM failure surfacing AUDCLNT_E_SERVICE_NOT_RUNNING must be recognized too — the
        // caller has no AudioDeviceException to unwrap in that case.
        var comFailure = new COMException("audio service down", unchecked((int)0x88890010));

        AudioDeviceException.IsAudioServiceNotRunning(comFailure).Should().BeTrue();
    }

    [Test]
    public void IsAudioServiceNotRunning_FalseForUnrelatedFailures()
    {
        AudioDeviceException.IsAudioServiceNotRunning(AudioDeviceException.FromHResult(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED, "Initialize")).Should().BeFalse();
        AudioDeviceException.IsAudioServiceNotRunning(new COMException("other", unchecked((int)0x88890004))).Should().BeFalse();
        AudioDeviceException.IsAudioServiceNotRunning(new InvalidOperationException()).Should().BeFalse();
        AudioDeviceException.IsAudioServiceNotRunning(null).Should().BeFalse();
    }
}
