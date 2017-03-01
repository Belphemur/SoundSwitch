/********************************************************************
* Copyright (C) 2015-2017 Antoine Aflalo
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 2
* of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
********************************************************************/

using System.Collections.Generic;
using System.Threading;
using AudioEndPoint;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework;

namespace SoundSwitch.Model
{
    public class AudioDeviceLister : IAudioDeviceLister
    {
        private readonly DeviceState _state;
        private readonly HashSet<IAudioDevice> _recording = new HashSet<IAudioDevice>();
        private readonly HashSet<IAudioDevice> _playback = new HashSet<IAudioDevice>();
        private volatile bool _needUpdate = true;
        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();

        public AudioDeviceLister(DeviceState state)
        {
            _state = state;
            AudioController.DeviceAdded += AudioControllerOnDeviceAdded;
            AudioController.DeviceRemoved += AudioControllerOnDeviceRemoved;
            AudioController.DeviceStateChanged += AudioControllerOnDeviceStateChanged;
            AudioController.DeviceDefaultChanged += AudioControllerOnDeviceDefaultChanged;
        }

        ~AudioDeviceLister()
        {
            AudioController.DeviceAdded -= AudioControllerOnDeviceAdded;
            AudioController.DeviceRemoved -= AudioControllerOnDeviceRemoved;
            AudioController.DeviceStateChanged -= AudioControllerOnDeviceStateChanged;
            AudioController.DeviceDefaultChanged -= AudioControllerOnDeviceDefaultChanged;
        }

        private void AudioControllerOnDeviceDefaultChanged(object sender, DeviceDefaultChangedEvent deviceDefaultChangedEvent)
        {
            _needUpdate = true;
        }

        private void AudioControllerOnDeviceStateChanged(object sender, DeviceStateChangedEvent deviceStateChangedEvent)
        {
            _needUpdate = true;
        }

        private void AudioControllerOnDeviceRemoved(object sender, DeviceRemovedEvent deviceRemovedEvent)
        {
            _needUpdate = true;
        }

        private void AudioControllerOnDeviceAdded(object sender, DeviceAddedEvent deviceAddedEvent)
        {
            _needUpdate = true;
        }


        /// <summary>
        /// Get the playback device in the set state
        /// </summary>
        /// <returns></returns>
        public ICollection<IAudioDevice> GetPlaybackDevices()
        {
            using (AppLogger.Log.DebugCall())
            {
                _cacheLock.EnterUpgradeableReadLock();
                try
                {
                    if (!_needUpdate) return _playback;

                    AppLogger.Log.Debug("Needs update");
                    Refresh();
                    return _playback;
                }
                finally
                {
                    AppLogger.Log.Debug("Get Playback Devices");
                    _cacheLock.ExitUpgradeableReadLock();
                }
            }


        }

        private void Refresh()
        {
            _cacheLock.EnterWriteLock();
            try
            {
                _playback.Clear();

                try
                {
                    AppLogger.Log.Debug("Refreshing playback devices");
                    _playback.UnionWith(AudioController.GetPlaybackDevices(_state));
                }
                catch (DefSoundException e)
                {
                    AppLogger.Log.Error("Problem getting Playback Devices ", e);
                }


                _recording.Clear();
                try
                {
                    AppLogger.Log.Debug("Refreshing recording devices");
                    _recording.UnionWith(AudioController.GetRecordingDevices(_state));
                }
                catch (DefSoundException e)
                {
                    AppLogger.Log.Error("Problem getting Recording Devices ", e);
                }
                _needUpdate = false;
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Get the recording device in the set state
        /// </summary>
        /// <returns></returns>
        public ICollection<IAudioDevice> GetRecordingDevices()
        {
            using (AppLogger.Log.DebugCall())
            {
                _cacheLock.EnterUpgradeableReadLock();
                try
                {
                    if (!_needUpdate) return _recording;

                    AppLogger.Log.Debug("Needs update");
                    Refresh();
                    return _recording;
                }
                finally
                {
                    AppLogger.Log.Debug("Get Recording Devices");
                    _cacheLock.ExitUpgradeableReadLock();
                }
            }
        }
    }
}