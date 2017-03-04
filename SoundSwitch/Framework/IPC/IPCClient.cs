/********************************************************************
* Copyright (C) 2015-2017 Antoine Aflalo
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 2
* of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
********************************************************************/

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