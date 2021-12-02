using System;
using SoundSwitch.Audio.Manager.Interop.Com.Threading;
using SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended;

namespace SoundSwitch.Audio.Manager.Interop.Client.Extended.Factory
{
    public static class AudioPolicyConfigFactory
    {
        public static IAudioPolicyConfig Create()
        {
            return ComThread.Invoke<IAudioPolicyConfig>(() =>
            {
                if (Environment.OSVersion.Version.Major < 10)
                {
                    return new UnsupportedAudioPolicyConfig();
                }

                try
                {
                    return new AudioPolicyConfig();
                }
                catch (BadImageFormatException)
                {
                    // Handles cases where the OS is Windows 10 but the AudioSes.dll is still an older version (or just invalid)
                    return new UnsupportedAudioPolicyConfig();
                }
            });
        }
    }
}