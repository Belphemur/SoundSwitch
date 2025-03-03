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
        /// <returns>Tuple with the device name and current mute state, null if couldn't interact with the microphone</returns>
        /// </summary>
        public (string Name, bool MuteState)? ToggleDefaultMute()
        {
            var microphone = _switcher.GetDefaultMmDevice(EDataFlow.eCapture, ERole.eCommunications);
            if (microphone == null)
            {
                Log.Information("Couldn't find a default microphone to toggle mute");
                return null;
            }

            var result = _switcher.InteractWithDevice<(string Name, bool NewMuteState)>(microphone, device =>
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

            _notificationManager.NotifyMuteChanged(result.Name, result.NewMuteState);
            return result;
        }

        /// <summary>
        /// Set mute state for the default microphone
        /// <param name="muteState">The mute state to set</param>
        /// <returns>Tuple with the device name and current mute state, null if couldn't interact with the microphone</returns>
        /// </summary>
        public (string Name, bool MuteState)? SetDefaultMuteState(bool muteState)
        {
            var microphone = _switcher.GetDefaultMmDevice(EDataFlow.eCapture, ERole.eCommunications);
            if (microphone == null)
            {
                Log.Information("Couldn't find a default microphone to set mute state");
                return null;
            }

            var result = _switcher.InteractWithDevice<(string Name, bool NewMuteState)>(microphone, device =>
            {
                try
                {
                    device.AudioEndpointVolume.Mute = muteState;
                    return (device.FriendlyName, muteState);
                }
                catch (Exception e)
                {
                    Log.Error("Couldn't set mute state to {state} on {device}:\n{exception}", muteState, device.FriendlyName, e);
                }

                return default;
            });

            if (result == default)
            {
                return null;
            }

            _notificationManager.NotifyMuteChanged(result.Name, result.NewMuteState);
            return result;
        }
    }
}