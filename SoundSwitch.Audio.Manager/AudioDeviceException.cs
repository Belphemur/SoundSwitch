#nullable enable
using System;
using System.Runtime.InteropServices;

using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager
{
    /// <summary>
    /// Thrown by the audio interop layer on failed COM calls. Carries the failing HRESULT so
    /// callers can distinguish expected transient conditions (the Windows audio service being
    /// stopped, an endpoint vanishing between enumeration and use) from real bugs.
    /// </summary>
    /// <remarks>
    /// Reachability caveat: <see cref="Interop.Com.Threading"/> marshalling swallows exceptions
    /// for callers that are not already on the ComThread (logged at Warning, default returned).
    /// The HRESULT is therefore only directly observable to code running on the ComThread; most
    /// public callers observe null/default plus a warning log. This matches the previous
    /// swallow-and-null boundary and is deliberate.
    /// </remarks>
    public class AudioDeviceException : Exception
    {
        /// <summary>The failing COM HRESULT (mirrored into <see cref="Exception.HResult"/>).</summary>
        public HRESULT Status { get; }

        public AudioDeviceException(HRESULT status, string message, Exception? innerException = null)
            : base($"{message} (HRESULT 0x{(uint)status:X8})", innerException)
        {
            Status = status;
            HResult = unchecked((int)status);
        }

        public static AudioDeviceException FromHResult(HRESULT status, string operation) => new(status, $"{operation} failed");

        public static AudioDeviceException ServiceNotRunning(string operation) => new(HRESULT.AUDCLNT_E_SERVICE_NOT_RUNNING, $"{operation}: the Windows audio service is not running");

        /// <summary>
        /// True when the exception represents the transient "Windows audio service is not running"
        /// condition (AUDCLNT_E_SERVICE_NOT_RUNNING) — whether surfaced as an
        /// <see cref="AudioDeviceException"/> or as a raw <see cref="COMException"/>-family
        /// exception carrying the same HRESULT.
        /// </summary>
        public static bool IsAudioServiceNotRunning(Exception? exception)
        {
            const int serviceNotRunning = unchecked((int)HRESULT.AUDCLNT_E_SERVICE_NOT_RUNNING);
            return exception is AudioDeviceException { Status: HRESULT.AUDCLNT_E_SERVICE_NOT_RUNNING }
                   || exception is { HResult: serviceNotRunning };
        }
    }
}
