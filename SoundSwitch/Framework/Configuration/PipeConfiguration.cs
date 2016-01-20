using System;
using System.Linq;
using System.Security.Cryptography;

namespace SoundSwitch.Framework.Configuration
{
    public class PipeConfiguration : IPipeConfiguration
    {
        public PipeConfiguration()
        {
            PipeName = "SoundSwitch_Pipe";
            using (var aes = Aes.Create())
            {
                AesKeyBytes = aes.Key;
            }
            AuthentificationString = RandomString(25);
        }

        public string FileLocation { get; set; }
        public void Save()
        {
            ConfigurationManager.SaveConfiguration(this);
        }

        public byte[] AesKeyBytes { get; set; }
        public string PipeName { get; set; }
        public string AuthentificationString { get; set; }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}