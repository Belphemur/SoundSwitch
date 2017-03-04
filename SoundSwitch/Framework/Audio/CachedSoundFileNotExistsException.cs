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
using System.Runtime.Serialization;

namespace SoundSwitch.Framework.Audio
{
    /// <summary>
    /// When the source on the disk of the CachedSound file doesn't exists.
    /// </summary>
    public class CachedSoundFileNotExistsException : ArgumentException
    {
        public CachedSoundFileNotExistsException()
        {
        }

        public CachedSoundFileNotExistsException(string message) : base(message)
        {
        }

        public CachedSoundFileNotExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public CachedSoundFileNotExistsException(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
        {
        }

        public CachedSoundFileNotExistsException(string message, string paramName) : base(message, paramName)
        {
        }

        protected CachedSoundFileNotExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}