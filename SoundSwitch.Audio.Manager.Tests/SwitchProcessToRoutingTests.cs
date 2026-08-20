using System;
using System.Runtime.InteropServices;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Client;
using SoundSwitch.Audio.Manager.Interop.Client.Extended;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended;

namespace SoundSwitch.Audio.Manager.Tests;

[TestFixture]
public sealed class SwitchProcessToRoutingTests
{
    private enum FakeMode
    {
        ReturnTrue,
        ReturnFalse,
        Throw
    }

    private sealed class FakePolicyConfig : IAudioPolicyConfig
    {
        private readonly FakeMode _mode;
        private readonly string _getResult;

        public FakePolicyConfig(FakeMode mode, string getResult = null)
        {
            _mode = mode;
            _getResult = getResult;
        }

        public bool SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, string deviceId)
        {
            switch (_mode)
            {
                case FakeMode.ReturnTrue:
                    return true;
                case FakeMode.ReturnFalse:
                    return false;
                case FakeMode.Throw:
                    throw new InvalidComObjectException("forced failure");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role) => _getResult;

        public void ClearAllPersistedApplicationDefaultEndpoints()
        {
        }

        public void Dispose()
        {
        }
    }

    private static uint ProcessId => (uint)Environment.ProcessId;
    private const string DeviceId = "test-device";
    private static readonly ERole[] AllRoles = { ERole.eConsole, ERole.eMultimedia, ERole.eCommunications };

    [Test]
    public void ExtendedPolicyClient_SetDefaultEndPoint_ReturnsTrue_WhenPolicySucceeds()
    {
        var client = new ExtendedPolicyClient(new FakePolicyConfig(FakeMode.ReturnTrue));

        var result = client.SetDefaultEndPoint(DeviceId, EDataFlow.eRender, AllRoles, ProcessId);

        result.Should().BeTrue();
    }

    [Test]
    public void ExtendedPolicyClient_SetDefaultEndPoint_ReturnsFalse_WhenPolicyReturnsFalse()
    {
        var client = new ExtendedPolicyClient(new FakePolicyConfig(FakeMode.ReturnFalse));

        var result = client.SetDefaultEndPoint(DeviceId, EDataFlow.eRender, AllRoles, ProcessId);

        result.Should().BeFalse();
    }

    [Test]
    public void ExtendedPolicyClient_SetDefaultEndPoint_ReturnsFalse_WhenPolicyThrows()
    {
        var client = new ExtendedPolicyClient(new FakePolicyConfig(FakeMode.Throw));

        var result = client.SetDefaultEndPoint(DeviceId, EDataFlow.eRender, AllRoles, ProcessId);

        result.Should().BeFalse();
    }

    [Test]
    public void ExtendedPolicyClient_SetDefaultEndPoint_ReturnsFalse_WhenDeviceIdIsEmpty()
    {
        var client = new ExtendedPolicyClient(new FakePolicyConfig(FakeMode.ReturnTrue));

        var result = client.SetDefaultEndPoint("", EDataFlow.eRender, AllRoles, ProcessId);

        result.Should().BeFalse();
    }

    [Test]
    public void ExtendedPolicyClient_SetDefaultEndPoint_ReturnsFalse_ForUnsupportedPolicy()
    {
        var client = new ExtendedPolicyClient(new UnsupportedAudioPolicyConfig());

        var result = client.SetDefaultEndPoint(DeviceId, EDataFlow.eRender, AllRoles, ProcessId);

        result.Should().BeFalse();
    }

    [Test]
    public void UnsupportedAudioPolicyConfig_SetPersistedDefaultAudioEndpoint_ReturnsFalse()
    {
        IAudioPolicyConfig config = new UnsupportedAudioPolicyConfig();

        var result = config.SetPersistedDefaultAudioEndpoint(ProcessId, EDataFlow.eRender, ERole.eConsole, DeviceId);

        result.Should().BeFalse();
    }

    [Test]
    [Platform("Win")]
    public void SwitchProcessTo_ReturnsFalse_WhenEndpointAlreadySelected()
    {
        var switcher = SoundSwitch.Audio.Manager.AudioSwitcher.Instance;
        switcher.SetExtendedPolicyClientForTest(new ExtendedPolicyClient(new FakePolicyConfig(FakeMode.ReturnTrue, DeviceId)));

        var result = switcher.SwitchProcessTo(DeviceId, ERole.ERole_enum_count, EDataFlow.eRender, ProcessId);

        result.Should().BeFalse();
    }

    [Test]
    [Platform("Win")]
    public void SwitchProcessTo_ReturnsTrue_WhenRoutingSucceeds()
    {
        var switcher = SoundSwitch.Audio.Manager.AudioSwitcher.Instance;
        switcher.SetExtendedPolicyClientForTest(new ExtendedPolicyClient(new FakePolicyConfig(FakeMode.ReturnTrue)));

        var result = switcher.SwitchProcessTo(DeviceId, ERole.ERole_enum_count, EDataFlow.eRender, ProcessId);

        result.Should().BeTrue();
    }

    [Test]
    [Platform("Win")]
    public void SwitchProcessTo_ReturnsFalse_WhenRoutingFails()
    {
        var switcher = SoundSwitch.Audio.Manager.AudioSwitcher.Instance;
        switcher.SetExtendedPolicyClientForTest(new ExtendedPolicyClient(new FakePolicyConfig(FakeMode.ReturnFalse)));

        var result = switcher.SwitchProcessTo(DeviceId, ERole.ERole_enum_count, EDataFlow.eRender, ProcessId);

        result.Should().BeFalse();
    }
}
