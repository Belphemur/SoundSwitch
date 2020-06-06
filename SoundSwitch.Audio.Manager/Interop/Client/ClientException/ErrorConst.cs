using System;

namespace SoundSwitch.Audio.Manager.Interop.Client.ClientException
{
    internal class ErrorConst
    {
        //FROM: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--1000-1299-
        public const ushort COM_ERROR_NOT_FOUND = 1168;
        public const int COM_ERROR_MASK = 0xFFFF;
    }
}