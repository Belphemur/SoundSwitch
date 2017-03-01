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
using System.Timers;

namespace SoundSwitch.Framework.Updater
{
    public class IntervalUpdateChecker : UpdateChecker
    {
        private readonly Timer _timer;

        /// <summary>
        /// Check for update at the wanted interval in seconds
        /// </summary>
        /// <param name="releaseUrl">Url of the GitHub release</param>
        /// <param name="interval">Seconds</param>
        public IntervalUpdateChecker(Uri releaseUrl, uint interval) : this(releaseUrl, interval, false)
        {
        }

        /// <summary>
        /// Check for update at the wanted interval in seconds
        /// </summary>
        /// <param name="releaseUrl">Url of the GitHub release</param>
        /// <param name="interval">Seconds</param>
        /// <param name="checkBeta"></param>
        public IntervalUpdateChecker(Uri releaseUrl, uint interval, bool checkBeta) : base(releaseUrl, checkBeta)
        {
            _timer = new Timer(interval * 1000U);
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            CheckForUpdate();
        }
        /// <summary>
        /// Stop the UpdateChecker
        /// </summary>
        public void StopCheckingUpdate()
        {
            _timer.Stop();
        }
    }
}
