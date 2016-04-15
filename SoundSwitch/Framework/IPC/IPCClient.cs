using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using SoundSwitch.Framework.IPC.RemoteObjects;

namespace SoundSwitch.Framework.IPC
{
    public class IPCClient : IDisposable
    {
        private readonly string _serverUrl;
        private readonly IpcChannel _ipcClient;

        public IPCClient(string serverURL)
        {
            _serverUrl = "ipc://" + serverURL + '/';
            _ipcClient = new IpcChannel();
            ChannelServices.RegisterChannel(_ipcClient, true);
        }

        public RemoteObject GetService()
        {
            var remoteType =
            new WellKnownClientTypeEntry(
                typeof(RemoteObject),
                _serverUrl + RemoteObject.ObjURL);
            RemotingConfiguration.RegisterWellKnownClientType(remoteType);
            return new RemoteObject();
        }

        public void Dispose()
        {
        }
    }
}