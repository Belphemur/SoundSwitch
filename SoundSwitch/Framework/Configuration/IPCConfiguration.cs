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

namespace SoundSwitch.Framework.Configuration
{
    public class IPCConfiguration : IIPCConfiguration
    {

        public int Port { get; set; }

        /// <summary>
        /// Increment the Port Number and return its value
        /// </summary>
        /// <returns></returns>
        public int IncrementPort()
        {
            Port = ++Port%64000;
            if (Port < 60000)
            {
                Port = new Random().Next(60000, 64000);
            }
            Save();
            return Port;
        }

        public string ServerUrl()
        {
            return "localhost:" + IncrementPort();
        }

        public string ClientUrl()
        {
            return "localhost:" + Port;
        }

        public string FileLocation { get; set; }
        public void Save()
        {
            ConfigurationManager.SaveConfiguration(this);
        }

    }
}