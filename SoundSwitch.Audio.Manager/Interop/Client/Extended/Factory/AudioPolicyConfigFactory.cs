using System;
using SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended;

namespace SoundSwitch.Audio.Manager.Interop.Client.Extended.Factory
{
    internal sealed class AudioPolicyConfigFactory
    {
        private const int OS_21H2_VERSION = 21390;

        public static IAudioPolicyConfig CreatePre21H2()
        {
            if (Environment.OSVersion.Version.Build >= OS_21H2_VERSION)
            {
                return new Post21H2AudioPolicyConfig();
            }

            return new Pre21H2AudioPolicyConfig();
        }
    }
}