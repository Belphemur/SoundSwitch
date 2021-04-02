using System;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Framework.Audio.Microphone
{
    public class MicrophoneMuteToggler
    {
        private readonly AudioSwitcher _switcher;

        public MicrophoneMuteToggler(AudioSwitcher switcher)
        {
            _switcher = switcher;
        }

        /// <summary>
        /// Toggle mute state for the default microphone
        /// <returns>The current mute state, null if couldn't interact with the microphone</returns>
        /// </summary>
        public bool? ToggleDefaultMute()
        {
            var microphone = _switcher.GetDefaultMmDevice(EDataFlow.eCapture, ERole.eCommunications);
            if (microphone == null)
            {
                Log.Information("Couldn't find a default microphone to toggle mute");
                return null;
            }

            return _switcher.InteractWithMmDevice<bool?>(microphone, device =>
            {
                try
                {
                    var newMuteState = !microphone.AudioEndpointVolume.Mute;
                    device.AudioEndpointVolume.Mute = newMuteState;
                    return newMuteState;
                }
                catch (Exception e)
                {
                    Log.Error("Couldn't toggle mute on {device}:\n{exception}", microphone.FriendlyName, e);
                }

                return null;
            });
        }
    }
}