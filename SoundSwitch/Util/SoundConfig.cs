using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SoundSwitch.Util
{
    public class SoundConfig
    {
        /// <summary>
        /// Sets the device by id. Note this has no error checking to indicate 
        /// </summary>
        /// <param name="id"></param>
        private static bool SetDefaultDevice(int id)
        {
            var process = Process.Start(new ProcessStartInfo("lib/SoundSwitch.AudioInterface.exe", id.ToString())
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });
            if (!process.WaitForExit(5000))
            {
                throw new TimeoutException("Timed out while trying to switch audio output.");
            }
            return (process.ExitCode == 0);
        }

        /// <summary>
        /// Tries to set the default (active) device. Returns false if the device is not found
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool SetDefaultDevice(string name)
        {
            var devices = ListDevices();
            var id = devices.First(m => m.Name == name).Id;

            return SetDefaultDevice(id);
        }

        public class OutputItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public override string ToString()
            {
                return Name;
            }
        }

        /// <summary>
        /// Executes endpointcontroller.exe to get a list of devices.
        /// </summary>
        /// <remarks>TODO: Make endpointcontroller a DLL that we can just directly call.</remarks>
        /// <returns></returns>
        public static IEnumerable<OutputItem> ListDevices()
        {
            var process = Process.Start(new ProcessStartInfo("lib/SoundSwitch.AudioInterface.exe")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true 
            });
            var output = process.StandardOutput.ReadToEnd();
            return ParseEndpointOutput(output);
        }

        /// <summary>
        /// Parses the output returned by endpointcontroller.exe.
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        private static IEnumerable<OutputItem> ParseEndpointOutput(string output)
        {
            var list = new List<OutputItem>();
            var lineMatch = new Regex("^(?<id>\\d+): (?<name>.*)$");

            foreach (var line in output.Split('\n'))
            {
                Match match;
                if ((match = lineMatch.Match(line)).Success)
                {
                    list.Add(new OutputItem
                    {
                        Id = System.Convert.ToInt32(match.Groups["id"].Value),
                        Name = match.Groups["name"].Value.Trim()
                    });
                }
            }
            return list;
        }

    }
}
