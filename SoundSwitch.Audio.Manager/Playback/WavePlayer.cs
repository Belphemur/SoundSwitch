#nullable enable
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Serilog;

using SoundSwitch.Audio.Manager.Interop.Com.Base;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface;

namespace SoundSwitch.Audio.Manager.Playback
{
    /// <summary>
    /// WASAPI shared-mode, event-driven renderer — the in-house replacement for the legacy
    /// third-party <c>WasapiOut</c>. One instance renders one buffer of samples once.
    ///
    /// Threading contract: each instance owns a dedicated COM-initialized STA thread. The shared
    /// <see cref="Interop.Com.Threading.ComThread"/> is never used for the render loop — a render
    /// loop blocking on buffer events would stall device switching. The endpoint (<c>IMMDevice</c>)
    /// is resolved on the shared ComThread via <see cref="AudioSwitcher"/> and unmarshalled onto
    /// this thread; the audio client, render client, and everything the render loop touches are
    /// then created, used, and released on this thread.
    ///
    /// Completion contract (mirrors the legacy <c>PlaybackStopped</c> semantics):
    /// - The returned <see cref="Task"/> completes when playback has fully drained, the device is
    ///   missing (a warning is logged; nothing played), or cancellation stops playback early.
    /// - The task faults only on initialization failure — including when the default-endpoint
    ///   fallback after a specified-device failure also fails — so the scheduling layer surfaces
    ///   it instead of the failure vanishing into a swallowed-marshal path.
    /// - <c>onCompleted</c> fires exactly once on the stopped paths with <see langword="null"/>
    ///   (played to completion) or the playback error; it does not fire on cancellation or when
    ///   no device was found.
    ///
    /// Cancellation teardown: the render loop waits on the WASAPI buffer event and the
    /// cancellation handle together, so cancellation wakes the player thread itself; the thread
    /// then stops the stream and releases every COM reference before completing the task — the
    /// task completing <em>is</em> the "render thread is dead" signal, which keeps back-to-back
    /// notification sounds (cancel + replay) safe.
    /// </summary>
    internal sealed class WavePlayer
    {
        private static readonly ILogger Logger = Log.ForContext<WavePlayer>();

        // CLSCTX_ALL (in-proc server | in-proc handler | local server | remote server)
        private const uint ClsCtxAll = 0x1 | 0x2 | 0x4 | 0x10;
        private const int LatencyMilliseconds = 200;
        // 100-ns units per millisecond — IAudioClient.Initialize takes buffer durations in hns.
        private const long HnsPerMillisecond = 10_000;

        private static readonly Guid IidAudioClient = new("1CB9AD4C-DBFA-4C32-B178-C2F568A703B2");
        private static readonly Guid IidAudioRenderClient = new("F294ACFC-3146-4483-A7BF-ADDCA7C260E2");

        private readonly byte[] _audioData;
        private readonly WaveFormat _sourceFormat;
        private readonly string? _deviceId;
        private readonly CancellationToken _cancellationToken;
        private readonly Action<Exception?>? _onCompleted;
        private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public WavePlayer(byte[] audioData, WaveFormat sourceFormat, string? deviceId, CancellationToken cancellationToken, Action<Exception?>? onCompleted)
        {
            _audioData = audioData;
            _sourceFormat = sourceFormat;
            _deviceId = deviceId;
            _cancellationToken = cancellationToken;
            _onCompleted = onCompleted;
        }

        public Task Task => _completion.Task;

        public void Start()
        {
            var thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "SoundSwitch.WavePlayer"
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void Run()
        {
            try
            {
                if (!_cancellationToken.IsCancellationRequested)
                    Play();
                _completion.TrySetResult();
            }
            catch (Exception ex)
            {
                // Initialization-phase failure (the default-endpoint fallback already ran inside
                // Play when applicable): fault the returned task so the failure is surfaced.
                _completion.TrySetException(ex);
            }
        }

        /// <summary>Plays the sound; throws only for initialization-phase failures.</summary>
        private void Play()
        {
            IMMDevice? device = null;
            RenderSession? session = null;
            try
            {
                device = ResolveDevice(_deviceId);
                if (device == null)
                {
                    Logger.Warning("No audio device found for notification playback (device id: {DeviceId}).", string.IsNullOrEmpty(_deviceId) ? "<default>" : _deviceId);
                    return;
                }

                try
                {
                    session = CreateSession(device);
                    session.Start();
                }
                catch (Exception ex) when (!string.IsNullOrEmpty(_deviceId))
                {
                    // Fall back to the default render endpoint (the legacy catch-and-retry-default
                    // for a specified device that fails to initialize).
                    Logger.Error(ex, "Failed to initialize playback on device {DeviceId}; falling back to the default render endpoint.", _deviceId);
                    session?.Dispose();
                    session = null;
                    Marshal.ReleaseComObject(device);
                    device = ResolveDevice(null);
                    if (device == null)
                        throw new AudioDeviceException(HRESULT.PROCESS_NO_AUDIO, "No default render endpoint available for playback fallback");
                    session = CreateSession(device);
                    session.Start();
                }

                var cancelled = RenderLoop(session, out var playbackError);
                if (!cancelled)
                    RaiseCompleted(playbackError);
            }
            finally
            {
                // Teardown on the owning thread: stop the stream, then release every COM reference.
                session?.Dispose();
                if (device != null) Marshal.ReleaseComObject(device);
            }
        }

        /// <summary>
        /// Event-driven render loop: refill on buffer events, then drain until the engine has
        /// played the tail (padding reaches zero). Returns <see langword="true"/> when cancelled
        /// (stop promptly, no drain); HRESULT failures surface as <paramref name="playbackError"/>.
        /// </summary>
        private bool RenderLoop(RenderSession session, out Exception? playbackError)
        {
            playbackError = null;
            var blockAlign = session.OutputFormat.BlockAlign;
            var totalFrames = session.Data.Length / blockAlign;
            var offsetFrames = session.FramesWritten;
            var sourceExhausted = offsetFrames >= totalFrames;

            try
            {
                var waitHandles = new WaitHandle[] { session.BufferEvent, _cancellationToken.WaitHandle };
                while (true)
                {
                    var signaled = WaitHandle.WaitAny(waitHandles, LatencyMilliseconds * 3);
                    if (signaled == 1) return true; // cancellation handle

                    var hr = session.AudioClient.GetCurrentPadding(out var padding);
                    if (hr != HRESULT.S_OK) throw AudioDeviceException.FromHResult(hr, "IAudioClient.GetCurrentPadding");

                    if (sourceExhausted)
                    {
                        if (padding == 0) return false; // fully drained
                        continue;
                    }

                    var framesAvailable = session.BufferFrameCount - padding;
                    if (framesAvailable == 0) continue; // nothing to fill (also the timeout path)

                    var framesToWrite = (uint)Math.Min(framesAvailable, totalFrames - offsetFrames);
                    hr = session.RenderClient.GetBuffer(framesToWrite, out var buffer);
                    if (hr != HRESULT.S_OK) throw AudioDeviceException.FromHResult(hr, "IAudioRenderClient.GetBuffer");
                    Marshal.Copy(session.Data, offsetFrames * blockAlign, buffer, (int)framesToWrite * blockAlign);
                    hr = session.RenderClient.ReleaseBuffer(framesToWrite, AudioClientBufferFlags.None);
                    if (hr != HRESULT.S_OK) throw AudioDeviceException.FromHResult(hr, "IAudioRenderClient.ReleaseBuffer");

                    offsetFrames += (int)framesToWrite;
                    sourceExhausted = offsetFrames >= totalFrames;
                }
            }
            catch (Exception ex)
            {
                // The caller may cancel *and dispose* the token source while the loop is parked
                // on its wait handle — any failure racing a requested cancellation is a cancel.
                if (_cancellationToken.IsCancellationRequested) return true;
                playbackError = ex;
                return false;
            }
        }

        private void RaiseCompleted(Exception? playbackError)
        {
            try
            {
                _onCompleted?.Invoke(playbackError);
            }
            catch (Exception ex)
            {
                // A throwing completion callback must not kill the render thread before teardown.
                Logger.Warning(ex, "Playback completion callback threw.");
            }
        }

        private static IMMDevice? ResolveDevice(string? deviceId)
        {
            // Resolve the endpoint on the shared ComThread (via AudioSwitcher) and unmarshal the
            // IMMDevice reference onto this thread. No private enumerator is ever constructed here.
            var stream = AudioSwitcher.Instance.GetDeviceStream(deviceId);
            if (stream == IntPtr.Zero)
                return null;

            var iid = new Guid(ComGuid.AUDIO_IMMDEVICE_IID);
            var hr = Ole32.CoGetInterfaceAndReleaseStream(stream, ref iid, out var devicePointer);
            if (hr != HRESULT.S_OK || devicePointer == IntPtr.Zero)
            {
                if (devicePointer != IntPtr.Zero) Marshal.Release(devicePointer);
                return null;
            }

            var device = (IMMDevice)Marshal.GetObjectForIUnknown(devicePointer);
            Marshal.Release(devicePointer);
            return device;
        }

        /// <summary>
        /// Activate the audio client, negotiate a render format the engine accepts (converting the
        /// source samples when needed), and wire up event-driven buffering.
        /// </summary>
        private RenderSession CreateSession(IMMDevice device)
        {
            var iid = IidAudioClient;
            var hr = device.Activate(ref iid, ClsCtxAll, IntPtr.Zero, out var clientPointer);
            if (hr != HRESULT.S_OK || clientPointer == IntPtr.Zero)
            {
                if (clientPointer != IntPtr.Zero) Marshal.Release(clientPointer);
                throw AudioDeviceException.FromHResult(hr, "IMMDevice.Activate(IAudioClient)");
            }

            var audioClient = (IAudioClient)Marshal.GetObjectForIUnknown(clientPointer);
            Marshal.Release(clientPointer);

            try
            {
                var outputFormat = NegotiateFormat(audioClient, _sourceFormat);
                var renderData = outputFormat == _sourceFormat
                    ? _audioData
                    : AudioConverter.Convert(_audioData, _sourceFormat, outputFormat);

                var formatPointer = WaveFormatMarshaller.ToUnmanaged(outputFormat);
                try
                {
                    var sessionGuid = Guid.Empty;
                    hr = audioClient.Initialize(AudioClientShareMode.Shared, AudioClientStreamFlags.EventCallback,
                        LatencyMilliseconds * HnsPerMillisecond, 0, formatPointer, ref sessionGuid);
                    if (hr != HRESULT.S_OK) throw AudioDeviceException.FromHResult(hr, "IAudioClient.Initialize");
                }
                finally
                {
                    Marshal.FreeHGlobal(formatPointer);
                }

                var bufferEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
                try
                {
                    hr = audioClient.SetEventHandle(bufferEvent.SafeWaitHandle.DangerousGetHandle());
                    if (hr != HRESULT.S_OK) throw AudioDeviceException.FromHResult(hr, "IAudioClient.SetEventHandle");

                    hr = audioClient.GetBufferSize(out var bufferFrameCount);
                    if (hr != HRESULT.S_OK) throw AudioDeviceException.FromHResult(hr, "IAudioClient.GetBufferSize");

                    var serviceIid = IidAudioRenderClient;
                    hr = audioClient.GetService(ref serviceIid, out var servicePointer);
                    if (hr != HRESULT.S_OK || servicePointer == IntPtr.Zero)
                    {
                        if (servicePointer != IntPtr.Zero) Marshal.Release(servicePointer);
                        throw AudioDeviceException.FromHResult(hr, "IAudioClient.GetService(IAudioRenderClient)");
                    }

                    var renderClient = (IAudioRenderClient)Marshal.GetObjectForIUnknown(servicePointer);
                    Marshal.Release(servicePointer);
                    return new RenderSession(audioClient, renderClient, bufferEvent, renderData, outputFormat, bufferFrameCount);
                }
                catch
                {
                    bufferEvent.Dispose();
                    throw;
                }
            }
            catch
            {
                Marshal.ReleaseComObject(audioClient);
                throw;
            }
        }

        /// <summary>
        /// Render in the source format when the engine takes it; otherwise in the engine's closest
        /// match, and failing that in the engine mix format (the common case: shared mode wants
        /// e.g. 48 kHz / 32-bit float while the notification WAV is 44.1 kHz / 16-bit PCM).
        /// </summary>
        private static WaveFormat NegotiateFormat(IAudioClient audioClient, WaveFormat source)
        {
            var sourcePointer = WaveFormatMarshaller.ToUnmanaged(source);
            // IsFormatSupported's closest-match output is task-allocated (COM allocator) — it must
            // be freed with CoTaskMemFree, on every path, exactly like the GetMixFormat output.
            var closestMatchPointer = IntPtr.Zero;
            try
            {
                var hr = audioClient.IsFormatSupported(AudioClientShareMode.Shared, sourcePointer, out closestMatchPointer);
                if (hr == HRESULT.S_OK) return source;
                if (hr == HRESULT.S_FALSE && closestMatchPointer != IntPtr.Zero)
                    return WaveFormatMarshaller.FromPointer(closestMatchPointer);

                var mixFormatPointer = IntPtr.Zero;
                try
                {
                    hr = audioClient.GetMixFormat(out mixFormatPointer);
                    if (hr != HRESULT.S_OK || mixFormatPointer == IntPtr.Zero)
                        throw AudioDeviceException.FromHResult(hr, "IAudioClient.GetMixFormat");
                    return WaveFormatMarshaller.FromPointer(mixFormatPointer);
                }
                finally
                {
                    if (mixFormatPointer != IntPtr.Zero) Marshal.FreeCoTaskMem(mixFormatPointer);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(sourcePointer);
                if (closestMatchPointer != IntPtr.Zero) Marshal.FreeCoTaskMem(closestMatchPointer);
            }
        }

        /// <summary>
        /// Everything the render loop needs, plus the teardown order: stop the stream (when
        /// started), release the COM references, then close the buffer event the engine signals.
        /// </summary>
        private sealed class RenderSession(
            IAudioClient audioClient,
            IAudioRenderClient renderClient,
            EventWaitHandle bufferEvent,
            byte[] data,
            WaveFormat outputFormat,
            uint bufferFrameCount) : IDisposable
        {
            public IAudioClient AudioClient { get; } = audioClient;
            public IAudioRenderClient RenderClient { get; } = renderClient;
            public EventWaitHandle BufferEvent { get; } = bufferEvent;
            public byte[] Data { get; } = data;
            public WaveFormat OutputFormat { get; } = outputFormat;
            public uint BufferFrameCount { get; } = bufferFrameCount;
            public int FramesWritten { get; private set; }
            private bool Started { get; set; }

            /// <summary>Pre-roll the buffer, then start the stream. Throws on failure.</summary>
            public void Start()
            {
                var blockAlign = OutputFormat.BlockAlign;
                var totalFrames = (uint)(Data.Length / blockAlign);
                var framesToWrite = Math.Min(BufferFrameCount, totalFrames);
                if (framesToWrite > 0)
                {
                    var hr = RenderClient.GetBuffer(framesToWrite, out var buffer);
                    if (hr != HRESULT.S_OK) throw AudioDeviceException.FromHResult(hr, "IAudioRenderClient.GetBuffer");
                    Marshal.Copy(Data, 0, buffer, (int)framesToWrite * blockAlign);
                    hr = RenderClient.ReleaseBuffer(framesToWrite, AudioClientBufferFlags.None);
                    if (hr != HRESULT.S_OK) throw AudioDeviceException.FromHResult(hr, "IAudioRenderClient.ReleaseBuffer");
                    FramesWritten = (int)framesToWrite;
                }

                var startResult = AudioClient.Start();
                if (startResult != HRESULT.S_OK) throw AudioDeviceException.FromHResult(startResult, "IAudioClient.Start");
                Started = true;
            }

            public void Dispose()
            {
                if (Started) AudioClient.Stop(); // teardown: the returned HRESULT is intentionally ignored
                Marshal.ReleaseComObject(RenderClient);
                Marshal.ReleaseComObject(AudioClient);
                BufferEvent.Dispose();
            }
        }
    }

    /// <summary>
    /// WAVEFORMATEX marshalling for mix-format negotiation. Reads resolve WAVE_FORMAT_EXTENSIBLE
    /// headers down to their base encoding (the container size in <c>wBitsPerSample</c> is what
    /// the render path writes).
    /// </summary>
    internal static class WaveFormatMarshaller
    {
        public static IntPtr ToUnmanaged(WaveFormat format)
        {
            var header = new WaveFormatEx
            {
                wFormatTag = (ushort)format.Encoding,
                nChannels = (ushort)format.Channels,
                nSamplesPerSec = (uint)format.SampleRate,
                nAvgBytesPerSec = (uint)format.AverageBytesPerSecond,
                nBlockAlign = (ushort)format.BlockAlign,
                wBitsPerSample = (ushort)format.BitsPerSample,
                cbSize = 0
            };
            var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<WaveFormatEx>());
            Marshal.StructureToPtr(header, pointer, false);
            return pointer; // caller frees with FreeHGlobal
        }

        public static WaveFormat FromPointer(IntPtr pointer)
        {
            var header = Marshal.PtrToStructure<WaveFormatEx>(pointer);
            var encoding = header.wFormatTag switch
            {
                (ushort)WaveFormatEncoding.Pcm => WaveFormatEncoding.Pcm,
                (ushort)WaveFormatEncoding.IeeeFloat => WaveFormatEncoding.IeeeFloat,
                WaveFormatSubTypes.ExtensibleTag => ReadExtensibleSubType(pointer),
                var tag => throw new InvalidDataException($"Unsupported wave format tag 0x{tag:X4} returned by the audio engine.")
            };
            return new WaveFormat(encoding, (int)header.nSamplesPerSec, header.wBitsPerSample, header.nChannels);
        }

        private static WaveFormatEncoding ReadExtensibleSubType(IntPtr pointer)
        {
            // WAVEFORMATEXTENSIBLE: WAVEFORMATEX (18 bytes, Pack=2) + wValidBitsPerSample (2) +
            // dwChannelMask (4) + SubFormat GUID at offset 24.
            var guidBytes = new byte[16];
            Marshal.Copy(pointer + 24, guidBytes, 0, guidBytes.Length);
            var subType = new Guid(guidBytes);
            if (!WaveFormatSubTypes.TryMap(subType, out var encoding))
                throw new InvalidDataException($"Unsupported extensible wave sub-format {subType} returned by the audio engine.");
            return encoding;
        }
    }
}
