using System;
using SoundSwitch.Audio.Manager.Interop.Com.Base;
using SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended;
using WinRT;

namespace SoundSwitch.Audio.Manager.Interop.Factory
{
    internal sealed class AudioPolicyConfigFactory
    {
        private const int OS_21H2_VERSION = 21390;

        public static IAudioPolicyConfigFactory Create()
        {
            using var name = HSTRING.FromString("Windows.Media.Internal.AudioPolicyConfig");

            if (Environment.OSVersion.Version.Build >= OS_21H2_VERSION)
            {
                var iid21H2 = GuidGenerator.CreateIID(typeof(IAudioPolicyConfigFactoryVariant21H2Windows11));
                ComBase.RoGetActivationFactory(name, ref iid21H2, out object factory21H2);
                return factory21H2.As<IAudioPolicyConfigFactoryVariant21H2Windows11>();
            }

            var iid = GuidGenerator.CreateIID(typeof(IAudioPolicyConfigFactoryWindows10Pre21H2));
            ComBase.RoGetActivationFactory(name, ref iid, out object factory);
            return factory.As<IAudioPolicyConfigFactoryWindows10Pre21H2>();
        }
    }
}