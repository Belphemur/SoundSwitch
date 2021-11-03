using System;
using SoundSwitch.Audio.Manager.Interop.Com.Threading;
using SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended;

namespace SoundSwitch.Audio.Manager.Interop.Client.Extended.Factory
{
    internal static class AudioPolicyConfigFactory
    {
        private const int OS_21H2_VERSION = 21390;
        private const int OS_1709_VERSION = 16299;

        public static IAudioPolicyConfig Create()
        {
            return ComThread.Invoke<IAudioPolicyConfig>(() =>
            {
                if (Environment.OSVersion.Version.Major < 10)
                {
                    return new UnsupportedAudioPolicyConfig();
                }

                return Environment.OSVersion.Version.Build switch
                {
                    <= OS_1709_VERSION => new UnsupportedAudioPolicyConfig(),
                    >= OS_21H2_VERSION => new Post21H2AudioPolicyConfig(),
                    _                  => new Pre21H2AudioPolicyConfig()
                };
            });
        }
    }
}