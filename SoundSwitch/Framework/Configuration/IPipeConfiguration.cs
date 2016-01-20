namespace SoundSwitch.Framework.Configuration
{
    public interface IPipeConfiguration : IConfiguration
    {
        byte[] AesKeyBytes { get; set; }
        string PipeName { get; set; }
        string AuthentificationString { get; set; }
    }
}