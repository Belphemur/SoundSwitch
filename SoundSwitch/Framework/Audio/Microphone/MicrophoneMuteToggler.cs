using System;
using JetBrains.Annotations;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Framework.Audio.Microphone
{
    /// <summary>
    /// Provides functionality to toggle microphone mute state
    /// </summary>
    public class MicrophoneMuteToggler
    {
        private readonly AudioSwitcher _switcher;

        /// <summary>
        /// Initializes a new instance of the MicrophoneMuteToggler class
        /// </summary>
        /// <param name="switcher">The audio switcher instance</param>
        public MicrophoneMuteToggler(AudioSwitcher switcher)
        {
            _switcher = switcher;
        }

        /// <summary>
        /// Toggle mute state for the default microphone
        /// </summary>
        /// <returns>Tuple with the device name and current mute state, null if couldn't interact with the microphone</returns>
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

            return result == default ? null : result;
        }

        /// <summary>
        /// Set mute state for the default microphone
        /// </summary>
        /// <param name="muteState">The mute state to set</param>
        /// <returns>Tuple with the device name and current mute state, null if couldn't interact with the microphone</returns>
        public (string Name, bool MuteState)? SetDefaultMuteState(bool muteState)
        {
            var microphone = _switcher.GetDefaultMmDevice(EDataFlow.eCapture, ERole.eCommunications);
            return SetMuteState(muteState, microphone);
        }

        /// <summary>
        /// Set mute state for the microphone
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="muteState">The mute state to set</param>
        /// <returns>Tuple with the device name and current mute state, null if couldn't interact with the microphone</returns>
        public (string Name, bool MuteState)? SetMicrophoneMuteState(string deviceId, bool muteState)
        {
            var microphone = _switcher.GetDevice(deviceId);
            return SetMuteState(muteState, microphone);
        }

        private (string Name, bool MuteState)? SetMuteState(bool muteState, [CanBeNull] MMDevice microphone)
        {
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

            return result == default ? null : result;
        }
    }
}