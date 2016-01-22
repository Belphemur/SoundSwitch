using System;
using System.IO;

namespace SoundSwitch.Framework.Pipe
{
    public class PipeCommand
    {
        public PipeCommandType Type { get; }
        public string Data { get; }
        public string Auth { get; set; }

        public PipeCommand(PipeCommandType type, string data)
        {
            Type = type;
            Data = data;
            Auth = "";
        }
        /// <summary>
        /// Write the PipeCommand on a stream in Bytes
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public int Write(Stream stream)
        {
            var bytePipeCmd = (byte)Type;
            stream.WriteByte(bytePipeCmd);
            var streamString = new StreamString(stream);
            return streamString.WriteString(Data) + streamString.WriteString(Auth) + 1;
        }

        public static PipeCommand GetPipeCommand(Stream stream)
        {
            var readByte = stream.ReadByte();

            if (readByte < 0)
                return null;

            var pipeCmdType = (PipeCommandType)Enum.ToObject(typeof(PipeCommandType), readByte);
            var streamString = new StreamString(stream);
            var data = streamString.ReadString();
            var auth = streamString.ReadString();
            return new PipeCommand(pipeCmdType, data) {Auth = auth};
        }

    }
}