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

using System.Collections.Generic;
using System.Linq;
using SoundSwitch.Framework.Factory;
using SoundSwitch.Framework.NotificationManager.Notification;

namespace SoundSwitch.Framework.NotificationManager
{
    public class NotificationFactory : AbstractFactory<NotificationTypeEnum, INotification>
    {
        private static readonly IEnumImplList<NotificationTypeEnum, INotification> Notifications = new EnumImplList
            <NotificationTypeEnum, INotification>
            {
                new NotificationNone(),
                new NotificationBanner(),
                new NotificationWindows(),
                new NotificationSound()
            };

        public NotificationFactory() : base(Notifications)
        {
        }

        protected override IReadOnlyDictionary<NotificationTypeEnum, INotification> DataSource()
        {
            return AllImplementations.Where(pair => pair.Value.IsAvailable()).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}