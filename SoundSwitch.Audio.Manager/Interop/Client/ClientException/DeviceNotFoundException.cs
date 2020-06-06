namespace SoundSwitch.Audio.Manager.Interop.Client.ClientException
{
    public class DeviceNotFoundException : System.Exception
    {
        public string DeviceId { get; }

        public DeviceNotFoundException(string message, System.Exception innerException, string deviceId) : base(message, innerException)
        {
            DeviceId = deviceId;
        }
    }
}