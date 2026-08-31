namespace SoundSwitch.Audio.Manager.Interop.Enum
{
    public enum HRESULT : uint
    {
        S_OK = 0x0,
        S_FALSE = 0x1,
        AUDCLNT_E_DEVICE_INVALIDATED = 0x88890004,
        /// <summary>The Windows audio service (Audiosrv) is not running.</summary>
        AUDCLNT_E_SERVICE_NOT_RUNNING = 0x88890010,
        AUDCLNT_S_NO_SINGLE_PROCESS = 0x889000d,
        ERROR_NOT_FOUND = 0x80070490,
        PROCESS_NO_AUDIO = 0x80070057
    }

    public static class HRESULTExtensions
    {
        /// <summary>
        /// COM success is "severity bit clear": only a negative (as int32) HRESULT is a failure.
        /// S_FALSE (0x1) is a success code returned e.g. by IAudioEndpointVolume setters when the
        /// endpoint is already in the requested state.
        /// </summary>
        public static bool Failed(this HRESULT hr) => (int)hr < 0;
    }
}
