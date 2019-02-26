using System;

namespace SoundSwitch.Audio.Manager.Interop.Threading
{
    public sealed class InvalidThreadException : Exception
    {

        public InvalidThreadException()
        {

        }

        public InvalidThreadException(string message)
            : base(message)
        {
        }
    }
}