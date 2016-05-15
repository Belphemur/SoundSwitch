using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Windows.Forms;
using SoundSwitch.Framework.IPC.RemoteObjects;

namespace SoundSwitch.Framework.IPC
{
    public class IPCServer : IDisposable
    {
        private readonly IpcChannel _ipcServer;

        public IPCServer(string serverURL)
        {
            System.Collections.IDictionary properties = new System.Collections.Hashtable();
            properties["portName"] = serverURL;
            properties["exclusiveAddressUse"] = false;
            properties["name"] = Application.ProductName;
            _ipcServer = new IpcChannel(properties, null, null);
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

        public void Dispose()
        {
            _ipcServer.StopListening(null);
            ChannelServices.UnregisterChannel(_ipcServer);
        }
    }
}