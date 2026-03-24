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

#nullable enable
using System;
using System.Runtime.Serialization;
using Microsoft.Extensions.Caching.Memory;

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

    /// <summary>
    /// General-purpose icon extractor with caching and GDI handle management.
    /// Cached icons are owned by the cache and must not be disposed by callers.
    /// </summary>
    public static class IconExtractor
    {
        private static readonly IMemoryCache IconCache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 500
        });

        /// <summary>
        ///     Extract an icon from an executable or DLL, with caching.
        ///     The returned icon is owned by the cache and must not be disposed by the caller.
        /// </summary>
        /// <param name="file">Path to the executable or DLL file.</param>
        /// <param name="iconIndex">Zero-based icon index within the file.</param>
        /// <param name="largeIcon">When true, extract a large (32×32) icon; otherwise a small (16×16) icon.</param>
        /// <exception cref="IconExtractionException">Thrown when the icon cannot be extracted.</exception>
        /// <returns>A cached icon. Do not dispose the returned icon.</returns>
        public static System.Drawing.Icon Extract(string file, int iconIndex, bool largeIcon)
        {
            var key = $"{file}|{iconIndex}|{largeIcon}";

            try
            {
                if (IconCache.TryGetValue(key, out System.Drawing.Icon? cached) && cached != null && cached.Handle != IntPtr.Zero)
                    return cached;
            }
            catch (ObjectDisposedException)
            {
                IconCache.Remove(key);
            }

            try
            {
                var icon = System.Drawing.Icon.ExtractIcon(file, iconIndex, largeIcon ? 32 : 16)
                           ?? throw new IconExtractionException($"Can't extract icon from file: {file} / index:{iconIndex}");

                using var entry = IconCache.CreateEntry(key);
                entry.SetValue(icon)
                    .SetSize(largeIcon ? 2 : 1)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetPriority(largeIcon ? CacheItemPriority.High : CacheItemPriority.Low)
                    .RegisterPostEvictionCallback((_, value, _, _) =>
                    {
                        if (value is IDisposable disposable)
                            disposable.Dispose();
                    });
                return icon;
            }
            catch (Exception e) when (e is not IconExtractionException)
            {
                throw new IconExtractionException($"Can't extract icon from file: {file} / index:{iconIndex}", e);
            }
        }

        /// <summary>
        ///     Extract an icon from a path that is either a <c>.ico</c> file or a
        ///     <c>dllPath,iconIndex</c> string (the format used by Windows audio device icon paths),
        ///     with caching and GDI handle management.
        ///     The returned icon is owned by the cache and must not be disposed by the caller.
        /// </summary>
        /// <param name="path">
        ///     Either a path ending in <c>.ico</c>, or a comma-separated string of the form
        ///     <c>&lt;dll path&gt;,&lt;icon index&gt;</c>.
        /// </param>
        /// <param name="largeIcon">When true, extract a large (32×32) icon; otherwise a small (16×16) icon.</param>
        /// <exception cref="IconExtractionException">Thrown when the icon cannot be extracted.</exception>
        /// <returns>A cached icon. Do not dispose the returned icon.</returns>
        public static System.Drawing.Icon ExtractFromPath(string path, bool largeIcon)
        {
            var key = $"{path}|{largeIcon}";

            try
            {
                if (IconCache.TryGetValue(key, out System.Drawing.Icon? cached) && cached != null && cached.Handle != IntPtr.Zero)
                    return cached;
            }
            catch (ObjectDisposedException)
            {
                IconCache.Remove(key);
            }

            try
            {
                System.Drawing.Icon icon;
                if (path.EndsWith(".ico"))
                {
                    icon = System.Drawing.Icon.ExtractAssociatedIcon(path)
                           ?? throw new IconExtractionException($"Can't extract icon from .ico file: {path}");
                }
                else
                {
                    var iconInfo = path.Split(',');
                    var dllPath = iconInfo[0];
                    var iconIndex = int.Parse(iconInfo[1]);
                    icon = System.Drawing.Icon.ExtractIcon(dllPath, iconIndex, largeIcon ? 32 : 16)
                           ?? throw new IconExtractionException($"Can't extract icon from path: {path}");
                }

                using var entry = IconCache.CreateEntry(key);
                entry.SetValue(icon)
                    .SetSize(largeIcon ? 2 : 1)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetPriority(largeIcon ? CacheItemPriority.High : CacheItemPriority.Low)
                    .RegisterPostEvictionCallback((_, value, _, _) =>
                    {
                        if (value is IDisposable disposable)
                            disposable.Dispose();
                    });
                return icon;
            }
            catch (Exception e) when (e is not IconExtractionException)
            {
                throw new IconExtractionException($"Can't extract icon from path: {path}", e);
            }
        }
    }
}