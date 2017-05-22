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

using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using SoundSwitch.Framework.Factory;
using SoundSwitch.Framework.NotificationManager.Notification;

namespace SoundSwitch.Framework.NotificationManager
{
    public class NotificationFactory : AbstractFactory<NotificationTypeEnum, INotification>
    {
        private static readonly IEnumImplList<NotificationTypeEnum, INotification> Notifications = new EnumImplList
            <NotificationTypeEnum, INotification>
            {
                new NotificationWindows(),
                new NotificationSound(),
                new NotificationCustom(),
                new NotificationToast(),
                new NotificationBanner(),
                new NotificationNone()
            };

        public NotificationFactory() : base(Notifications)
        {
        }


        /// <summary>
        ///     Get the implementation for the given Enum
        /// </summary>
        /// <param name="eEnum"></param>
        /// <returns></returns>
        public new INotification Get(NotificationTypeEnum eEnum)
        {
            INotification value;
            if (!AllImplementations.TryGetValue(eEnum, out value))
                throw new InvalidEnumArgumentException();
            if (!value.IsAvailable())
                throw new InvalidEnumArgumentException(@"Can't be selected");
            return value;
        }

        /// <summary>
        ///     Configure the list control DataSource, ValueMember and DisplayMember
        /// </summary>
        /// <param name="list"></param>
        public new void ConfigureListControl(ListControl list)
        {
            list.DataSource =
                AllImplementations.Values.Where(notif => notif.IsAvailable()).Select(
                        implementation => new {Type = implementation.TypeEnum, Display = implementation.Label})
                    .ToArray();
            list.ValueMember = "Type";
            list.DisplayMember = "Display";
        }
    }
}