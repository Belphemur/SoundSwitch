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