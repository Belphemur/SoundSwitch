using System;
using System.Collections.Generic;

using FluentAssertions;

using NAudio.CoreAudioApi;

using NUnit.Framework;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.DeviceCyclerManager.DeviceCycler;

namespace SoundSwitch.Tests;

[TestFixture]
public class DeviceCyclerTests
{
    private class TestDeviceCycler : ADeviceCycler
    {
        private readonly List<DeviceFullInfo> _devices;

        public override DeviceCyclerType TypeEnum => DeviceCyclerType.Available;
        public override string Label => "Test Cycler";

        public TestDeviceCycler(List<DeviceFullInfo> devices)
        {
            _devices = devices;
        }

        protected override IEnumerable<DeviceFullInfo> GetDevices(DataFlow type)
        {
            return _devices;
        }
    }

    [Test]
    public void TestCycleAudioDevice_WhenDeviceIsAlreadyDefault_ReturnsFalse()
    {
        if (Environment.GetEnvironmentVariable("CI") != null)
        {
            Assert.Ignore("CI doesn't have audio device to make this test work");
        }

        using var enumerator = AudioSwitcher.Instance.GetAudioEndpoints(EDataFlow.eRender, EDeviceState.Active).GetEnumerator();
        if (!enumerator.MoveNext())
        {
            Assert.Ignore("No active playback devices found to run the test");
        }

        var singleDevice = enumerator.Current;
        var cycler = new TestDeviceCycler(new List<DeviceFullInfo> { singleDevice });
        using var originalDefault = AudioSwitcher.Instance.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole);

        try
        {
            // Arrange: force singleDevice to be the system default
            AudioSwitcher.Instance.SwitchTo(singleDevice.Id, ERole.eConsole);
            AudioSwitcher.Instance.SwitchTo(singleDevice.Id, ERole.eMultimedia);

            // Act & Assert: device is already the default, so cycling should return false
            cycler.CycleAudioDevice(DataFlow.Render).Should().BeFalse();

            // Assert default endpoint ID is unchanged
            using var currentDefault = AudioSwitcher.Instance.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole);
            currentDefault?.Id.Should().Be(singleDevice.Id);
        }
        finally
        {
            if (originalDefault != null)
            {
                AudioSwitcher.Instance.SwitchTo(originalDefault.Id, ERole.eConsole);
                AudioSwitcher.Instance.SwitchTo(originalDefault.Id, ERole.eMultimedia);
            }
        }
    }

    [Test]
    public void TestCycleAudioDevice_WhenDeviceIsNotDefault_ReturnsTrueAndSetsDefault()
    {
        if (Environment.GetEnvironmentVariable("CI") != null)
        {
            Assert.Ignore("CI doesn't have audio device to make this test work");
        }

        using var enumerator = AudioSwitcher.Instance.GetAudioEndpoints(EDataFlow.eRender, EDeviceState.Active).GetEnumerator();
        if (!enumerator.MoveNext())
        {
            Assert.Ignore("No active playback devices found to run the test");
        }

        var singleDevice = enumerator.Current;

        if (!enumerator.MoveNext())
        {
            Assert.Ignore("Need at least 2 active playback devices to test the mismatched-default scenario");
        }

        var otherDevice = enumerator.Current;
        var cycler = new TestDeviceCycler(new List<DeviceFullInfo> { singleDevice });
        using var originalDefault = AudioSwitcher.Instance.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole);

        try
        {
            // Arrange: make otherDevice the system default so singleDevice is not the default
            AudioSwitcher.Instance.SwitchTo(otherDevice.Id, ERole.eConsole);
            AudioSwitcher.Instance.SwitchTo(otherDevice.Id, ERole.eMultimedia);

            // Act & Assert: singleDevice is not the default, so cycling should activate it and return true
            cycler.CycleAudioDevice(DataFlow.Render).Should().BeTrue();

            // Assert default endpoint is now singleDevice
            using var currentDefault = AudioSwitcher.Instance.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole);
            currentDefault?.Id.Should().Be(singleDevice.Id);
        }
        finally
        {
            if (originalDefault != null)
            {
                AudioSwitcher.Instance.SwitchTo(originalDefault.Id, ERole.eConsole);
                AudioSwitcher.Instance.SwitchTo(originalDefault.Id, ERole.eMultimedia);
            }
        }
    }
}
