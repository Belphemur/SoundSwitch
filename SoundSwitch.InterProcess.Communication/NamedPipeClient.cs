using System;
using System.IO.Pipes;
using System.Security.Principal;
using SoundSwitch.InterProcess.Communication.Protocol;

namespace SoundSwitch.InterProcess.Communication
{
    public class NamedPipeClient : IDisposable
    {
        public string Name { get; }
        private readonly NamedPipeClientStream _clientStream;

        public NamedPipeClient(string name)
        {
            Name = name;
            _clientStream = new NamedPipeClientStream(".", Name, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
            _clientStream.Connect();
        }

        public void SendMsg(string msg)
        {
            var ss = new StreamString(_clientStream);
            ss.WriteString(msg);
        }

        public void Dispose()
        {
            _clientStream?.Dispose();
        }
    }
}