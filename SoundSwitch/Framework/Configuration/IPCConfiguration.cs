using System;
using System.Linq;
using System.Security.Cryptography;

namespace SoundSwitch.Framework.Configuration
{
    public class IPCConfiguration : IIPCConfiguration
    {

        public int Port { get; set; }

        /// <summary>
        /// Increment the Port Number and return its value
        /// </summary>
        /// <returns></returns>
        public int IncrementPort()
        {
            Port = ++Port%64000;
            if (Port < 60000)
            {
                Port = new Random().Next(60000, 64000);
            }
            Save();
            return Port;
        }

        public string ServerUrl()
        {
            return "localhost:" + IncrementPort();
        }

        public string ClientUrl()
        {
            return "localhost:" + Port;
        }

        public string FileLocation { get; set; }
        public void Save()
        {
            ConfigurationManager.SaveConfiguration(this);
        }
    
    }
}