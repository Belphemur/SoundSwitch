using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace SoundSwitch
{
    public interface IConfiguration
    {

    }

    public static class ConfigurationManager<T> where T : IConfiguration, new()
    {
 
        private static readonly string SRoot = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData), Application.ProductName);

        /// <summary>
        ///     Return the filepath of the configuration file
        /// </summary>
        public static string FilePath
        {
            get
            {
                var filePath = Path.Combine(SRoot, typeof (T).Name + ".json");
                return filePath;
            }
        }
        /// <summary>
        /// Load a ConfigurationManager from its file
        /// </summary>
        /// <returns>The loaded configuration if the file exists, else a new instance of the configuration</returns>
        public static T LoadConfiguration()
        {
            var filePath = FilePath;
            if (!File.Exists(filePath))
            {
                return new T();
            }
            var contents = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(contents);
        }
        /// <summary>
        /// Save the configuration in a json file.
        /// </summary>
        /// <param name="configuration">configuration object to save</param>
        public static void SaveConfiguration(T configuration)
        {
            var serializer = new JsonSerializer {NullValueHandling = NullValueHandling.Ignore};
            using (var sw = new StreamWriter(FilePath))
            using (var writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, configuration);
            }
        }
    }
}