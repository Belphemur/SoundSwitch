#nullable enable
using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;

using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Playback
{
    /// <summary>
    /// MP3 decoder — the in-house replacement for the legacy third-party MP3 reader,
    /// built on the Windows Media Foundation source reader. Decodes the whole file to
    /// PCM in memory (notification sounds are short by design) and returns the same
    /// output contract as <see cref="WaveFileReader"/>: raw PCM bytes plus a
    /// <see cref="WaveFormat"/>.
    ///
    /// The source reader is asked for uncompressed PCM at the file's native sample rate
    /// and channel count; Media Foundation's MP3 decoder delivers 16-bit PCM there.
    /// The negotiated output format is read back from the reader and reported as-is.
    ///
    /// Threading contract (mirrors <see cref="WavePlayer"/>): Media Foundation is COM,
    /// so all its objects are created, used, and released on a dedicated STA thread.
    /// The shared <see cref="Interop.Com.Threading.ComThread"/> is never used — it
    /// swallows exceptions from foreign callers, and decode failures must surface as
    /// <see cref="InvalidDataException"/> to the caller.
    ///
    /// FLAC/AAC support is a possible follow-up: the same source-reader pipeline
    /// decodes them once the MP3 sniffing and the media-subtype check are extended.
    /// </summary>
    public static class Mp3FileReader
    {
        private const int TargetBitsPerSample = 16;

        /// <summary>
        /// Decode an MP3 file from disk.
        /// </summary>
        /// <exception cref="InvalidDataException">Not a decodable MP3 file.</exception>
        public static (byte[] AudioData, WaveFormat Format) ReadFile(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("MP3 file not found.", path);

            (byte[] AudioData, WaveFormat Format)? result = null;
            ExceptionDispatchInfo? failure = null;

            var thread = new Thread(() =>
            {
                try
                {
                    result = Decode(path);
                }
                catch (Exception ex)
                {
                    failure = ExceptionDispatchInfo.Capture(ex);
                }
            })
            {
                IsBackground = true,
                Name = "SoundSwitch.Mp3Decoder"
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            failure?.Throw();
            return result ?? throw new InvalidDataException("MP3 decoding produced no result.");
        }

        /// <summary>
        /// Decode an MP3 file from a stream. The stream is left open.
        /// </summary>
        /// <exception cref="InvalidDataException">Not a decodable MP3 file.</exception>
        public static (byte[] AudioData, WaveFormat Format) Read(Stream stream)
        {
            // Media Foundation opens sources by URL, so buffer the stream to a temporary
            // .mp3 file; this also works for non-seekable streams.
            var tempPath = Path.Combine(Path.GetTempPath(), $"soundswitch-{Guid.NewGuid():N}.mp3");
            try
            {
                if (stream.CanSeek)
                    stream.Position = 0;

                using (var file = File.Create(tempPath))
                {
                    stream.CopyTo(file);
                }

                return ReadFile(tempPath);
            }
            finally
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }

        /// <summary>Decodes on the current (STA) thread. All failures map to <see cref="InvalidDataException"/>.</summary>
        private static (byte[] AudioData, WaveFormat Format) Decode(string path)
        {
            var hr = MediaFoundationInterop.MFStartup(MediaFoundationInterop.MfVersion, MediaFoundationInterop.MfStartupLite);
            if (hr != HRESULT.S_OK)
                throw new InvalidDataException($"Failed to initialize Media Foundation (0x{(uint)hr:X8}).");

            IMFSourceReader? reader = null;
            IMFMediaType? nativeType = null;
            IMFMediaType? targetType = null;
            IMFMediaType? actualType = null;
            try
            {
                hr = MediaFoundationInterop.MFCreateSourceReaderFromURL(path, IntPtr.Zero, out reader);
                if (hr != HRESULT.S_OK)
                    throw new InvalidDataException($"The file is not a valid MP3 file (0x{(uint)hr:X8}).");

                hr = reader.SetStreamSelection(MediaFoundationInterop.SourceReaderAllStreams, false);
                if (hr != HRESULT.S_OK)
                    throw new InvalidDataException($"Failed to configure the MP3 audio stream (0x{(uint)hr:X8}).");
                hr = reader.SetStreamSelection(MediaFoundationInterop.SourceReaderFirstAudioStream, true);
                if (hr != HRESULT.S_OK)
                    throw new InvalidDataException($"Failed to select the MP3 audio stream (0x{(uint)hr:X8}).");

                hr = reader.GetNativeMediaType(MediaFoundationInterop.SourceReaderFirstAudioStream, 0, out nativeType);
                if (hr != HRESULT.S_OK)
                    throw new InvalidDataException($"The file has no audio stream (0x{(uint)hr:X8}).");
                if (GetGuid(nativeType, MediaFoundationInterop.MtSubType) != MediaFoundationInterop.AudioFormatMp3)
                    throw new InvalidDataException("The file is not an MP3 file.");

                // Partial output type: uncompressed PCM, native rate and channels, 16-bit.
                var channels = GetUInt32(nativeType, MediaFoundationInterop.MtAudioNumChannels);
                var sampleRate = GetUInt32(nativeType, MediaFoundationInterop.MtAudioSamplesPerSecond);

                hr = MediaFoundationInterop.MFCreateMediaType(out targetType);
                if (hr != HRESULT.S_OK)
                    throw new InvalidDataException($"Failed to create the PCM output format (0x{(uint)hr:X8}).");
                SetGuid(targetType, MediaFoundationInterop.MtMajorType, MediaFoundationInterop.MediaTypeAudio, "set the output major type");
                SetGuid(targetType, MediaFoundationInterop.MtSubType, MediaFoundationInterop.AudioFormatPcm, "set the output subtype to PCM");
                SetUInt32(targetType, MediaFoundationInterop.MtAudioNumChannels, channels, "set the output channel count");
                SetUInt32(targetType, MediaFoundationInterop.MtAudioSamplesPerSecond, sampleRate, "set the output sample rate");
                SetUInt32(targetType, MediaFoundationInterop.MtAudioBitsPerSample, TargetBitsPerSample, "set the output bit depth");

                hr = reader.SetCurrentMediaType(MediaFoundationInterop.SourceReaderFirstAudioStream, IntPtr.Zero, targetType);
                if (hr != HRESULT.S_OK)
                    throw new InvalidDataException($"The MP3 file could not be converted to PCM (0x{(uint)hr:X8}).");

                // Read the negotiated format back — it is authoritative.
                hr = reader.GetCurrentMediaType(MediaFoundationInterop.SourceReaderFirstAudioStream, out actualType);
                if (hr != HRESULT.S_OK)
                    throw new InvalidDataException($"Failed to read the decoded audio format (0x{(uint)hr:X8}).");

                var format = BuildFormat(actualType);

                using var pcmData = new MemoryStream();
                while (true)
                {
                    hr = reader.ReadSample(MediaFoundationInterop.SourceReaderFirstAudioStream, 0, out _, out var streamFlags, out _, out var sample);
                    if (hr != HRESULT.S_OK)
                        throw new InvalidDataException($"Failed to read MP3 audio data (0x{(uint)hr:X8}).");
                    // A null sample without the end-of-stream flag is a stream tick (gap), not
                    // genuine EOS — keep reading so the decoded audio isn't truncated.
                    if (sample == null)
                    {
                        if ((streamFlags & MediaFoundationInterop.SourceReaderFlagEndOfStream) != 0)
                            break;
                        continue;
                    }

                    IMFMediaBuffer? buffer = null;
                    try
                    {
                        hr = sample.ConvertToContiguousBuffer(out buffer);
                        if (hr != HRESULT.S_OK)
                            throw new InvalidDataException($"Failed to read MP3 audio data (0x{(uint)hr:X8}).");

                        hr = buffer.Lock(out var data, out _, out var length);
                        if (hr != HRESULT.S_OK)
                            throw new InvalidDataException($"Failed to read MP3 audio data (0x{(uint)hr:X8}).");
                        try
                        {
                            var chunk = new byte[length];
                            Marshal.Copy(data, chunk, 0, (int)length);
                            pcmData.Write(chunk, 0, chunk.Length);
                        }
                        finally
                        {
                            buffer.Unlock();
                        }
                    }
                    finally
                    {
                        Release(ref buffer);
                        Release(ref sample);
                    }
                }

                var audioData = pcmData.ToArray();
                if (audioData.Length == 0)
                    throw new InvalidDataException("The MP3 file contains no audio data.");

                return (audioData, format);
            }
            finally
            {
                Release(ref actualType);
                Release(ref targetType);
                Release(ref nativeType);
                Release(ref reader);
                MediaFoundationInterop.MFShutdown();
            }
        }

        private static WaveFormat BuildFormat(IMFMediaType mediaType)
        {
            var subType = GetGuid(mediaType, MediaFoundationInterop.MtSubType);
            WaveFormatEncoding encoding;
            if (subType == MediaFoundationInterop.AudioFormatPcm)
                encoding = WaveFormatEncoding.Pcm;
            else if (subType == MediaFoundationInterop.AudioFormatFloat)
                encoding = WaveFormatEncoding.IeeeFloat;
            else
                throw new InvalidDataException($"Unsupported decoded format {subType} (only PCM and IEEE float are supported).");
            var channels = (int)GetUInt32(mediaType, MediaFoundationInterop.MtAudioNumChannels);
            var sampleRate = (int)GetUInt32(mediaType, MediaFoundationInterop.MtAudioSamplesPerSecond);
            var bitsPerSample = (int)GetUInt32(mediaType, MediaFoundationInterop.MtAudioBitsPerSample);

            if (channels < 1) throw new InvalidDataException("The MP3 file has no audio channels.");
            if (sampleRate <= 0) throw new InvalidDataException("The MP3 file has an invalid sample rate.");

            return new WaveFormat(encoding, sampleRate, bitsPerSample, channels);
        }

        private static Guid GetGuid(IMFMediaType mediaType, Guid key)
        {
            var hr = mediaType.GetGUID(ref key, out var value);
            if (hr != HRESULT.S_OK)
                throw new InvalidDataException($"Failed to read the decoded audio format (0x{(uint)hr:X8}).");
            return value;
        }

        private static uint GetUInt32(IMFMediaType mediaType, Guid key)
        {
            var hr = mediaType.GetUINT32(ref key, out var value);
            if (hr != HRESULT.S_OK)
                throw new InvalidDataException($"Failed to read the decoded audio format (0x{(uint)hr:X8}).");
            return value;
        }

        private static void SetGuid(IMFMediaType mediaType, Guid key, Guid value, string operation)
        {
            ThrowIfFailed(mediaType.SetGUID(ref key, ref value), operation);
        }

        private static void SetUInt32(IMFMediaType mediaType, Guid key, uint value, string operation)
        {
            ThrowIfFailed(mediaType.SetUINT32(ref key, value), operation);
        }

        private static void ThrowIfFailed(HRESULT hr, string operation)
        {
            if (hr != HRESULT.S_OK)
                throw new InvalidDataException($"Failed to {operation} (0x{(uint)hr:X8}).");
        }

        private static void Release<T>(ref T? comObject) where T : class
        {
            if (comObject == null) return;
            Marshal.ReleaseComObject(comObject);
            comObject = null;
        }
    }
}
