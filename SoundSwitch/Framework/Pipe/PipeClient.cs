using System;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading.Tasks;
using SoundSwitch.Framework.Configuration;

namespace SoundSwitch.Framework.Pipe
{
    public class PipeClient : IDisposable
    {
        private readonly NamedPipeClientStream _namedPipe =
            new NamedPipeClientStream(".", AppConfigs.PipeConfiguration.PipeName, PipeDirection.InOut, PipeOptions.None,
                TokenImpersonationLevel.Impersonation);

        private string _serverAuth;


        public PipeClient(bool exitIfNoConnection = false)
        {
            ExitIfNoConnection = exitIfNoConnection;
            InitPipeClient();
        }

        public bool IsConnected { get; private set; }
        public bool ExitIfNoConnection { get; }

        public void Dispose()
        {
            IsConnected = false;
            _namedPipe.Close();
        }

        public event EventHandler<PipeCommandReceivedEvent> PipeCommandReceived;

        private void InitPipeClient()
        {
            try
            {
                _namedPipe.Connect(2000);
            }
            catch (TimeoutException)
            {
                if (ExitIfNoConnection)
                {
                    AppLogger.Log.Warn("No Pipe available. Closing the application.");
                    Environment.FailFast("No pipe available");
                }
                else
                {
                    throw;
                }
            }

            SendCommand(new PipeCommand(PipeCommandType.InitiateService,
                BitConverter.ToString(AppConfigs.PipeConfiguration.AesKeyBytes)));
            var cmd = PipeCommand.GetPipeCommand(_namedPipe);
            if (cmd == null)
            {
                throw new Exception("No handshake");
            }
            if (cmd.Type != PipeCommandType.InitiateService)
            {
                throw new Exception("Wrong Command: " + cmd.Type);
            }
            if (cmd.Data != AppConfigs.PipeConfiguration.AuthentificationString)
            {
                throw new Exception("Wrong Auth: " + cmd.Data);
            }
            _serverAuth = cmd.Auth;
            IsConnected = true;
            var task = new Task(() =>
            {
                while (_namedPipe.IsConnected)
                {
                    var pipeCmd = PipeCommand.GetPipeCommand(_namedPipe);
                    if (pipeCmd != null)
                    {
                        PipeCommandReceived?.Invoke(this, new PipeCommandReceivedEvent(pipeCmd));
                    }
                }
            });
            task.Start();
        }

        /// <summary>
        ///     Send a command to the Pipe Server
        /// </summary>
        /// <param name="cmd"></param>
        public void SendCommand(PipeCommand cmd)
        {
            if (_serverAuth != null)
            {
                cmd.Auth = _serverAuth;
            }
            cmd.Write(_namedPipe);
            _namedPipe.WaitForPipeDrain();
        }

        #region Events

        public class PipeCommandReceivedEvent : EventArgs
        {
            public PipeCommandReceivedEvent(PipeCommand command)
            {
                Command = command;
            }

            public PipeCommand Command { get; set; }
        }

        #endregion
    }
}