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
        private IAsyncResult _arConnectionRequest;

        public PipeServer()
        {
            Application.ApplicationExit += (sender, args) => _applicationRunning = false;
            var thread = new Thread(GetMessages) {Name = "PipeServer"};
            thread.Start();
            AppConfigs.PipeConfiguration.Save();
        }


        private void GetMessages()
        {
            _namedPipe.BeginWaitForConnection(ProcessPipeCommands, null );
        }

        private void ProcessPipeCommands(IAsyncResult arResult)
        {
            _arConnectionRequest = arResult;
            if (!arResult.IsCompleted) return;
            _namedPipe.WaitForConnection();
            while (_applicationRunning)
            {
                var pipeCmd = PipeCommand.GetPipeCommand(_namedPipe);
                if(pipeCmd == null)
                    continue;
                ExecuteCommand(pipeCmd);
            }
            _namedPipe.EndWaitForConnection(_arConnectionRequest);
            _namedPipe.Close();
        }
        /// <summary>
        /// Send command to the pipe client
        /// </summary>
        /// <param name="cmd"></param>
        public void SendCommand(PipeCommand cmd)
        {
            cmd.Write(_namedPipe);
            _namedPipe.WaitForPipeDrain();
        }

        private void ExecuteCommand(PipeCommand cmd)
        {
            switch (cmd.Type)
            {
                case PipeCommandType.InitiateService:
                    if (cmd.Data != AppConfigs.PipeConfiguration.AesKeyBytes.ToString())
                    {
                        SendCommand(new PipeCommand(PipeCommandType.WrongAuth, cmd.Data));
                    }
                    else
                    {
                        SendCommand(new PipeCommand(PipeCommandType.InitiateService, AppConfigs.PipeConfiguration.AuthentificationString));
                    }
                    break;
                case PipeCommandType.StopApplication:
                    AppLogger.Log.Warn("Received stopping app from ", cmd.Data);
                    Application.Exit();
                    break;
                case PipeCommandType.WrongAuth:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cmd), cmd, null);
            }
        }
    }
}