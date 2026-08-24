#nullable enable
using System;
using System.IO;
using System.Text;

namespace SoundSwitch.Audio.Manager.Playback
{
    /// <summary>
    /// Minimal RIFF/WAVE parser — the in-house replacement for the legacy third-party
    /// wave reader, scoped to what the notification path needs:
    /// PCM (16/24/32-bit) and IEEE float (32-bit) WAV, including WAVE_FORMAT_EXTENSIBLE
    /// headers whose sub-format is PCM or IEEE float. Anything else (compressed formats such as
    /// MP3-in-WAV, mu-law, ...) is rejected with <see cref="InvalidDataException"/>.
    ///
    /// The whole data chunk is loaded into memory: notification sounds are short by design, and
    /// the playback path renders from a contiguous buffer.
    /// </summary>
    public static class WaveFileReader
    {
        /// <summary>
        /// Parse a WAV file from a seekable stream. The stream is left open.
        /// </summary>
        /// <exception cref="InvalidDataException">Malformed RIFF/WAVE, or an unsupported encoding.</exception>
        public static (byte[] AudioData, WaveFormat Format) Read(Stream stream)
        {
            var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);

            if (ReadChunkId(reader) != "RIFF") throw new InvalidDataException("Not a RIFF file.");
            reader.ReadUInt32(); // RIFF chunk size — advisory, not trusted
            if (ReadChunkId(reader) != "WAVE") throw new InvalidDataException("Not a WAVE file.");

            WaveFormat? format = null;
            byte[]? audioData = null;

            // Chunks may appear in any order (fmt before data is conventional, not required);
            // stop as soon as both are found.
            while ((format == null || audioData == null) && stream.Position + 8 <= stream.Length)
            {
                var chunkId = ReadChunkId(reader);
                if (chunkId.Length < 4) break; // truncated header
                var chunkSize = reader.ReadUInt32();
                var chunkStart = stream.Position;

                switch (chunkId)
                {
                    case "fmt ":
                        format = ParseFormat(reader);
                        break;
                    case "data":
                        audioData = reader.ReadBytes(RemainingBytes(stream, chunkStart, chunkSize));
                        break;
                }

                // Chunks are word-aligned: an odd-sized chunk is followed by one pad byte.
                stream.Position = Math.Min(chunkStart + chunkSize + (chunkSize & 1), stream.Length);
            }

            if (format == null) throw new InvalidDataException("WAV file has no fmt chunk.");
            if (audioData == null) throw new InvalidDataException("WAV file has no data chunk.");
            Validate(format);
            return (audioData, format);
        }

        /// <summary>
        /// Parse a WAV file from disk.
        /// </summary>
        /// <exception cref="InvalidDataException">Malformed RIFF/WAVE, or an unsupported encoding.</exception>
        public static (byte[] AudioData, WaveFormat Format) ReadFile(string path)
        {
            using var stream = File.OpenRead(path);
            return Read(stream);
        }

        private static string ReadChunkId(BinaryReader reader)
        {
            // ASCII: exactly one byte per character — ReadChars could consume more under a
            // multi-byte encoding, so read the raw bytes instead.
            return Encoding.ASCII.GetString(reader.ReadBytes(4));
        }

        private static int RemainingBytes(Stream stream, long chunkStart, uint chunkSize)
        {
            // Clamp a lying chunk-size header to what is actually left in the stream.
            return (int)Math.Min(chunkSize, Math.Max(stream.Length - chunkStart, 0));
        }

        private static WaveFormat ParseFormat(BinaryReader reader)
        {
            var tag = reader.ReadUInt16();
            var channels = reader.ReadUInt16();
            var sampleRate = reader.ReadUInt32();
            reader.ReadUInt32(); // avg bytes/sec — recomputed from the format instead
            reader.ReadUInt16(); // block align — recomputed from the format instead
            var bitsPerSample = reader.ReadUInt16();

            var encoding = tag switch
            {
                (ushort)WaveFormatEncoding.Pcm => WaveFormatEncoding.Pcm,
                (ushort)WaveFormatEncoding.IeeeFloat => WaveFormatEncoding.IeeeFloat,
                WaveFormatSubTypes.ExtensibleTag => ParseExtensibleSubType(reader),
                _ => throw new InvalidDataException($"Unsupported WAV encoding 0x{tag:X4} (only PCM and IEEE float are supported).")
            };

            return new WaveFormat(encoding, (int)sampleRate, bitsPerSample, channels);
        }

        private static WaveFormatEncoding ParseExtensibleSubType(BinaryReader reader)
        {
            var extensionSize = reader.ReadUInt16(); // cbSize — 22 for a real extensible header
            if (extensionSize < 22)
                throw new InvalidDataException("Truncated WAVE_FORMAT_EXTENSIBLE header.");

            reader.ReadUInt16(); // valid bits per sample — the container size (wBitsPerSample) is what we render
            reader.ReadUInt32(); // channel mask — not needed by the shared-mode render path
            var subType = new Guid(reader.ReadBytes(16));

            if (!WaveFormatSubTypes.TryMap(subType, out var encoding))
                throw new InvalidDataException($"Unsupported WAVE_FORMAT_EXTENSIBLE sub-format {subType} (only PCM and IEEE float are supported).");
            return encoding;
        }

        private static void Validate(WaveFormat format)
        {
            if (format.Channels < 1) throw new InvalidDataException("WAV file has no audio channels.");
            if (format.SampleRate <= 0) throw new InvalidDataException("WAV file has an invalid sample rate.");

            var supported = format.Encoding switch
            {
                WaveFormatEncoding.Pcm => format.BitsPerSample is 16 or 24 or 32,
                WaveFormatEncoding.IeeeFloat => format.BitsPerSample == 32,
                _ => false
            };
            if (!supported)
                throw new InvalidDataException($"Unsupported WAV format: {format.Encoding} {format.BitsPerSample}-bit (only PCM 16/24/32-bit and IEEE float 32-bit are supported).");
        }
    }
}
