#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Threading;

using Serilog;

using SoundSwitch.Audio.Manager.Interop.Com.Threading;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface;

namespace SoundSwitch.Audio.Manager
{
    /// <summary>
    /// Managed wrapper over <see cref="IAudioEndpointVolume"/>: master volume/mute get/set,
    /// channel count, per-channel scalar get/set, and one change notification.
    ///
    /// The native interface is activated and owned on the ComThread; every operation is marshalled
    /// back onto the ComThread (a no-op pass-through when the caller is already on it). The
    /// <see cref="VolumeNotification"/> event is raised on the audio service's own callback
    /// thread — subscribers must not assume a particular thread and must not block.
    /// </summary>
    public sealed class AudioEndpointVolumeClient : IDisposable
    {
        private static readonly ILogger Logger = Log.ForContext<AudioEndpointVolumeClient>();

        private readonly IAudioEndpointVolume _volume;
        private readonly object _callbackLock = new();
        private EndpointVolumeCallback? _callback;
        private EventHandler<AudioVolumeNotificationData>? _volumeNotification;
        private int _disposed;

        internal AudioEndpointVolumeClient(IAudioEndpointVolume volume)
        {
            _volume = volume;
        }

        public float MasterVolumeLevelScalar
        {
            get => ComThread.Invoke(() =>
            {
                var hr = _volume.GetMasterVolumeLevelScalar(out var level);
                if (hr.Failed()) throw AudioDeviceException.FromHResult(hr, "IAudioEndpointVolume.GetMasterVolumeLevelScalar");
                return level;
            });
            set => ComThread.Invoke(() =>
            {
                var eventContext = Guid.Empty;
                var hr = _volume.SetMasterVolumeLevelScalar(value, ref eventContext);
                if (hr.Failed()) throw AudioDeviceException.FromHResult(hr, "IAudioEndpointVolume.SetMasterVolumeLevelScalar");
            });
        }

        public bool Mute
        {
            get => ComThread.Invoke(() =>
            {
                var hr = _volume.GetMute(out var mute);
                if (hr.Failed()) throw AudioDeviceException.FromHResult(hr, "IAudioEndpointVolume.GetMute");
                return mute;
            });
            set => ComThread.Invoke(() =>
            {
                var eventContext = Guid.Empty;
                var hr = _volume.SetMute(value, ref eventContext);
                if (hr.Failed()) throw AudioDeviceException.FromHResult(hr, "IAudioEndpointVolume.SetMute");
            });
        }

        public int ChannelCount => ComThread.Invoke(() =>
        {
            var hr = _volume.GetChannelCount(out var count);
            if (hr.Failed()) throw AudioDeviceException.FromHResult(hr, "IAudioEndpointVolume.GetChannelCount");
            return (int)count;
        });

        public float GetChannelVolumeLevelScalar(int channel) => ComThread.Invoke(() =>
        {
            var hr = _volume.GetChannelVolumeLevelScalar((uint)channel, out var level);
            if (hr.Failed()) throw AudioDeviceException.FromHResult(hr, "IAudioEndpointVolume.GetChannelVolumeLevelScalar");
            return level;
        });

        public void SetChannelVolumeLevelScalar(int channel, float level) => ComThread.Invoke(() =>
        {
            var eventContext = Guid.Empty;
            var hr = _volume.SetChannelVolumeLevelScalar((uint)channel, level, ref eventContext);
            if (hr.Failed()) throw AudioDeviceException.FromHResult(hr, "IAudioEndpointVolume.SetChannelVolumeLevelScalar");
        });

        /// <summary>
        /// Raised when the endpoint's volume or mute state changes. The handler runs on the audio
        /// service's notification thread.
        /// </summary>
        public event EventHandler<AudioVolumeNotificationData>? VolumeNotification
        {
            add
            {
                if (value == null) return;
                // Marshal first, then lock inside the delegate: ReleaseResources acquires the same
                // lock while already running on the ComThread, so taking the lock before invoking
                // would invert the order and deadlock (lock holder waits on ComThread, ComThread
                // waits on the lock).
                ComThread.Invoke(() =>
                {
                    lock (_callbackLock)
                    {
                        if (_disposed != 0) return;
                        if (_callback == null)
                        {
                            var callback = new EndpointVolumeCallback(this);
                            var hr = _volume.RegisterControlChangeNotify(callback);
                            if (hr.Failed()) throw AudioDeviceException.FromHResult(hr, "IAudioEndpointVolume.RegisterControlChangeNotify");
                            _callback = callback;
                        }

                        _volumeNotification += value;
                    }
                });
            }
            remove
            {
                if (value == null) return;
                // Same ordering as add: ComThread first, lock inside the delegate.
                ComThread.Invoke(() =>
                {
                    lock (_callbackLock)
                    {
                        _volumeNotification -= value;
                        if (_volumeNotification == null && _callback != null && _disposed == 0)
                        {
                            var hr = _volume.UnregisterControlChangeNotify(_callback);
                            _callback = null;
                            if (hr != HRESULT.S_OK)
                                Logger.Warning("IAudioEndpointVolume.UnregisterControlChangeNotify failed: 0x{HResult:X8}", (uint)hr);
                        }
                    }
                });
            }
        }

        private void RaiseVolumeNotification(AudioVolumeNotificationData data)
        {
            var handler = _volumeNotification;
            handler?.Invoke(this, data);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            GC.SuppressFinalize(this);
            try
            {
                ComThread.Invoke(ReleaseResources);
            }
            catch (Exception e)
            {
                Logger.Warning(e, "Exception while disposing the endpoint volume client; releasing the COM reference directly");
                ReleaseNativeReferenceOnly();
            }
        }

        ~AudioEndpointVolumeClient()
        {
            // Finalizer path: do not marshal to the ComThread (it may already be gone during
            // process teardown). Releasing the RCW is safe from any thread and destroys the native
            // object, which also drops any outstanding notification registration.
            try
            {
                ReleaseNativeReferenceOnly();
            }
            catch
            {
                // Never let a finalizer throw (would terminate the process): the GC has already
                // released the RCW reference by the time we run.
            }
        }

        private void ReleaseResources()
        {
            lock (_callbackLock)
            {
                if (_callback != null)
                {
                    _volume.UnregisterControlChangeNotify(_callback);
                    _callback = null;
                }
            }

            Marshal.ReleaseComObject(_volume);
        }

        private void ReleaseNativeReferenceOnly()
        {
            Marshal.ReleaseComObject(_volume);
        }

        private sealed class EndpointVolumeCallback : IAudioEndpointVolumeCallback
        {
            private readonly AudioEndpointVolumeClient _parent;

            public EndpointVolumeCallback(AudioEndpointVolumeClient parent)
            {
                _parent = parent;
            }

            public void OnNotify(IntPtr notifyData)
            {
                try
                {
                    var native = Marshal.PtrToStructure<AudioVolumeNotificationDataNative>(notifyData);
                    _parent.RaiseVolumeNotification(new AudioVolumeNotificationData(native.fMasterVolume, native.bMuted != 0));
                }
                catch (Exception e)
                {
                    Logger.Warning(e, "Error while processing an endpoint volume notification");
                }
            }
        }
    }
}
