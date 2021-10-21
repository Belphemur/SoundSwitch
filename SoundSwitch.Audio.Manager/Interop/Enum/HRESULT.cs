namespace SoundSwitch.Audio.Manager.Interop.Enum
{
    public enum HRESULT : uint
    {
        S_OK = 0x0,
        S_FALSE = 0x1,
        AUDCLNT_E_DEVICE_INVALIDATED = 0x88890004,
        AUDCLNT_S_NO_SINGLE_PROCESS = 0x889000d,
        ERROR_NOT_FOUND = 0x80070490,
        PROCESS_NO_AUDIO = 0x80070057
    }
}