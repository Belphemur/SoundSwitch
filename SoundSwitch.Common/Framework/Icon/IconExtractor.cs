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
//source : https://stackoverflow.com/questions/6872957/how-can-i-use-the-images-within-shell32-dll-in-my-c-sharp-project

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace SoundSwitch.Common.Framework.Icon
{
    public class IconExtractionException : Exception
    {
        public IconExtractionException()
        {
        }

        public IconExtractionException(string message) : base(message)
        {
        }

        public IconExtractionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected IconExtractionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
    public static class IconExtractor
    {
        /// <summary>
        ///     Extract Icon from executable or DLL
        /// </summary>
        /// <param name="file"></param>
        /// <param name="iconIndex"></param>
        /// <param name="largeIcon"></param>
        /// <exception cref="IconExtractionException">Problem while extracting the icon</exception>
        /// <returns></returns>
        public static System.Drawing.Icon Extract(string file, int iconIndex, bool largeIcon)
        {
            IntPtr large;
            IntPtr small;
            NativeMethods.ExtractIconEx(file, iconIndex, out large, out small, 1);
            try
            {
                return System.Drawing.Icon.FromHandle(largeIcon ? large : small);
            }
            catch(Exception e)
            {
                throw new IconExtractionException($"Can't extract icon from file: {file} / index:{iconIndex}", e);
            }
        }

        private static class NativeMethods
        {
            [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true,
                CallingConvention = CallingConvention.StdCall)]
            public static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion,
                out IntPtr piSmallVersion, int amountIcons);
        }
    }
}