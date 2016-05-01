namespace SoundSwitch.Framework.Configuration
{
    public interface IIPCConfiguration : IConfiguration
    {
        int Port { get; set; }

        /// <summary>
        /// Increment the Port Number and return its value
        /// </summary>
        /// <returns></returns>
        int IncrementPort();

        string ServerUrl();
        string ClientUrl();
    }
}