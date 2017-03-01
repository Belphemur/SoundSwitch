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

using System;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;

namespace SoundSwitch.Framework.Toast
{
    public class ToastData
    {
        public string Title { get; set; } = "";
        public string Line0 { get; set; } = "";
        public string Line1 { get; set; } = "";
        public int TimeoutSeconds { get; set; } = 20;

        public string ImagePath { get; set; }
        public bool Silent { get; set; } = true;
        public string SoundFilePath { get; set; }

        /// <summary>
        /// Build the toast with the given data
        /// </summary>
        /// <returns></returns>
        public ToastNotification BuildNotificationOld()
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);
            PopulateAudio(toastXml);
            PopulateImage(toastXml);
            PopulateLines(toastXml);
            return new ToastNotification(toastXml);
        }

        /// <summary>
        /// Build the toast with the given data
        /// </summary>
        /// <returns></returns>
        public ToastNotification BuildNotification()
        {
            var toastContent = BuildContent();
            // Parse to XML
            XmlDocument toastXml = new XmlDocument();
            toastXml.LoadXml(toastContent.GetContent());
            return new ToastNotification(toastXml)
            {
                ExpirationTime = DateTime.Now.AddSeconds(TimeoutSeconds)
            };
        }

        public ToastContent BuildContent()
        {
            ToastVisual visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText()
                        {
                            Text = Title
                        },
                        new AdaptiveText()
                        {
                            Text = Line0
                        },
                    },
                    AppLogoOverride = new ToastGenericAppLogo()
                    {
                        Source = ImagePath,
                        HintCrop = ToastGenericAppLogoCrop.Default
                    }
                }
            };
            var toastAudio = new ToastAudio()
            {
                Silent =  Silent
            };
            if (SoundFilePath != null)
            {
                toastAudio.Src = new Uri("file:///" + SoundFilePath);
            }
            return new ToastContent()
            {
                Visual = visual,
                Audio = toastAudio,
                Duration = ToastDuration.Short

            };
        }


        private void PopulateAudio(XmlDocument toastXml)
        {
            //XmlNodeList audio = toastXml.GetElementsByTagName("audio");
            // audio[0].Attributes.GetNamedItem("silent").NodeValue = Silent;
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
            if (Title != null)
                text[0].AppendChild(toastXml.CreateTextNode(Title));
            if (Line0 != null)
                text[1].AppendChild(toastXml.CreateTextNode(Line0));
            if (Line1 != null)
                text[2].AppendChild(toastXml.CreateTextNode(Line1));
        }
    }
}