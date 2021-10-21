using System.Runtime.InteropServices;
using SoundSwitch.Audio.Manager.Interop.Com.Base;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended;
using WinRT;

namespace SoundSwitch.Audio.Manager.Interop.Client.Extended;

public class Post21H2AudioPolicyConfig : IAudioPolicyConfig
{
    private readonly IAudioPolicyConfigFactoryVariant21H2Windows11 _factory;

    public Post21H2AudioPolicyConfig()
    {
        using var name = HSTRING.FromString("Windows.Media.Internal.AudioPolicyConfig");

        var iid = GuidGenerator.CreateIID(typeof(IAudioPolicyConfigFactoryVariant21H2Windows11));
        ComBase.RoGetActivationFactory(name, ref iid, out object factory);
        _factory = factory.As<IAudioPolicyConfigFactoryVariant21H2Windows11>();
    }

    public void SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, string deviceId)
    {
        using var deviceIdHString = HSTRING.FromString(deviceId);
        var result = _factory.SetPersistedDefaultAudioEndpoint(processId, flow, role, deviceIdHString);
        if (result != HRESULT.S_OK && result != HRESULT.PROCESS_NO_AUDIO)
        {
            throw new InvalidComObjectException($"Can't set the persistent audio endpoint: {result}");
        }
    }

    public string GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role)
    {
        var result = _factory.GetPersistedDefaultAudioEndpoint(processId, flow, role, out var deviceId);
        if (result != HRESULT.S_OK)
        {
            if (result != HRESULT.PROCESS_NO_AUDIO)
            {
                throw new InvalidComObjectException($"Can't set the persistent audio endpoint: {result}");
            }

            return null;
        }

        var deviceIdStr = deviceId.ToString();
        deviceId.Dispose();
        return deviceIdStr;
    }

    public void ClearAllPersistedApplicationDefaultEndpoints()
    {
        var result = _factory.ClearAllPersistedApplicationDefaultEndpoints();
        if (result != HRESULT.S_OK)
        {
            throw new InvalidComObjectException($"Reset audio endpoints: {result}");
        }
    }
}