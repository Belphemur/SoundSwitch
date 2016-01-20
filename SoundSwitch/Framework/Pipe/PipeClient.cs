using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Principal;
using SoundSwitch.Framework.Configuration;

namespace SoundSwitch.Framework.Pipe
{
    public class PipeClient : IDisposable
    {
        private readonly NamedPipeClientStream _namedPipe =
    new NamedPipeClientStream(".", AppConfigs.PipeConfiguration.PipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);

        private readonly StreamString _streamString;

        public PipeClient()
        {
            _streamString = new StreamString(_namedPipe);
            InitPipeClient();
        }

        private void InitPipeClient()
        {
            _namedPipe.Connect();
            var stream = _streamString;
            var commandString = stream.ReadString();
            PipeCommand cmd;
            Enum.TryParse<PipeCommand>(commandString, out cmd);
            if (cmd != PipeCommand.InitiateService)
            {
                throw new Exception("No Handshake");
            }
            var authString = stream.ReadString();
            if (authString != AppConfigs.PipeConfiguration.AuthentificationString)
            {
                throw new Exception("Wrong auth string");
            }
        }

        public void SendCommand(PipeCommand cmd)
        {
            _streamString.WriteString(cmd.ToString());
            _namedPipe.WaitForPipeDrain();
        }


        public void Dispose()
        {
           _namedPipe.Close();
        }
    }
}