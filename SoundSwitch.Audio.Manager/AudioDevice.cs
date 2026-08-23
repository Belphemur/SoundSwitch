#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using Serilog;

using SoundSwitch.Audio.Manager.Interop.Com.Threading;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface;

namespace SoundSwitch.Audio.Manager
{
    /// <summary>
    /// Managed wrapper over a Windows audio endpoint (<see cref="IMMDevice"/>) — the in-house
    /// replacement for the legacy third-party MMDevice.
    ///
    /// Threading / snapshot contract: instances are created on the ComThread (via
    /// AudioDeviceEnumerator / AudioSwitcher). The metadata properties (<see cref="Id"/>,
    /// <see cref="FriendlyName"/>, <see cref="IconPath"/>, <see cref="DataFlow"/>,
    /// <see cref="State"/>, <see cref="IsUsb"/>) are captured defensively at creation time into an
    /// immutable snapshot, so they are safe to read from any thread — app code operates on the
    /// snapshot and never touches live COM state off the owning STA. Live operations
    /// (<see cref="EndpointVolume"/>, <see cref="GetSessions"/>) are marshalled back onto the
    /// ComThread internally. <see cref="Dispose()"/> is likewise marshalled; finalization releases
    /// the native reference without leaving the finalizer thread.
    ///
    /// Defensive reads: a failed metadata read (e.g. the Windows audio service stopping between
    /// enumeration and use) yields the documented fallback ("", <see cref="EDeviceState.Active"/>,
    /// false) and is logged at Information for the transient service-not-running case, Warning
    /// otherwise — the PR #2371 semantics, baked in instead of layered on top.
    ///
    /// Ownership: whoever constructs (or receives) an <see cref="AudioDevice"/> owns it and must
    /// dispose it. Any <see cref="IAudioEndpointVolumeCallback"/> subscription made through
    /// <see cref="EndpointVolume"/> is torn down on dispose.
    /// </summary>
    public sealed class AudioDevice : IDisposable
    {
        private static readonly ILogger Logger = Log.ForContext<AudioDevice>();

        // CLSCTX_ALL (in-proc server | in-proc handler | local server | remote server)
        private const uint ClsCtxAll = 0x1 | 0x2 | 0x4 | 0x10;
        // STGM_READ
        private const uint StgmRead = 0x0;

        private static readonly Guid IidAudioEndpointVolume = new("5CDF2C82-841E-4546-9722-0CF74078229A");
        private static readonly Guid IidAudioSessionManager2 = new("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");

        private IMMDevice? _device;
        private readonly object _endpointVolumeLock = new();
        private AudioEndpointVolumeClient? _endpointVolume;
        private int _disposed;

        /// <summary>Endpoint identifier (e.g. "{0.0.0.00000000}.{...}"). Snapshot.</summary>
        public string Id { get; }

        /// <summary>Device friendly name (PKEY_Device_FriendlyName). Snapshot.</summary>
        public string FriendlyName { get; }

        /// <summary>Icon path (PKEY_Device_IconPath). Snapshot.</summary>
        public string IconPath { get; }

        /// <summary>Render/capture direction (via IMMEndpoint). Snapshot.</summary>
        public EDataFlow DataFlow { get; }

        /// <summary>Endpoint state at creation time. Snapshot.</summary>
        public EDeviceState State { get; }

        /// <summary>True when the device enumerator name (DEVPKEY_Device_EnumeratorName) is "USB". Snapshot.</summary>
        public bool IsUsb { get; }

        /// <summary>
        /// Creates the wrapper over a native device. Must be called on the ComThread.
        /// </summary>
        internal AudioDevice(IMMDevice device)
        {
            ComThread.Assert();
            _device = device;
            (Id, FriendlyName, IconPath, DataFlow, State, IsUsb) = CaptureSnapshot(device);
        }

        /// <summary>
        /// Capture the immutable metadata snapshot with the defensive-read semantics: one guarded
        /// block, all-or-defaults, matching the previous construction behavior.
        /// </summary>
        private static (string Id, string FriendlyName, string IconPath, EDataFlow DataFlow, EDeviceState State, bool IsUsb) CaptureSnapshot(IMMDevice device)
        {
            try
            {
                var id = ReadId(device);
                var state = ReadState(device);
                var dataFlow = ReadDataFlow(device);

                string friendlyName, iconPath, enumeratorName;
                var store = OpenPropertyStore(device);
                try
                {
                    friendlyName = PropertyStoreReader.ReadString(store, PropertyKeys.PKEY_Device_FriendlyName);
                    iconPath = PropertyStoreReader.ReadString(store, PropertyKeys.PKEY_Device_IconPath);
                    enumeratorName = PropertyStoreReader.ReadString(store, PropertyKeys.DEVPKEY_Device_EnumeratorName);
                }
                finally
                {
                    Marshal.ReleaseComObject(store);
                }

                return (id, friendlyName, iconPath, dataFlow, state, enumeratorName == "USB");
            }
            catch (Exception ex)
            {
                // The Windows audio service can disappear between device enumeration and object
                // creation (service stop, fast-user-switch, RDP disconnect, sleep/resume). Never
                // let that crash construction — fall back to safe defaults instead. Only the
                // "service not running" HRESULT is the expected/transient case (Information);
                // any other failure is unexpected and stays at Warning.
                if (AudioDeviceException.IsAudioServiceNotRunning(ex))
                {
                    Logger.Information(ex, "Failed to read device metadata: Windows audio service not running; using defaults.");
                }
                else
                {
                    Logger.Warning(ex, "Failed to read device metadata; using defaults.");
                }

                return (string.Empty, string.Empty, string.Empty, EDataFlow.eRender, EDeviceState.Active, false);
            }
        }

        private static string ReadId(IMMDevice device)
        {
            var hr = device.GetId(out var idPtr);
            if (hr != HRESULT.S_OK) throw AudioDeviceException.FromHResult(hr, "IMMDevice.GetId");
            try
            {
                return Marshal.PtrToStringUni(idPtr) ?? string.Empty;
            }
            finally
            {
                // GetId returns a caller-owned LPWSTR allocated with the COM task allocator.
                if (idPtr != IntPtr.Zero) Marshal.FreeCoTaskMem(idPtr);
            }
        }

        private static EDeviceState ReadState(IMMDevice device)
        {
            var hr = device.GetState(out var state);
            if (hr != HRESULT.S_OK) throw AudioDeviceException.FromHResult(hr, "IMMDevice.GetState");
            return state;
        }

        private static EDataFlow ReadDataFlow(IMMDevice device)
        {
            // The QueryInterface cast reuses the same RCW; no extra release is required.
            var endpoint = (IMMEndpoint)device;
            var hr = endpoint.GetDataFlow(out var dataFlow);
            if (hr != HRESULT.S_OK) throw AudioDeviceException.FromHResult(hr, "IMMEndpoint.GetDataFlow");
            return dataFlow;
        }

        private static IPropertyStore OpenPropertyStore(IMMDevice device)
        {
            var hr = device.OpenPropertyStore(StgmRead, out var store);
            if (hr != HRESULT.S_OK || store == null) throw AudioDeviceException.FromHResult(hr, "IMMDevice.OpenPropertyStore");
            return store;
        }

        private IntPtr Activate(Guid iid, string operation)
        {
            ComThread.Assert();
            var device = _device ?? throw new ObjectDisposedException(nameof(AudioDevice));
            var hr = device.Activate(ref iid, ClsCtxAll, IntPtr.Zero, out var pointer);
            if (hr != HRESULT.S_OK || pointer == IntPtr.Zero)
            {
                if (pointer != IntPtr.Zero) Marshal.Release(pointer);
                throw AudioDeviceException.FromHResult(hr, $"IMMDevice.Activate({operation})");
            }

            return pointer;
        }

        /// <summary>
        /// The endpoint's volume client, or <see langword="null"/> when it cannot be activated
        /// off the ComThread (activation failures throw <see cref="AudioDeviceException"/> to
        /// callers that are already on the ComThread).
        /// </summary>
        public AudioEndpointVolumeClient? EndpointVolume
        {
            get
            {
                if (_disposed != 0) return null;
                lock (_endpointVolumeLock)
                {
                    if (_endpointVolume != null || _disposed != 0) return _endpointVolume;
                    _endpointVolume = ComThread.Invoke(() =>
                    {
                        var iid = IidAudioEndpointVolume;
                        var pointer = Activate(iid, "IAudioEndpointVolume");
                        var volume = (IAudioEndpointVolume)Marshal.GetObjectForIUnknown(pointer);
                        Marshal.Release(pointer);
                        return new AudioEndpointVolumeClient(volume);
                    });
                    return _endpointVolume;
                }
            }
        }

        /// <summary>
        /// Snapshot of the device's audio sessions (process id + state). ComThread-marshalled.
        /// </summary>
        public IReadOnlyList<AudioSessionInfo> GetSessions()
        {
            return ComThread.Invoke(GetSessionsCore) ?? (IReadOnlyList<AudioSessionInfo>)Array.Empty<AudioSessionInfo>();
        }

        private List<AudioSessionInfo> GetSessionsCore()
        {
            var sessions = new List<AudioSessionInfo>();
            var iid = IidAudioSessionManager2;
            var pointer = Activate(iid, "IAudioSessionManager2");
            var manager = (IAudioSessionManager2)Marshal.GetObjectForIUnknown(pointer);
            Marshal.Release(pointer);
            try
            {
                var hr = manager.GetSessionEnumerator(out var enumerator);
                if (hr != HRESULT.S_OK || enumerator == null)
                    throw AudioDeviceException.FromHResult(hr, "IAudioSessionManager2.GetSessionEnumerator");
                try
                {
                    hr = enumerator.GetCount(out var count);
                    if (hr != HRESULT.S_OK)
                        throw AudioDeviceException.FromHResult(hr, "IAudioSessionEnumerator.GetCount");

                    for (var i = 0; i < count; i++)
                    {
                        hr = enumerator.GetSession(i, out var session);
                        if (hr != HRESULT.S_OK || session == null) continue;
                        try
                        {
                            var stateResult = session.GetState(out var state);
                            var processResult = session.GetProcessId(out var processId);
                            if (stateResult == HRESULT.S_OK && processResult == HRESULT.S_OK)
                            {
                                sessions.Add(new AudioSessionInfo(processId, state));
                            }
                        }
                        finally
                        {
                            Marshal.ReleaseComObject(session);
                        }
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(enumerator);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(manager);
            }

            return sessions;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            GC.SuppressFinalize(this);
            try
            {
                // Marshal teardown back to the ComThread that owns the native device.
                ComThread.Invoke(ReleaseResources);
            }
            catch (Exception e)
            {
                Logger.Warning(e, "Exception while disposing audio device {DeviceId}; releasing the COM reference directly", Id);
                ReleaseNativeReferenceOnly();
            }
        }

        ~AudioDevice()
        {
            // Finalizer path: do not marshal to the ComThread (it may already be gone during
            // process teardown). Releasing the RCW is safe from any thread and destroys the
            // native object, which also drops any outstanding callback registrations.
            ReleaseNativeReferenceOnly();
        }

        private void ReleaseResources()
        {
            _endpointVolume?.Dispose();
            ReleaseNativeReferenceOnly();
        }

        private void ReleaseNativeReferenceOnly()
        {
            var device = Interlocked.Exchange(ref _device, null);
            if (device != null) Marshal.ReleaseComObject(device);
        }

        public override string ToString() => $"[{DataFlow}]{FriendlyName}";
    }
}
