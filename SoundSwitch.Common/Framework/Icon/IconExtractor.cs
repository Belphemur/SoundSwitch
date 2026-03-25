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
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.Serialization;
using System;

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
    /// General-purpose icon extractor with GDI reference counting and caching.
    /// <para>
    /// Each call to <see cref="Extract"/> or <see cref="ExtractFromPath"/> returns an
    /// <see cref="IconHandle"/> that the caller <strong>must dispose</strong> when the icon is no
    /// longer needed. The underlying HICON is kept alive until every outstanding
    /// <see cref="IconHandle"/> and the cache itself have released their references, at which
    /// point it is destroyed automatically.
    /// </para>
    /// </summary>
    public static class IconExtractor
    {
        /// <summary>
        /// Reference-counted entry stored in the icon cache.
        /// Thread-safe: all reference mutations are guarded by <c>_gate</c>.
        /// </summary>
        internal sealed class CachedEntry
        {
            private readonly System.Drawing.Icon _icon;
            private int _refCount;
            private readonly object _gate = new();

            internal CachedEntry(System.Drawing.Icon icon)
            {
                _icon = icon;
                _refCount = 1; // one reference held on behalf of the cache
            }

            internal System.Drawing.Icon Icon => _icon;
            internal IntPtr Handle => _icon.Handle;

            /// <summary>
            /// Increments the reference count and returns a new <see cref="IconHandle"/>.
            /// Throws <see cref="ObjectDisposedException"/> if the entry has already been fully released.
            /// </summary>
            internal IconHandle AcquireHandle()
            {
                lock (_gate)
                {
                    if (_refCount <= 0)
                        throw new ObjectDisposedException(nameof(CachedEntry));
                    _refCount++;
                }
                return new IconHandle(this);
            }

            /// <summary>
            /// Decrements the reference count. When it reaches zero the underlying icon is disposed.
            /// Called both by <see cref="IconHandle.Dispose"/> and by the cache eviction callback.
            /// </summary>
            internal void Release()
            {
                bool dispose;
                lock (_gate)
                {
                    dispose = --_refCount == 0;
                }
                if (dispose)
                    _icon.Dispose();
            }
        }

        private static readonly IMemoryCache IconCache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 500
        });

        /// <summary>
        /// Configures size, expiration, and eviction-disposal for a cache entry.
        /// </summary>
        private static void ConfigureEntry(ICacheEntry entry, CachedEntry cachedEntry, bool largeIcon)
        {
            entry.SetValue(cachedEntry)
                .SetSize(largeIcon ? 2 : 1)
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetPriority(largeIcon ? CacheItemPriority.High : CacheItemPriority.Low)
                .RegisterPostEvictionCallback((_, value, _, _) =>
                {
                    // Release the cache's reference; the HICON is destroyed only when all
                    // outstanding IconHandles have also been disposed.
                    if (value is CachedEntry e)
                        e.Release();
                });
        }

        /// <summary>
        /// Retrieves a cached <see cref="CachedEntry"/> for <paramref name="key"/>, or creates one
        /// using <paramref name="factory"/> and commits it to the cache. Returns an
        /// <see cref="IconHandle"/> with the caller's reference already incremented.
        /// </summary>
        private static IconHandle GetOrCreate(string key, bool largeIcon, Func<System.Drawing.Icon> factory)
        {
            while (true)
            {
                // Fast path: entry is already in cache.
                if (IconCache.TryGetValue(key, out CachedEntry? existing) && existing != null)
                {
                    try
                    {
                        return existing.AcquireHandle();
                    }
                    catch (ObjectDisposedException)
                    {
                        // The entry was evicted between TryGetValue and AcquireHandle.
                        // Remove the stale key and fall through to re-create.
                        IconCache.Remove(key);
                    }
                }

                // Slow path: create a new icon and commit it.
                var icon = factory();
                var newEntry = new CachedEntry(icon); // starts with refCount = 1 (for the cache)

                // Acquire the caller's handle BEFORE committing so the entry cannot be evicted
                // (and refCount dropped to 0) between commit and the caller receiving the handle.
                var handle = newEntry.AcquireHandle(); // refCount = 2: cache(1) + caller(1)

                using var cacheEntry = IconCache.CreateEntry(key);
                ConfigureEntry(cacheEntry, newEntry, largeIcon);
                // cacheEntry is committed when the using block exits.
                return handle;
            }
        }

        /// <summary>
        /// Extracts an icon from an executable or DLL with GDI reference counting and caching.
        /// </summary>
        /// <param name="file">Path to the executable or DLL file.</param>
        /// <param name="iconIndex">Zero-based icon index within the file.</param>
        /// <param name="largeIcon">When <see langword="true"/>, extracts a 32×32 icon; otherwise 16×16.</param>
        /// <returns>
        /// An <see cref="IconHandle"/> the caller <strong>must dispose</strong> when done.
        /// </returns>
        /// <exception cref="IconExtractionException">Thrown when the icon cannot be extracted.</exception>
        public static IconHandle Extract(string file, int iconIndex, bool largeIcon)
        {
            var key = $"{file}|{iconIndex}|{largeIcon}";
            try
            {
                return GetOrCreate(key, largeIcon, () =>
                    System.Drawing.Icon.ExtractIcon(file, iconIndex, largeIcon ? 32 : 16)
                    ?? throw new IconExtractionException($"Can't extract icon from file: {file} / index:{iconIndex}"));
            }
            catch (Exception e) when (e is not IconExtractionException)
            {
                throw new IconExtractionException($"Can't extract icon from file: {file} / index:{iconIndex}", e);
            }
        }

        /// <summary>
        /// Extracts an icon from a path that is either a <c>.ico</c> file or a
        /// <c>dllPath,iconIndex</c> string (the format used by Windows audio device icon paths),
        /// with GDI reference counting and caching.
        /// </summary>
        /// <param name="path">
        /// Either a path ending in <c>.ico</c>, or a comma-separated string of the form
        /// <c>&lt;dll path&gt;,&lt;icon index&gt;</c>.
        /// </param>
        /// <param name="largeIcon">When <see langword="true"/>, extracts a 32×32 icon; otherwise 16×16.</param>
        /// <returns>
        /// An <see cref="IconHandle"/> the caller <strong>must dispose</strong> when done.
        /// </returns>
        /// <exception cref="IconExtractionException">Thrown when the icon cannot be extracted.</exception>
        public static IconHandle ExtractFromPath(string path, bool largeIcon)
        {
            var key = $"{path}|{largeIcon}";
            try
            {
                return GetOrCreate(key, largeIcon, () =>
                {
                    if (path.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
                        return System.Drawing.Icon.ExtractAssociatedIcon(path)
                               ?? throw new IconExtractionException($"Can't extract icon from .ico file: {path}");

                    var iconInfo = path.Split(',');
                    var dllPath = iconInfo[0];
                    var iconIndex = int.Parse(iconInfo[1]);
                    return System.Drawing.Icon.ExtractIcon(dllPath, iconIndex, largeIcon ? 32 : 16)
                           ?? throw new IconExtractionException($"Can't extract icon from path: {path}");
                });
            }
            catch (Exception e) when (e is not IconExtractionException)
            {
                throw new IconExtractionException($"Can't extract icon from path: {path}", e);
            }
        }

        /// <summary>
        /// Creates a permanent (non-cached, non-evictable) <see cref="CachedEntry"/> around a
        /// static application-lifetime icon and returns an <see cref="IconHandle"/> that acts as the
        /// permanent holder.
        /// <para>
        /// Store the returned handle in a <c>static readonly</c> field and never dispose it.
        /// Hand out individual disposable references to callers via <see cref="IconHandle.Acquire"/>.
        /// </para>
        /// <para>
        /// The permanent holder keeps the reference count at ≥ 1, so the underlying icon is
        /// never destroyed as long as the static field is live.
        /// </para>
        /// </summary>
        /// <param name="icon">The icon to wrap. Must be a static application-lifetime singleton.</param>
        /// <returns>
        /// A permanent <see cref="IconHandle"/> that should be stored as a static field and
        /// never disposed.
        /// </returns>
        public static IconHandle CreatePermanent(System.Drawing.Icon icon)
        {
            var entry = new CachedEntry(icon); // _refCount = 1, held permanently by the returned handle
            return new IconHandle(entry);
        }
    }
}
