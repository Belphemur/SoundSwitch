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

using Windows.UI.Notifications;

namespace SoundSwitch.Framework.Toast
{
    public class ToastManager
    {
        private const string APP_ID = "aaflalo.SoundSwitch.Application";

        /// <summary>
        /// Show a toast notification with the given data
        /// </summary>
        /// <param name="data"></param>
        public void ShowNotification(ToastData data)
        {
            var notification = data.BuildNotification();
            ToastNotificationManager.CreateToastNotifier(APP_ID).Show(notification);
        }
    }
}