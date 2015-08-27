/********************************************************************
* Copyright (C) 2015 Antoine Aflalo
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
using System.Timers;

namespace SoundSwitch.Framework.Updater
{
    public class IntervalUpdateChecker : UpdateChecker
    {
        /// <summary>
        /// Check for update at the wanted interval in seconds
        /// </summary>
        /// <param name="releaseUrl">Url of the GitHub release</param>
        /// <param name="interval">Seconds</param>
        public IntervalUpdateChecker(Uri releaseUrl, uint interval) : base(releaseUrl)
        {
            var timer = new Timer(interval * 1000U);
            timer.Elapsed += TimerElapsed;
            timer.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            CheckForUpdate();
        }
    }
}
