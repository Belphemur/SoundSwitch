using System.Runtime.InteropServices;
using SoundSwitch.Audio.Manager.Interop.Com.Base;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended;
using WinRT;

namespace SoundSwitch.Audio.Manager.Interop.Client.Extended;

public class Pre21H2AudioPolicyConfig : IAudioPolicyConfig
{
    private readonly IAudioPolicyConfigFactoryWindows10Pre21H2 _factory;

    public Pre21H2AudioPolicyConfig()
    {
        using var name = HSTRING.FromString("Windows.Media.Internal.AudioPolicyConfig");

        var iid = GuidGenerator.CreateIID(typeof(IAudioPolicyConfigFactoryWindows10Pre21H2));
        ComBase.RoGetActivationFactory(name, ref iid, out object factory);
        _factory = factory.As<IAudioPolicyConfigFactoryWindows10Pre21H2>();
    }

    public void SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, string deviceId)
    {
        using var deviceIdHString = HSTRING.FromString(deviceId);
        Marshal.ThrowExceptionForHR((int)_factory.SetPersistedDefaultAudioEndpoint(processId, flow, role, deviceIdHString));
    }

    public string GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role)
    {
        _factory.GetPersistedDefaultAudioEndpoint(processId, flow, role, out var deviceId);
        var deviceIdStr = deviceId.ToString();
        deviceId.Dispose();
        return deviceIdStr;
    }

    public void ClearAllPersistedApplicationDefaultEndpoints()
    {
        Marshal.ThrowExceptionForHR((int)_factory.ClearAllPersistedApplicationDefaultEndpoints());
    }
}