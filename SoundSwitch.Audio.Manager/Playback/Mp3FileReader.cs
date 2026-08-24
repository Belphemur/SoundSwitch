#nullable enable
using System;
using System.IO;

using MP3Sharp;

namespace SoundSwitch.Audio.Manager.Playback
{
    /// <summary>
    /// MP3 decoder — decodes the whole file to PCM in memory (notification sounds
    /// are short by design) and returns the same output contract as
    /// <see cref="WaveFileReader"/>: raw PCM bytes plus a <see cref="WaveFormat"/>.
    ///
    /// Decoding is done by the vendored MP3Sharp library (a pure-managed C# port of
    /// JavaLayer, LGPL-3.0 — see <c>Playback/Mp3Sharp/LICENSE.txt</c>), so it has no
    /// OS dependency and runs on any machine, unlike the previous Media Foundation
    /// pipeline. MP3Sharp always emits 16-bit stereo PCM (mono sources are doubled
    /// to stereo) at the file's native sample rate, which is what the returned
    /// <see cref="WaveFormat"/> reports.
    /// </summary>
    public static class Mp3FileReader
    {
        /// <summary>
        /// Decode an MP3 file from disk.
        /// </summary>
        /// <exception cref="InvalidDataException">Not a decodable MP3 file.</exception>
        public static (byte[] AudioData, WaveFormat Format) ReadFile(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("MP3 file not found.", path);

            using var fs = File.OpenRead(path);
            return Read(fs);
        }

        /// <summary>
        /// Decode an MP3 file from a stream. The stream is left open.
        /// </summary>
        /// <exception cref="InvalidDataException">Not a decodable MP3 file.</exception>
        public static (byte[] AudioData, WaveFormat Format) Read(Stream stream)
        {
            if (stream.CanSeek)
                stream.Position = 0;

            byte[] audioData;
            int frequency;
            try
            {
                // MP3Stream closes its source stream when disposed; wrap the caller's
                // stream so it stays open, as the Read contract requires.
                using var mp3 = new MP3Stream(new NonClosingReadStream(stream));
                using var pcm = new MemoryStream();
                var buffer = new byte[8192];
                int read;
                while ((read = mp3.Read(buffer, 0, buffer.Length)) > 0)
                    pcm.Write(buffer, 0, read);
                audioData = pcm.ToArray();
                frequency = mp3.Frequency;
            }
            catch (MP3SharpException ex)
            {
                // MP3Sharp signals any input it cannot parse — garbage bytes, truncated
                // frames, unsupported channel counts — with MP3SharpException (or its
                // BitstreamException/DecoderException subclasses). Map them all to the
                // reader's failure contract.
                throw new InvalidDataException("The file is not a valid MP3 file.", ex);
            }

            if (audioData.Length == 0)
                throw new InvalidDataException("The MP3 file contains no audio data.");

            // MP3Sharp outputs 16-bit stereo PCM (mono sources are doubled to stereo).
            var format = new WaveFormat(WaveFormatEncoding.Pcm, frequency, 16, 2);
            return (audioData, format);
        }

        /// <summary>
        /// Read-only pass-through over the caller's stream whose <see cref="Stream.Dispose"/>
        /// does not close it — MP3Sharp's <c>MP3Stream</c> closes whatever stream it is
        /// handed, and <see cref="Read"/> promises to leave the input stream open.
        /// </summary>
        private sealed class NonClosingReadStream : Stream
        {
            private readonly Stream _inner;

            internal NonClosingReadStream(Stream inner) => _inner = inner;

            public override bool CanRead => _inner.CanRead;
            public override bool CanSeek => _inner.CanSeek;
            public override bool CanWrite => false;
            public override long Length => _inner.Length;
            public override long Position
            {
                get => _inner.Position;
                set => _inner.Position = value;
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

            public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                // Deliberately does not dispose the inner stream.
            }
        }
    }
}
