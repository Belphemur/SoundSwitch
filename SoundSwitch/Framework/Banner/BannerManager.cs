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
using Windows.UI.Notifications;

namespace SoundSwitch.Framework.Banner
{
    /// <summary>
    /// Class to manage the banners. This class is the entrypoint to show a notification banner.
    /// </summary>
    public class BannerManager
    {
        private static System.Threading.SynchronizationContext syncContext;
        private static BannerForm banner;

        /// <summary>
        /// Show a banner notification with the given data
        /// </summary>
        /// <param name="data"></param>
        public void ShowNotification(BannerData data)
        {
            // Execute the banner in the context of the UI thread
            syncContext.Post((d) =>
            {
                if (banner == null)
                {
                    banner = new BannerForm();
                    banner.Disposed += (s, e) => banner = null;
                }
                banner.SetData(data);
            }, null);
        }

        /// <summary>
        /// Because notifications dispatched asynchronously, this method must be called in the context of the UI thread
        /// <remarks>This method requires that at least one System.Windows.Form.Control has been created or Application.Run() called</remarks>
        /// </summary>
        internal static void Setup()
        {
            // Grab the synchronization context of the UI thread!
            syncContext = System.Threading.SynchronizationContext.Current;
            if (!(syncContext is System.Windows.Forms.WindowsFormsSynchronizationContext))
                throw new InvalidOperationException("BannerManager must be called in the context of the UI thread.");
        }
    }
}