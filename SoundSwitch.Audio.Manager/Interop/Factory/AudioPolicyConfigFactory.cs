using SoundSwitch.Audio.Manager.Interop.Com.Base;
using SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended;
using WinRT;

namespace SoundSwitch.Audio.Manager.Interop.Factory
{
    internal sealed class AudioPolicyConfigFactory
    {
        public static IAudioPolicyConfigFactory Create()
        {
            var iid = GuidGenerator.CreateIID(typeof(IAudioPolicyConfigFactory));
            using var name = HSTRING.FromString("Windows.Media.Internal.AudioPolicyConfig");
            ComBase.RoGetActivationFactory(name, ref iid, out object factory);
            return factory.As<IAudioPolicyConfigFactory>();
        }
    }
}