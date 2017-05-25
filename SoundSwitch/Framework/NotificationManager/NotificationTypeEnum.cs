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

namespace SoundSwitch.Framework.NotificationManager
{
    public enum NotificationTypeEnum
    {
        DefaultWindowsNotification = 0,
        SoundNotification = 1,
        NoNotification = 2,
        CustomNotification = 3,
        ToastNotification = 4,
        BannerNotification = 5
    }
}