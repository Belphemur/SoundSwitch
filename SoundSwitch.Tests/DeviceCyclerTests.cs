using System;
using System.Collections.Generic;
using System.Linq;

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
    public void TestCycleAudioDevice_WithSingleDevice()
    {
        if (Environment.GetEnvironmentVariable("CI") != null)
        {
            Assert.Ignore("CI doesn't have audio device to make this test work");
        }

        var activeEndpoints = AudioSwitcher.Instance.GetAudioEndpoints(EDataFlow.eRender, EDeviceState.Active).ToList();
        if (!activeEndpoints.Any())
        {
            Assert.Ignore("No active playback devices found to run the test");
        }

        var singleDevice = activeEndpoints.First();
        var cycler = new TestDeviceCycler(new List<DeviceFullInfo> { singleDevice });

        using var currentDefaultDevice = AudioSwitcher.Instance.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole);

        if (currentDefaultDevice != null && currentDefaultDevice.Id == singleDevice.Id)
        {
            // If the single device is already default, CycleAudioDevice should return false
            cycler.CycleAudioDevice(DataFlow.Render).Should().BeFalse();
        }
        else
        {
            // If the single device is not default, CycleAudioDevice should set it as active and return true
            cycler.CycleAudioDevice(DataFlow.Render).Should().BeTrue();
        }
    }
}
