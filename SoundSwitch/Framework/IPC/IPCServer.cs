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