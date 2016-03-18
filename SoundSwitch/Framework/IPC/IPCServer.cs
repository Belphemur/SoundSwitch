using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using SoundSwitch.Framework.IPC.RemoteObjects;

namespace SoundSwitch.Framework.IPC
{
    public class IPCServer
    {
        private readonly IpcChannel _ipcServer;

        public IPCServer(string serverURL)
        {
            _ipcServer = new IpcChannel(serverURL);
        }
        /// <summary>
        /// Initialize the server
        /// </summary>
        public void InitServer()
        {
            ChannelServices.RegisterChannel(
         _ipcServer, true);
            RemotingConfiguration.
           RegisterWellKnownServiceType(
               typeof(RemoteObject), RemoteObject.ObjURL,
               WellKnownObjectMode.Singleton);
        }
    }
}