using System.IO;
using System.Security.Cryptography;
using System.Text;
using SoundSwitch.Framework.Configuration;
using static System.UInt16;

namespace SoundSwitch.Framework.Pipe
{
    public class StreamString
    {
        private readonly Stream _ioStream;
        private readonly UnicodeEncoding _streamEncoding;

        public StreamString(Stream ioStream)
        {
            this._ioStream = ioStream;
            _streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            var len = 0;

            len = _ioStream.ReadByte()*256;
            len += _ioStream.ReadByte();
            if (len < 0)
                return null;
            var inBuffer = new byte[len];
            _ioStream.Read(inBuffer, 0, len);

            return _streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            var outBuffer = _streamEncoding.GetBytes(outString);
            var len = outBuffer.Length;
            if (len > MaxValue)
            {
                len = (int) MaxValue;
            }
            _ioStream.WriteByte((byte) (len/256));
            _ioStream.WriteByte((byte) (len & 255));
            _ioStream.Write(outBuffer, 0, len);
            _ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}