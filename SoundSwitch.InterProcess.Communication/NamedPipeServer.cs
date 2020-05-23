using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using SoundSwitch.InterProcess.Communication.Protocol;

namespace SoundSwitch.InterProcess.Communication
{
    public class NamedPipeServer : IDisposable
    {
        public string Name { get; }
        private Thread _runningThread;

        public NamedPipeServer(string name)
        {
            Name = name;
        }

        public void Start(Action<string> handleMessage)
        {
            _runningThread = new Thread(ServerThread) {IsBackground = true};
            _runningThread.Start(handleMessage);
        }

        private void ServerThread(object data)
        {
            var handleMsg = (Action<string>) data;
            using var pipeServer = new NamedPipeServerStream(Name, PipeDirection.InOut);

            // Wait for a client to connect
            pipeServer.WaitForConnection();
            try
            {
                // Read the request from the client. Once the client has
                // written to the pipe its security token will be available.

                var ss = new StreamString(pipeServer);
                var msg = ss.ReadString();
                handleMsg(msg);
            }
            // Catch the IOException that is raised if the pipe is broken
            // or disconnected.
            catch (IOException e)
            {
                Trace.TraceError("ERROR: {0}", e.Message);
            }
        }

        public void Dispose()
        {
            _runningThread.Interrupt();
        }
    }
}