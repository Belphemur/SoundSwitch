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

#nullable enable
using System.Drawing;
using System.Threading;
using System;

namespace SoundSwitch.Common.Framework.Icon
{
    /// <summary>
    /// A disposable reference to a cached GDI icon managed by <see cref="IconExtractor"/>.
    /// <para>
    /// Each <see cref="IconHandle"/> holds one counted reference on the underlying HICON.
    /// The HICON is only destroyed (via <see cref="System.Drawing.Icon.Dispose"/>) when every
    /// reference — both caller handles and the cache entry itself — has been released.
    /// </para>
    /// <para>
    /// Callers <strong>must</strong> dispose this handle when they no longer need the icon.
    /// A finalizer ensures the reference is eventually released even if <see cref="Dispose"/>
    /// is not called explicitly, but explicit disposal is strongly preferred.
    /// </para>
    /// <para>
    /// "Permanent" handles (returned by <see cref="IconExtractor.CreatePermanent"/>) are intended
    /// to be stored in static fields and never disposed. Call <see cref="Acquire"/> on them to
    /// hand out individual disposable references to callers.
    /// </para>
    /// </summary>
    public sealed class IconHandle : IDisposable
    {
        private readonly IconExtractor.CachedEntry _entry;
        private int _disposed; // 0 = not disposed, 1 = disposed; use Interlocked for thread safety

        internal IconHandle(IconExtractor.CachedEntry entry)
        {
            _entry = entry;
        }

        /// <summary>
        /// Gets the underlying <see cref="System.Drawing.Icon"/>.
        /// Valid for as long as this handle is not disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when this handle has already been disposed.</exception>
        public System.Drawing.Icon Icon
        {
            get
            {
                ObjectDisposedException.ThrowIf(_disposed != 0, this);
                return _entry.Icon;
            }
        }

        /// <summary>
        /// Converts the icon to a <see cref="Bitmap"/>. The caller owns and must dispose the returned bitmap.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when this handle has already been disposed.</exception>
        public Bitmap ToBitmap() => Icon.ToBitmap();

        /// <summary>
        /// Creates a new independent <see cref="IconHandle"/> pointing to the same underlying cached
        /// entry, incrementing the reference count. The new handle must be disposed independently.
        /// </summary>
        /// <remarks>
        /// Useful when a shared "permanent" handle (e.g. a static fallback) needs to hand out
        /// individual disposable references to callers.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">Thrown when this handle has already been disposed.</exception>
        public IconHandle Acquire()
        {
            ObjectDisposedException.ThrowIf(_disposed != 0, this);
            return _entry.AcquireHandle();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            _entry.Release();
            GC.SuppressFinalize(this);
        }

        /// <summary>Fallback finalizer — releases the GDI reference if the caller forgot to dispose.</summary>
        ~IconHandle()
        {
            Dispose();
        }
    }
}
