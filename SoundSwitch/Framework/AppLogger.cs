/********************************************************************
* Copyright (C) 2015 Jeroen Pelgrims
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
using System.IO;
using System.Windows.Forms;
using TracerX;

namespace SoundSwitch.Framework
{
    public static class AppLogger
    {
        public class LogRestartor : IDisposable
        {
            public LogRestartor()
            {
                Log.BinaryFile.Close();
                Log.TextFile.Close();
            }
            public void Dispose()
            {
                Log.BinaryFile.CloseAndReopen();
                Log.TextFile.CloseAndReopen();
            }
        }
        static AppLogger()
        {
            SetLoggerOptions(Log.TextFile);
            SetLoggerOptions(Log.BinaryFile);
            Log.TextFileTraceLevel = TraceLevel.Warn;
            Log.EventLogTraceLevel = TraceLevel.Error;
            Log.BinaryFileTraceLevel = TraceLevel.Verbose;
            Log.BinaryFile.Open();
            Log.TextFile.Open();
        }

        public static Logger Log { get; } = Logger.GetLogger(Application.ProductName);

        private static void SetLoggerOptions(FileBase file)
        {
            file.Directory = ApplicationPath.Logs;
            file.MaxSizeMb = 1;
            file.FullFilePolicy = FullFilePolicy.Wrap;
            file.CircularStartSizeKb = 5;
        }
    }
}