using System;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Framework.Audio.Microphone
{
    public class MicrophoneMuteToggler
    {
        private readonly AudioSwitcher _switcher;
        private readonly NotificationManager.NotificationManager _notificationManager;

        public MicrophoneMuteToggler(AudioSwitcher switcher, NotificationManager.NotificationManager notificationManager)
        {
            _switcher = switcher;
            _notificationManager = notificationManager;
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

            var result = _switcher.InteractWithMmDevice<(string Name, bool NewMuteState)>(microphone, device =>
            {
                try
                {
                    var newMuteState = !device.AudioEndpointVolume.Mute;
                    device.AudioEndpointVolume.Mute = newMuteState;
                    return (device.FriendlyName, newMuteState);
                }
                catch (Exception e)
                {
                    Log.Error("Couldn't toggle mute on {device}:\n{exception}", device.FriendlyName, e);
                }

                return default;
            });

            if (result == default)
            {
                return null;
            }

            return result.NewMuteState;
        }
    }
}