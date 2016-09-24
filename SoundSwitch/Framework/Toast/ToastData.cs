using System.IO;
using System.Runtime.InteropServices;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace SoundSwitch.Framework.Toast
{
    public class ToastData
    {
        public string Title { get; set; }
        public string Line0 { get; set; }
        public string Line1 { get; set; }

        public string ImagePath { get; set; }
        public bool Silent { get; set; } = false;

        /// <summary>
        /// Build the toast with the given data
        /// </summary>
        /// <returns></returns>
        public ToastNotification BuildNotification()
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);
            PopulateAudio(toastXml);
            PopulateImage(toastXml);
            PopulateLines(toastXml);
            return new ToastNotification(toastXml);
        }


        private void PopulateAudio(XmlDocument toastXml)
        {
            XmlNodeList audio = toastXml.GetElementsByTagName("audio");
            audio[0].Attributes.GetNamedItem("silent").NodeValue = Silent;
        }

        /// <summary>
        /// Populate the set image
        /// </summary>
        /// <param name="toastXml"></param>
        private void PopulateImage(XmlDocument toastXml)
        {
            var imagePath = "file:///" + ImagePath;
            XmlNodeList imageElements = toastXml.GetElementsByTagName("image");
            imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;
        }

        /// <summary>
        /// Popualte the line of the toast
        /// </summary>
        /// <param name="toastXml"></param>
        private void PopulateLines(XmlDocument toastXml)
        {
            var text = toastXml.GetElementsByTagName("text");
            text[0].AppendChild(toastXml.CreateTextNode(Title));
            text[1].AppendChild(toastXml.CreateTextNode(Line0));
            text[2].AppendChild(toastXml.CreateTextNode(Line1));
        }
    }
}