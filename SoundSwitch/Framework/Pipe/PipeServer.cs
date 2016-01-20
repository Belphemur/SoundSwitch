using System;
using System.IO.Pipes;
using System.Threading;
using System.Windows.Forms;
using SoundSwitch.Framework.Configuration;

namespace SoundSwitch.Framework.Pipe
{
    public class PipeServer
    {
        private readonly NamedPipeServerStream _namedPipe =
            new NamedPipeServerStream(AppConfigs.PipeConfiguration.PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

        private bool _applicationRunning = true;
        private readonly StreamString _streamString;
        private IAsyncResult _arConnectionRequest;

        public PipeServer()
        {
            Application.ApplicationExit += (sender, args) => _applicationRunning = false;
            var thread = new Thread(GetMessages) {Name = "PipeServer"};
            thread.Start();
            _streamString = new StreamString(_namedPipe);
            AppConfigs.PipeConfiguration.Save();
        }


        private void GetMessages()
        {
            _namedPipe.BeginWaitForConnection(SendAuth, null );
        }

        private void SendAuth(IAsyncResult arResult)
        {
            _arConnectionRequest = arResult;
            if (!arResult.IsCompleted) return;
            _namedPipe.WaitForConnection();
            _streamString.WriteString(PipeCommand.InitiateService.ToString());
            _streamString.WriteString(AppConfigs.PipeConfiguration.AuthentificationString);
            _namedPipe.WaitForPipeDrain();
            while (_applicationRunning)
            {
                var command = _streamString.ReadString();
                if(command == null)
                    continue;

                PipeCommand cmdEnum;
                Enum.TryParse(command, false, out cmdEnum);
                ExecuteCommand(cmdEnum);
            }
            _namedPipe.EndWaitForConnection(_arConnectionRequest);
            _namedPipe.Close();
        }

        private void ExecuteCommand(PipeCommand cmdEnum)
        {
            switch (cmdEnum)
            {
                case PipeCommand.InitiateService:
                    break;
                case PipeCommand.StopApplication:
                    Application.Exit();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cmdEnum), cmdEnum, null);
            }
        }
    }
}