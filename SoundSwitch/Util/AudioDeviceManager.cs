//Source: https://github.com/danielsunnerberg/AudioDevice-Quickswitcher
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using SoundSwitch.Models;


namespace SoundSwitch.Util
{
    /// <summary>
    /// Manages the operating system's audio devices by wrapping Dan Steven's AudioEndPointController (https://github.com/DanStevens/AudioEndPointController).
    /// </summary>
    public class AudioDeviceManager
    {
        private static readonly string ServiceOutputFormat =
            @"<device>
                <index>%d</index>
                <friendlyName>%s</friendlyName>
                <state>%d</state>
                <default>%d</default>
                <description>%s</description>
                <interfaceFriendlyName>%s</interfaceFriendlyName>
                <deviceId>%s
                </deviceId>
            </device>".Replace("\n", "").Replace("\r", "").Replace(" ", "");

        private readonly ProcessExecutor _processExecutor;

        /// <summary>
        /// Creates a new audio device manager, wrapping the AudioEndPointController executable found at the specified path. 
        /// </summary>
        /// <param name="audioEndPointControllerPath">Path to AudioEndPointController executable</param>
        public AudioDeviceManager(string audioEndPointControllerPath)
        {
            _processExecutor = new ProcessExecutor(audioEndPointControllerPath);
        }

        /// <summary>
        /// Returns all connected audio devices.
        /// </summary>
        /// <returns>All connected audio devices</returns>
        public IList<AudioDevice> GetDevices()
        {
            string serviceResponse = _processExecutor.Query("-f " + ServiceOutputFormat);
            string devicesXml = string.Format("<devices>{0}</devices>", serviceResponse);

            XmlSerializer serializer = new XmlSerializer(typeof (AudioDevices));
            var devicesWrapper = (AudioDevices) serializer.Deserialize(new StringReader(devicesXml));
            return devicesWrapper.Devices;
        }

        /// <summary>
        /// Sets the specified audio device as the system default.
        /// </summary>
        /// <param name="audioDevice">Device to be set as system default</param>
        public bool SetDeviceAsDefault(AudioDevice audioDevice)
        {
           return _processExecutor.Start(string.Format(" {0}", audioDevice.Index));
        }
    }
}