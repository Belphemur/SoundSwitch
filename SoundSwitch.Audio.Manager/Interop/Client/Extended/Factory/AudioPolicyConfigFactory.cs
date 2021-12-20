using System;
using SoundSwitch.Audio.Manager.Interop.Com.Threading;
using SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended;

namespace SoundSwitch.Audio.Manager.Interop.Client.Extended.Factory
{
    internal static class AudioPolicyConfigFactory
    {
        internal const int OS_1709_VERSION = 16299;

        public static IAudioPolicyConfig Create()
        {
            return ComThread.Invoke<IAudioPolicyConfig>(() =>
            {
                if (Environment.OSVersion.Version.Major < 10 || Environment.OSVersion.Version.Build <= OS_1709_VERSION)
                {
                    return new UnsupportedAudioPolicyConfig();
                }

                try
                {
                    return new AudioPolicyConfig();
                }
                catch (BadImageFormatException)
                {
                    // Handles cases where the OS is a supported version of Windows 10 but the AudioSes.dll is still an older version (or just invalid)
                    return new UnsupportedAudioPolicyConfig();
                }
            });
        }
    }
}