/********************************************************************
* Copyright (C) 2015-2017 Antoine Aflalo
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 2
* of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
********************************************************************/

using System.IO;
using Newtonsoft.Json;
using Serilog;

namespace SoundSwitch.Framework.Configuration
{
    public interface IConfiguration
    {
        /// <summary>
        /// Where is the configuration saved
        /// Any change to this will be discarded at the next loading of the configuration
        /// </summary>
        string FileLocation { get; set; }

        void Save();

        /// <summary>
        /// Migrate configuration to a new schema
        /// </summary>
        bool Migrate();
    }

    public static class ConfigurationManager
    {
        private static readonly string SRoot = ApplicationPath.Default;

        static ConfigurationManager()
        {
            if (Directory.Exists(SRoot))
                return;

            Directory.CreateDirectory(SRoot);
        }

        /// <summary>
        /// Load a ConfigurationManager from its file
        /// </summary>
        /// <returns>The loaded configuration if the file exists, else a new instance of the configuration</returns>
        public static T LoadConfiguration<T>() where T : IConfiguration, new()
        {
            var filePath = GetFilePath<T>();
            T obj;
            if (!File.Exists(filePath))
            {
                obj = new T();
            }
            else
            {
                var serializer = new JsonSerializer {NullValueHandling = NullValueHandling.Ignore};
                using (var fileOpen = File.OpenRead(filePath))
                using (var reader = new JsonTextReader(new StreamReader(fileOpen)))
                {
                    obj = serializer.Deserialize<T>(reader);
                }

                if (obj == null)
                {
                    Log.Warning("Can't deserialize the config file {type}: {filePath}", typeof(T), filePath);
                    obj = new T();
                }
            }

            obj.FileLocation = filePath;
            if (obj.Migrate())
            {
                obj.Save();
            }

            return obj;
        }

        private static string GetFilePath<T>() where T : IConfiguration, new()
        {
            var filePath = Path.Combine(SRoot, typeof(T).Name + ".json");
            return filePath;
        }

        /// <summary>
        /// Save the configuration in a json file.
        /// </summary>
        /// <param name="configuration">configuration object to save</param>
        public static void SaveConfiguration<T>(T configuration) where T : IConfiguration, new()
        {
            configuration.FileLocation = null;
            var serializer = new JsonSerializer {NullValueHandling = NullValueHandling.Ignore};
            using var streamWriter = new StreamWriter(GetFilePath<T>());
            using var writer = new JsonTextWriter(streamWriter);
            serializer.Serialize(writer, configuration);
        }
    }
}