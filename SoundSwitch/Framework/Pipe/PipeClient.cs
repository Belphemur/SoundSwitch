using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SoundSwitch.Framework.Configuration;

namespace SoundSwitch.Framework.Pipe
{
    public class PipeClient : IDisposable
    {
        private readonly NamedPipeClientStream _namedPipe =
    new NamedPipeClientStream(".", AppConfigs.PipeConfiguration.PipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);

        public bool IsConnected { get; private set; }

        private readonly StreamString _streamString;

        public PipeClient(bool exitIfNoConnection = false)
        {
            _streamString = new StreamString(_namedPipe);
            if (exitIfNoConnection)
                CheckForConnectionTask();
            InitPipeClient();
        }

        private void InitPipeClient()
        {
            _namedPipe.Connect();
            SendCommand(new PipeCommand(PipeCommandType.InitiateService, AppConfigs.PipeConfiguration.AesKeyBytes.ToString()));
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
            IsConnected = true;
        }
        /// <summary>
        /// Send a command to the Pipe Server
        /// </summary>
        /// <param name="cmd"></param>
        public void SendCommand(PipeCommand cmd)
        {
            cmd.Write(_namedPipe);
            _namedPipe.WaitForPipeDrain();
        }
        /// <summary>
        /// If the Pipe client doens't connect after 2000 ms, stop the application
        /// </summary>
        private void CheckForConnectionTask()
        {
            var task = new Task(() =>
            {
                Thread.Sleep(2000);
                if (IsConnected) return;
                AppLogger.Log.Error("The launched version of " + Application.ProductName + " doesn't support pipes.");
                Environment.FailFast("Old Version of SoundSwitch still launched");
            });
            task.Start();
        }


        public void Dispose()
        {
           _namedPipe.Close();
        }
    }
}