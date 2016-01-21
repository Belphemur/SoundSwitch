using System;
using System.IO;

namespace SoundSwitch.Framework.Pipe
{
    public class PipeCommand
    {
        public PipeCommandType Type { get; }
        public string Data { get; }

        public PipeCommand(PipeCommandType type, string data)
        {
            Type = type;
            Data = data;
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
            return streamString.WriteString(Data) + 1;
        }

        public static PipeCommand GetPipeCommand(Stream stream)
        {
            var readByte = stream.ReadByte();

            if (readByte < 0)
                return null;

            var pipeCmdType = (PipeCommandType)Enum.ToObject(typeof(PipeCommandType), readByte);
            var streamString = new StreamString(stream);
            var data = streamString.ReadString();
            return new PipeCommand(pipeCmdType, data);
        }

    }
}