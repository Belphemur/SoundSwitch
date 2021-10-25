using System;
using SoundSwitch.Audio.Manager.Interop.Com.Threading;
using SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended;

namespace SoundSwitch.Audio.Manager.Interop.Client.Extended.Factory
{
    internal static class AudioPolicyConfigFactory
    {
        private const int OS_21H2_VERSION = 21390;

        public static IAudioPolicyConfig Create()
        {
            return ComThread.Invoke<IAudioPolicyConfig>(() =>
            {
                if (Environment.OSVersion.Version.Major < 10)
                {
                    return new UnsupportedAudioPolicyConfig();
                }

                if (Environment.OSVersion.Version.Build >= OS_21H2_VERSION)
                {
                    return new Post21H2AudioPolicyConfig();
                }

                return new Pre21H2AudioPolicyConfig();
            });
        }
    }
}