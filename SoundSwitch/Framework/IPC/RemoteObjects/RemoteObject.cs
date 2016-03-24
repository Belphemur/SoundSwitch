using System;
using System.Windows.Forms;

namespace SoundSwitch.Framework.IPC.RemoteObjects
{
    public class RemoteObject : MarshalByRefObject
    {
        public static string ObjURL => "RemoteObj.rem";

        /// <summary>
        /// Stop the application
        /// </summary>
        public void StopApplication()
        {
            Application.Exit();
        }

        /// <summary>
        /// Return the version of the server
        /// </summary>
        /// <returns></returns>
        public string GetVersion()
        {
            return Application.ProductVersion;
        }
    }
}